using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Base;

namespace Assets.Scripts
{
    public static class TacticalBattleExtension
    {
        public static IEnumerable<SquareScript> MultiplyBySize(this SquareScript original, int size)
        {
            var newSquares = new List<SquareScript>();
            newSquares.Add(original);

            for (int i = 0; i < size; i++ )
            {
                var addedSquares = newSquares.SelectMany(square => square.GetNeighbours(true)).Distinct().Materialize(); ;
                newSquares.AddRange(addedSquares);
            }

            return newSquares.Distinct();
        }

        public static IEnumerable<SquareScript> MultiplyBySize(this IEnumerable<SquareScript> originals, int size)
        {
            return originals.SelectMany(square => square.MultiplyBySize(size)).Distinct();
        }
    }
}
