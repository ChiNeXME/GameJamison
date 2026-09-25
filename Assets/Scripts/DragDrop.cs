using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles world-space mouse dragging for recipe ingredients.
/// Valid drops snap the ingredient to a target instead of destroying it.
/// A future recipe controller can subscribe to IngredientDropped.
/// </summary>
public sealed class DragDrop : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask draggableLayer = 0;
    [SerializeField] private LayerMask dropTargetLayer = 0;

    [Header("Drop Behaviour")]
    [SerializeField] private bool snapToTarget = true;

    public event Action<GameObject, Collider2D> IngredientDropped;

    private Camera mainCamera;
    private Collider2D draggedCollider;
    private Vector3 originalPosition;
    private Vector3 pointerOffset;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || mainCamera == null)
        {
            return;
        }

        Vector2 pointerPosition = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());

        if (mouse.leftButton.wasPressedThisFrame)
        {
            BeginDrag(pointerPosition);
        }

        if (draggedCollider != null && mouse.leftButton.isPressed)
        {
            draggedCollider.transform.position = (Vector3)pointerPosition + pointerOffset;
        }

        if (draggedCollider != null && mouse.leftButton.wasReleasedThisFrame)
        {
            EndDrag(pointerPosition);
        }
    }

    private void BeginDrag(Vector2 pointerPosition)
    {
        Collider2D hit = Physics2D.OverlapPoint(pointerPosition, draggableLayer);
        if (hit == null)
        {
            return;
        }

        draggedCollider = hit;
        originalPosition = draggedCollider.transform.position;
        pointerOffset = originalPosition - (Vector3)pointerPosition;
    }

    private void EndDrag(Vector2 pointerPosition)
    {
        Collider2D ingredient = draggedCollider;
        draggedCollider = null;

        Collider2D target = Physics2D.OverlapPoint(pointerPosition, dropTargetLayer);
        if (target == null)
        {
            ingredient.transform.position = originalPosition;
            return;
        }

        if (snapToTarget)
        {
            Vector3 targetPosition = target.bounds.center;
            targetPosition.z = ingredient.transform.position.z;
            ingredient.transform.position = targetPosition;
        }

        IngredientDropped?.Invoke(ingredient.gameObject, target);
    }

    private void OnDisable()
    {
        CancelDrag();
    }

    public void CancelDrag()
    {
        if (draggedCollider == null)
        {
            return;
        }

        draggedCollider.transform.position = originalPosition;
        draggedCollider = null;
    }
}
