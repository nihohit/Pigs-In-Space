using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Base;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.LogicBase
{
    public delegate void SquareEffect(SquareScript square);

    public delegate IEnumerator TimedSquareEffect(SquareScript square);

    public enum EffectType { Shot = 1, Mine = 2 }

    #region ActionableItem

    // represent an item that can affect squares
    public class ActionableItem
    {
        #region properties

        public float Range { get; private set; }

        public virtual TimedSquareEffect Effect { get; private set; }

        public int ShotsAmount { get; private set; }

        public int ShotSpread { get; private set; }

        #endregion properties

        #region constructors

        public ActionableItem(EffectType type, double minPower, double maxPower, float range, Entity owner, int shotsAmount, int shotSpread)
        {
            Range = range;
            SetEffect(type, minPower, maxPower, owner);
            ShotsAmount = shotsAmount;
            ShotSpread = shotSpread;
        }

        public ActionableItem(EffectType type, double minPower, double maxPower, float range, Entity owner)
            : this(type, minPower, maxPower, range, owner, 1, 0)
        {
        }

        public ActionableItem(EffectType type, double minPower, double maxPower, float range, int shotsAmount, int shotSpread)
            : this(type, minPower, maxPower, range, null, shotsAmount, shotSpread)
        {
        }

        public ActionableItem(EffectType type, double minPower, double maxPower, float range)
            : this(type, minPower, maxPower, range, null, 1, 0)
        {
        }

        #endregion constructor

        public override bool Equals(object obj)
        {
            var item = obj as ActionableItem;
            return item != null &&
                Range == item.Range &&
                ShotsAmount == item.ShotsAmount &&
                ShotSpread == item.ShotSpread;
        }

        #region private methods

        private void SetEffect(EffectType type, double minPower, double maxPower, Entity owner)
        {
            switch (type)
            {
                case EffectType.Shot:
                    Effect = (square) => ShotAction(square, minPower, maxPower);
                    break;

                case EffectType.Mine:
                    Effect = (square) => MineAction(square, minPower, maxPower);
                    break;

                default:
                    throw new UnknownValueException(type);
            }
        }

        private IEnumerator MineAction(SquareScript square, double minPower, double maxPower)
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
                    hitSquare.OccupyingEntity.Damage(Randomiser.NextDouble(minPower, maxPower));
                }
            }
            yield return new WaitForSeconds(0.2f);
        }

        private IEnumerator ShotAction(SquareScript square, double minPower, double maxPower)
        {
            // shoot random shots at the square and its surroundings
            var squares = new List<SquareScript>();
            squares.Add(square);
            for (int i = 0; i < ShotSpread; i++ )
            {
                var tempSquares = squares.Select(target => target).Materialize();
                foreach (var potentialTarget in tempSquares)
                {
                    squares.AddRange(potentialTarget.GetNeighbours(true));
                }
            }

            float timePerShot = (1.8f / (ShotsAmount * 10));

            for (int i = 0; i < ShotsAmount; i++)
            {
                SingleShot(squares.ChooseRandomValue(), minPower, maxPower);
                yield return new WaitForSeconds(timePerShot);
            }
        }

        private void SingleShot(SquareScript square, double minPower, double maxPower)
        {
            var hitSquare = FindHitSquare(square);
            if (hitSquare.OccupyingEntity != null) hitSquare.OccupyingEntity.Damage(Randomiser.NextDouble(minPower, maxPower));
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

        #region constructors

        public EquipmentPiece(EffectType type, double minPower, double maxPower, float range, int shotsAmount, int shotSpread, string name, 
            double energyCost, Loot cost, IEnumerable<EquipmentPiece> upgrades) :
            base(type, minPower, maxPower, range, Entity.Player, shotsAmount, shotSpread)
        {
            Cost = cost;
            EnergyCost = energyCost;
            PossibleUpgrades = upgrades;
            Name = name;
        }

        #endregion constructor

        public override bool Equals(object obj)
        {
            var item = obj as EquipmentPiece;
            return item != null &&
                base.Equals(item) &&
                Name.Equals(item.Name) &&
                EnergyCost == item.EnergyCost;
        }
    }

    #endregion EquipmentPiece
}