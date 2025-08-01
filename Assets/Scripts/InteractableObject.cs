using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem; // Dodaj przestrzeñ nazw dla Input System

public class InteractableObject : MonoBehaviour
{           
    [SerializeField] private ContactFilter2D ContactFilter;
    [SerializeField] private Rigidbody2D rb;
    public InputActionReference interactAction; // Nowa zmienna dla akcji wejœciowej
    
    public bool IsPlayerInsideTriggerCollider => rb.IsTouching(ContactFilter);
    public UnityEvent OnProximityEnable;
    public UnityEvent OnInteract;
    public UnityEvent OnProximityDisable;

    [SerializeField] private HorseMovement horseMovement;
    private KnightMovement knightMovement;

    public enum BoolType { Swap, Jump, DoubleJump, Pickup, Drop, Attack }
    [SerializeField] private BoolType _boolToUse;

    public bool TargetBool
    {
        get
        {
            switch (_boolToUse)
            {
                case BoolType.Pickup: return horseMovement.CanPickUp;
                case BoolType.Drop: return horseMovement.CanDrop;
                case BoolType.Jump: return horseMovement.CanJump;
                case BoolType.DoubleJump: return horseMovement.CanDoubleJump;

                //knight
                case BoolType.Swap: return horseMovement.CanSpawnedKnightSwap;
                case BoolType.Attack: return horseMovement.CanSpawnedKnightAttack;
                default: return false;
            }
        }
        set
        {
            switch (_boolToUse)
            {
                case BoolType.Pickup:
                    if (horseMovement != null) horseMovement.CanPickUp = value;
                    break;
                case BoolType.Drop:
                    if (horseMovement != null) horseMovement.CanDrop = value;
                    break;
                case BoolType.DoubleJump:
                    if (horseMovement != null) horseMovement.CanDoubleJump = value;
                    break;
                case BoolType.Jump:
                    if (horseMovement != null) horseMovement.CanJump = value;
                    break;

                //knight
                case BoolType.Swap:
                    if (knightMovement != null)
                    {
                        knightMovement.CanSwap = value;
                        horseMovement.CanSpawnedKnightSwap = value;
                    }
                    break;
                case BoolType.Attack:
                    if (knightMovement != null)
                    {
                        knightMovement.CanAttack = value;
                        horseMovement.CanSpawnedKnightAttack = value;
                    }
                    break;
            }
        }
    }

    public bool canBeTriggered = true;
    private bool canInteract = false;
    public bool CanBeTriggered 
    {
        get { return canBeTriggered; }
        set { canBeTriggered = value; } 
    }

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.Enable();

    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.Disable();
    }

    public void EnableMechanic()
    {
        if(horseMovement.GetComponent<HorseController2D>().spawnedKnight != null)
        {
            knightMovement = horseMovement.GetComponent<HorseController2D>().spawnedKnight.GetComponent<KnightMovement>();
        }
        TargetBool = true;
    }

    void Update()
    {
        if (IsPlayerInsideTriggerCollider && CanBeTriggered)
        {
            
            OnProximityEnable.Invoke();
            EnableMechanic();  // Call the mechanic activation
            canInteract = true;
            
            
        }

        if (interactAction != null && interactAction.action.triggered && canInteract)
        {
            OnInteract.Invoke();
        }
    }

}