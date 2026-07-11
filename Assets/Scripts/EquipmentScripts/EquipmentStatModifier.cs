using System;

[Serializable]
public class EquipmentStatModifier
{
    public int str;
    public int dex;
    public int con;
    public int intelligence;

    public int attackPower;
    public int accuracyBonus;

    public EquipmentStatModifier()
    {
        Clear();
    }

    public EquipmentStatModifier(EquipmentStatModifier source)
    {
        if (source == null)
        {
            Clear();
            return;
        }

        str = source.str;
        dex = source.dex;
        con = source.con;
        intelligence = source.intelligence;
        attackPower = source.attackPower;
        accuracyBonus = source.accuracyBonus;
    }

    public void Clear()
    {
        str = 0;
        dex = 0;
        con = 0;
        intelligence = 0;
        attackPower = 0;
        accuracyBonus = 0;
    }

    public void Add(EquipmentStatModifier other)
    {
        if (other == null)
            return;

        str += other.str;
        dex += other.dex;
        con += other.con;
        intelligence += other.intelligence;
        attackPower += other.attackPower;
        accuracyBonus += other.accuracyBonus;
    }
}