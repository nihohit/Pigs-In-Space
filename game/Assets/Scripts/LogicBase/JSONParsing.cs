using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Base;

namespace Assets.Scripts.LogicBase
{
    #region EquipmentJSONParser

    public sealed class EquipmentJSONParser : JSONParser<EquipmentPiece>
    {
        protected override EquipmentPiece ConvertToObject(Dictionary<string, object> item)
        {
            return new EquipmentPiece(
                (EffectType)TryGetValue<long>(item, "EffectType"),
                TryGetValue<double>(item, "MinPower"),
                TryGetValue<double>(item, "MaxPower"),
                (float)TryGetValue<double>(item, "Range"),
                (int)TryGetValue<long>(item, "ShotSAmount", 1),
                (int)TryGetValue<long>(item, "ShotSpread", 0),
                TryGetValue<string>(item, "Name"),
                TryGetValue<double>(item, "EnergyCost"),
                TryGetValue<Loot>(item, "Cost", null),
                TryGetValue<IEnumerable<EquipmentPiece>>(item, "Upgrades", null));
        }
    }

    #endregion
}
