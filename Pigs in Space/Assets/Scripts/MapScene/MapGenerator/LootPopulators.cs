using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region ITreasurePopulator

    public interface ITreasurePopulator
    {
        void PopulateMap(SquareScript[,] map, IEnumerable<Loot> loot);
    }

    #endregion ITreasurePopulator

    #region LootPopulator

    public abstract class LootPopulator : BasePopulator<Loot>, ITreasurePopulator
    {
        protected abstract PopulationWeighting ChosenWeights { get; }

        public override void PopulateMap(SquareScript[,] map, IEnumerable<Loot> loot)
        {
            PlaceFuelCell();
            PlaceBy(map, loot);
        }

        protected void PlaceBy(SquareScript[,] map, IEnumerable<Loot> items)
        {
            PlaceByConstraint(map, items, ChosenWeights, true);
        }

        protected override void Place(SquareScript square, Loot loot)
        {
            // if it's a rock, replace it.
            if (square.TraversingCondition == Traversability.Blocking)
            {
                square.TerrainType = TerrainType.Rock_Crystal;
            }
            square.AddLoot(loot);
        }

        /// <summary>
        /// Place fuel cell so that it is in a minimum distance from starting location
        /// </summary>
        protected void PlaceFuelCell()
        {
            InitializeDistanceFromPlayer();
            var maxDistance = DistanceFromPlayer.Values.Max();
            var chosenSquare = DistanceFromPlayer.
                Where(pair => pair.Key.TraversingCondition == Traversability.Walkable &&
                    pair.Value >= maxDistance * 2 / 3).
                Select(pair => pair.Key).
                ChooseRandomValue();
            chosenSquare.AddLoot(new Loot(0, true));
        }
    }

    #endregion LootPopulator

    #region UniformTreasurePopulatorPopulator

    public class UniformTreasurePopulator : LootPopulator
    {
        protected override PopulationWeighting ChosenWeights
        {
            get { return PopulationWeighting.None; }
        }
    }

    #endregion UniformTreasurePopulatorPopulator

    #region DistanceAndDensityBasedTreasurePopulator

    public class DistanceAndDensityBasedTreasurePopulator : LootPopulator
    {
        protected override PopulationWeighting ChosenWeights
        {
            get { return PopulationWeighting.DistanceFromPlayer | PopulationWeighting.SpaceDensity; }
        }
    }

    #endregion DistanceAndDensityBasedTreasurePopulator
}