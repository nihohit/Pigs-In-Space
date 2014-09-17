using Assets.Scripts.Base;
using Assets.Scripts.MapScene;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region CellularAutomataCaveMapGenerator

    /// <summary>
    /// using the B678_S345678 cellular automata
    /// </summary>
    public class CellularAutomataCaveMapGenerator : BaseTerrainGenerator
    {
        private const double c_initialChanceOfWall = 0.45;

        private bool[,] m_cellularMap;

        protected override bool WallOnSpot(int x, int y)
        {
            return m_cellularMap[x, y];
        }

        public override SquareScript[,] GenerateMap(int x, int y)
        {
            GenerateArray(x, y);

            IterateOnArray(x, y);

            return base.GenerateMap(x, y);
        }

        private void GenerateArray(int x, int y)
        {
            m_cellularMap = new bool[x, y];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    m_cellularMap[i, j] = Randomiser.ProbabilityCheck(c_initialChanceOfWall);
                }
            }

            for (int i = 0; i < x; i++)
            {
                m_cellularMap[i, 0] = true;
                m_cellularMap[i, y - 1] = true;
            }

            for (int i = 0; i < y; i++)
            {
                m_cellularMap[0, i] = true;
                m_cellularMap[x - 1, i] = true;
            }
        }

        private void IterateOnArray(int x, int y)
        {
            var changed = true;
            var decisionMatrix = new bool[x, y];
            int counter = 1;
            while (changed && counter < 100)
            {
                Debug.Log("Iteration {0}".FormatWith(counter++));
                changed = false;
                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        decisionMatrix[i, j] =
                            ((m_cellularMap[i, j] && Survive(i, j)) || //either the cell is alive & survives,
                            (!m_cellularMap[i, j] && Born(i, j))); // or it's dead and born
                    }
                }

                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        if (m_cellularMap[i, j] != decisionMatrix[i, j])
                        {
                            changed = true;
                            m_cellularMap[i, j] = decisionMatrix[i, j];
                        }
                    }
                }
            }
        }

        //B23
        private bool Born(int x, int y)
        {
            var neighbours = CountNeighbours(x, y);
            return neighbours > 5;
        }

        //S345678
        private bool Survive(int x, int y)
        {
            var neighbours = CountNeighbours(x, y);
            return neighbours > 2;
        }

        private int CountNeighbours(int x, int y)
        {
            int counter = 0;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    var newX = x + i;
                    var newY = y + j;
                    if ((i != 0 || j != 0) &&
                        (newX >= 0) &&
                        (newX < m_cellularMap.GetLength(0)) &&
                        (newY >= 0) &&
                        (newY < m_cellularMap.GetLength(1)) &&
                        m_cellularMap[newX, newY])
                    {
                        counter++;
                    }
                }
            }
            return counter;
        }
    }

    #endregion CellularAutomataCaveMapGenerator
}