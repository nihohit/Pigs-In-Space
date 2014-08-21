using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Base;
using UnityEngine;
using System.Linq;
using System;

namespace Assets.Scripts.LogicBase
{
    public delegate void SquareEffect(SquareScript square);

    public delegate IEnumerator TimedSquareEffect(SquareScript square);

    [Flags]
    public enum SpecialEffects 
    { 
        None = 0, 
        Mine = 1, 
        Piercing = 2, 
        Exploding = 4 
    }

    #region ActionableItem

    // represent an item that can affect squares
    public class ActionableItem
    {

        #region properties

        public float Range { get; private set; }

        public virtual SpecialEffects Effects { get; private set; }

        public int ShotsAmount { get; private set; }

        public int ShotSpread { get; private set; }

        public double MinPower { get; private set; }

        public double MaxPower { get; private set; }

        public Entity Owner { get; private set; }

        #endregion properties

        #region constructors

        public ActionableItem(SpecialEffects type, double minPower, double maxPower, float range, Entity owner, int shotsAmount, int shotSpread)
        {
            Range = range;
            Effects = type;
            ShotsAmount = shotsAmount;
            ShotSpread = shotSpread;
            MinPower = minPower;
            MaxPower = maxPower;
            Owner = owner;
        }

        #endregion constructor

        public override bool Equals(object obj)
        {
            var item = obj as ActionableItem;
            return item != null &&
                Range == item.Range &&
                MinPower == item.MinPower && 
                MaxPower == item.MaxPower &&
                ShotsAmount == item.ShotsAmount &&
                ShotSpread == item.ShotSpread &&
                Effects == item.Effects &&
                Owner.Equals(item.Owner);
        }

        public virtual IEnumerator Effect(SquareScript square)
        {
            // shoot random shots at the square and its surroundings
            var squares = new List<SquareScript>();
            squares.Add(square);
            for (int i = 0; i < ShotSpread; i++)
            {
                var tempSquares = squares.Select(target => target).Materialize();
                foreach (var potentialTarget in tempSquares)
                {
                    squares.AddRange(potentialTarget.GetNeighbours(true));
                }
            }

            float timePerShot = ((0.2f + (ShotsAmount / 10)) / (ShotsAmount));

            for (int i = 0; i < ShotsAmount; i++)
            {
                ActOn(squares.ChooseRandomValue());
                yield return new WaitForSeconds(timePerShot);
            }
        }

        #region private methods

        private void MineAction(SquareScript square)
        {
            if (square.GetComponent<SpriteRenderer>().sprite == SpriteManager.Rock_Crystal)
            {
                square.GetComponent<SpriteRenderer>().sprite = SpriteManager.Empty;
                var mineral = new Loot();
                mineral.BlueCrystal = 5;
                square.AddLoot(mineral);
                square.TerrainType = TerrainType.Empty;
            }
        }


        private void ActOn(SquareScript square)
        {
            foreach (var hitSquare in FindHitSquares(square))
            {
                if (Effects.HasFlag(SpecialEffects.Mine) &&
                    hitSquare.GetComponent<SpriteRenderer>().sprite == SpriteManager.Rock_Crystal)
                {
                    MineAction(hitSquare);
                }
                else
                {
                    if (hitSquare.OccupyingEntity != null) hitSquare.OccupyingEntity.Damage(Randomiser.NextDouble(MinPower, MaxPower));
                }
            }
        }
            

        private IEnumerable<SquareScript> FindHitSquares(SquareScript target)
        {
            var laser = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("laser"), Entity.Player.Location.transform.position, Quaternion.identity));
            var ShotScript = laser.GetComponent<ShotScript>();
            ShotScript.Init(target, Entity.Player.Location, "Laser shot", Range, Effects.HasFlag(SpecialEffects.Piercing));
            return ShotScript.HitSquares;
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

        public override IEnumerator Effect(SquareScript square)
        {
            if (Entity.Player.Energy >= EnergyCost)
            {
                Entity.Player.EndTurn(EnergyCost);
                return base.Effect(square);
            }

            Entity.Player.EndTurn(0);
            // do nothing
            var array = new object[1];
            array[0] = new WaitForSeconds(0.1f);
            return array.GetEnumerator();
        }

        #endregion properties

        #region constructors

        public EquipmentPiece(SpecialEffects type, double minPower, double maxPower, float range, int shotsAmount, int shotSpread, string name, 
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