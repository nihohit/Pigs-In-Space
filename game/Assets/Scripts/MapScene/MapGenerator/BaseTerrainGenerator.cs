using Assets.Scripts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region BaseTerrainGenerator

    public abstract class BaseTerrainGenerator : ITerrainGenerator
    {
        private const double c_minimumSizeToRegenerateMap = 0.4;

        private const double c_requiredSizeOfMap = 0.6;

        private const int c_emptySpaceAroundPlayer = 3;

        protected bool[,] m_boolMap;

        protected abstract void CreateInitialMap(int x, int y);

        public virtual SquareScript[,] GenerateMap(int width, int height, int playerStartX, int playerStartY)
        {
            int size = 0;
            int minimalSize = Convert.ToInt32(width * height * c_minimumSizeToRegenerateMap);
            while (size < minimalSize)
            {
                // get base map
                CreateInitialMap(width, height);

                // empty space around player
                for (int i = -c_emptySpaceAroundPlayer; i < c_emptySpaceAroundPlayer; i++)
                {
                    for (int j = -c_emptySpaceAroundPlayer; j < c_emptySpaceAroundPlayer; j++)
                    {
                        m_boolMap[i + playerStartX, j + playerStartY] = false;
                    }
                }

                Debug.Log("initial map: {0} squares".FormatWith(m_boolMap.ToEnumerable().Count(cell => !cell)));
                // remove unreachable areas
                FindAndRemoveUnreachableSpaces(width, height, playerStartX, playerStartY);

                //verify the map is still large enough
                size = m_boolMap.ToEnumerable().Count(cell => !cell);
                Debug.Log("after removing unreachable squares: {0} squares".FormatWith(m_boolMap.ToEnumerable().Count(cell => !cell)));
            }

            // add additional space until map is spacious enough
            var mapFiller = new MinerSpawningCaveMapGenerator();
            var expectedAmount = Convert.ToInt32(width * height * c_requiredSizeOfMap);
            Debug.Log("expected amount {0}".FormatWith(expectedAmount));
            mapFiller.MineTheMap(m_boolMap, expectedAmount);
            Debug.Log("after filling out the map: {0} squares".FormatWith(m_boolMap.ToEnumerable().Count(cell => !cell)));

            // create squarescript map
            return CreateSquareScriptMap(width, height);
        }

        #region private methods

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

        private SquareScript[,] CreateSquareScriptMap(int width, int height)
        {
            var squares = new SquareScript[width, height];

            var squareSize = SquareScript.PixelsPerSquare * MapSceneScript.UnitsToPixelsRatio; // 1f
            var currentPosition = Vector3.zero;

            for (int j = height - 1; j >= 0; j--) // invert y axis
            {
                for (int i = 0; i < width; i++)
                {
                    squares[i, j] = createSquare(i, j, currentPosition);

                    currentPosition = new Vector3(currentPosition.x + squareSize, currentPosition.y, 0);
                }

                currentPosition = new Vector3(0, currentPosition.y + squareSize, 0);
            }

            return squares;
        }

        private SquareScript createSquare(int x, int y, Vector3 location)
        {
            var tile = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("SquareTileResource"), location, Quaternion.identity));
            var script = tile.GetComponent<SquareScript>();
            script.setLocation(x, y);

            if (m_boolMap[x, y])
            {
                script.TerrainType = GetTerrainType(x, y);
            }
            else
            {
                script.TerrainType = TerrainType.Empty;
            }

            return script;
        }

        private TerrainType GetTerrainType(int x, int y)
        {
            int newX = x - 1, newY = y;
            bool left = (newX < 0 ||
                m_boolMap[newX, y]);

            newX = x + 1;
            bool right = (newX >= m_boolMap.GetLength(0) ||
                m_boolMap[newX, y]);

            newY = y - 1;
            bool up = (newY < 0 ||
                m_boolMap[x, newY]);

            newY = y + 1;
            bool down = (newY >= m_boolMap.GetLength(1) ||
                m_boolMap[x, newY]);

            if (up &&
                left &&
                !down &&
                !right)
            {
                return TerrainType.Rock_Top_Left_Corner;
            }

            if (up &&
                !left &&
                !down &&
                right)
            {
                return TerrainType.Rock_Top_Right_Corner;
            }

            if (!up &&
                left &&
                down &&
                !right)
            {
                return TerrainType.Rock_Bottom_Left_Corner;
            }

            if (!up &&
                !left &&
                down &&
                right)
            {
                return TerrainType.Rock_Bottom_Right_Corner;
            }

            if (!up &&
                left &&
                !down &&
                !right)
            {
                return TerrainType.Rock_Side_Left;
            }

            if (up &&
                !left &&
                !down &&
                !right)
            {
                return TerrainType.Rock_Side_Top;
            }

            if (!up &&
                !left &&
                down &&
                !right)
            {
                return TerrainType.Rock_Side_Bottom;
            }

            if (!up &&
                !left &&
                !down &&
                right)
            {
                return TerrainType.Rock_Side_Right;
            }

            return TerrainType.Rock_Full;
        }

        #endregion private methods
    }

    #endregion BaseTerrainGenerator
}