using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region BasePopulator

    public class BasePopulator
    {
        #region private fields

        private List<SquareScript> m_freeOrNearSquares;
        private List<SquareScript> m_freeSquares;

        #endregion private fields

        #region properties

        protected IDictionary<SquareScript, int> DistanceFromPlayer { get; private set; }

        protected IDictionary<SquareScript, int> SurroundingDensity { get; private set; }

        #endregion properties

        #region protected methods

        #region tile filters

        protected IEnumerable<SquareScript> GetFreeOrNearFreeTiles(SquareScript[,] map)
        {
            if (m_freeOrNearSquares == null)
            {
                m_freeOrNearSquares = new List<SquareScript>(map.ToEnumerable().Where(square => IsEmpty(square) ||
               (square.TraversingCondition == Traversability.Blocking &&
               square.GetNeighbours().Count(neighbour => neighbour.TraversingCondition == Traversability.Walkable) > 3)));
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

        protected int NonWalkableAmount(SquareScript square, int size)
        {
            return square.MultiplyBySize(size).Count(neighbour => neighbour.TraversingCondition != Traversability.Walkable);
        }

        #endregion tile filters

        #region placers

        protected void PlaceLoot(SquareScript square, Loot loot)
        {
            // if it's a rock, replace it.
            if (square.TraversingCondition == Traversability.Blocking)
            {
                square.TerrainType = TerrainType.Rock_Crystal;
            }
            square.AddLoot(loot);
        }

        protected void PlaceEnemy(SquareScript square, MonsterTemplate ent)
        {
            EnemiesManager.CreateEnemy(ent, square);
        }

        #endregion placers

        #region distance from player

        protected void InitializeDistanceFromPlayer()
        {
            DistanceFromPlayer = new Dictionary<SquareScript, int>();
            DistanceFromPlayer.Add(Entity.Player.Location, 0);

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

        protected void InitializeSquareDensity(SquareScript[,] map, int checkSize)
        {
            SurroundingDensity = new Dictionary<SquareScript, int>();

            foreach (var square in GetFreeOrNearFreeTiles(map))
            {
                SurroundingDensity.Add(square, square.MultiplyBySize(checkSize).Count(neighbour => !IsEmpty(neighbour)));
            }
        }

        #endregion square density

        #endregion protected methods

        #region private methods

        private bool IsEmpty(SquareScript square)
        {
            return square.TraversingCondition == Traversability.Walkable && square.OccupyingEntity == null;
        }

        #endregion private methods
    }

    #endregion BasePopulator
}