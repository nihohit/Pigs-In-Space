using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.LogicBase
{
    public delegate void SquareEffect(SquareScript square, double strength);

    #region ActionableItem

    public class ActionableItem
    {
        #region fields

        private SquareEffect m_effect;
        private double m_effectStrength;
        // TODO - do we want to add cooldown timer, so that not all equipment piece could be operated everyturn?

        #endregion

        #region properties

        public double Range { get; private set; }

        #endregion

        #region public methods

        public void Operate(SquareScript target)
        {
            m_effect(target, m_effectStrength);
        }

        #endregion
    }

    #endregion

    #region EquipmentPiece

    public class EquipmentPiece : ActionableItem
    { 
		#region properties

        public Loot Cost { get; private set; }

        public IEnumerable<EquipmentPiece> PossibleUpgrades { get; private set; }

        public double EnergyCost { get; private set; }

		#endregion
    }

    #endregion

}
