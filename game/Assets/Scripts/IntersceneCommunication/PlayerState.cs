using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.IntersceneCommunication
{
    using Assets.Scripts.Base;
    using Assets.Scripts.LogicBase;

    #region PlayerState

    public class PlayerState
    {
        #region properties

        public List<PlayerEquipment> Equipment { get; private set; }

        public Loot Loot { get; private set; }

        #endregion properties

        #region constructor

        public PlayerState()
        {
            Loot = new Loot();
            Equipment = new List<PlayerEquipment>();
        }

        #endregion constructor
    }

    #endregion PlayerState
}