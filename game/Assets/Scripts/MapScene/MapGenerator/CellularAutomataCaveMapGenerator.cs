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

        protected override void CreateInitialMap(int x, int y)
        {
            GenerateArray(x, y);

            IterateOnArray(x, y);
        }

        private void GenerateArray(int x, int y)
        {
            m_boolMap = new bool[x, y];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    m_boolMap[i, j] = c_initialChanceOfWall.ProbabilityCheck();
                }
            }

            for (int i = 0; i < x; i++)
            {
                m_boolMap[i, 0] = true;
                m_boolMap[i, y - 1] = true;
            }

            for (int i = 0; i < y; i++)
            {
                m_boolMap[0, i] = true;
                m_boolMap[x - 1, i] = true;
            }
        }

        private void IterateOnArray(int x, int y)
        {
            var changed = true;
            var decisionMatrix = new bool[x, y];
            int counter = 1;
            while (changed && counter < 100)
            {
                changed = false;
                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        decisionMatrix[i, j] =
                            ((m_boolMap[i, j] && Survive(i, j)) || //either the cell is alive & survives,
                            (!m_boolMap[i, j] && Born(i, j))); // or it's dead and born
                    }
                }

                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        if (m_boolMap[i, j] != decisionMatrix[i, j])
                        {
                            changed = true;
                            m_boolMap[i, j] = decisionMatrix[i, j];
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
                        (newX < m_boolMap.GetLength(0)) &&
                        (newY >= 0) &&
                        (newY < m_boolMap.GetLength(1)) &&
                        m_boolMap[newX, newY])
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