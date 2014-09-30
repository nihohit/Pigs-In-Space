using Assets.Scripts.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region BasePopulator

    [Flags]
    public enum PopulationWeighting { None, SpaceDensity, DistanceFromPlayer }

    /// <summary>
    /// the base class for all populators. Populators receive the map and a list of items,
    /// and populate the map with those items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BasePopulator<T>
    {
        #region private fields

        private List<SquareScript> m_freeOrNearSquares;
        private List<SquareScript> m_freeSquares;

        private readonly int c_densityCheckDistance = SimpleConfigurationHandler.GetIntProperty("density check distance", FileAccessor.TerrainGeneration);

        #endregion private fields

        #region properties

        protected static IDictionary<SquareScript, int> DistanceFromPlayer { get; private set; }

        protected static IDictionary<SquareScript, int> SurroundingDensity { get; private set; }

        #endregion properties

        #region public methods

        public static void Clear()
        {
            SurroundingDensity = null;
            DistanceFromPlayer = null;
        }

        public abstract void PopulateMap(SquareScript[,] map, IEnumerable<T> items);

        #endregion public methods

        #region protected methods

        #region tile filters

        /// <summary>
        /// return all squares which are free or next to free squares.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        protected IEnumerable<SquareScript> GetFreeOrNearFreeSquares(SquareScript[,] map)
        {
            if (m_freeOrNearSquares == null)
            {
                m_freeOrNearSquares = new List<SquareScript>(map.ToEnumerable().Where(square => IsEmpty(square) ||
               (square.TraversingCondition == Traversability.Blocking &&
               square.GetNeighbours().Any(neighbour => neighbour.TraversingCondition == Traversability.Walkable))));
            }
            return m_freeOrNearSquares;
        }

        protected IEnumerable<SquareScript> GetFreeTiles(SquareScript[,] map)
        {
            if (m_freeSquares == null)
            {
                m_freeSquares = new List<SquareScript>(map.ToEnumerable().Where(IsEmpty));
            }
            return m_freeSquares;
        }

        /// <summary>
        /// return the amount of nearby squares that can't be walked into.
        /// </summary>
        /// <param name="square"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        protected int NonWalkableAmount(SquareScript square, int size)
        {
            return square.MultiplyBySize(size).Count(neighbour => neighbour.TraversingCondition != Traversability.Walkable);
        }

        #endregion tile filters

        #region placers

        /// <summary>
        ///  place an item in the square
        /// </summary>
        /// <param name="square"></param>
        /// <param name="item"></param>
        protected abstract void Place(SquareScript square, T item);

        /// <summary>
        /// place a collection of items with a given kinds of weights.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="items"></param>
        /// <param name="weightConsiderations"></param>
        /// <param name="wallsToo"></param>
        protected void PlaceByConstraint(SquareScript[,] map, IEnumerable<T> items, PopulationWeighting weightConsiderations, bool wallsToo)
        {
            // initialize the relevant dictionaries
            if (weightConsiderations.HasFlag(PopulationWeighting.DistanceFromPlayer))
            {
                InitializeDistanceFromPlayer();
            }
            if (weightConsiderations.HasFlag(PopulationWeighting.SpaceDensity))
            {
                InitializeSquareDensity(map, c_densityCheckDistance);
            }

            var relevantSquares = wallsToo ? GetFreeOrNearFreeSquares(map) : GetFreeTiles(map);

            // create a dictionary from each square to its weight
            var dict = relevantSquares.ToDictionary(
                square => square,
                square => GetSquareWeight(square, weightConsiderations));

            var itemEnumerator = items.Shuffle().GetEnumerator();
            foreach (var square in dict.ChooseWeightedValues(items.Count()))
            {
                itemEnumerator.MoveNext();
                Place(square, itemEnumerator.Current);
            }
        }

        #endregion placers

        #region distance from player

        /// <summary>
        /// Initialize a dictionary from each square to its distance from the player's starting position.
        /// Uses BFS to determine distance.
        /// </summary>
        protected static void InitializeDistanceFromPlayer()
        {
            if (DistanceFromPlayer != null) return;

            DistanceFromPlayer = new Dictionary<SquareScript, int> { { Entity.Player.Location, 0 } };

            var previousSquares = new Dictionary<SquareScript, SquareScript>();
            var queue = new Queue<SquareScript>();

            foreach (var square in Entity.Player.Location.GetNeighbours())
            {
                previousSquares.Add(square, Entity.Player.Location);
                queue.Enqueue(square);
            }

            while (queue.Any())
            {
                var square = queue.Dequeue();
                if (DistanceFromPlayer.ContainsKey(square))
                    continue;

                DistanceFromPlayer.Add(square, DistanceFromPlayer.Get(previousSquares.Get(square)) + 1);

                if (square.TraversingCondition == Traversability.Walkable)
                {
                    foreach (var newSquare in square.GetNeighbours())
                    {
                        if (!previousSquares.ContainsKey(newSquare))
                        {
                            previousSquares.Add(newSquare, square);
                            queue.Enqueue(newSquare);
                        }
                    }
                }
            }
        }

        #endregion distance from player

        #region square density

        /// <summary>
        /// Initialize a dictionary from each square to the amount of non-walkable squares near it
        /// </summary>
        /// <param name="map"></param>
        /// <param name="checkDistance"></param>
        protected void InitializeSquareDensity(SquareScript[,] map, int checkDistance)
        {
            if (SurroundingDensity != null) return;

            SurroundingDensity = new Dictionary<SquareScript, int>();

            foreach (var square in GetFreeOrNearFreeSquares(map))
            {
                SurroundingDensity.Add(square, square.MultiplyBySize(checkDistance).Count(neighbour => !IsEmpty(neighbour)));
            }
        }

        #endregion square density

        #endregion protected methods

        #region private methods

        private bool IsEmpty(SquareScript square)
        {
            return square.TraversingCondition == Traversability.Walkable && square.OccupyingEntity == null;
        }

        private double GetSquareWeight(SquareScript square, PopulationWeighting constraints)
        {
            return Convert.ToDouble(
                (constraints.HasFlag(PopulationWeighting.DistanceFromPlayer) ? DistanceFromPlayer.Get(square, "distance from player dictionary") : 1) *
                (constraints.HasFlag(PopulationWeighting.SpaceDensity) ? SurroundingDensity.Get(square, "space density dictionary") : 1));
        }

        #endregion private methods
    }

    #endregion BasePopulator
}