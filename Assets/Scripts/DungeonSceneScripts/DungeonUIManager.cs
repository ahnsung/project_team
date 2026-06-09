using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject directionPanel;
    [SerializeField] private GameObject minimapRoot;

    [Header("Inventory")]
    [SerializeField] private GameObject inventoryRoot;

    [Header("Buttons")]
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("Reference")]
    [SerializeField] private RoomTraversalController roomTraversalController;

    private void Start()
    {
        HideDirectionPanel();

        if (minimapRoot != null)
            minimapRoot.SetActive(true);

        CloseInventory();
    }

    public void RefreshDirectionButtons(Dictionary<MoveDirection, bool> availableDirections)
    {
        if (upButton != null)
            upButton.interactable =
                availableDirections.ContainsKey(MoveDirection.Up) &&
                availableDirections[MoveDirection.Up];

        if (downButton != null)
            downButton.interactable =
                availableDirections.ContainsKey(MoveDirection.Down) &&
                availableDirections[MoveDirection.Down];

        if (leftButton != null)
            leftButton.interactable =
                availableDirections.ContainsKey(MoveDirection.Left) &&
                availableDirections[MoveDirection.Left];

        if (rightButton != null)
            rightButton.interactable =
                availableDirections.ContainsKey(MoveDirection.Right) &&
                availableDirections[MoveDirection.Right];
    }

    public void ShowDirectionPanel()
    {
        if (directionPanel != null)
            directionPanel.SetActive(true);
    }

    public void HideDirectionPanel()
    {
        if (directionPanel != null)
            directionPanel.SetActive(false);
    }

    public void OpenInventory()
    {
        if (inventoryRoot != null)
            inventoryRoot.SetActive(true);
    }

    public void CloseInventory()
    {
        if (inventoryRoot != null)
            inventoryRoot.SetActive(false);
    }

    public void ToggleInventory()
    {
        if (inventoryRoot == null)
            return;

        inventoryRoot.SetActive(!inventoryRoot.activeSelf);
    }

    public void OnClickMoveUp()
    {
        if (roomTraversalController != null)
            roomTraversalController.SelectNextRoom(MoveDirection.Up);
    }

    public void OnClickMoveDown()
    {
        if (roomTraversalController != null)
            roomTraversalController.SelectNextRoom(MoveDirection.Down);
    }

    public void OnClickMoveLeft()
    {
        if (roomTraversalController != null)
            roomTraversalController.SelectNextRoom(MoveDirection.Left);
    }

    public void OnClickMoveRight()
    {
        if (roomTraversalController != null)
            roomTraversalController.SelectNextRoom(MoveDirection.Right);
    }
}