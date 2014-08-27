using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public enum MovementType { Walking, Flying }

#region Entity

public abstract class Entity
{
    #region fields

    private const int c_startHealth = 15;
    private const int c_startAttackRange = 5;
    private const int c_startMinDamage = 3;
    private const int c_startMaxDamage = 7;
    private const int c_startOxygen = 200;
    private const int c_startEnergy = 10;
    private const int c_HiveHealth = 20;

    protected bool m_active;

    #endregion fields

    #region properties

    public static PlayerEntity Player { get; set; }

    public double Health { get; private set; }

    public SquareScript Location { get; protected set; }

    public IUnityMarker Image { get; protected set; }

    #endregion properties

    #region constructor

    public Entity(double health, SquareScript location, IUnityMarker image)
    {
        // TODO: Complete member initialization
        this.Health = health;
        this.Location = location;
        this.Image = image;
        Image.Mark(location.transform.position);
        Location.OccupyingEntity = this;
    }

    #endregion constructor

    #region public methods

    public virtual void Damage(double damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Destroy();
        }
    }

    public virtual void SetActive(bool active)
    {
        m_active = active;
    }

    #region static generation methods

    public static void CreatePlayerEntity(int x, int y)
    {
        var square = SquareScript.GetSquare(x, y);
        Entity.Player = new PlayerEntity(c_startHealth, c_startAttackRange, c_startMinDamage, c_startMaxDamage,
            square,
            ((GameObject)MonoBehaviour.Instantiate(Resources.Load("PlayerSprite"),
                                                        square.transform.position,
                                                        Quaternion.identity)).GetComponent<MarkerScript>(),
            c_startEnergy,
            c_startOxygen);
        Entity.Player.SetEquipment(new EquipmentJSONParser().GetConfigurations("equipment"));
    }

    public static EnemyEntity CreateTentacleMonster(int x, int y)
    {
        return CreateTentacleMonster(SquareScript.GetSquare(x, y));
    }

    public static EnemyEntity CreateTentacleMonster(SquareScript square)
    {
        return new EnemyEntity(10, 1, 1, 2,
            square,
            ((GameObject)MonoBehaviour.Instantiate(Resources.Load("TentacleMonster"),
                                                        square.transform.position,
                                                        Quaternion.identity)).GetComponent<MarkerScript>(),
            MovementType.Walking);
    }

    public static Hive CreateHive(int x, int y)
    {
        var square = SquareScript.GetSquare(x, y);
        return new Hive(c_HiveHealth,
            square,
            ((GameObject)MonoBehaviour.Instantiate(Resources.Load("Hive"),
                                                        square.transform.position,
                                                        Quaternion.identity)).GetComponent<MarkerScript>());
    }

    #endregion static generation methods

    #endregion public methods

    #region private and protected methods

    protected virtual void Destroy()
    {
        this.Location.OccupyingEntity = null;
        this.Image.DestroyGameObject();
    }

    #endregion private and protected methods
}

#endregion Entity

#region AttackingEntity

public abstract class AttackingEntity : Entity
{
    #region fields

    private MovementType m_movementType;

    #endregion fields

    #region properties

    public double AttackRange { get; private set; }

    public float MinDamage { get; private set; }

    public float MaxDamage { get; private set; }

    #endregion properties

    public AttackingEntity(double health, double attackRange, float minDamage, float maxDamage, SquareScript location, IUnityMarker image, MovementType movementType) :
        base(health, location, image)
    {
        m_movementType = movementType;
        AttackRange = attackRange;
        MinDamage = minDamage;
        MaxDamage = maxDamage;
    }

    public virtual bool TryMoveTo(SquareScript newLocation)
    {
        if (!CanEnter(newLocation))
        {
            return false;
        }
        Location.OccupyingEntity = null;
        newLocation.OccupyingEntity = this;
        Location = newLocation;
        Image.Position = Location.transform.position;
        return true;
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

    protected bool WithinRange(Entity ent)
    {
        return WithinRange(ent.Location);
    }

    protected bool WithinRange(SquareScript otherLocation)
    {
        return AttackRange > this.Location.transform.position.Distance(otherLocation.transform.position);
    }

    protected void Attack(AttackingEntity ent)
    {
        ent.Damage(Randomiser.NextDouble(MinDamage, MaxDamage));
    }
}

#endregion AttackingEntity

#region PlayerEntity

public class PlayerEntity : AttackingEntity
{
    #region fields

    private Stopwatch m_playerActionTimer;

    private double m_maxEnergy = 20;

    private bool m_hasFuelCell = false;

    private const int c_minimumEquipmentAmount = 2;
    private const int c_maximumEquipmentAmount = 8;

    #endregion fields

    #region Properties

    public double Energy { get; private set; }

    public double Oxygen { get; private set; }

    public double BlueCrystal { get; private set; }

