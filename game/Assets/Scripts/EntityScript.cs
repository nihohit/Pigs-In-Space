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
using UnityEngine;
using System.Linq;

public enum MovementType { Walking, Flying }

#region Entity

public abstract class Entity
{
    public static PlayerEntity Player  { get; set; }

    public double Health { get; private set; }

    public SquareScript Location { get; protected set; }

    public SpriteRenderer Image { get; protected set; }

    public Entity(double health, SquareScript location, SpriteRenderer image)
    {
        // TODO: Complete member initialization
        this.Health = health;
        this.Location = location;
        this.Image = image;
        Location.OccupyingEntity = this;
    }

    public virtual void Damage(double damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Destroy();
        }
    }

    protected virtual void Destroy()
    {
        this.Location.OccupyingEntity = null;
        UnityEngine.Object.Destroy(this.Image);
    }
}

#endregion

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

    public AttackingEntity(double health, double attackRange, float minDamage, float maxDamage, SquareScript location, SpriteRenderer image, MovementType movementType) :
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
        Image.transform.position = Location.transform.position;
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
        ent.Damage(Randomizer.NextDouble(MinDamage, MaxDamage));
    }
}

#endregion

#region PlayerEntity

public class PlayerEntity : AttackingEntity
{

    private double m_maxEnergy = 10;

    public double Energy { get; private set; }

    public double Oxygen { get; private set; }

    public double BlueCrystal { get; private set; }

    private bool m_hasFuelCell = false;

    public PlayerEntity(double health, double attackRange, float minDamage, float maxDamage, SquareScript location, SpriteRenderer image, double energy, double oxygen) :
        base(health, attackRange, minDamage, maxDamage, location, image, MovementType.Walking)
    {
        Energy = energy;
        Oxygen = oxygen;
        UpdateUI();
    }

    public bool Move(SquareScript newLocation)
    {
        if (TryMoveTo(newLocation))
        {
            TakeLoot(newLocation);
            return true;
        }
        return false;
    }

    protected override void Destroy()
    {
        Debug.Log("Game Over");
        var gameOverThing = new GameObject();
        gameOverThing.AddComponent<GameOverScript>();
        //base.Destroy();
    }

    private void TakeLoot(SquareScript newLocation)
    {
        var loot = newLocation.TakeLoot();
        if (loot != null)
        {
            BlueCrystal += loot.BlueCrystal;
            loot.BlueCrystal = 0;
            if(loot.FuelCell)
            {
                m_hasFuelCell = true;
                loot.FuelCell = false;
                MapSceneScript.EnterEscapeMode();
            }
        }
        UpdateUI("Blue Crystals", BlueCrystal);
    }

    public override void Damage(double damage)
    {
        base.Damage(damage);
        UpdateUI("Health", Health);
    }

    private void UpdateUI()
    {
        UpdateUI("Health", Health);
        UpdateUI("Oxygen", Oxygen);
        UpdateUI("Energy", Energy);
        UpdateUI("Blue Crystals", BlueCrystal);
    }

    private void UpdateUI(string updatedProperty, double updatedValue)
    {
        Camera.main.GetComponent<MapSceneScript>().UpdatePlayerState(updatedProperty, updatedValue);
    }

    public bool ShootLaser(Vector3 mousePosition)
    {
        if (Energy <2)
        {
            return false;
        }
        Energy -= 2;
        var laser = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("laser"), Player.Location.transform.position, Quaternion.identity));
        var laserScript = laser.GetComponent<LaserScript>();
        var MousePos = Input.mousePosition;
        var translatedPosition = Camera.main.ScreenToWorldPoint(MousePos);
        var vec2 = new Vector2(translatedPosition.x, translatedPosition.y);
        laserScript.Init(vec2, Player.Location.transform.position, "Laser shot", MinDamage, MaxDamage);
        return true;
    }

    public void MineAsteroid()
    {
        if ((Extensions.AreNeighbors(SquareScript.s_markedSquare, this.Location ) && 
            SquareScript.s_markedSquare.GetComponent<SpriteRenderer>().sprite == SpriteManager7.Rock_Crystal))
        {            
            SquareScript.s_markedSquare.GetComponent<SpriteRenderer>().sprite = SpriteManager7.Empty;
            var mineral = new Loot();
            mineral.BlueCrystal = 5;           
            SquareScript.s_markedSquare.AddLoot(mineral);
            SquareScript.s_markedSquare.TraversingCondition = Traversability.Walkable;
            EndTurn();
        }           
    }

    public void EndTurn()
    {
        EnemiesManager.EnemiesTurn();
        Oxygen--;
        if (Oxygen <= 0)
        {
            Destroy();
        }
        Energy++;
        Energy = Math.Min(Energy, m_maxEnergy);
        UpdateUI();
    }
}

#endregion

#region EnemyEntity

public class EnemyEntity : AttackingEntity, IHostileEntity
{
    protected override void Destroy()
    {
        base.Destroy();
        EnemiesManager.Remove(this);
    }

    public EnemyEntity(double health, double attackRange, float minDamage, float maxDamage, SquareScript location, SpriteRenderer image, MovementType movementType) :
        base(health, attackRange, minDamage, maxDamage, location, image, movementType)
    {
        EnemiesManager.AddEnemy(this);
    }

    public void Act()
    {
        if (WithinRange(Player))
        {
            //Show hit action                        
            var Location = Player.Location.transform.position;
            var otherLocation = this.Location.transform.position;
            var powPosition = new Vector3((Location.x + otherLocation.x)/2, (Location.y + otherLocation.y)/2, Location.z );

            var pow = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("pow"), powPosition, Quaternion.identity));
            UnityEngine.Object.Destroy(pow, 0.3f);

            Attack(Player);
        }
        else
        {
            MoveTowards();
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

#endregion

#region Hive

public class Hive : Entity, IHostileEntity
{
    private const int c_turnsToSpawn = 20;
    private int m_turnsToSpawn;

    public Hive(double health, SquareScript location, SpriteRenderer image) :
        base(health, location, image)
    { 
        EnemiesManager.AddEnemy(this);
        m_turnsToSpawn = c_turnsToSpawn;
    }

    protected override void Destroy()
    {
        base.Destroy();
        EnemiesManager.Remove(this);
    }

    public void Act()
    {
        m_turnsToSpawn--;
        if(m_turnsToSpawn == 0)
        {
            MapSceneScript.CreateTentacleMonster(ChooseRandomFreeSquare());
            m_turnsToSpawn = c_turnsToSpawn;
        }
    }

    private SquareScript ChooseRandomFreeSquare()
    {
        return Location.GetNeighbours().Where(square => square.TraversingCondition == Traversability.Walkable && square.OccupyingEntity == null).ChooseRandomMember();
    }
}

#endregion
