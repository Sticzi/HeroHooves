using UnityEngine;
using cherrydev;
using UnityEngine.U2D;
using DG.Tweening;  // Make sure DOTween is installed
using Cinemachine;
using System.Threading.Tasks;


public class StartDialogue : MonoBehaviour
{
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph dialogGraphIntro;
    [SerializeField] private DialogNodeGraph dialogGraphKnightBump;
    [SerializeField] private DoorFlyOff doors;
    [SerializeField] private GameObject goblin;
    [SerializeField] private GameObject knight;
    [SerializeField] private GameObject actualKnight;
    [SerializeField] private GameObject player;
    [SerializeField] private ContactFilter2D WallFilter;
    [SerializeField] private ContactFilter2D GroundFilter;
    [SerializeField] private Sprite KnightIdle;

    [SerializeField] private GameObject protip;

    private Tween moveTween;
    private Tween landTween;



   [Header("Camera Settings")]
    public PixelPerfectCamera pixelPerfectCamera;
    public CinemachineBrain cinemachineBrain;
    public CinemachineVirtualCamera MaineMenuVirtualCamera;


    public void IntroCutscene()
    {
        MaineMenuVirtualCamera.Priority = 12;
        DOVirtual.DelayedCall(2f, () =>
        {
            doors.Kick(); // Call the Kick method after the delay            
            StartPrefabDialogue();
        });
    }

    // aight for some reason nie dzia³a function w pierwszym nodzie aleee to mo¿na daæ do tego dziadostwa on dialogue start
    public void StartPrefabDialogue()
    {
        dialogBehaviour.StartDialog(dialogGraphIntro);

        dialogBehaviour.BindExternalFunction("Camera", CameraOnPlayer);        
        dialogBehaviour.BindExternalFunction("ZoomOut", ZoomOut);
        dialogBehaviour.BindExternalFunction("kurwachuj", GoblinStartRun);
        dialogBehaviour.BindExternalFunction("rycerzwybiegawchuj", KnightStartRun);


    }

    //po wszystkim zrob cos co wylazy player freeze na sie kliknie spacje i zrobi jump na koniku

    public void StartKnightBumpDialogue()
    {
        dialogBehaviour.StartDialog(dialogGraphKnightBump);

        dialogBehaviour.BindExternalFunction("JumpToolTip", EnableToolTipBool);
    }

    private bool canActivateToolTip = false;
    public void EnableToolTipBool()
    {
        canActivateToolTip = true;
    }

    public async void ActiveToolTip()
    {
        if (canActivateToolTip)
        {
            protip.SetActive(true);

            // Wait for the jump key to be pressed
            await WaitForJumpInputAsync();

            // Enable the player's HorseMovement component immediately
            player.GetComponent<HorseMovement>().enabled = true;
            player.GetComponent<HorseController2D>().Jump(false);

            // Play the delete animation
            protip.GetComponent<Animator>().SetTrigger("Delete");

            // Destroy the tooltip after 2 seconds
            Destroy(protip, 2f);
        }
    }

    private async Task WaitForJumpInputAsync()
    {
        while (!Input.GetButtonDown("Jump"))
        {
            await Task.Yield(); // Wait for the next frame
        }
    }

    private void Running(GameObject runner, float speed, float distance, System.Action onStopCallback = null)
    {
        Rigidbody2D runnerRb = runner.GetComponent<Rigidbody2D>();
        Animator runnerAnimator = runner.GetComponent<Animator>();
        float runDuration = distance / speed; // Calculate how long the run should last

        // Start running animation
        runnerAnimator.SetBool("isRunning", true);
        runnerAnimator.ResetTrigger("Land");

        // Set velocity for running
        runnerRb.velocity = new Vector2(speed, runnerRb.velocity.y);

        // Stop running after the calculated time
        DOVirtual.DelayedCall(runDuration, () =>
        {
            // Stop running
            runnerRb.velocity = Vector2.zero;
            runnerAnimator.SetBool("isRunning", false);

            // Callback after stopping
            onStopCallback?.Invoke();
        });
    }

