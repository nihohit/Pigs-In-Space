using System.Collections;
using System.Collections.Generic;
using Assets.scripts.Base;
using UnityEngine;

namespace Assets.Scripts.LogicBase
{
    public delegate void SquareEffect(SquareScript square);

    public delegate IEnumerator TimedSquareEffect(SquareScript square);

    public enum EffectType { SingleShot, SprayShot, Mine }

    #region ActionableItem

    // represent an item that can affect squares
    public class ActionableItem
    {
        //TODO - read from a configuration
        private int m_amountOfShots = 3;

        // TODO - do we want to add cooldown timer, so that not all equipment piece could be operated everyturn?

        #region properties

        public float Range { get; private set; }

        public virtual TimedSquareEffect Effect { get; private set; }

        #endregion properties

        #region constructor

        public ActionableItem(EffectType type, double minStrength, double maxStrength, float range, Entity owner)
        {
            Range = range;
            SetEffect(type, minStrength, maxStrength, owner);
        }

        public ActionableItem(EffectType type, double minStrength, double maxStrength, float range)
            : this(type, minStrength, maxStrength, range, null)
        {
        }

        #endregion constructor

        #region private methods

        private void SetEffect(EffectType type, double minStrength, double maxStrength, Entity owner)
        {
            switch (type)
            {
                case EffectType.SingleShot:
                    Effect = (square) => SingleShotAction(square, minStrength, maxStrength);
                    break;

                case EffectType.SprayShot:
                    Effect = (square) => SprayShotAction(square, minStrength, maxStrength);
                    break;

                case EffectType.Mine:
                    Effect = (square) => MineAction(square, minStrength, maxStrength);
                    break;

                default:
                    throw new UnknownValueException(type);
            }
        }

        private IEnumerator MineAction(SquareScript square, double minStrength, double maxStrength)
        {
            var hitSquare = FindHitSquare(square);
            if (hitSquare.GetComponent<SpriteRenderer>().sprite == SpriteManager.Rock_Crystal)
            {
                hitSquare.GetComponent<SpriteRenderer>().sprite = SpriteManager.Empty;
                var mineral = new Loot();
                mineral.BlueCrystal = 5;
                hitSquare.AddLoot(mineral);
                hitSquare.TerrainType = TerrainType.Empty;
            }
            else
            {
                if (hitSquare.OccupyingEntity != null)
                {
                    hitSquare.OccupyingEntity.Damage(Randomiser.NextDouble(minStrength, maxStrength));
                }
            }
            yield return new WaitForSeconds(0.2f);
        }

        private IEnumerator SprayShotAction(SquareScript square, double minStrength, double maxStrength)
        {
            // shoot random shots at the square and its surroundings
            var squares = new List<SquareScript>();
            squares.Add(square);
            squares.AddRange(square.GetNeighbours(true));

            for (int i = 0; i < m_amountOfShots; i++)
            {
                SingleShot(squares.ChooseRandomValue(), minStrength, maxStrength);
                yield return new WaitForSeconds(0.05f);
            }
        }

        private IEnumerator SingleShotAction(SquareScript square, double minStrength, double maxStrength)
        {
            SingleShot(square, minStrength, maxStrength);
            yield return new WaitForSeconds(0.2f);
        }

        private void SingleShot(SquareScript square, double minStrength, double maxStrength)
        {
            var hitSquare = FindHitSquare(square);
            if (hitSquare.OccupyingEntity != null) hitSquare.OccupyingEntity.Damage(Randomiser.NextDouble(minStrength, maxStrength));
        }

        private SquareScript FindHitSquare(SquareScript target)
        {
            var laser = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("laser"), Entity.Player.Location.transform.position, Quaternion.identity));
            var ShotScript = laser.GetComponent<ShotScript>();
            ShotScript.Init(target, Entity.Player.Location, "Laser shot", Range);
            return ShotScript.HitSquare;
        }

        #endregion private methods
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

        public override TimedSquareEffect Effect
        {
            get
            {
                if (Entity.Player.Energy >= EnergyCost)
                {
                    Entity.Player.EndTurn(EnergyCost);
                    return base.Effect;
                }

                Entity.Player.EndTurn(0);
                // do nothing
                return (square) =>
                {
                    var array = new object[1];
                    array[0] = new WaitForSeconds(0.1f);
                    return array.GetEnumerator();
                };
            }
        }

        #endregion properties

        #region constructor

        public EquipmentPiece(EffectType type, double minStrength, double maxStrength, float range, string name,
            double energyCost, Loot cost, IEnumerable<EquipmentPiece> upgrades) :
            base(type, minStrength, maxStrength, range, Entity.Player)
        {
            Cost = cost;
            EnergyCost = energyCost;
            PossibleUpgrades = upgrades;
            Name = name;
        }

        public EquipmentPiece(EffectType type, double minStrength, double maxStrength, float range, string name, double energyCost) :
            this(type, minStrength, maxStrength, range, name, energyCost, null, null) { }

        #endregion constructor
    }

    #endregion EquipmentPiece

    //TODO - remove and replace with configuration files

    #region Equipment examples

    public class LaserPistol : EquipmentPiece
    {
        public LaserPistol()
            : base(EffectType.SingleShot, 2, 5, 5, "pistol", 1)
        {
        }
    }

    public class LaserRifle : EquipmentPiece
    {
        public LaserRifle()
            : base(EffectType.SingleShot, 3, 5, 10, "rifle", 2)
        {
        }
    }

    public class LaserMachinegun : EquipmentPiece
    {
        public LaserMachinegun()
            : base(EffectType.SprayShot, 3, 5, 7, "machinegun", 3)
        {
        }
    }

    public class Digger : EquipmentPiece
    {
        public Digger()
            : base(EffectType.Mine, 4, 8, 0.5f, "digger", 1)
        {
        }
    }

    #endregion Equipment examples
}