using UnityEngine;
using cherrydev;
using System.Collections;

public class DialogStarter : MonoBehaviour
{
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph dialogGraph;

    private void Start()
    {
        dialogBehaviour.BindExternalFunction("Test", DebugExternal);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Horse"))
        {
            dialogBehaviour.StartDialog(dialogGraph);
            Collider2D collider = GetComponent<Collider2D>();
            collider.enabled = false;
        }
    }

    private void DebugExternal()
    {
        Debug.Log("External function works!");
    }
}