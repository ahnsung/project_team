using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonUIManager : MonoBehaviour
{
    // =========================================================
    // Panels
    // =========================================================

    [Header("Panels")]
    [SerializeField]
    private GameObject directionPanel;

    [SerializeField]
    private GameObject minimapRoot;


    // =========================================================
    // Direction Buttons
    // =========================================================

    [Header("Direction Buttons")]

    [SerializeField]
    private Button upButton;

    [SerializeField]
    private Button downButton;

    [SerializeField]
    private Button leftButton;

    [SerializeField]
    private Button rightButton;


    // =========================================================
    // Reference
    // =========================================================

    [Header("Reference")]

    [SerializeField]
    private RoomTraversalController roomTraversalController;


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        ResolveReferences();


        HideDirectionPanel();


        if (minimapRoot != null)
        {
            minimapRoot.SetActive(true);
        }
    }


    // =========================================================
    // References
    // =========================================================

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
    // Generic Button Refresh
    // =========================================================

    /*
     * DungeonManager.RefreshAll() 등에서
     * 기존 호환을 위해 남겨두는 함수.
     *
     * 이 함수는 버튼 GameObject 자체를 숨기지 않고
     * interactable만 변경한다.
     */
    public void RefreshDirectionButtons(
        Dictionary<MoveDirection, bool> availableDirections)
    {
        SetButtonInteractable(
            upButton,
            IsAvailable(
                availableDirections,
                MoveDirection.Up
            )
        );


        SetButtonInteractable(
            downButton,
            IsAvailable(
                availableDirections,
                MoveDirection.Down
            )
        );


        SetButtonInteractable(
            leftButton,
            IsAvailable(
                availableDirections,
                MoveDirection.Left
            )
        );


        SetButtonInteractable(
            rightButton,
            IsAvailable(
                availableDirections,
                MoveDirection.Right
            )
        );
    }


    // =========================================================
    // Special Direction Panel
    // =========================================================

    /*
     * Space 입력 전용.
     *
     * Door / OneWay 등 실제 사용 가능한 방향만
     * 화면에 표시한다.
     *
     * false인 방향 버튼은
     * 비활성화가 아니라 GameObject 자체를 숨긴다.
     */
    public void ShowSpecialDirectionPanel(
        Dictionary<MoveDirection, bool> availableDirections)
    {
        ResolveReferences();


        SetButtonVisible(
            upButton,
            IsAvailable(
                availableDirections,
                MoveDirection.Up
            )
        );


        SetButtonVisible(
            downButton,
            IsAvailable(
                availableDirections,
                MoveDirection.Down
            )
        );


        SetButtonVisible(
            leftButton,
            IsAvailable(
                availableDirections,
                MoveDirection.Left
            )
        );


        SetButtonVisible(
            rightButton,
            IsAvailable(
                availableDirections,
                MoveDirection.Right
            )
        );


        if (directionPanel != null)
        {
            directionPanel.SetActive(true);
        }


        Debug.Log(
            "[DungeonUIManager] " +
            "특수 이동 방향 패널 표시"
        );
    }


    // =========================================================
    // Show Normal
    // =========================================================

    public void ShowDirectionPanel()
    {
        /*
         * 기존 코드 호환용.
         *
         * 일반 Show를 호출하면
         * 버튼들을 모두 다시 보이게 한다.
         */

        ShowAllDirectionButtons();


        if (directionPanel != null)
        {
            directionPanel.SetActive(true);
        }
    }


    // =========================================================
    // Hide
    // =========================================================

    public void HideDirectionPanel()
    {
        if (directionPanel != null)
        {
            directionPanel.SetActive(false);
        }
    }


    // =========================================================
    // Button Click
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


    // =========================================================
    // Close Button
    // =========================================================

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
            HideDirectionPanel();
        }
    }


    // =========================================================
    // Helpers
    // =========================================================

    private bool IsAvailable(
        Dictionary<MoveDirection, bool> directions,
        MoveDirection direction)
    {
        if (directions == null)
        {
            return false;
        }


        if (
            !directions.TryGetValue(
                direction,
                out bool available
            )
        )
        {
            return false;
        }


        return available;
    }


    private void SetButtonInteractable(
        Button button,
        bool value)
    {
        if (button == null)
        {
            return;
        }


        button.interactable =
            value;
    }


    private void SetButtonVisible(
        Button button,
        bool value)
    {
        if (button == null)
        {
            return;
        }


        button.gameObject.SetActive(
            value
        );


        /*
         * 표시된 버튼은 반드시 클릭 가능하게.
         */
        if (value)
        {
            button.interactable =
                true;
        }
    }


    private void ShowAllDirectionButtons()
    {
        ShowButton(
            upButton
        );

        ShowButton(
            downButton
        );

        ShowButton(
            leftButton
        );

        ShowButton(
            rightButton
        );
    }


    private void ShowButton(
        Button button)
    {
        if (button == null)
        {
            return;
        }


        button.gameObject.SetActive(
            true
        );
    }
}