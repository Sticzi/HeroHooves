using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRandomizer : MonoBehaviour
{
    [SerializeField]
    private Vector2 positionOffsetRange = new Vector2(0.5f, 0.5f); // X and Y max offset

    private Animator animator;

    void Start()
    {
        // Randomize starting position using the serialized range
        float xOffset = Random.Range(-positionOffsetRange.x, positionOffsetRange.x);
        float yOffset = Random.Range(-positionOffsetRange.y, positionOffsetRange.y);
        transform.position += new Vector3(xOffset, yOffset, 0f);

        animator = GetComponent<Animator>();
        if (animator != null)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            float randomTime = Random.Range(0f, 1f);
            animator.Play(state.fullPathHash, 0, randomTime);
        }
        else
        {
            Debug.LogWarning("Animator component not found on " + gameObject.name);
        }
    }
}