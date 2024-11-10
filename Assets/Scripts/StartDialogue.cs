using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cherrydev;
using UnityEngine.U2D;
using DG.Tweening;  // Make sure DOTween is installed
using Cinemachine;


public class StartDialogue : MonoBehaviour
{
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph dialogGraph;

    

    [Header("Camera Settings")]
    public PixelPerfectCamera pixelPerfectCamera;
    public CinemachineVirtualCamera virtualCamera;

    [Space]
    public float zoomedOrthographicSize = 5f;    
    private float originalOrthographicSize;    
    public float zoomDuration = 1f;
    
    public void StartPrefabDialogue()
    {
        dialogBehaviour.StartDialog(dialogGraph);

        dialogBehaviour.BindExternalFunction("zoomIn", ZoomIn);
        dialogBehaviour.BindExternalFunction("zoomOut", ZoomOut);
    }


    void Start()
    {
        // Store the initial orthographic size
        originalOrthographicSize = virtualCamera.m_Lens.OrthographicSize;
    }

    public void ZoomIn()
    {
        // Smoothly change the orthographic size
        DOTween.To(() => virtualCamera.m_Lens.OrthographicSize, x => virtualCamera.m_Lens.OrthographicSize = x, zoomedOrthographicSize, zoomDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                // Enable Pixel Perfect Camera after zooming in
                if (pixelPerfectCamera != null) pixelPerfectCamera.enabled = true;
            });
    }

    public void ZoomOut()
    {
        // Disable Pixel Perfect Camera before starting to zoom out
        if (pixelPerfectCamera != null) pixelPerfectCamera.enabled = false;

        // Smoothly reset to the original orthographic size
        DOTween.To(() => virtualCamera.m_Lens.OrthographicSize, x => virtualCamera.m_Lens.OrthographicSize = x, originalOrthographicSize, zoomDuration)
            .SetEase(Ease.InOutSine);
    }
}