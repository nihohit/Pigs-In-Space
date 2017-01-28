using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.IntersceneCommunication
{
    using Assets.Scripts.Base;
    using Assets.Scripts.LogicBase;

    public class PlayerState
    {
        #region properties

        public List<PlayerEquipment> Equipment { get; private set; }

        public Loot Loot { get; private set; }

        public int Health { get; private set; }

        public int Energy { get; private set; }

        public int Oxygen { get; private set; }

        #endregion properties

        #region constructor

        public PlayerState(int health, int energy, int oxygen, int crystals)
        {
            Loot = new Loot(crystals);
            Equipment = new List<PlayerEquipment>();
            Health = health;
            Oxygen = oxygen;
            Energy = energy;
        }

        #endregion constructor
    }
}