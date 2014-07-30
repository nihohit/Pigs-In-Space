public class Loot
{
    public int BlueCrystal { get; set; }

    public bool FuelCell { get; set; }
    public Loot() { }

    public Loot(int blueCrystal, bool fuelCell)
    {
        BlueCrystal = blueCrystal;
        FuelCell = fuelCell;
    }

    public void AddLoot(Loot loot)
    {
        BlueCrystal += loot.BlueCrystal;
    }
}