using UnityEngine;
using UnityEngine.EventSystems;

public class BattleEnemyClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public BattleUnit enemyUnit;
    public BattleManager battleManager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (battleManager != null)
            battleManager.HoverEnemy(enemyUnit);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (battleManager != null)
            battleManager.ExitHoverEnemy(enemyUnit);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (battleManager != null)
            battleManager.ClickEnemy(enemyUnit);
    }
}