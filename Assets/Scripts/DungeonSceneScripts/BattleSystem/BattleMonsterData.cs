using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Battle/Monster Data")]
public class BattleMonsterData : ScriptableObject
{
    public string monsterName = "Dog Monster";

    public GameObject monsterPrefab;

    public int maxHP = 20;
    public int attackPower = 10;
    public int accuracy = 70;
    public int evasion = 30;
}