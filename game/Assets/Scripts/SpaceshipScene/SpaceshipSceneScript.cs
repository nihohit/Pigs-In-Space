

namespace Assets.Scripts.SpaceshipScene
{
    using Assets.Scripts.Base;
    using Assets.Scripts.LogicBase;
    using System;
    using System.Collections.Generic;
    using UnityEngine.UI;
    using System.Linq;
    using Assets.Scripts.IntersceneCommunication;
    using Assets.Scripts.UnityBase;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Events;

    public class SpaceshipSceneScript : MonoBehaviour
    {
        #region private members

        private List<Button> m_itemCreationButtons;

        private List<EquipmentSlot> m_equipmentSlots;

        private List<UpgradeOption> m_initialEquipment;

        private Dictionary<string, List<UpgradeOption>> m_upgradeOptions;

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

        public void UpgradeItem(UpgradeOption upgradeOption)
        {
            Assert.StringNotNullOrEmpty(upgradeOption.From, "upgradeOption.From");
            Assert.AssertConditionMet(GlobalState.Instance.Player.Equipment.Any(item => item.Name.Equals(upgradeOption.From, StringComparison.InvariantCultureIgnoreCase)), "No {0} found to upgrade".FormatWith(upgradeOption.From));
            var toBeUpgraded = GlobalState.Instance.Configurations.Equipment.GetConfiguration(upgradeOption.From);
            GlobalState.Instance.Player.Equipment.Remove(toBeUpgraded);
            CreateItem(upgradeOption);
        }

        #endregion public methods

        #region Unity methods

        // Use this for initialization
        private void Start()
        {
            HandleLastLevel();
            StoreItemConfigurations();
            GuiInit();
            EquipmentSlot.Scene = this;
        }

        // Update is called once per frame
        private void Update()
        {
        }

        #endregion Unity methods

        #region private methods

        private void StoreItemConfigurations()
        {
            var upgradeOptions = GlobalState.Instance.Configurations.Upgrades.GetAllConfigurations().ToList();
            var equipment = GlobalState.Instance.Configurations.Equipment.GetAllConfigurations().ToList();

            m_initialEquipment = upgradeOptions.Where(upgrade => string.IsNullOrEmpty(upgrade.From)).ToList();
            m_upgradeOptions = equipment.Select(equipmentPiece => equipmentPiece.Name)
                .ToDictionary(
                    equipmentPieceName => equipmentPieceName,
                    equipmentPieceName => upgradeOptions.Where(option => equipmentPieceName.Equals(option.From)).ToList());
        }

        private void GuiInit()
        {
            var canvas = GameObject.Find("Canvas");
            m_itemCreationButtons =
                canvas.GetComponentsInChildren<Button>().Where(button => button.name.Contains("CreateButton")).ToList();
            m_equipmentSlots = canvas.GetComponentsInChildren<EquipmentSlot>().ToList();

            UpdateItemButtons();
        }

        private void CreateItem(UpgradeOption upgradeOption)
        {
            var item = GlobalState.Instance.Configurations.Equipment.GetConfiguration(upgradeOption.Name);
            CreateItem(item, upgradeOption.Cost);
            UpdateItemButtons();
        }

        private void CreateItem(PlayerEquipment result, Loot cost)
        {
            Assert.AssertConditionMet(
                GlobalState.Instance.Player.Loot.RemoveIfEnough(cost),
                "Can't pay cost for {0}. Cost is {1}, loot is {2}".FormatWith(result.Name, cost, GlobalState.Instance.Player.Loot));
            GlobalState.Instance.Player.Equipment.Add(result);
        }

        private void UpdateItemButtons()
        {
            UpdateItemUpgradeButtons();
            UpdateItemCreationButtons();
        }

        private void UpdateItemUpgradeButtons()
        {
            var equipment = GlobalState.Instance.Player.Equipment;
            UnityHelper.SetFunctionalityForFirstItems<EquipmentSlot, PlayerEquipment>(
                m_equipmentSlots,
                equipment,
                (slot, equipmentPiece) => slot.SetEquipment(equipmentPiece, m_upgradeOptions.Get(equipmentPiece.Name)));
        }

        private void UpdateItemCreationButtons()
        {
            var freeToCreate = m_initialEquipment.Where(item => GlobalState.Instance.Player.Equipment.None(equipment => equipment.Name.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase))).ToList();

            var freeUpgradeSlots = m_equipmentSlots.Any(slot => !slot.gameObject.active);

            UnityHelper.SetFunctionalityForFirstItems<Button, UpgradeOption>(
                m_itemCreationButtons,
                freeToCreate,
                (button, upgrade) =>
                {
                    button.GetComponentInChildren<Text>().text = "Create {0}".FormatWith(upgrade.Name);
                    button.interactable = GlobalState.Instance.Player.Loot.IsEnoughToCover(upgrade.Cost) && freeUpgradeSlots;
                    if (button.interactable)
                    {
                        button.SetButtonFunctionality(() => CreateItem(upgrade));
                    }
                });
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