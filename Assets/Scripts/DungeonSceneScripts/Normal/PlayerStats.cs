using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Base Stats")]
    public int STR = 5;
    public int DEX = 5;
    public int CON = 5;
    public int INT = 5;

    [Header("Legacy Weapon")]
    [Tooltip("기존 프로젝트의 무기 공격력 필드입니다.")]
    public int weaponDamage = 0;

    [Header("Equipment Bonus - Runtime")]
    [SerializeField]
    private int equipmentSTR;

    [SerializeField]
    private int equipmentDEX;

    [SerializeField]
    private int equipmentCON;

    [SerializeField]
    private int equipmentINT;

    [SerializeField]
    private int equipmentAttackPower;

    [SerializeField]
    private int equipmentAccuracyBonus;

    public int TotalSTR
    {
        get
        {
            return STR + equipmentSTR;
        }
    }

    public int TotalDEX
    {
        get
        {
            return DEX + equipmentDEX;
        }
    }

    public int TotalCON
    {
        get
        {
            return CON + equipmentCON;
        }
    }

    public int TotalINT
    {
        get
        {
            return INT + equipmentINT;
        }
    }

    public int EquipmentAttackPower
    {
        get
        {
            return equipmentAttackPower;
        }
    }

    public int EquipmentAccuracyBonus
    {
        get
        {
            return equipmentAccuracyBonus;
        }
    }

    public int MaxHealth
    {
        get
        {
            return 50 + TotalCON * 10;
        }
    }

    public int MaxHunger
    {
        get
        {
            return 100 + TotalCON * 5;
        }
    }

    public int MaxMental
    {
        get
        {
            return 50 + TotalINT * 5;
        }
    }

    public int InventoryCapacity
    {
        get
        {
            return 32;
        }
    }

    private const string STR_KEY = "STAT_STR";
    private const string DEX_KEY = "STAT_DEX";
    private const string CON_KEY = "STAT_CON";
    private const string INT_KEY = "STAT_INT";

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadStats();
    }

    public int GetBaseAttackDamage()
    {
        return TotalSTR * 2 +
               weaponDamage +
               equipmentAttackPower;
    }

    public int GetFinalAttackDamage()
    {
        int damage =
            GetBaseAttackDamage();

        if (PlayerResourceManager.Instance != null &&
            PlayerResourceManager.Instance
                .IsMentalDamagePenaltyActive())
        {
            damage =
                Mathf.RoundToInt(
                    damage * 0.5f
                );
        }

        return Mathf.Max(1, damage);
    }

    public int GetFinalAccuracy(
        int enemyEvasion)
    {
        int accuracy =
            100 -
            (
                enemyEvasion -
                TotalDEX * 3
            );

        accuracy +=
            equipmentAccuracyBonus;

        if (PlayerResourceManager.Instance != null)
        {
            accuracy -=
                PlayerResourceManager.Instance
                    .GetMentalAccuracyPenalty();
        }

        return Mathf.Clamp(
            accuracy,
            10,
            95
        );
    }

    public int GetFinalEvasion(
        int enemyAccuracy)
    {
        int evasionChance =
            100 -
            (
                enemyAccuracy -
                TotalDEX * 3
            );

        return Mathf.Clamp(
            evasionChance,
            5,
            95
        );
    }

    public int GetRunSuccessPercent()
    {
        return Mathf.Clamp(
            50 + TotalDEX * 2,
            0,
            95
        );
    }

    public void SetEquipmentBonuses(
        EquipmentStatModifier modifier)
    {
        if (modifier == null)
        {
            equipmentSTR = 0;
            equipmentDEX = 0;
            equipmentCON = 0;
            equipmentINT = 0;

            equipmentAttackPower = 0;
            equipmentAccuracyBonus = 0;
        }
        else
        {
            equipmentSTR =
                modifier.str;

            equipmentDEX =
                modifier.dex;

            equipmentCON =
                modifier.con;

            equipmentINT =
                modifier.intelligence;

            equipmentAttackPower =
                modifier.attackPower;

            equipmentAccuracyBonus =
                modifier.accuracyBonus;
        }

        ApplyStatChange();
    }

    public void ClearEquipmentBonuses()
    {
        SetEquipmentBonuses(
            new EquipmentStatModifier()
        );
    }

    public void AddSTR()
    {
        STR++;

        SaveStats();
        ApplyStatChange();
    }

    public void SubSTR()
    {
        STR = Mathf.Max(0, STR - 1);

        SaveStats();
        ApplyStatChange();
    }

    public void AddDEX()
    {
        DEX++;

        SaveStats();
        ApplyStatChange();
    }

    public void SubDEX()
    {
        DEX = Mathf.Max(0, DEX - 1);

        SaveStats();
        ApplyStatChange();
    }

    public void AddCON()
    {
        CON++;

        SaveStats();
        ApplyStatChange();
    }

    public void SubCON()
    {
        CON = Mathf.Max(0, CON - 1);

        SaveStats();
        ApplyStatChange();
    }

    public void AddINT()
    {
        INT++;

        SaveStats();
        ApplyStatChange();
    }

    public void SubINT()
    {
        INT = Mathf.Max(0, INT - 1);

        SaveStats();
        ApplyStatChange();
    }

    private void ApplyStatChange()
    {
        if (PlayerResourceManager.Instance != null)
        {
            PlayerResourceManager.Instance
                .ApplyMaxResourceFromStats();
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance
                .ApplyCapacityFromStats();
        }
    }

    private void SaveStats()
    {
        PlayerPrefs.SetInt(
            STR_KEY,
            STR
        );

        PlayerPrefs.SetInt(
            DEX_KEY,
            DEX
        );

        PlayerPrefs.SetInt(
            CON_KEY,
            CON
        );

        PlayerPrefs.SetInt(
            INT_KEY,
            INT
        );

        PlayerPrefs.Save();
    }

    private void LoadStats()
    {
        STR =
            PlayerPrefs.GetInt(
                STR_KEY,
                STR
            );

        DEX =
            PlayerPrefs.GetInt(
                DEX_KEY,
                DEX
            );

        CON =
            PlayerPrefs.GetInt(
                CON_KEY,
                CON
            );

        INT =
            PlayerPrefs.GetInt(
                INT_KEY,
                INT
            );
    }
}