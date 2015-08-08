using Assets.Scripts.IntersceneCommunication;
using Assets.Scripts.UnityBase;
using UnityEngine;

namespace Assets.Scripts.SpaceshipScene
{
    using Assets.Scripts.Base;
    using Assets.Scripts.LogicBase;
    using System;

    public class SpaceshipSceneScript : MonoBehaviour
    {
        #region private members

        #endregion private members

        #region public methods

        public void QuitGame()
        {
            Application.Quit();
        }

        public void NextLevel()
        {
            Application.LoadLevel("MapScene");
        }

        public void CreateDigger()
        {
            CreateItem("digger");
        }

        public void CreatePistol()
        {
            CreateItem("pistol");
        }

        public void CreateMedkit()
        {
            CreateItem("medkit");
        }

        #endregion public methods

        #region Unity methods

        // Use this for initialization
        private void Start()
        {
            HandleLastLevel();
        }

        // Update is called once per frame
        private void Update()
        {
        }

        #endregion Unity methods

        #region private methods

        private void UpgradeItem(PlayerEquipment toBeUpgraded, PlayerEquipment result)
        {
            CreateItem(result);
            GlobalState.Instance.Player.Equipment.Remove(toBeUpgraded);
        }

        private void CreateItem(PlayerEquipment result)
        {
            Assert.AssertConditionMet(
                GlobalState.Instance.Player.Loot.RemoveIfEnough(result.Cost),
                "Can't pay cost for {0}. Cost is {1}, loot is {2}".FormatWith(result.Name, result.Cost, GlobalState.Instance.Player.Loot));
            GlobalState.Instance.Player.Equipment.Add(result);
        }

        private void CreateItem(string itemName)
        {
            var item = GlobalState.Instance.Configurations.Equipment.GetConfiguration(itemName);
            CreateItem(item);
            UpdateItemButtons();
        }

        private void UpdateItemButtons()
        {
            UpdateItemCreationButtons();
            UpdateItemUpgradeButtons();
        }

        private void UpdateItemUpgradeButtons()
        {
            throw new NotImplementedException();
        }

        private void UpdateItemCreationButtons()
        {
            throw new NotImplementedException();
        }

        private void HandleLastLevel()
        {
            if (GlobalState.Instance.EndLevel != null)
            {
                GlobalState.Instance.Player.Loot.AddLoot(GlobalState.Instance.EndLevel.GainedLoot);
                GlobalState.Instance.EndLevel = null;
            }
        }

        #endregion private methods
    }
}