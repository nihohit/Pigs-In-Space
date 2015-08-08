using Assets.Scripts.Base;
using Assets.Scripts.Base.JsonParsing;
using Assets.Scripts.IntersceneCommunication;
using Assets.Scripts.MapScene;
using Assets.Scripts.UnityBase;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.LogicBase
{
    #region ActionableItem

    // represent an item that can affect squares
    public class ActionableItem : IIdentifiable<string>
    {
        #region private fields

        private static TextureManager s_textureManager;

        private int m_hash;

        #endregion private fields

        #region properties

        // How many squares around the hit square are affected
        public int EffectSize { get; private set; }

        // the item's range
        public float Range { get; private set; }

        // Defines what does the item do
        public virtual EffectTypes Effects { get; private set; }

        // The amount of shots the item shoots
        public int ShotsAmount { get; private set; }

        // how much do the different shots spread
        public float ShotSpread { get; private set; }

        // the minimal power of the item
        public double MinPower { get; private set; }

        // the maximal power of the item
        public double MaxPower { get; private set; }

        // the item's owner
        public virtual Entity Owner { get; private set; }

        // the item's name
        public string Name { get; set; }

        // the name of the shot type
        public string ShotType { get; private set; }

        // the type of the monster this creates
        public string CreatedMonsterType { get; private set; }

        #endregion properties

        #region constructors

        // copy constructor, used in order to create a new item for a new owner
        public ActionableItem(ActionableItem other, Entity owner)
            : this(other.Name, owner, other.Effects, other.MinPower, other.MaxPower,
            other.Range, other.ShotsAmount, other.ShotSpread, other.EffectSize, other.ShotType, other.CreatedMonsterType)
        { }

        // full constructor
        [ChosenConstructorForParsing]
        public ActionableItem(
            string name,
            Entity owner = null,
            EffectTypes effectType = EffectTypes.DamageDealing,
            double minPower = 0,
            double maxPower = 0,
            float range = 0,
            int shotsAmount = 1,
            float shotSpread = 0,
            int effectSize = 0,
            string shotType = "slimeball",
            string createdMonsterType = null)
        {
            Assert.NotNullOrEmpty(name, "Equipment name");
            Assert.EqualOrGreater(maxPower, minPower, "Equipment {0}'s MaxPower is lower than MinPower.".FormatWith(name));
            Assert.EqualOrGreater(range, 0, "Equipment {0}'s range".FormatWith(name));
            Assert.EqualOrGreater(shotsAmount, 0, "Equipment {0}'s shot amount".FormatWith(name));
            Assert.EqualOrGreater(shotSpread, 0, "Equipment {0}'s shot spread".FormatWith(name));
            Assert.EqualOrGreater(effectSize, 0, "Equipment {0}'s effect size".FormatWith(name));

            Range = range;
            Effects = effectType;
            ShotsAmount = shotsAmount;
            ShotSpread = shotSpread;
            MinPower = minPower;
            MaxPower = maxPower;
            Owner = owner;
            EffectSize = effectSize;
            Name = name;
            ShotType = shotType;
            CreatedMonsterType = createdMonsterType;

            m_hash = Hasher.GetHashCode(Range, MinPower, MaxPower, ShotsAmount, ShotSpread, EffectSize, EffectSize, ShotType, CreatedMonsterType);
        }

        public static void Init(TextureManager manager)
        {
            s_textureManager = manager;
        }

        #endregion constructors

        #region public methods

        // affect a given square and wait roughly the amount of given time
        public virtual IEnumerator Effect(SquareScript square, float timeForAction)
        {
            float timePerShot = timeForAction.TimePerAmount(ShotsAmount, 0.03f);

            for (int i = 0; i < ShotsAmount; i++)
            {
                ActOn(square);
                yield return new WaitForSeconds(timePerShot);
                var enumerator = Entity.DestroyKilledEntities();
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        #region objects overrides

        public override bool Equals(object obj)
        {
            var item = obj as ActionableItem;
            return item != null &&
                Name.Equals(item.Name) &&
                Range == item.Range &&
                MinPower == item.MinPower &&
                MaxPower == item.MaxPower &&
                ShotsAmount == item.ShotsAmount &&
                ShotSpread == item.ShotSpread &&
                Effects == item.Effects &&
                Owner.Equals(item.Owner) &&
                EffectSize == item.EffectSize &&
                ShotType.Equals(item.ShotType);
        }

        public override string ToString()
        {
            return "Item {0} - Range: {1} MinPower: {2} MaxPower: {3} ShotsAmount: {4} ShotSpread {5} Effects {6} EffectSize {7} ShotType {8}".FormatWith(
                Name, Range, MinPower, MaxPower, ShotsAmount, ShotSpread, Effects, EffectSize, ShotType);
        }

        public override int GetHashCode()
        {
            return m_hash;
        }

        #endregion objects overrides

        #endregion public methods

        #region private methods

        // Mine minerals from blocked squares
        private void MineAction(SquareScript square)
        {
            if (square.GetComponent<SpriteRenderer>().sprite == SpriteManager.Rock_Crystal)
            {
                square.GetComponent<SpriteRenderer>().sprite = SpriteManager.Empty;
                square.TerrainType = TerrainType.Empty;
            }
        }

        // check all effect flags and operate the relevant effects on the square.
        private void ActOn(SquareScript square)
        {
            // Entity creation is seperate, because currently it is the only effect which doesn't create a shot.
            if (Effects.HasFlag(EffectTypes.CreateEntity))
            {
                CreateEnemy();
                return;
            }

            var hitSquares = GetHitSquares(square);

            foreach (var hitSquare in hitSquares)
            {
                var hitEntity = hitSquare.OccupyingEntity;

                // push
                if (Effects.HasFlag(EffectTypes.Push) && (hitEntity != null))
                {
                    Push(hitEntity, square.transform.position - Owner.Location.transform.position);
                }

                // heal
                if (Effects.HasFlag(EffectTypes.Heal) && (hitEntity != null))
                {
                    Owner.Damage(-PowerRoll());
                }

                // mine
                if (Effects.HasFlag(EffectTypes.RockBreaking) &&
                    hitSquare.GetComponent<SpriteRenderer>().sprite == SpriteManager.Rock_Crystal)
                {
                    MineAction(hitSquare);
                }

                // damage entities
                if (Effects.HasFlag(EffectTypes.DamageDealing) && (hitEntity != null))
                {
                    hitEntity.Damage(PowerRoll());
                }

                // create acid squares
                if (Effects.HasFlag(EffectTypes.CreateAcid))
                {
                    MapSceneScript.AddGroundEffect(GroundEffect.StandardAcid, hitSquare);
                }
            }
        }

        private void Push(Entity hitEntity, Vector3 direction)
        {
            var differenceVector = Owner.Location.transform.position - hitEntity.Location.transform.position;
            var remainingPower = (float)PowerRoll() - differenceVector.Distance() / 2;
            Push(hitEntity, remainingPower, direction);
        }

        private bool Push(Entity hitEntity, float power, Vector3 direction)
        {
            // if there's not enough power to push, don't do anything
            if (power < 1)
            {
                return false;
            }

            var layerMask = 1 << LayerMask.NameToLayer("Ground");

            // return all colliders thast the ray passes through
            var rayHits = Physics2D.RaycastAll((Vector2)hitEntity.Location.transform.position, (Vector2)direction, power, layerMask);
            var moved = false;
            var first = true;

            foreach (var hitSquare in rayHits.Select(hit => hit.collider.gameObject.GetComponent<SquareScript>()))
            {
                // check that this isn't the origin square
                if (first)
                {
                    first = false;
                    continue;
                }

                // if hit wall
                if (hitSquare.TraversingCondition == Traversability.Blocking)
                {
                    //Debug.Log("{0} pushed on wall, taking {1} damage.".FormatWith(hitEntity.Name, power));
                    hitEntity.Damage(power);
                    return moved || hitEntity.Destroyed();
                }

                // if pushed onto other entity, push it. if it moved or was destroyed, take its place.
                if (hitSquare.OccupyingEntity != null)
                {
                    //Debug.Log("{0} pushed on {1}, taking {2} damage.".FormatWith(hitEntity.Name, hitSquare.OccupyingEntity.Name, power));
                    if (Push(hitSquare.OccupyingEntity, power - 1, direction))
                    {
                        hitEntity.Location = hitSquare;
                    }
                    hitEntity.Damage(power);
                    return moved || hitEntity.Destroyed();
                }

                moved = true;
                hitEntity.Location = hitSquare;
                power--;
            }

            return moved;
        }

        private IEnumerable<SquareScript> GetHitSquares(SquareScript square)
        {
            if (Range == 0)
            {
                return new[] { Owner.Location };
            }
            return FindHitSquares(square).MultiplyBySize(EffectSize);
        }

        private IEnumerable<SquareScript> FindHitSquares(SquareScript target)
        {
            var shot = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("shot"), Owner.Location.transform.position, Quaternion.identity));
            var ShotScript = shot.GetComponent<ShotScript>();
            s_textureManager.ReplaceTexture(ShotScript, ShotType);
            ShotScript.Init(target, Owner.Location, Range, Effects.HasFlag(EffectTypes.Piercing), EffectSize, ShotSpread);
            return ShotScript.HitSquares;
        }

        #region Creating units

        private void CreateEnemy()
        {
            var emptyNeighbours = Owner.Location.GetNeighbours().
                Where(square => square.TraversingCondition == Traversability.Walkable &&
                    square.OccupyingEntity == null);

            if (emptyNeighbours.Any() &&
                Randomiser.ProbabilityCheck(MapSceneScript.EscapeMode ? MaxPower : MinPower))
            {
                EnemiesManager.CreateEnemy(
                    GlobalState.Instance.Configurations.Monsters.GetConfiguration(CreatedMonsterType),
                    emptyNeighbours.ChooseRandomValue());
            }
        }

        #endregion Creating units

        private double PowerRoll()
        {
            return Randomiser.NextDouble(MinPower, MaxPower);
        }

        #endregion private methods
    }

    #endregion ActionableItem

    #region PlayerEquipment

    // represents an equipment item for the player's usage.
    public class PlayerEquipment : ActionableItem
    {
        private int m_hash;

        #region properties

        public Loot Cost { get; private set; }

        // the names of the possible upgrades to each equipment piece.
        // That actual equipment configuration can be pulled from the configuration storage using these names
        public IEnumerable<string> PossibleUpgrades { get; private set; }

        public double EnergyCost { get; private set; }

        public override Entity Owner
        {
            get
            {
                return Entity.Player;
            }
        }

        public override IEnumerator Effect(SquareScript square, float timePerAction)
        {
            // if there's enough energy, act
            if (Entity.Player.Energy >= EnergyCost)
            {
                return base.Effect(square, timePerAction).Join(Entity.Player.EndTurn(EnergyCost));
            }

            // do nothing
            return this.WaitAndEndTurn(0.1f, 0);
        }

        #endregion properties

        #region constructors

        public PlayerEquipment(
            string name,
            double minPower,
            double maxPower,
            double energyCost,
            EffectTypes effectType = EffectTypes.DamageDealing,
            float range = 0,
            int shotsAmount = 1,
            float shotSpread = 0,
            int effectSize = 0,
            string shotType = "laser",
            string createdMonsterType = null,
            Loot cost = null,
            IEnumerable<string> upgrades = null) :
            base(name, Entity.Player, effectType, minPower, maxPower, range, shotsAmount, shotSpread, effectSize, shotType, createdMonsterType)
        {
            Cost = cost;
            EnergyCost = energyCost;
            PossibleUpgrades = upgrades;
            Name = name;
            m_hash = Hasher.GetHashCode(base.GetHashCode(), Name, Cost, EnergyCost);
        }

        #endregion constructors

        #region object overrides

        public override string ToString()
        {
            return "{0} {1} Cost: {2} EnergyCost: {3} ".FormatWith(Name, base.ToString(), Cost, EnergyCost);
        }

        public override bool Equals(object obj)
        {
            var item = obj as PlayerEquipment;
            return item != null &&
                base.Equals(item) &&
                Name.Equals(item.Name) &&
                EnergyCost == item.EnergyCost;
        }

        public override int GetHashCode()
        {
            return m_hash;
        }

        #endregion object overrides
    }

    #endregion PlayerEquipment
}