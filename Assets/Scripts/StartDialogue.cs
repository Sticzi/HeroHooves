using UnityEngine;
using cherrydev;
using UnityEngine.U2D;
using DG.Tweening;  // Make sure DOTween is installed
using Cinemachine;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class StartDialogue : MonoBehaviour
{
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph dialogGraphIntro;
    [SerializeField] private DialogNodeGraph dialogGraphKnightBump;
    [SerializeField] private DoorFlyOff doors;
    [SerializeField] private GameObject goblin;
    [SerializeField] private GameObject runningKnight;
    private GameObject actualKnight;
    [SerializeField] private GameObject player;
    [SerializeField] private ContactFilter2D WallFilter;
    [SerializeField] private ContactFilter2D GroundFilter;
    [SerializeField] private Sprite KnightIdle;

    [SerializeField] private PlayerInputActions playerInput;

    [SerializeField] private GameObject protip;

    private Tween moveTween;
    private Tween landTween;



   [Header("Camera Settings")]
    public PixelPerfectCamera pixelPerfectCamera;
    public CinemachineBrain cinemachineBrain;
    public CinemachineVirtualCamera MaineMenuVirtualCamera;
    public CinemachineVirtualCamera DialogueVirtualCamera;
   

    public void IntroCutscene()
    {
        actualKnight = GameObject.FindGameObjectWithTag("Knight");

        var impulseSource = GetComponent<CinemachineImpulseSource>();
        MaineMenuVirtualCamera.Priority = 0;
        DOVirtual.DelayedCall(10f, () =>
        {
            FindObjectOfType<AudioManager>().Play("Hit");
            impulseSource.GenerateImpulse();            
            //DialogueVirtualCamera.Priority = 15;
            DOVirtual.DelayedCall(1.5f, () =>
            {
                FindObjectOfType<AudioManager>().Play("Hit");
                impulseSource.GenerateImpulse();
                DOVirtual.DelayedCall(1.5f, () =>
                {
                    FindObjectOfType<AudioManager>().Play("Attack");
                    impulseSource.GenerateImpulse();
                    doors.Kick(); // Call the Kick method after the delay
                    DOVirtual.DelayedCall(1.5f, () =>
                    {

                        StartPrefabDialogue();
                    });
                });

            });
            
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


            //z powodu jak dzia³a ten skrypt na dialogi to trzeba najpierw
            //poczekaæ a¿ skipniemy dialog i wy³¹czymy dialogue boxa zanim skoczymy bo takto oba sie naraz odpal¹ w sumie to nie wiem
            //czy to prawda bo tam jakies dziwne rzeczy sie dzialy al
            //e juz zostaw tak jak jest
            //await WaitForSkipInputAsync();
            protip.GetComponent<InteractableObject>().canBeTriggered = true;
            //await Task.Delay(1000);
            await WaitForJumpInputAsync();            
            actualKnight.GetComponent<KnightMovement>().enabled = true;

            // Enable the player's HorseMovement component immediately
            player.GetComponent<HorseMovement>().enabled = true;
            player.GetComponent<HorseController2D>().ExecuteJump();

            // Play the delete animation    
            protip.GetComponent<Animator>().SetTrigger("Delete");
            DialogueVirtualCamera.Priority = 0;
            // Destroy the tooltip after 2 seconds
            Destroy(protip, 2f);
        }
    }

    private async Task WaitForSkipInputAsync()
    {
        // SprawdŸ, czy mamy PlayerInput, jeœli u¿ywasz tej opcji
        if (playerInput != null)
        {
            InputAction skipAction;
            skipAction = playerInput.UI.SkipDialogue;

            // Aktywuj akcjê, jeœli nie jest aktywna
            playerInput.Enable();

            // Czekaj, a¿ akcja zostanie wykonana
            while (!skipAction.WasPressedThisFrame())
            {
                await Task.Yield(); // Czekaj na kolejn¹ klatkê
            }
        }
    }

    private async Task WaitForJumpInputAsync()
    {
        playerInput = new PlayerInputActions();
        InputAction jumpAction;
        jumpAction = playerInput.HorseActionMap.Jump;

        // Aktywuj akcjê, jeœli nie jest aktywna
        playerInput.Enable();

        // Czekaj, a¿ akcja zostanie wykonana
        while (!jumpAction.WasPressedThisFrame())
        {
            await Task.Yield(); // Czekaj na kolejn¹ klatkê
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
        runningKnight.GetComponent<Animator>().SetBool("isControlled", true);
    }

    private void StartKnightMovement(float duration)
    {
        bool siema = false;
        // Use a dummy tween as a timer
        landTween = DOVirtual.Float(0, 1, duration, _ =>
        {
            runningKnight.GetComponent<Animator>().SetFloat("speed", Mathf.Abs(runningKnight.GetComponent<Rigidbody2D>().velocity.x));
            runningKnight.GetComponent<KnightController2D>().Move(15); // Call knightMove() every frame

            if (runningKnight.GetComponent<Rigidbody2D>().IsTouching(WallFilter))
            {
                runningKnight.GetComponent<Animator>().SetBool("isControlled", false);
                
                if(siema == false)
                {
                    FindObjectOfType<AudioManager>().Play("bump");
                    runningKnight.GetComponent<Animator>().enabled = false;
                    runningKnight.GetComponent<SpriteRenderer>().sprite = KnightIdle;
                    siema = true;
                }
                

                StartKnightBumpDialogue();
                Vector3 tempPos = runningKnight.transform.position;
                Destroy(runningKnight);
                actualKnight.transform.position = tempPos;
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
        //MaineMenuVirtualCamera.Priority = 12;
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