using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Base Stats")]
    public int STR = 5;
    public int DEX = 5;
    public int CON = 5;
    public int INT = 5;

    [Header("Weapon")]
    public int weaponDamage = 0;

    public int MaxHealth => 50 + CON * 10;
    public int MaxHunger => 100 + CON * 5;
    public int MaxMental => 50 + INT * 5;
    public int InventoryCapacity => 32;

    private const string STR_KEY = "STAT_STR";
    private const string DEX_KEY = "STAT_DEX";
    private const string CON_KEY = "STAT_CON";
    private const string INT_KEY = "STAT_INT";

    private void Awake()
    {
        Instance = this;
        LoadStats();
    }

    public int GetBaseAttackDamage()
    {
        return STR * 2 + weaponDamage;
    }

    public int GetFinalAttackDamage()
    {
        int damage = GetBaseAttackDamage();

        if (PlayerResourceManager.Instance != null &&
            PlayerResourceManager.Instance.IsMentalDamagePenaltyActive())
        {
            damage = Mathf.RoundToInt(damage * 0.5f);
        }

        return Mathf.Max(1, damage);
    }

    public int GetFinalAccuracy(int enemyEvasion)
    {
        int accuracy = 100 - (enemyEvasion - DEX * 3);

        if (PlayerResourceManager.Instance != null)
            accuracy -= PlayerResourceManager.Instance.GetMentalAccuracyPenalty();

        return Mathf.Clamp(accuracy, 10, 95);
    }

    public int GetFinalEvasion(int enemyAccuracy)
    {
        int evasionChance = 100 - (enemyAccuracy - DEX * 3);
        return Mathf.Clamp(evasionChance, 5, 95);
    }

    public int GetRunSuccessPercent()
    {
        return Mathf.Clamp(50 + DEX * 2, 0, 95);
    }

    public void AddSTR() { STR++; SaveStats(); ApplyStatChange(); }
    public void SubSTR() { STR = Mathf.Max(0, STR - 1); SaveStats(); ApplyStatChange(); }

    public void AddDEX() { DEX++; SaveStats(); ApplyStatChange(); }
    public void SubDEX() { DEX = Mathf.Max(0, DEX - 1); SaveStats(); ApplyStatChange(); }

    public void AddCON() { CON++; SaveStats(); ApplyStatChange(); }
    public void SubCON() { CON = Mathf.Max(0, CON - 1); SaveStats(); ApplyStatChange(); }

    public void AddINT() { INT++; SaveStats(); ApplyStatChange(); }
    public void SubINT() { INT = Mathf.Max(0, INT - 1); SaveStats(); ApplyStatChange(); }

    private void ApplyStatChange()
    {
        if (PlayerResourceManager.Instance != null)
            PlayerResourceManager.Instance.ApplyMaxResourceFromStats();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ApplyCapacityFromStats();
    }

    private void SaveStats()
    {
        PlayerPrefs.SetInt(STR_KEY, STR);
        PlayerPrefs.SetInt(DEX_KEY, DEX);
        PlayerPrefs.SetInt(CON_KEY, CON);
        PlayerPrefs.SetInt(INT_KEY, INT);
        PlayerPrefs.Save();
    }

    private void LoadStats()
    {
        STR = PlayerPrefs.GetInt(STR_KEY, STR);
        DEX = PlayerPrefs.GetInt(DEX_KEY, DEX);
        CON = PlayerPrefs.GetInt(CON_KEY, CON);
        INT = PlayerPrefs.GetInt(INT_KEY, INT);
    }
}