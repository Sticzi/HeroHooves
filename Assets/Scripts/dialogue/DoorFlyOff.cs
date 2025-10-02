using UnityEngine;
using DG.Tweening;

public class DoorFlyOff : MonoBehaviour
{
    public float flyOffDuration = 2f;   // Duration of the fly-off effect
    public float flyOffDistanceX = 15f; // Horizontal distance to fly
    public float flyOffDistanceY = 10f; // Initial vertical distance to fly
    public float spinSpeed = 720f;      // Total rotation in degrees
    public float scaleReduction = 0.1f; // Target scale factor at the end of fly-off

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    void Start()
    {
        // Store the door's initial position, scale, and rotation for reset
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
    }

    public void Kick()
    {
        // Reset the door's transform before flying off (in case it's reset)
        transform.position = originalPosition;
        transform.localScale = originalScale;
        transform.rotation = originalRotation;

        // Create the fly-off animation sequence
        Sequence flyOffSequence = DOTween.Sequence();

        // Move the door diagonally up, then let it "fall" down
        flyOffSequence.Append(transform.DOMove(new Vector3(originalPosition.x + flyOffDistanceX, originalPosition.y + flyOffDistanceY, 0), flyOffDuration / 2).SetEase(Ease.OutQuad));
        flyOffSequence.Append(transform.DOMoveY(originalPosition.y - flyOffDistanceY, flyOffDuration / 2).SetEase(Ease.InQuad));

        // Rotate the door as it flies off
        transform.DORotate(new Vector3(0, 0, spinSpeed), flyOffDuration, RotateMode.FastBeyond360);

        // Scale down the door for a "flying away" effect
        transform.DOScale(originalScale * scaleReduction, flyOffDuration).SetEase(Ease.InExpo);

        // Add a callback at the end of the sequence to reset the door
        flyOffSequence.OnComplete(ResetDoor);
    }

    private void ResetDoor()
    {
        // Optionally reset door position, scale, and rotation
        transform.position = originalPosition;
        transform.localScale = originalScale;
        transform.rotation = originalRotation;
    }
}
