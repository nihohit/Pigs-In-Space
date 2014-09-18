using Assets.Scripts;
using Assets.Scripts.LogicBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
     public class ShipData
    {
        #region data members

         private static ShipData s_ShipData;

        public  double BlueCrystal { get; set; }

        public  int KilledSlimes { get; set; }

        public  int KilledTentacles { get; set; }

        public  int KilledHives { get; set; }

        public IEnumerable<PlayerEquipment> PossibleEquipment = new EquipmentConfigurationStorage("equipment").GetAllConfigurations();

        public IList<PlayerEquipment> OwnedEquipment = new List<PlayerEquipment>();
        #endregion

        private ShipData()
        {
            BlueCrystal = 0;
            KilledSlimes = 0;
            KilledTentacles = 0;
            KilledHives = 0;
        }

        #region Public methods

        public static ShipData Get
        {
            get
            {
                if (s_ShipData == null)
                {
                    NewGame();
                }
                return s_ShipData;
            }
        }

        public  void FinishLevel ()
        {
            BlueCrystal += Entity.Player.BlueCrystal;
            KilledSlimes += EnemiesManager.KilledSlimes;
            KilledTentacles += EnemiesManager.KilledTentacles;
            KilledHives += EnemiesManager.KilledHives;
        }

        public static void NewGame()
        {
            s_ShipData = new ShipData();
            s_ShipData.OwnedEquipment.Add(s_ShipData.PossibleEquipment.First());
            s_ShipData.OwnedEquipment.Add(s_ShipData.PossibleEquipment.ElementAt(1));
        }

        #endregion
    }
}
