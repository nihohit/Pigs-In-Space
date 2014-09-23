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
        protected IDictionary<SquareScript, int> m_distanceFromPlayer;

        #region protected methods

        protected IEnumerable<SquareScript> GetFreeOrNearFreeTiles(SquareScript[,] map)
        {
            return map.ToEnumerable().Where(square => IsEmpty(square) ||
               (square.TraversingCondition == Traversability.Blocking &&
               square.GetNeighbours().Count(neighbour => neighbour.TraversingCondition == Traversability.Walkable) > 3));
        }

        protected IEnumerable<SquareScript> GetFreeTiles(SquareScript[,] map)
        {
            return map.ToEnumerable().Where(IsEmpty);
        }

        protected int FindDistanceFromPlayer(SquareScript square)
        {
            if (m_distanceFromPlayer == null)
            {
                InitializeDistanceFromPlayer();
            }
            return m_distanceFromPlayer.Get(square);
        }

        protected void PlaceLoot(SquareScript square, Loot loot)
        {
            // if it's a rock, replace it.
            if (square.TraversingCondition == Traversability.Blocking)
            {
                square.TerrainType = TerrainType.Rock_Crystal;
            }
            square.AddLoot(loot);
        }

        protected void InitializeDistanceFromPlayer()
        {
            m_distanceFromPlayer = new Dictionary<SquareScript, int>();
            m_distanceFromPlayer.Add(Entity.Player.Location, 0);

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
                if (m_distanceFromPlayer.ContainsKey(square))
                    continue;

                m_distanceFromPlayer.Add(square, m_distanceFromPlayer.Get(previousSquares.Get(square)) + 1);

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

        #endregion protected methods

        #region private methods

        private bool IsEmpty(SquareScript square)
        {
            return square.TraversingCondition == Traversability.Walkable && square.OccupyingEntity == null && square.DroppedLoot == null;
        }

        #endregion private methods
    }

    #endregion BasePopulator
}