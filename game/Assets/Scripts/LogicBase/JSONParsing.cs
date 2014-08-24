using Assets.Scripts.Base;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.LogicBase
{
    #region EquipmentJSONParser

    public sealed class EquipmentJSONParser : JSONParser<EquipmentPiece>
    {
        protected override EquipmentPiece ConvertToObject(Dictionary<string, object> item)
        {
            //TODO - debugging only. reduce to single line
            var temp = new EquipmentPiece(
                (SpecialEffects)TryGetValue<long>(item, "EffectType"),
                TryGetValue<double>(item, "MinPower"),
                TryGetValue<double>(item, "MaxPower"),
                (float)TryGetValue<double>(item, "Range"),
                (int)TryGetValue<long>(item, "ShotSAmount", 1),
                (int)TryGetValue<long>(item, "ShotSpread", 0),
                (int)TryGetValue<long>(item, "EffectSize", 0),
                TryGetValue<string>(item, "Name"),
                TryGetValue<double>(item, "EnergyCost"),
                TryGetValue<Loot>(item, "Cost", null),
                TryGetValue<IEnumerable<EquipmentPiece>>(item, "Upgrades", null));

            Debug.Log(temp.ToString());

            return temp;
        }
    }

    #endregion EquipmentJSONParser
}