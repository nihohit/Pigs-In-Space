using Assets.Scripts.Base;
using System.Collections.Generic;

namespace Assets.Scripts.LogicBase
{
    #region ActionableItemStorage

    public class ActionableItemStorage : ConfigurationStorage<ActionableItem>
    {
        public ActionableItemStorage(string filename)
            : base(filename)
        { }

        protected override JSONParser<ActionableItem> GetParser()
        {
            return new ItemJSONParser();
        }

        #region ItemJSONParser

        private sealed class ItemJSONParser : JSONParser<ActionableItem>
        {
            protected override ActionableItem ConvertToObject(Dictionary<string, object> item)
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
                    TryGetValueOrSetDefaultValue<string>("ShotType", "slimeball"));
            }
        }

        #endregion ItemJSONParser
    }

    #endregion ActionableItemStorage

    #region EquipmentConfigurationStorage

    public class EquipmentConfigurationStorage : ConfigurationStorage<PlayerEquipment>
    {
        public EquipmentConfigurationStorage(string filename)
            : base(filename)
        { }

        protected override JSONParser<PlayerEquipment> GetParser()
        {
            return new EquipmentJSONParser();
        }

        #region EquipmentJSONParser

        private sealed class EquipmentJSONParser : JSONParser<PlayerEquipment>
        {
            protected override PlayerEquipment ConvertToObject(Dictionary<string, object> item)
            {
                return new PlayerEquipment(
                    TryGetValueAndFail<string>(item, "Name"),
                    (EffectTypes)TryGetValueOrSetDefaultValue<int>(item, "EffectType", 1),
                    TryGetValueAndFail<float>(item, "MinPower"),
                    TryGetValueAndFail<float>(item, "MaxPower"),
                    TryGetValueAndFail<float>(item, "Range"),
                    TryGetValueOrSetDefaultValue<int>(item, "ShotSAmount", 1),
                    TryGetValueOrSetDefaultValue<float>(item, "ShotSpread", 0),
                    TryGetValueOrSetDefaultValue<int>(item, "EffectSize", 0),
                    TryGetValueOrSetDefaultValue<string>(item, "ShotType", "laser"),
                    TryGetValueAndFail<float>(item, "EnergyCost"),
                    TryGetValueOrSetDefaultValue<Loot>(item, "Cost", null),
                    TryGetValueOrSetDefaultValue<IEnumerable<string>>(item, "Upgrades", null));
            }
        }

        #endregion EquipmentJSONParser
    }

    #endregion EquipmentConfigurationStorage
}