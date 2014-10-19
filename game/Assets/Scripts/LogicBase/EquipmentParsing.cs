using Assets.Scripts.Base;
using System.Collections.Generic;

namespace Assets.Scripts.LogicBase
{
    #region ActionableItemStorage

    public class ActionableItemStorage : ConfigurationStorage<ActionableItem, ActionableItemStorage>
    {
        public ActionableItemStorage()
            : base("monsterItems")
        { }

        protected override JSONParser<ActionableItem> GetParser()
        {
            return new ItemJSONParser();
        }

        #region ItemJSONParser

        private sealed class ItemJSONParser : JSONParser<ActionableItem>
        {
            protected override ActionableItem ConvertCurrentItemToObject()
            {
                return new ActionableItem(
                    TryGetValueAndFail<string>("Name"),
                    TryGetValueOrSetDefaultValue<EffectTypes>("EffectType", EffectTypes.DamageDealing),
                    TryGetValueOrSetDefaultValue<float>("MinPower", 0),
                    TryGetValueOrSetDefaultValue<float>("MaxPower", 0),
                    TryGetValueOrSetDefaultValue<float>("Range", 0),
                    TryGetValueOrSetDefaultValue<int>("ShotSAmount", 1),
                    TryGetValueOrSetDefaultValue<float>("ShotSpread", 0),
                    TryGetValueOrSetDefaultValue<int>("EffectSize", 0),
                    TryGetValueOrSetDefaultValue<string>("ShotType", "slimeball"),
                    TryGetValueOrSetDefaultValue<string>("CreatedMonsterType", null));
            }
        }

        #endregion ItemJSONParser
    }

    #endregion ActionableItemStorage

    #region EquipmentConfigurationStorage

    public class EquipmentConfigurationStorage : ConfigurationStorage<PlayerEquipment, EquipmentConfigurationStorage>
    {
        public EquipmentConfigurationStorage()
            : base("equipment")
        { }

        protected override JSONParser<PlayerEquipment> GetParser()
        {
            return new EquipmentJSONParser();
        }

        #region EquipmentJSONParser

        private sealed class EquipmentJSONParser : JSONParser<PlayerEquipment>
        {
            protected override PlayerEquipment ConvertCurrentItemToObject()
            {
                return new PlayerEquipment(
                    TryGetValueAndFail<string>("Name"),
                    TryGetValueOrSetDefaultValue<EffectTypes>("EffectType", EffectTypes.DamageDealing),
                    TryGetValueAndFail<float>("MinPower"),
                    TryGetValueAndFail<float>("MaxPower"),
                    TryGetValueOrSetDefaultValue<float>("Range",0),
                    TryGetValueOrSetDefaultValue<int>("ShotSAmount", 1),
                    TryGetValueOrSetDefaultValue<float>("ShotSpread", 0),
                    TryGetValueOrSetDefaultValue<int>("EffectSize", 0),
                    TryGetValueOrSetDefaultValue<string>("ShotType", "laser"),
                    TryGetValueOrSetDefaultValue<string>("CreatedMonsterType", null),
                    TryGetValueAndFail<float>("EnergyCost"),
                    TryGetValueOrSetDefaultValue<Loot>("Cost", null),
                    TryGetValueOrSetDefaultValue<IEnumerable<string>>("Upgrades", null));
            }
        }

        #endregion EquipmentJSONParser
    }

    #endregion EquipmentConfigurationStorage
}