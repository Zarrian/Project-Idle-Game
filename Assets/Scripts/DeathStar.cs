using System.Collections.Generic;
using UnityEngine;

public class DeathStar : MonoBehaviour
{
    public static DeathStar instance;
    public List<List<GameObject>> listWeapons;

    private void Awake()
    {
        instance = this;
    }
}
