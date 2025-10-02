using UnityEngine;

public class GoblinMove : MonoBehaviour
{
    public float speed = 5f;
    
    public bool isRunning;
    public GameObject runner;
    

    private void Update()
    {
        if(isRunning)
        {
            Running(runner, speed);
        }
    }

    private void Running(GameObject runner, float speed)
    {
        // Move the goblin to the right
        transform.Translate(Vector2.right * speed * Time.deltaTime);

    }
}
