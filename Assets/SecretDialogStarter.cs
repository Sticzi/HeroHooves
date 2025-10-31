using UnityEngine;
using cherrydev;
using System.Collections;

public class SecretDialogStarter : MonoBehaviour
{
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph dialogGraph;
    [SerializeField] private DialogNodeGraph revealDialogGraph;

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
    public void TriggerRevealDialog()
    {
        StartCoroutine(TriggerRevealDialogRoutine());
    }

    private IEnumerator TriggerRevealDialogRoutine()
    {
        yield return new WaitForSeconds(0.4f);
        dialogBehaviour.StartDialog(revealDialogGraph);
    }

    private void DebugExternal()
    {
        Debug.Log("External function works!");
    }
}