using TMPro;
using UnityEngine;

public class PlayerStatDisplay : MonoBehaviour
{
    public TextMeshProUGUI strText;
    public TextMeshProUGUI dexText;
    public TextMeshProUGUI conText;
    public TextMeshProUGUI intText;

    private void Update()
    {
        if (PlayerStats.Instance == null)
            return;

        strText.text = "STR : " + PlayerStats.Instance.STR;
        dexText.text = "DEX : " + PlayerStats.Instance.DEX;
        conText.text = "CON : " + PlayerStats.Instance.CON;
        intText.text = "INT : " + PlayerStats.Instance.INT;
    }
}