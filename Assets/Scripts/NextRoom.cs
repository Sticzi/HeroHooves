using UnityEngine;
using Cinemachine;
using System.Threading.Tasks;
using DG.Tweening;

public class NextRoom : MonoBehaviour
{
    public float tossForce;
    public float transitionDuration = 0.75f;
    public float damping = 1;
    public float UnfreezeDelay;
    public float push;
    
    public bool isFrozen;

    public LayerMask roomLayerMask;

    public void Awake()
    {
        roomLayerMask = LayerMask.GetMask("CameraBounds");
        CompositeCollider2D compositeCollider = GetComponent<CompositeCollider2D>();

        if (compositeCollider != null)
        {
            // Set the geometry type to polygons
            compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
            compositeCollider.isTrigger = true;
        }
    }  
}
