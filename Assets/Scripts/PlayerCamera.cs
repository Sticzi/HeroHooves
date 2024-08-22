using UnityEngine;
using Cinemachine;
using System.Threading.Tasks;
using DG.Tweening;

public class PlayerCamera : MonoBehaviour
{
    CinemachineConfiner horseCameraConfinerComponent;
    CinemachineConfiner knightCameraConfinerComponent;
    private CinemachineConfiner currentConfinerComponent;

    private GameMaster gameMaster;
    private Transform nextSpawnPosition;

    private GameObject horse;
    private GameObject knight;

    private Transform background;
    public LayerMask roomLayerMask;
    private float roomCheckRadius = 0.25f;
    public float damping = 1;
    public float transitionDuration = 0.75f;

    private Transform FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child;
            }
        }
        return null;
    }

    private void Awake()
    {
        horse = GameObject.FindGameObjectWithTag("Horse");
        knight = GameObject.FindGameObjectWithTag("Knight");

        horseCameraConfinerComponent = GameObject.FindGameObjectWithTag("VirtualCameraHorse").GetComponent<CinemachineConfiner>();
        knightCameraConfinerComponent = GameObject.FindGameObjectWithTag("VirtualCameraKnight").GetComponent<CinemachineConfiner>();
        gameMaster = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();

        background = GameObject.FindGameObjectWithTag("Background").GetComponent<Transform>();
    }
    
    void Start()
    {        
               
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        
        if (collision.gameObject.layer == 15)
        {                      
            if (CompareTag("Knight"))
            {
                CheckPlayerRoom();
            }
            else
            {
                CheckPlayerRoom();
            }
        }        
    }

    public void SavePlayerSpawnPoint()
    {
        if (CompareTag("Knight"))
        {
            gameMaster.savedKnightPosition = nextSpawnPosition.position;
        }
        else
        {
            gameMaster.savedHorsePosition = nextSpawnPosition.position;
            if (GetComponent<HorseController2D>().KnightPickedUp)
            {
                gameMaster.savedKnightPosition = nextSpawnPosition.position;
            }
        }

    }

    private void CheckPlayerRoom()
    {        

        if (CompareTag("Knight"))
        {
            currentConfinerComponent = knightCameraConfinerComponent;
        }
        else
        {
            currentConfinerComponent = horseCameraConfinerComponent;
        }

        Collider2D checkedRoomCameraBound = Physics2D.OverlapCircle(transform.position, roomCheckRadius, roomLayerMask);
        nextSpawnPosition = FindChildWithTag(checkedRoomCameraBound.transform.parent.parent.Find("Entities"), "Checkpoint");

        if (checkedRoomCameraBound != null && currentConfinerComponent.m_BoundingShape2D != checkedRoomCameraBound)
        {
            SavePlayerSpawnPoint();
            SaveBackgroundParalaxPos();

            // Player is in another room
            CameraTransition(checkedRoomCameraBound, currentConfinerComponent);
        }
    }

    public void SaveBackgroundParalaxPos()
    {
        int childCount = background.childCount;
        gameMaster.savedBackgroundPos = new Vector2[childCount];

        for (int i = 0; i < childCount; i++)
        {
            gameMaster.savedBackgroundPos[i] = background.GetChild(i).position;
        }
    }

    private void PlayerFreeze()
    {
        horse.GetComponent<HorseController2D>().PlayerFreeze();
    }
    private void PlayerUnfreeze()
    {
        horse.GetComponent<HorseController2D>().KnightAndHorseFreeze();
    }

    public async void CameraTransition(Collider2D roomCameraBound, CinemachineConfiner confiner)
    {
        //freeze the player in place for the duration of camera transition. Might need some updating Knight-wise, not sure have to check
        PlayerFreeze();

        //tu by³o przedtem save paralaxy ale chyba niepotrzebnie
        //je¿eli wchodzi od do³u
        //if (tossForce > 0)
        //{
        //    horse.GetComponent<BetterJump>().isTossed = true;
        //}

        confiner.m_Damping = damping;
        confiner.m_BoundingShape2D = roomCameraBound;
        
        if (CompareTag("Horse"))
        {
            if(GetComponent<HorseController2D>().KnightPickedUp == true)
            {
                knightCameraConfinerComponent.m_BoundingShape2D = roomCameraBound;
            }            
        }
        await DOVirtual.Float(damping, 0.2f, transitionDuration, v =>
        {
            confiner.m_Damping = v;

        }).AsyncWaitForCompletion();
        PlayerUnfreeze();

        confiner.m_Damping = 0;
        await Task.Yield();
    }
}
