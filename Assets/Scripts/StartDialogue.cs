using UnityEngine;
using cherrydev;
using UnityEngine.U2D;
using DG.Tweening;  // Make sure DOTween is installed
using Cinemachine;


public class StartDialogue : MonoBehaviour
{
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph dialogGraph;   
    [SerializeField] private DoorFlyOff doors;

    [Header("Camera Settings")]
    public PixelPerfectCamera pixelPerfectCamera;
    public CinemachineBrain cinemachineBrain;
    public CinemachineVirtualCamera MaineMenuVirtualCamera;

    public void IntroCutscene()
    {
        MaineMenuVirtualCamera.Priority = 0;
        DOVirtual.DelayedCall(2f, () =>
        {
            doors.Kick(); // Call the Kick method after the delay            
            StartPrefabDialogue();
        });

    }


    public void StartPrefabDialogue()
    {
        dialogBehaviour.StartDialog(dialogGraph);

        dialogBehaviour.BindExternalFunction("CameraOnPlayer", CameraOnPlayer);
        dialogBehaviour.BindExternalFunction("zoomOut", ZoomOut);
    }



    public void CameraOnPlayer()
    {
        MaineMenuVirtualCamera.Priority = 0;

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