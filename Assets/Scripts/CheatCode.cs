using System.Collections.Generic;
using UnityEngine;

public class CheatCode : MonoBehaviour
{
    public Weapon[] weapons;

    private void Start()
    {
        weapons = Object.FindObjectsByType<Weapon>(FindObjectsSortMode.None);
    }

    //Afficher les code de triches dans un button
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Keypad0))
        {
            foreach (Weapon item in weapons)
            {
                item.currentTier = 0;
            }
        }

        if (Input.GetKeyUp(KeyCode.Keypad1))
        {
            foreach (Weapon item in weapons)
            {
                item.currentTier = 1;
            }
        }

        if (Input.GetKeyUp(KeyCode.Keypad2))
        {
            foreach (Weapon item in weapons)
            {
                item.currentTier = 2;
            }
        }

        if (Input.GetKeyUp(KeyCode.Keypad3))
        {
            foreach (Weapon item in weapons)
            {
                item.currentTier = 3;
            }
        }

        if (Input.GetKeyUp(KeyCode.Keypad4))
        {
            foreach (Weapon item in weapons)
            {
                item.currentTier = 4;
            }
        }
    }
}
