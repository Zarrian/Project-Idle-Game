using UnityEngine;

public class PlanetStats : MonoBehaviour
{
    public PlanetSO planetSO;

    public int hp;
    public int hpRegen;

    private void Awake()
    {
        hp = planetSO.HP;
        hpRegen = planetSO.HpRegen;
    }
}
