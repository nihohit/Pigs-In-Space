namespace Assets.Scripts.LogicBase
{
    public class Loot
    {
        public int BlueCrystal { get; set; }

        public bool FuelCell { get; set; }

        public Loot()
        {
        }

        public Loot(int blueCrystal, bool fuelCell)
        {
            BlueCrystal = blueCrystal;
            FuelCell = fuelCell;
        }

        public void AddLoot(Loot loot)
        {
            BlueCrystal += loot.BlueCrystal;
        }

        // represents what happens when pressing a "buy" button
        public bool RemoveIfEnough(Loot cost)
        {
            if (IsEnoughToCover(cost))
            {
                BlueCrystal -= cost.BlueCrystal;
                return true;
            }
            return false;
        }

        public bool IsEnoughToCover(Loot cost)
        {
            return BlueCrystal > cost.BlueCrystal;
        }
    }
}