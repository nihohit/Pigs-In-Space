using Assets.Scripts.IntersceneCommunication;
using Assets.Scripts.UnityBase;
using UnityEngine;

namespace Assets.Scripts.SpaceshipScene
{
    using Assets.Scripts.Base;
    using Assets.Scripts.LogicBase;
    using System;
    using System.Collections.Generic;
    using UnityEngine.UI;
    using System.Linq;

    public class SpaceshipSceneScript : MonoBehaviour
    {
        #region private members

        private IEnumerable<Button> m_itemCreationButtons;

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
            GuiInit();
        }

        // Update is called once per frame
        private void Update()
        {
        }

        #endregion Unity methods

        #region private methods

        private void GuiInit()
        {
            var canvas = GameObject.Find("Canvas");
            m_itemCreationButtons =
                canvas.GetComponentsInChildren<Button>().Where(button => button.name.Contains("Create")).Materialize();
        }

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
            //throw new NotImplementedException();
        }

        private void UpdateItemCreationButtons()
        {
            foreach (var button in m_itemCreationButtons)
            {
                var itemName = button.name.Substring(7);
                button.gameObject.SetActive(!
                    GlobalState.Instance.Player.Equipment.Any(item => item.Name.Equals(itemName, StringComparison.InvariantCultureIgnoreCase)));
            }
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