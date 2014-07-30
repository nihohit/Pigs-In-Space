public class Loot
{
    public int BlueCrystal { get; set; }

    public bool fuelCell { get; set; }

    public void AddLoot(Loot loot)
    {
        BlueCrystal += loot.BlueCrystal;
    }
}