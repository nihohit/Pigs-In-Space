using UnityEngine;
using System.Collections;
using Assets.Scripts.LogicBase;
using UnityEngine.UI;
using System.Linq;
using Assets.Scripts.Base;
using Assets.Scripts.IntersceneCommunication;
using Assets.Scripts.SpaceshipScene;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;

namespace Assets.Scripts.SpaceshipScene
{
    public class EquipmentSlot : MonoBehaviour
    {
        public static SpaceshipSceneScript Scene { get; set; }

        public void SetEquipment(PlayerEquipment equipment, List<UpgradeOption> upgradeOptions)
        {
            Debug.Log("{0} set to {1}".FormatWith(this.name, equipment.Name));

            var image = GetComponentsInChildren<Image>().First(img => img.gameObject.name.Equals("EquipmentButton"));

            image.sprite = GlobalState.Instance.TextureManager.GetTexture(equipment);

            var buttons = gameObject.GetComponentsInChildren<Button>(true);
            var upgradeButtons = buttons.Where(button => button.name.Contains("Upgrade button")).ToList();

            UnityHelper.SetFunctionalityForFirstItems<Button, UpgradeOption>(
                upgradeButtons,
                upgradeOptions,
                (button, upgrade) =>
                {
                    button.GetComponentInChildren<Text>().text = "Create {0}".FormatWith(upgrade.Name);
                    button.interactable = GlobalState.Instance.Player.Loot.IsEnoughToCover(upgrade.Cost);
                    if (button.interactable)
                    {
                        button.SetButtonFunctionality(() => Scene.UpgradeItem(upgrade));
                    }
                });
        }
    }
}
