using Assets.Scripts.Base;
using System.Collections.Generic;

namespace Assets.Scripts.LogicBase
{
    #region EquipmentJSONParser

    public sealed class EquipmentJSONParser : JSONParser<EquipmentPiece>
    {
        protected override EquipmentPiece ConvertToObject(Dictionary<string, object> item)
        {
            return new EquipmentPiece(
                (SpecialEffects)TryGetValueOrSetDefaultValue<int>(item, "EffectType", 0),
                TryGetValueAndFail<float>(item, "MinPower"),
                TryGetValueAndFail<float>(item, "MaxPower"),
                TryGetValueAndFail<float>(item, "Range"),
                TryGetValueOrSetDefaultValue<int>(item, "ShotSAmount", 1),
                TryGetValueOrSetDefaultValue<float>(item, "ShotSpread", 0),
                TryGetValueOrSetDefaultValue<int>(item, "EffectSize", 0),
                TryGetValueOrSetDefaultValue<string>(item, "ShotType", "laser"),
                TryGetValueAndFail<string>(item, "Name"),
                TryGetValueAndFail<float>(item, "EnergyCost"),
                TryGetValueOrSetDefaultValue<Loot>(item, "Cost", null),
                TryGetValueOrSetDefaultValue<IEnumerable<EquipmentPiece>>(item, "Upgrades", null));
        }
    }

    #endregion EquipmentJSONParser
}