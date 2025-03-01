using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectOnEnableUI : MonoBehaviour
{
    
    private void OnEnable()
    {

        EventSystem.current.SetSelectedGameObject(this.gameObject);
    }
}