using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject directionPanel;

    [SerializeField]
    private GameObject minimapRoot;


    [Header("Inventory")]
    [SerializeField]
    private GameObject inventoryRoot;


    [Header("Direction Buttons")]
    [SerializeField]
    private Button upButton;

    [SerializeField]
    private Button downButton;

    [SerializeField]
    private Button leftButton;

    [SerializeField]
    private Button rightButton;


    [Header("References")]
    [SerializeField]
    private RoomTraversalController
        roomTraversalController;


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        ResolveReferences();


        HideDirectionPanel();


        if (minimapRoot != null)
        {
            minimapRoot
                .SetActive(true);
        }


        CloseInventory();
    }


    private void ResolveReferences()
    {
        if (roomTraversalController == null)
        {
            roomTraversalController =
                FindFirstObjectByType<
                    RoomTraversalController
                >();
        }
    }


    // =========================================================
    // Direction Buttons Refresh
    // =========================================================

    public void RefreshDirectionButtons(
        Dictionary<
            MoveDirection,
            bool
        > availableDirections)
    {
        if (availableDirections == null)
            return;


        if (upButton != null)
        {
            upButton.interactable =
                availableDirections
                    .ContainsKey(
                        MoveDirection.Up
                    )
                &&
                availableDirections[
                    MoveDirection.Up
                ];
        }


        if (downButton != null)
        {
            downButton.interactable =
                availableDirections
                    .ContainsKey(
                        MoveDirection.Down
                    )
                &&
                availableDirections[
                    MoveDirection.Down
                ];
        }


        if (leftButton != null)
        {
            leftButton.interactable =
                availableDirections
                    .ContainsKey(
                        MoveDirection.Left
                    )
                &&
                availableDirections[
                    MoveDirection.Left
                ];
        }


        if (rightButton != null)
        {
            rightButton.interactable =
                availableDirections
                    .ContainsKey(
                        MoveDirection.Right
                    )
                &&
                availableDirections[
                    MoveDirection.Right
                ];
        }
    }


    // =========================================================
    // Direction Panel
    // =========================================================

    public void ShowDirectionPanel()
    {
        if (directionPanel != null)
        {
            directionPanel
                .SetActive(true);
        }
    }


    public void HideDirectionPanel()
    {
        if (directionPanel != null)
        {
            directionPanel
                .SetActive(false);
        }
    }


    /*
     * DirectionSelectPanel의
     * X 버튼에서 호출해야 하는 함수.
     *
     * 단순히 HideDirectionPanel()만 호출하면
     * RoomTraversalController의 상태가
     * DirectionChoosing으로 남는다.
     */
    public void OnClickCloseDirectionPanel()
    {
        ResolveReferences();


        if (roomTraversalController != null)
        {
            roomTraversalController
                .CloseDirectionPanel();
        }
        else
        {
            /*
             * 혹시 Controller가 없을 경우
             * UI만이라도 닫는다.
             */
            HideDirectionPanel();
        }
    }


    // =========================================================
    // Inventory
    // =========================================================

    public void OpenInventory()
    {
        if (inventoryRoot != null)
        {
            inventoryRoot
                .SetActive(true);
        }
    }


    public void CloseInventory()
    {
        if (inventoryRoot != null)
        {
            inventoryRoot
                .SetActive(false);
        }
    }


    public void ToggleInventory()
    {
        if (inventoryRoot == null)
            return;


        inventoryRoot.SetActive(
            !inventoryRoot.activeSelf
        );
    }


    // =========================================================
    // Direction Button Click
    // =========================================================

    public void OnClickMoveUp()
    {
        ResolveReferences();


        if (roomTraversalController != null)
        {
            roomTraversalController
                .SelectNextRoom(
                    MoveDirection.Up
                );
        }
    }


    public void OnClickMoveDown()
    {
        ResolveReferences();


        if (roomTraversalController != null)
        {
            roomTraversalController
                .SelectNextRoom(
                    MoveDirection.Down
                );
        }
    }


    public void OnClickMoveLeft()
    {
        ResolveReferences();


        if (roomTraversalController != null)
        {
            roomTraversalController
                .SelectNextRoom(
                    MoveDirection.Left
                );
        }
    }


    public void OnClickMoveRight()
    {
        ResolveReferences();


        if (roomTraversalController != null)
        {
            roomTraversalController
                .SelectNextRoom(
                    MoveDirection.Right
                );
        }
    }
}