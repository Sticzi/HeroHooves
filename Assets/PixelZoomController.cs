using UnityEngine;
using UnityEngine.U2D;

public class PixelZoomController : MonoBehaviour
{
    public PixelPerfectCamera pixelPerfectCamera;

    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            
            pixelPerfectCamera.refResolutionX += (int)(pixelPerfectCamera.refResolutionX*0.01);
            pixelPerfectCamera.refResolutionY += (int)(pixelPerfectCamera.refResolutionY * 0.01);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            pixelPerfectCamera.refResolutionX -= (int)(pixelPerfectCamera.refResolutionX * 0.01);
            pixelPerfectCamera.refResolutionY -= (int)(pixelPerfectCamera.refResolutionY * 0.01);
        }
    }
}