    private void GoblinJump(GameObject jumper, float jumpHeight, float jumpDistance, float jumpDuration, System.Action onLandCallback = null)
    {
        Rigidbody2D jumperRb = jumper.GetComponent<Rigidbody2D>();
        Animator jumperAnimator = jumper.GetComponent<Animator>();

        // Trigger crouch before jumping
        jumperAnimator.SetTrigger("Crouch");
        jumperAnimator.ResetTrigger("Jump");

        DOVirtual.DelayedCall(0.5f, () => // Delay for crouch animation
        {
            // Trigger jump animation
            jumperAnimator.SetTrigger("Jump");
            jumperAnimator.ResetTrigger("Crouch");


            // Calculate jump velocity
            float jumpForceY = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y) * jumperRb.gravityScale);
            float jumpForceX = jumpDistance;

            // Apply jump force
            jumperRb.velocity = new Vector2(jumpForceX, jumpForceY);



            // Check for landing
            //DOTween.To(() => jumperRb.velocity.y, _ => { }, 0, jumpDuration).OnComplete(() =>
            DOVirtual.DelayedCall(0.3f, () =>
            {
                moveTween = DOVirtual.Float(0, 1, 0.5f, _ =>
                {
                    if (jumperRb.IsTouching(GroundFilter))
                    {

                        
                        jumperRb.velocity = Vector2.zero;
                        // Trigger landing animation
                        jumperAnimator.SetTrigger("Land");
                        jumperAnimator.ResetTrigger("Jump");


                        // Delay for landing animation before next action
                        DOVirtual.DelayedCall(0.5f, () =>
                        {                            
                            onLandCallback?.Invoke();
                        });
                    }
                });
                
            });
        });
    }



    public void KnightStartRun()
    {
        StartKnightMovement(5);
        knight.GetComponent<Animator>().SetBool("isControlled", true);
    }

    private void StartKnightMovement(float duration)
    {
        bool siema = false;
        // Use a dummy tween as a timer
        landTween = DOVirtual.Float(0, 1, duration, _ =>
        {
            knight.GetComponent<Animator>().SetFloat("speed", Mathf.Abs(knight.GetComponent<Rigidbody2D>().velocity.x));
            knight.GetComponent<KnightController2D>().Move(15 * Time.fixedDeltaTime); // Call knightMove() every frame

            if (knight.GetComponent<Rigidbody2D>().IsTouching(WallFilter))
            {
                knight.GetComponent<Animator>().SetBool("isControlled", false);
                
                if(siema == false)
                {
                    FindObjectOfType<AudioManager>().Play("bump");
                    siema = true;
                }
                

                StartKnightBumpDialogue();
                landTween.Kill(); // Stop the tween
            }
        })
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Incremental); // Loop infinitely

        
    }   


    [Header("JumpSequence")]
    [SerializeField] float initialRunDistance = 12f;
    [SerializeField] float initialRunSpeed = 4f;
    [SerializeField] float firstJumpHeight = 5f;
    [SerializeField] float firstJumpDistance = 5f;
    [SerializeField] float firstJumpDuration = 1.25f;

    [SerializeField] float secondRunDistance = 6f; // Smaller distance for the second run
    [SerializeField] float secondRunSpeed = 4f;
    [SerializeField] float secondJumpHeight = 3f;
    [SerializeField] float secondJumpDistance = 3f;
    [SerializeField] float secondJumpDuration = 1f;


    public void GoblinStartRun()
    {
        // First run
        Running(goblin, initialRunSpeed, initialRunDistance, () =>
        {
            // First jump
            GoblinJump(goblin, firstJumpHeight, firstJumpDistance, firstJumpDuration, () =>
            {
                // Second run
                Running(goblin, secondRunSpeed, secondRunDistance, () =>
                {
                    // Second jump
                    GoblinJump(goblin, secondJumpHeight, secondJumpDistance, secondJumpDuration, () =>
                    {
                        Running(goblin, initialRunSpeed, initialRunDistance, () =>
                        {
                            Destroy(goblin);
                        });
                    });
                });
            });
        });
    }




    public void CameraOnPlayer()
    {
        MaineMenuVirtualCamera.Priority = 12;
        if (pixelPerfectCamera != null)
        {
            pixelPerfectCamera.enabled = true;
        }
    }

    public void ZoomOut()
    {
        // Disable Pixel Perfect Camera before starting to zoom out
        if (pixelPerfectCamera != null)
        {
            pixelPerfectCamera.enabled = false;
        }


    }
}