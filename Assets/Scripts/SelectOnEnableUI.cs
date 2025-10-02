using UnityEngine;
using UnityEngine.EventSystems;

public class SelectOnEnableUI : MonoBehaviour
{
    
    private void OnEnable()
    {
        if (EventSystem.current != null && gameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}