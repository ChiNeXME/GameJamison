using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DragDrop : MonoBehaviour
{
    public LayerMask dragLayer;
    public LayerMask InteractableLayer;
    private GameObject drag;
    private Camera mainCamera;
    bool isDragging = false;
    RaycastHit2D hit;
    Vector2 originalPos;
    private void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            isDragging = true;

            //get object under (change accordingly)
            if (!hit)
            {
                hit = Physics2D.Raycast(worldPos, Vector2.zero, 100, dragLayer);
                if (hit) originalPos = hit.collider.gameObject.transform.position;
            }
            
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;

            //check if the mouse released on smth (ie tray blender etc) and if not, reset back to original
            RaycastHit2D hitInteractable = Physics2D.Raycast(worldPos, Vector2.zero, 100, InteractableLayer);

            if (hit)
            {
                if (hitInteractable) //if on smth like tray, disappear + do smth with that thing idk (need an ashton to clarify)
                {
                    //delete to test
                    Destroy(hit.collider.gameObject);
                }
                else
                {
                    hit.collider.gameObject.transform.position = originalPos;
                }
                hit = new RaycastHit2D();
            }
        }

        if (hit && isDragging) //get dragged
        {
            hit.collider.gameObject.transform.position = worldPos;
        }
    }
}
