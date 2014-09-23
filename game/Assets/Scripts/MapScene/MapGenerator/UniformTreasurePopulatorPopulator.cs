using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region UniformTreasurePopulatorPopulator

    public class UniformTreasurePopulatorPopulator : BasePopulator, ITreasurePopulator
    {
        public void PopulateMap(SquareScript[,] map, IEnumerable<Loot> loot)
        {
            PlaceFuelCell();
            loot = loot.Shuffle();
            var freeTiles = GetFreeOrNearFreeTiles(map).Shuffle();
            Assert.Greater(freeTiles.Count(), loot.Count(), "There should be more free tiles than loot");
            var freeTilesEnumerator = freeTiles.GetEnumerator();

            foreach (var mineral in loot)
            {
                freeTilesEnumerator.MoveNext();
                PlaceLoot(freeTilesEnumerator.Current, mineral);
            }
        }

        private void PlaceFuelCell()
        {
            InitializeDistanceFromPlayer();
            var maxDistance = m_distanceFromPlayer.Values.Max();
            var chosenSquare = m_distanceFromPlayer.
                Where(pair => pair.Key.TraversingCondition == Traversability.Walkable &&
                    pair.Value >= maxDistance * 2 / 3).
                Select(pair => pair.Key).
                ChooseRandomValue();
            chosenSquare.AddLoot(new Loot(0, true));
            chosenSquare.TerrainType = TerrainType.Fuel_Cell;
        }
    }

    #endregion UniformTreasurePopulatorPopulator
}