using UnityEngine;

[ExecuteInEditMode]
public class CameraBoundsConfigurator : MonoBehaviour
{
    public GameObject world; // Referencja do obiektu World
    public GameObject mainMenu;

    private void OnValidate()
    {
        if (world == null)
        {
            Debug.LogWarning("World object reference is missing.");
            return;
        }

        // Iteracja przez wszystkie child obiekty World
        foreach (Transform level in world.transform)
        {
            if (level.name.StartsWith("Level_"))
            {
                // Znajdowanie obiektu CameraBound wewn¹trz Level_X
                Transform cameraBound = level.Find("CameraBound");

                if (cameraBound != null)
                {
                    // Znajdowanie child obiektu z CompositeCollider2D
                    CompositeCollider2D compositeCollider = cameraBound.GetComponentInChildren<CompositeCollider2D>();

                    if (compositeCollider != null)
                    {
                        // Ustawienie w³aœciwoœci CompositeCollider2D
                        compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
                        compositeCollider.isTrigger = true;

                        //Debug.Log($"Updated CompositeCollider2D in {level.name}/CameraBound.");
                    }
                    else
                    {
                        Debug.LogWarning($"CompositeCollider2D not found in {level.name}/CameraBound.");
                    }
                }
                else
                {
                    Debug.LogWarning($"CameraBound not found in {level.name}.");
                }
            }
        }
        if(mainMenu != null)
        {
            foreach (Transform level in mainMenu.transform)
            {
                if (level.name.StartsWith("Level_"))
                {
                    // Znajdowanie obiektu CameraBound wewn¹trz Level_X
                    Transform cameraBound = level.Find("CameraBound");

                    if (cameraBound != null)
                    {
                        // Znajdowanie child obiektu z CompositeCollider2D
                        CompositeCollider2D compositeCollider = cameraBound.GetComponentInChildren<CompositeCollider2D>();

                        if (compositeCollider != null)
                        {
                            // Ustawienie w³aœciwoœci CompositeCollider2D
                            compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
                            compositeCollider.isTrigger = true;

                            Debug.Log($"Updated CompositeCollider2D in {level.name}/CameraBound.");
                        }
                        else
                        {
                            Debug.LogWarning($"CompositeCollider2D not found in {level.name}/CameraBound.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"CameraBound not found in {level.name}.");
                    }
                }
            }
        }
        
    }
}
