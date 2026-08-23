using UnityEngine;

public class Upgrades : Interectable
{
    public GameObject uiUpragde;

    public override void Interact()
    {
        base.Interact();

        print("upgrades");
    }
}
