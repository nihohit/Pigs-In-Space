using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region DistanceAndDensityBasedTreasurePopulator

    public class DistanceAndDensityBasedTreasurePopulator : BasePopulator, ITreasurePopulator
    {
        private const int c_densitySizeCheck = 3;

        #region public methods

        public void PopulateMap(SquareScript[,] map, IEnumerable<Loot> loot)
        {
            InitializeDistanceFromPlayer();
            InitializeSquareDensity(map, c_densitySizeCheck);
            PlaceFuelCell();

            var dict = GetFreeOrNearFreeTiles(map).ToDictionary(
                square => square,
                square => Convert.ToDouble(DistanceFromPlayer[square] + SurroundingDensity[square]));

            var lootEnumerator = loot.Shuffle().GetEnumerator();
            foreach (var square in dict.ChooseWeightedValues(loot.Count()))
            {
                lootEnumerator.MoveNext();
                PlaceLoot(square, lootEnumerator.Current);
            }
        }

        private void PlaceFuelCell()
        {
            var maxDistance = DistanceFromPlayer.Values.Max();
            var chosenSquare = DistanceFromPlayer.
                Where(pair => pair.Key.TraversingCondition == Traversability.Walkable &&
                    pair.Value >= maxDistance * 2 / 3).ToDictionary(pair => pair.Key,
                                    pair => Convert.ToDouble(DistanceFromPlayer[pair.Key] + SurroundingDensity[pair.Key])).
                ChooseWeightedValues(1).First();
            chosenSquare.AddLoot(new Loot(0, true));
            chosenSquare.TerrainType = TerrainType.Fuel_Cell;
        }

        #endregion public methods
    }

    #endregion DistanceAndDensityBasedTreasurePopulator
}