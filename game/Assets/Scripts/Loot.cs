public class Loot
{
    public int BlueCrystal { get; set; }

    public Loot() { }

    public Loot(int blueCrystal)
    {
        BlueCrystal = blueCrystal;
    }

    public void AddLoot(Loot loot)
    {
        BlueCrystal += loot.BlueCrystal;
    }
}