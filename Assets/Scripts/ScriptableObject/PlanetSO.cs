using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlanetSO", order = 1)]
public class PlanetSO : ScriptableObject
{
    public string PlanetName;
    public int HP;
    public int HpRegen;
}
