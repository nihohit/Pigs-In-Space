using Assets.Scripts.Base;
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
    public class ActionableItem : IIdentifiable
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
        public Entity Owner { get; private set; }

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
            : this(other.Name, other.Effects, other.MinPower, other.MaxPower,
            other.Range, owner, other.ShotsAmount, other.ShotSpread, other.EffectSize, other.ShotType, other.CreatedMonsterType)
        { }

        // constructor without an owner
        public ActionableItem(string name, EffectTypes type, double minPower, double maxPower,
            float range, int shotsAmount, float shotSpread, int effectSize, string shotType, string createdMonsterType)
            : this(name, type, minPower, maxPower, range, null, shotsAmount, shotSpread, effectSize, shotType, createdMonsterType)
        { }

        // full constructor
        public ActionableItem(string name, EffectTypes type, double minPower, double maxPower,
            float range, Entity owner, int shotsAmount, float shotSpread, int effectSize, string shotType, string createdMonsterType)
        {
            Assert.NotNullOrEmpty(name, "Equipment name");
            Assert.EqualOrGreater(maxPower, minPower, "Equipment {0}'s MaxPower is lower than MinPower.".FormatWith(name));
            Assert.EqualOrGreater(range, 0, "Equipment {0}'s range".FormatWith(name));
            Assert.EqualOrGreater(shotsAmount, 0, "Equipment {0}'s shot amount".FormatWith(name));
            Assert.EqualOrGreater(shotSpread, 0, "Equipment {0}'s shot spread".FormatWith(name));
            Assert.EqualOrGreater(effectSize, 0, "Equipment {0}'s effect size".FormatWith(name));

            Range = range;
            Effects = type;
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
                var mineral = new Loot();
                mineral.BlueCrystal = 5;
                square.AddLoot(mineral);
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
            }
            else
            {
                // send a shot and act on all the hit squares
                foreach (var hitSquare in FindHitSquares(square).MultiplyBySize(EffectSize))
                {
                    // mine
                    if (Effects.HasFlag(EffectTypes.RockBreaking) &&
                        hitSquare.GetComponent<SpriteRenderer>().sprite == SpriteManager.Rock_Crystal)
                    {
                        MineAction(hitSquare);
                    }

                    // damage entities
                    if (Effects.HasFlag(EffectTypes.DamageDealing) && (hitSquare.OccupyingEntity != null))
                    {
                        hitSquare.OccupyingEntity.Damage(Randomiser.NextDouble(MinPower, MaxPower));
                    }

                    // create acid squares
                    if (Effects.HasFlag(EffectTypes.CreateAcid))
                    {
                        MapSceneScript.AddGroundEffect(GroundEffect.StandardAcid, hitSquare);
                    }
                }
            }
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

        private SquareScript ChooseRandomFreeSquare()
        {
            return Owner.Location.GetNeighbours().Where(square => square.TraversingCondition == Traversability.Walkable && square.OccupyingEntity == null).ChooseRandomValue();
        }

        private void CreateEnemy()
        {
            if (Randomiser.ProbabilityCheck(MapSceneScript.EscapeMode ? MaxPower : MinPower))
            {
                EnemiesManager.CreateEnemy(MonsterTemplateStorage.Instance.GetConfiguration(CreatedMonsterType), ChooseRandomFreeSquare());
            }
        }

        #endregion Creating units

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

        public PlayerEquipment(string name, EffectTypes type, double minPower, double maxPower, float range, int shotsAmount,
            float shotSpread, int effectSize, string shotType, string createdMonsterType, double energyCost, Loot cost,
            IEnumerable<string> upgrades) :
            base(name, type, minPower, maxPower, range, Entity.Player, shotsAmount, shotSpread, effectSize, shotType, createdMonsterType)
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