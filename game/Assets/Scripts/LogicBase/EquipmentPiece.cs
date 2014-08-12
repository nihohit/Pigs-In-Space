using System.Collections.Generic;
using Assets.scripts.Base;

namespace Assets.Scripts.LogicBase
{
    public delegate void SquareEffect(SquareScript square);
    public enum EffectType { SingleShot, SprayShot, Mine }

    #region ActionableItem

    public class ActionableItem
    {
        // TODO - do we want to add cooldown timer, so that not all equipment piece could be operated everyturn?
        // TODO - implement range
        // TODO - implement different shots
        #region properties

        public double Range { get; private set; }
        public SquareEffect Effect { get; private set; }

        #endregion properties

        #region constructor

        public ActionableItem(EffectType type, double minStrength, double maxStrength, double range, Entity owner)
        {
            Range = range;
            Effect = GetEffect(type, minStrength, maxStrength, owner);
        }

        public ActionableItem(EffectType type, double minStrength, double maxStrength, double range) : this(type, minStrength, maxStrength, range, null) { }

        #endregion

        #region private methods

        private SquareEffect GetEffect(EffectType type, double minStrength, double maxStrength, Entity owner)
        {
            switch(type)
            {
                case EffectType.SingleShot:
                    return SingleShot(minStrength, maxStrength);

                case EffectType.SprayShot:
                    return SprayShot(minStrength, maxStrength);

                case EffectType.Mine:
                    return Mine(minStrength, maxStrength);

                default:
                    throw new UnknownValueException(type);
            }
        }

        private SquareEffect Mine(double minStrength, double maxStrength)
        {
            /* TODO.
            if (SquareScript.s_markedSquare.GetNeighbours().Contains(this.Location) &&
            SquareScript.s_markedSquare.GetComponent<SpriteRenderer>().sprite == SpriteManager.Rock_Crystal)
            {
                Energy -= 1;
                SquareScript.s_markedSquare.GetComponent<SpriteRenderer>().sprite = SpriteManager.Empty;
                var mineral = new Loot();
                mineral.BlueCrystal = 5;
                SquareScript.s_markedSquare.AddLoot(mineral);
                SquareScript.s_markedSquare.TerrainType = TerrainType.Empty;
                EndTurn();
            }
             * */
        }

        private SquareEffect SprayShot(double minStrength, double maxStrength)
        {
            //TODO - add chance to miss
            var singleShot = SingleShot(minStrength, maxStrength);
            return (SquareScript square) => 
            {
                singleShot(square);
                square.GetNeighbours(true).ForEach(neighbour => singleShot(neighbour));
            };
        }

        private SquareEffect SingleShot(double minStrength, double maxStrength)
        {
            return (SquareScript square) => 
            { 
                if (square.OccupyingEntity != null) square.OccupyingEntity.Damage(Randomiser.NextDouble(minStrength, maxStrength)); 
            };
        }

        #endregion
    }

    #endregion ActionableItem

    #region EquipmentPiece

    public class EquipmentPiece : ActionableItem
    {
        #region properties

        public Loot Cost { get; private set; }

        public IEnumerable<EquipmentPiece> PossibleUpgrades { get; private set; }

        public double EnergyCost { get; private set; }

        public string Name { get; set; }

        #endregion properties

        #region constructor

        public EquipmentPiece(EffectType type, double minStrength, double maxStrength, double range, string name,
            double energyCost, Loot cost, IEnumerable<EquipmentPiece> upgrades) :
            base(type, minStrength, maxStrength, range, Entity.Player)
        {
            Cost = cost;
            EnergyCost = energyCost;
            PossibleUpgrades = upgrades;
            Name = name;
        }

        public EquipmentPiece(EffectType type, double minStrength, double maxStrength, double range, string name, double energyCost) :
            this(type, minStrength, maxStrength, range, name, energyCost, null, null) { }

        #endregion
    }

    #endregion EquipmentPiece

    //TODO - remove and replace with configuration files
    #region Equipment examples

    public class LaserPistol : EquipmentPiece
    {
        public LaserPistol() : base(EffectType.SingleShot, 2, 5, 5, "pistol", 1) { }
    }

    public class LaserRifle : EquipmentPiece
    {
        public LaserRifle() : base(EffectType.SingleShot, 3, 5, 10, "rifle", 2) { }
    }

    public class LaserMachinegun : EquipmentPiece
    {
        public LaserMachinegun() : base(EffectType.SprayShot, 3, 5, 7, "machinegun", 3) { }
    }

    public class Digger : EquipmentPiece
    {
        public Digger() : base(EffectType.Mine, 4, 8, 1, "digger", 1) { }
    }

    #endregion
}