using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.MapScene
{
    using Assets.Scripts.IntersceneCommunication;

    #region Entity

    public abstract class Entity
    {
        #region fields

        private static long s_entityId = 0;
        private static readonly List<Entity> s_killedEntities = new List<Entity>();
        private const int c_startHealth = 15;
        private const int c_startOxygen = 200;
        private const int c_startEnergy = 20;

        private readonly long m_id;

        private readonly MovementType m_movementType;

        protected bool m_alwaysActive;

        private bool m_active;

        private SquareScript m_location;

        #endregion fields

        #region properties

        public static PlayerEntity Player { get; set; }

        public double Health { get; private set; }

        public SquareScript Location
        {
            get
            {
                return m_location;
            }
            set
            {
                //Debug.Log("{0} moved to {1}".FormatWith(Name, value));
                if (m_location != null)
                {
                    m_location.OccupyingEntity = null;
                    var movementSpeed = 10 * m_location.transform.position.Distance(value.transform.position);
                    Image.BeginMove(new[] { new MoveOrder(value.transform.position) }, movementSpeed, false);
                }

                m_location = value;
                m_location.OccupyingEntity = this;
            }
        }

        public IUnityMarker Image { get; protected set; }

        public string TypeOfEntity { get; private set; }

        public string Name { get; private set; }

        public bool Active
        {
            get { return m_active; }
            set { m_active = m_alwaysActive || value; }
        }

        #endregion properties

        #region constructor

        public Entity(EntityTemplate template, SquareScript location)
        {
            m_id = ++s_entityId;
            Name = "{0}{1}".FormatWith(template.Name, m_id);
            TypeOfEntity = template.Name;
            m_movementType = template.MovementType;
            this.Health = template.Health;
            this.Image = ((GameObject)MonoBehaviour.Instantiate(Resources.Load(template.Name),
                                                            location.transform.position,
                                                            Quaternion.identity)).GetComponent<MarkerScript>();
            this.Location = location;
            Image.Mark(location.transform.position);
        }

        #endregion constructor

        #region public methods

        public virtual void Damage(double damage)
        {
            Health -= damage;
            Health = Math.Max(Health, 0);
            Health = Math.Min(Health, GlobalState.Instance.Player.Health);
            if (Destroyed())
            {
                s_killedEntities.Add(this);
            }
        }

        public bool Destroyed()
        {
            return Health <= 0;
        }

        public static IEnumerator DestroyKilledEntities()
        {
            float timeToWaitPerEntity = s_killedEntities.TimePerItem(0.1f, 0.01f);
            IEnumerator enumerator = new EmptyEnumerator();
            foreach (var ent in s_killedEntities)
            {
                enumerator = enumerator.Join(ent.Destroy(timeToWaitPerEntity));
            }
            s_killedEntities.Clear();
            return enumerator;
        }

        public bool TryMoveTo(SquareScript newLocation)
        {
            if (m_movementType == MovementType.NonMoving) return false;
            if (!CanEnter(newLocation))
            {
                return false;
            }

            // update all relevant properties
            Location = newLocation;
            return true;
        }

        // if the entity is standing on some ground effect, affect it.
        public virtual void ApplyGroundEffects()
        {
            Assert.NotEqual(Location.GroundEffect.EffectType, GroundEffectType.None);
            switch (Location.GroundEffect.EffectType)
            {
                case (GroundEffectType.Acid):
                    Damage(Location.GroundEffect.Power);
                    break;

                default:
                    throw new UnknownValueException(Location.GroundEffect.EffectType);
            }
        }

        #region static generation methods

        //TODO - should be moved somewhere more appropriate
        public static void CreatePlayerEntity(int x, int y)
        {
            var square = SquareScript.GetSquare(x, y);
            Entity.Player = new PlayerEntity(
                GlobalState.Instance.Player.Health,
                GlobalState.Instance.Player.Energy,
                GlobalState.Instance.Player.Oxygen, square);
            Entity.Player.SetEquipment();
        }

        #endregion static generation methods

        public override string ToString()
        {
            return Name;
        }

        #endregion public methods

        #region private and protected methods

        protected virtual IEnumerator Destroy(float timeToWait)
        {
            //Debug.Log("Destroy {0}".FormatWith(Name));
            Assert.EqualOrLesser(Health, 0, "Entity {0} was destroyed with {1} health".FormatWith(Name, Health));
            this.Location.OccupyingEntity = null;
            this.Image.DestroyGameObject();
            return this.Wait(timeToWait);
        }

        private bool CanEnter(SquareScript newLocation)
        {
            if (newLocation.OccupyingEntity != null)
            {
                return false;
            }
            if (m_movementType == MovementType.Walking)
            {
                return newLocation.TraversingCondition == Traversability.Walkable;
            }
            //else flying
            return newLocation.TraversingCondition != Traversability.Blocking;
        }

        #endregion private and protected methods
    }

    #endregion Entity

    #region EnemyEntity

    public class EnemyEntity : Entity
    {
        #region fields

        private TimedSquareEffect m_action;

        private ActionableItem m_mainActionItem;

        private ActionableItem m_destructionItem;

        #endregion fields

        #region constructor

        public EnemyEntity(MonsterTemplate template, SquareScript location) :
            base(template, location)
        {
            // create a copy of action / destruction items, with this entity as the owner
            m_mainActionItem = new ActionableItem(GlobalState.Instance.Configurations.MonsterEquipment.GetConfiguration(template.ActionItem), this);

            if (template.DestructionItem != null)
            {
                m_destructionItem = new ActionableItem(GlobalState.Instance.Configurations.MonsterEquipment.GetConfiguration(template.DestructionItem), this);
            }

            switch (template.Tactics)
            {
                case EntityTactics.ActInRange:
                    m_action = MoveToRange;
                    m_alwaysActive = false;
                    break;

                case EntityTactics.AlwaysAct:
                    m_action = m_mainActionItem.Effect;
                    m_alwaysActive = true;
                    break;

                default:
                    throw new UnknownValueException(template.Tactics);
            }
        }

        #endregion constructor

        #region public methods

        public IEnumerator Act(float timePerMonster)
        {
            if (Destroyed())
            {
                return new EmptyEnumerator();
            }
            return m_action(Player.Location, timePerMonster);
        }

        #endregion public methods

        #region private methods

        protected override IEnumerator Destroy(float timeToWait)
        {
            EnemiesManager.Remove(this);
            IEnumerator enumerator = new EmptyEnumerator();
            if (m_destructionItem != null)
            {
                // TODO - wait for effect
                enumerator = m_destructionItem.Effect(Location, 0.01f);
            }
            return enumerator.Join(base.Destroy(timeToWait));
        }

        #region attacking

        private IEnumerator MoveToRange(SquareScript playerLocation, float timePerMonster)
        {
            if (WithinRange(playerLocation))
            {
                var enumerator = m_mainActionItem.Effect(playerLocation, timePerMonster);
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
            else
            {
                yield return MoveTowards(timePerMonster);
            }
        }

        protected bool WithinRange(Entity ent)
        {
            return WithinRange(ent.Location);
        }

        protected bool WithinRange(SquareScript otherLocation)
        {
            return m_mainActionItem.Range > this.Location.transform.position.Distance(otherLocation.transform.position);
        }

        private object MoveTowards(float timePerMonster)
        {
            var direction = new Vector2(Player.Location.transform.position.x - Location.transform.position.x, Location.transform.position.y - Player.Location.transform.position.y);
            var absX = Math.Abs(direction.x);
            var absY = Math.Abs(direction.y);
            var possibleLocationMovingX = Location.GetNextSquare((int)(direction.x / absX), 0);
            var possibleLocationMovingY = Location.GetNextSquare(0, (int)(direction.y / absY));
            if (absX > absY)
            {
                if (!TryMoveTo(possibleLocationMovingX))
                {
                    TryMoveTo(possibleLocationMovingY);
                }
            }
            else
            {
                if (!TryMoveTo(possibleLocationMovingY))
                {
                    TryMoveTo(possibleLocationMovingX);
                }
            }
            return new WaitForSeconds(timePerMonster);
        }

        #endregion attacking

        #endregion private methods
    }

    #endregion EnemyEntity

    #region PlayerEntity

    public class PlayerEntity : Entity
    {
        #region fields

        private System.Diagnostics.Stopwatch m_playerActionTimer;

        private readonly double m_maxEnergy;

        private bool m_hasFuelCell = false;

        private const int c_minimumEquipmentAmount = 2;
        private const int c_maximumEquipmentAmount = 8;

        #endregion fields

        #region Properties

        public double Energy { get; private set; }

        public double Oxygen { get; private set; }

        public IEnumerable<SquareScript> LastSeen { get; set; }

        public PlayerEquipment LeftHandEquipment { get; set; }

        public PlayerEquipment RightHandEquipment { get; set; }

        public Loot GainedLoot { get; set; }

        #endregion Properties

        #region constructor

        public PlayerEntity(double health, double energy, double oxygen, SquareScript location) :
            base(new EntityTemplate("PlayerSprite", health, MovementType.Walking), location)
        {
            m_maxEnergy = energy;
            Energy = energy;
            Oxygen = oxygen;
            m_playerActionTimer = new System.Diagnostics.Stopwatch();
            m_playerActionTimer.Start();
            GainedLoot = new Loot();
        }

        #endregion constructor

        #region public methods

        public void SetEquipment()
        {
            Assert.EqualOrLesser(GlobalState.Instance.Player.Equipment.Count(), c_maximumEquipmentAmount);
            Assert.EqualOrGreater(GlobalState.Instance.Player.Equipment.Count(), c_minimumEquipmentAmount);
            LeftHandEquipment = GlobalState.Instance.Player.Equipment.First();
            RightHandEquipment = GlobalState.Instance.Player.Equipment.ElementAt(1);
        }

        public bool Move(SquareScript newLocation)
        {
            if (BackToShip(newLocation))
            {
                EndGame(GameState.Won);
                return false;
            }
            if (TryMoveTo(newLocation))
            {
                TakeLoot(newLocation);
                newLocation.FogOfWar();
                return true;
            }
            return false;
        }

        public IEnumerator EndTurn(double energyCost)
        {
            Energy -= energyCost;
            double timeSinceLastAction = m_playerActionTimer.ElapsedMilliseconds / 1000.0;
            m_playerActionTimer.Reset();

            var enumerator = EnemiesManager.EnemiesTurn();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }

            Oxygen -= Math.Min(2, timeSinceLastAction);
            if (Oxygen <= 0)
            {
                enumerator = Destroy(0.1f);
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
            Energy = Math.Min(Energy + 1, m_maxEnergy);
            MapSceneScript.ReduceEffectsDuration();

            enumerator = Entity.DestroyKilledEntities();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
            m_playerActionTimer.Start();
        }

        public Boolean BackToShip(SquareScript newLocation)
        {
            if (m_hasFuelCell &&
                (newLocation.TerrainType == TerrainType.Spaceship_Bottom_Left ||
                newLocation.TerrainType == TerrainType.Spaceship_Bottom_Right ||
                newLocation.TerrainType == TerrainType.Spaceship_Top_Left ||
                newLocation.TerrainType == TerrainType.Spaceship_Top_Right))
            {
                return true;
            }
            return false;
        }

        public void EndGame(GameState state)
        {
            MapSceneScript.ChangeGameState(state);
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// Game Over
        /// </summary>
        protected override IEnumerator Destroy(float timeToWait)
        {
            EndGame(GameState.Lost);
            return new EmptyEnumerator();
        }

        private void TakeLoot(SquareScript newLocation)
        {
            var loot = newLocation.TakeLoot();
            if (loot != null)
            {
                GainedLoot.AddLoot(loot);
                if (loot.FuelCell)
                {
                    m_hasFuelCell = true;
                    loot.FuelCell = false;
                    MapSceneScript.EnterEscapeMode();
                }
            }
        }

        #endregion private methods
    }

    #endregion PlayerEntity
}