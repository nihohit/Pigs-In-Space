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

public enum MovementType { Walking, Flying }

#region Entity

public abstract class Entity
{
    #region fields

    private MovementType m_movementType;

    #endregion fields

    #region properties

    public static PlayerEntity Player { get; set; }

    public double Health { get; private set; }

    public double AttackRange { get; private set; }

    public float MinDamage { get; private set; }

    public float MaxDamage { get; private set; }

    public SquareScript Location { get; private set; }

    public SpriteRenderer Image { get; private set; }

    #endregion properties

    public Entity(double health, double attackRange, float minDamage, float maxDamage, SquareScript location, SpriteRenderer image, MovementType movementType)
    {
        m_movementType = movementType;
        Health = health;
        AttackRange = attackRange;
        MinDamage = minDamage;
        MaxDamage = maxDamage;
        Location = location;
        Location.OccupyingEntity = this;
        Image = image;
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

    protected void Attack(Entity ent)
    {
        ent.Damage(Randomizer.NextDouble(MinDamage, MaxDamage));
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

#region PlayerEntity

public class PlayerEntity : Entity
{
    public double Energy { get; private set; }

    public double Oxygen { get; private set; }

    public double BlueCrystal { get; private set; }

    public PlayerEntity(double health, double attackRange, float minDamage, float maxDamage, SquareScript location, SpriteRenderer image, double energy, double oxygen) :
        base(health, attackRange, minDamage, maxDamage, location, image, MovementType.Walking)
    {
        Energy = energy;
        Oxygen = oxygen;
        UpdateUI("Health", Health);
        UpdateUI("Oxygen", Oxygen);
        UpdateUI("Energy", Energy);
        UpdateUI("Blue Crystals", BlueCrystal);
    }

    public void Move(SquareScript newLocation)
    {
        if (TryMoveTo(newLocation))
        {
            TakeLoot(newLocation);
        }
    }

    protected override void Destroy()
    {
        base.Destroy();
        Debug.Log("Game Over");
    }

    private void TakeLoot(SquareScript newLocation)
    {
        var loot = newLocation.TakeLoot();
        if (loot != null)
        {
            BlueCrystal += loot.BlueCrystal;
            loot.BlueCrystal = 0;
        }
        UpdateUI("Blue Crystals", BlueCrystal);
    }

    public override void Damage(double damage)
    {
        base.Damage(damage);
        UpdateUI("Health", Health);
    }

    private void UpdateUI(string updatedProperty, double updatedValue)
    {
        Camera.main.GetComponent<MapSceneScript>().UpdatePlayerState(updatedProperty, updatedValue);
    }

    public void ShootLaser(Vector3 mousePosition)
    {
        var destination = Input.mousePosition;

        var laser = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("laser"), Entity.Player.Location.transform.position, Quaternion.identity));
        var laserScript = laser.GetComponent<LaserScript>();
        var MousePos = Input.mousePosition;
        var translatedPosition = Camera.main.ScreenToWorldPoint(MousePos);
        var vec2 = new Vector2(translatedPosition.x, translatedPosition.y);
        laserScript.Init(vec2, Entity.Player.Location.transform.position, "Laser shot", MinDamage, MaxDamage);
    }
}

#endregion

#region EnemyEntity

public class EnemyEntity : Entity
{
    private static List<EnemyEntity> s_activeEntities = new List<EnemyEntity>();

    public static void EnemiesTurn()
    {
        foreach (var enemy in s_activeEntities)
        {
            enemy.Act();
        }
    }

    protected override void Destroy()
    {
        base.Destroy();
        s_activeEntities.Remove(this);
    }

    public EnemyEntity(double health, double attackRange, float minDamage, float maxDamage, SquareScript location, SpriteRenderer image, MovementType movementType) :
        base(health, attackRange, minDamage, maxDamage, location, image, movementType)
    {
        s_activeEntities.Add(this);
    }

    public void Act()
    {
        if (WithinRange(Entity.Player))
        {
            //Show hit action                        
            var playerLocation =  Entity.Player.Location.transform.position;
            var otherLocation = this.Location.transform.position;
            var powPosition = new Vector3((playerLocation.x + otherLocation.x)/2, (playerLocation.y + otherLocation.y)/2, playerLocation.z );

            var pow = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("pow"), powPosition, Quaternion.identity));
            UnityEngine.Object.Destroy(pow, 0.3f);      
            
            Attack(Entity.Player);
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    private void MoveTowardsPlayer()
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
