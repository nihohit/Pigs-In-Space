public class Loot
{
    public int BlueCrystal { get; set; }

    public void AddLoot(Loot loot)
    {
        BlueCrystal += loot.BlueCrystal;
    }
}