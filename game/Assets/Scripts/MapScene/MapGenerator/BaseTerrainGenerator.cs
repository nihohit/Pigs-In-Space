using Assets.Scripts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region ITerrainGenerator

    public interface ITerrainGenerator
    {
        SquareScript[,] GenerateMap(int x, int y, int playerStartX, int playerStartY);
    }

    #endregion ITerrainGenerator

    #region BaseTerrainGenerator

    /// <summary>
    /// Base class for all terrain generators.
    /// </summary>
    public abstract class BaseTerrainGenerator : ITerrainGenerator
    {
        #region fields

        private readonly float c_minimumSizeToRegenerateMap = SimpleConfigurationHandler.GetFloatProperty("map re-generation bar", FileAccessor.TerrainGeneration);

        private readonly float c_requiredSizeOfMap = SimpleConfigurationHandler.GetFloatProperty("required map ratio", FileAccessor.TerrainGeneration);

        private readonly int c_emptySpaceAroundPlayer = SimpleConfigurationHandler.GetIntProperty("empty space around player", FileAccessor.TerrainGeneration);

        protected bool[,] m_boolMap;

        #endregion fields

        #region public methods

        public virtual SquareScript[,] GenerateMap(int width, int height, int playerStartX, int playerStartY)
        {
            int size = 0;
            int minimalSize = Convert.ToInt32(width * height * c_minimumSizeToRegenerateMap);
            while (size < minimalSize)
            {
                // get base map
                CreateInitialMap(width, height);
                Assert.NotNull(m_boolMap, "m_boolMap");
                Assert.AreEqual(m_boolMap.GetLength(0), width, "map isn't in the right width");
                Assert.AreEqual(m_boolMap.GetLength(1), height, "map isn't in the right height");

                // empty space around player
                for (int i = -c_emptySpaceAroundPlayer; i < c_emptySpaceAroundPlayer; i++)
                {
                    for (int j = -c_emptySpaceAroundPlayer; j < c_emptySpaceAroundPlayer; j++)
                    {
                        m_boolMap[i + playerStartX, j + playerStartY] = false;
                    }
                }

                // add walls around the level
                for (int i = 0; i < m_boolMap.GetLength(0); i++)
                {
                    m_boolMap[i, 0] = true;
                    m_boolMap[i, m_boolMap.GetLength(1) - 1] = true;
                }
                for (int i = 0; i < m_boolMap.GetLength(1); i++)
                {
                    m_boolMap[0, i] = true;
                    m_boolMap[m_boolMap.GetLength(0) - 1, i] = true;
                }

                // remove unreachable areas
                FindAndRemoveUnreachableSpaces(width, height, playerStartX, playerStartY);

                //verify the map is still large enough
                size = m_boolMap.ToEnumerable().Count(cell => !cell);
            }

            // add additional space until map is spacious enough
            var expectedAmount = Convert.ToInt32(width * height * c_requiredSizeOfMap);
            FillMapToRequiredSize(expectedAmount);
            Assert.EqualOrGreater(m_boolMap.ToEnumerable().Count(cell => !cell), expectedAmount, "map hasn't reached required size");

            // add walls around the level, to ensure they're there
            for (int i = 0; i < m_boolMap.GetLength(0); i++)
            {
                m_boolMap[i, 0] = true;
                m_boolMap[i, m_boolMap.GetLength(1) - 1] = true;
            }
            for (int i = 0; i < m_boolMap.GetLength(1); i++)
            {
                m_boolMap[0, i] = true;
                m_boolMap[m_boolMap.GetLength(0) - 1, i] = true;
            }

            // create squarescript map
            return CreateSquareScriptMap(width, height);
        }

        #endregion public methods

        #region abstract methods

        /// <summary>
        /// increase the free space in the map until it reaches the required amount of free squares
        /// </summary>
        /// <param name="requiredAmountOfFreeSquares"></param>
        protected abstract void FillMapToRequiredSize(int requiredAmountOfFreeSquares);

        /// <summary>
        /// Create a new square, in the relevant style
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        protected abstract SquareScript CreateSquare(int x, int y, Vector3 location);

        /// <summary>
        /// Create a base map, before adjustments.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        protected abstract void CreateInitialMap(int x, int y);

        #endregion abstract methods

        #region private methods

        /// <summary>
        /// Using the Flood Fill algorithm, this method turns all squares that can't be reached from
        /// the player's starting position into walls.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="playerStartX"></param>
        /// <param name="playerStartY"></param>
        private void FindAndRemoveUnreachableSpaces(int width, int height, int playerStartX, int playerStartY)
        {
            var visitMap = new bool[width, height];

            Queue<Vector2> queue = new Queue<Vector2>();
            queue.Enqueue(new Vector2(playerStartX, playerStartY));

            while (queue.Any())
            {
                var point = queue.Dequeue();

                var x = (int)point.x;
                var y = (int)point.y;

                if (visitMap[x, y] || m_boolMap[x, y])
                    continue;

                visitMap[x, y] = true;

                EnqueueIfMatches(visitMap, queue, x - 1, y);
                EnqueueIfMatches(visitMap, queue, x + 1, y);
                EnqueueIfMatches(visitMap, queue, x, y - 1);
                EnqueueIfMatches(visitMap, queue, x, y + 1);
            }

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    m_boolMap[i, j] = m_boolMap[i, j] || !visitMap[i, j];
                }
            }
        }

        private void EnqueueIfMatches(bool[,] visitMap, Queue<Vector2> queue, int x, int y)
        {
            if (x >= 0 &&
                x < visitMap.GetLength(0) &&
                y >= 0 &&
                y < visitMap.GetLength(1) &&
                !m_boolMap[x, y] &&
                !visitMap[x, y])
            {
                queue.Enqueue(new Vector2(x, y));
            }
        }

        /// <summary>
        /// converts the bool map into a square script array.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private SquareScript[,] CreateSquareScriptMap(int width, int height)
        {
            var squares = new SquareScript[width, height];

            var squareSize = SquareScript.PixelsPerSquare * MapSceneScript.UnitsToPixelsRatio; // 1f
            var currentPosition = Vector3.zero;

            for (int j = height - 1; j >= 0; j--) // invert y axis
            {
                for (int i = 0; i < width; i++)
                {
                    squares[i, j] = CreateSquare(i, j, currentPosition);

                    currentPosition = new Vector3(currentPosition.x + squareSize, currentPosition.y, 0);
                }

                currentPosition = new Vector3(0, currentPosition.y + squareSize, 0);
            }

            return squares;
        }

        #endregion private methods
    }

    #endregion BaseTerrainGenerator
}