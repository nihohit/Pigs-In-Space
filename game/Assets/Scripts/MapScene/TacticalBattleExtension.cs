using Assets.Scripts.Base;
using Assets.Scripts.UnityBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.MapScene
{
    public static class TacticalBattleExtension
    {
        // find all squares in range of these squares
        public static IEnumerable<SquareScript> MultiplyBySize(this IEnumerable<SquareScript> originals, int size)
        {
            return originals.SelectMany(square => square.MultiplyBySize(size)).Distinct();
        }

        public static IEnumerator WaitAndEndTurn(this object obj, float time, double energyCost)
        {
            return obj.Wait(time).Join(Entity.Player.EndTurn(energyCost));
        }
    }
}