    public IEnumerable<SquareScript> LastSeen { get; set; }

    public IEnumerable<EquipmentPiece> Equipment { get; private set; }

    public EquipmentPiece LeftHandEquipment { get; set; }

    public EquipmentPiece RightHandEquipment { get; set; }

    #endregion Properties

    #region constructor

    public PlayerEntity(double health, double attackRange, float minDamage, float maxDamage, SquareScript location,
        IUnityMarker image, double energy, double oxygen) :
        base(health, attackRange, minDamage, maxDamage, location, image, MovementType.Walking)
    {
        Energy = energy;
        Oxygen = oxygen;
        m_playerActionTimer = new Stopwatch();
        m_playerActionTimer.Start();
    }

    #endregion constructor

    #region public methods

    public void SetEquipment(IEnumerable<EquipmentPiece> equipment)
    {
        Equipment = equipment;
        Assert.EqualOrLesser(Equipment.Count(), c_maximumEquipmentAmount);
        Assert.EqualOrGreater(Equipment.Count(), c_minimumEquipmentAmount);
        LeftHandEquipment = Equipment.First();
        RightHandEquipment = Equipment.ElementAt(1);
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

    public override void Damage(double damage)
    {
        base.Damage(damage);
    }

    public void EndTurn(double energyCost)
    {
        Energy -= energyCost;
        double timeSinceLastAction = m_playerActionTimer.ElapsedMilliseconds / 1000.0;
        m_playerActionTimer.Reset();
        EnemiesManager.EnemiesTurn();
        Oxygen -= Math.Min(2, timeSinceLastAction);
        if (Oxygen <= 0)
        {
            Destroy();
        }
        Energy++;
        Energy = Math.Min(Energy, m_maxEnergy);
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
    protected override void Destroy()
    {
        EndGame(GameState.Lost);
    }

    private void TakeLoot(SquareScript newLocation)
    {
        var loot = newLocation.TakeLoot();
        if (loot != null)
        {
            BlueCrystal += loot.BlueCrystal;
            loot.BlueCrystal = 0;
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

#region EnemyEntity

public class EnemyEntity : AttackingEntity, IHostileEntity
{
    private static int s_killed_Enemies = 0;

    public static int KilledEnemies { get { return s_killed_Enemies; } }

    protected override void Destroy()
    {
        base.Destroy();
        EnemiesManager.Remove(this);
        s_killed_Enemies++;
    }

    public EnemyEntity(double health, double attackRange, float minDamage, float maxDamage, SquareScript location, IUnityMarker image, MovementType movementType) :
        base(health, attackRange, minDamage, maxDamage, location, image, movementType)
    {
        EnemiesManager.AddEnemy(this);
    }

    public void Act()
    {
        if (m_active)
        {
            if (WithinRange(Player))
            {
                //Show hit action
                var Location = Player.Location.transform.position;
                var otherLocation = this.Location.transform.position;
                var powPosition = new Vector3((Location.x + otherLocation.x) / 2, (Location.y + otherLocation.y) / 2, Location.z);

                var pow = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("pow"), powPosition, Quaternion.identity));
                UnityEngine.Object.Destroy(pow, 0.3f);

                Attack(Player);
            }
            else
            {
                MoveTowards();
            }
        }
    }

    private void MoveTowards()
    {
        var direction = new Vector2(Player.Location.transform.position.x - Location.transform.position.x, Location.transform.position.y - Player.Location.transform.position.y);
        var absX = Math.Abs(direction.x);
        var absY = Math.Abs(direction.y);
        if (absX > absY)
        {
            TryMoveTo(Location.GetNextSquare((int)(direction.x / absX), 0));
        }
        else
        {
            TryMoveTo(Location.GetNextSquare(0, (int)(direction.y / absY)));
        }
    }
}

#endregion EnemyEntity

#region Hive

public class Hive : Entity, IHostileEntity
{
    private static double s_chanceToSpawn = 0.01;
    private static int s_killed_Hives = 0;

    public static int KilledHives { get { return s_killed_Hives; } }

    public Hive(double health, SquareScript location, IUnityMarker image) :
        base(health, location, image)
    {
        EnemiesManager.AddEnemy(this);
    }

    protected override void Destroy()
    {
        base.Destroy();
        EnemiesManager.Remove(this);
        s_killed_Hives++;
    }

    public void Act()
    {
        if (Randomiser.ProbabilityCheck(s_chanceToSpawn))
        {
            CreateTentacleMonster(ChooseRandomFreeSquare());
        }
    }

    public static void EnterEscapeMode()
    {
        s_chanceToSpawn = 0.1;
    }

    private SquareScript ChooseRandomFreeSquare()
    {
        return Location.GetNeighbours().Where(square => square.TraversingCondition == Traversability.Walkable && square.OccupyingEntity == null).ChooseRandomValue();
    }
}

#endregion Hive