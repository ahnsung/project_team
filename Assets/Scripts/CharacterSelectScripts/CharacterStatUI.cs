using UnityEngine;
using TMPro;

public class CharacterStatUI : MonoBehaviour
{
    public TextMeshProUGUI strText;
    public TextMeshProUGUI lukText;
    public TextMeshProUGUI intText;
    public TextMeshProUGUI dexText;

    public TextMeshProUGUI descriptionText;

    public void ShowKnight()
    {
        strText.text = "STR : 15";
        lukText.text = "LUK : 5";
        intText.text = "INT : 3";
        dexText.text = "DEX : 7";

        descriptionText.text = "A balanced warrior with high strength and defense.";
    }

    public void ShowMage()
    {
        strText.text = "STR : 3";
        lukText.text = "LUK : 6";
        intText.text = "INT : 15";
        dexText.text = "DEX : 8";

        descriptionText.text = "A master of magic with powerful spells.";
    }

    public void ShowArcher()
    {
        strText.text = "STR : 8";
        lukText.text = "LUK : 7";
        intText.text = "INT : 5";
        dexText.text = "DEX : 15";

        descriptionText.text = "A fast ranged attacker with high agility.";
    }
}