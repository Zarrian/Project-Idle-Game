public class Upgrades : Interectable
{
    public HangarShip hangar;
    public PanelUpgrade panelUpgrade;

    public override void Interact()
    {
        base.Interact();
        panelUpgrade.InteractPanel();
    }

    private void Start()
    {
        GeneratePanels();
    }

    public virtual void GeneratePanels()
    {

    }


}
