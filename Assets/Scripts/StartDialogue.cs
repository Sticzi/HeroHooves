using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cherrydev;
using UnityEngine.U2D;
using DG.Tweening;  // Make sure DOTween is installed

public class StartDialogue : MonoBehaviour
{
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph dialogGraph;

    public void StartPrefabDialogue()
    {
        dialogBehaviour.StartDialog(dialogGraph);

        dialogBehaviour.BindExternalFunction("zoomIn", SmoothZoomIn);
        dialogBehaviour.BindExternalFunction("zoomOut", SmoothZoomOut);
    }

    [Header("Camera Settings")]
    public PixelPerfectCamera pixelPerfectCamera;

    [Space]
    [Header("Zoom Settings")]
    public int defaultResolutionX = 448;
    public int defaultResolutionY = 252;
    public float zoomDuration = 1f;
    public int targetResolutionX = 448;
    public int targetResolutionY;

    // przy zmianach rozmiaru ekranu bêdziemy wypierdalaæ pixel perfect kamere i zmieniaæ ortographic size, domyœlny to 7.875
    public void SmoothZoomIn()
    {
        DOTween.To(() => pixelPerfectCamera.refResolutionX, x => pixelPerfectCamera.refResolutionX = x, defaultResolutionX, zoomDuration);
        DOTween.To(() => pixelPerfectCamera.refResolutionY, y => pixelPerfectCamera.refResolutionY = y, defaultResolutionY, zoomDuration);
    }

    public void SmoothZoomOut()
    {
        // Example of zoom out, you could set to default values or another target resolution
        DOTween.To(() => pixelPerfectCamera.refResolutionX, x => pixelPerfectCamera.refResolutionX = x, defaultResolutionX*2, zoomDuration);
        DOTween.To(() => pixelPerfectCamera.refResolutionY, y => pixelPerfectCamera.refResolutionY = y, defaultResolutionY*2, zoomDuration);
    }
}

