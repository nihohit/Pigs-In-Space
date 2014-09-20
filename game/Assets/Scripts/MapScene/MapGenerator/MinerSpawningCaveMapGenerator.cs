using Assets.Scripts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region MinerSpawningCaveMapGenerator

    // implements random walk algorithm with generation of additional random walkers
    public class MinerSpawningCaveMapGenerator : BaseTerrainGenerator
    {
        private int m_expectedFreeSpace, m_currentFreeSpace;

        private const double c_requiredSizeOfMap = 0.6;

        private List<Vector2> m_currentMiners = new List<Vector2>();

        private const double c_chanceToCreateNewMiner = 0.1;

        public void MineTheMap(bool[,] boolMap, int expectedFreeSpace)
        {
            m_boolMap = boolMap;
            m_expectedFreeSpace = expectedFreeSpace;
            foreach (var cell in boolMap)
            {
                if (!cell)
                {
                    m_currentFreeSpace++;
                }
            }

            m_currentMiners.Add(FindRandomEmptySpot());

            MineTheMap();
        }

        private Vector2 FindRandomEmptySpot()
        {
            var xRange = Enumerable.Range(0, m_boolMap.GetLength(0)).Shuffle();
            var yRange = Enumerable.Range(0, m_boolMap.GetLength(1)).Shuffle();

            foreach (var x in xRange)
            {
                foreach (var y in yRange)
                {
                    if (!m_boolMap[x, y])
                    {
                        return new Vector2(x, y);
                    }
                }
            }

            throw new UnreachableCodeException("Shouldn't call the random walk generator on an empty map");
        }

        protected override void CreateInitialMap(int x, int y)
        {
            CreateBoolMap(x, y);
            m_expectedFreeSpace = Convert.ToInt32(x * y * c_requiredSizeOfMap);
            m_currentMiners.Add(new Vector2(x / 2, y / 2));
            MineTheMap();
        }

        private void MineTheMap()
        {
            int minerIndex = 0;

            while (m_expectedFreeSpace > m_currentFreeSpace)
            {
                // choose the next miner and find its neighbouring walls
                minerIndex++;
                if (minerIndex >= m_currentMiners.Count)
                {
                    minerIndex = 0;
                }
                var miner = m_currentMiners.ElementAt(minerIndex);
                m_currentMiners.RemoveAt(minerIndex);
                var wallNeighbours = NeighbouringWalls(miner);

                if (wallNeighbours.Count() == 0)
                {
                    // if there are no miners, create a random one
                    if (m_currentMiners.Count() == 0)
                    {
                        m_currentMiners.Add(NeighbouringCells(miner, true).ChooseRandomValue());
                    }
                    else
                    {
                        // in order not to skip a miner
                        minerIndex--;
                    }
                }
                else
                {
                    var newMiner = wallNeighbours.ChooseRandomValue();
                    m_currentMiners.Insert(minerIndex, newMiner);
                    m_boolMap[(int)newMiner.x, (int)newMiner.y] = false;
                    m_currentFreeSpace++;
                    if (c_chanceToCreateNewMiner.ProbabilityCheck())
                    {
                        m_currentMiners.Insert(minerIndex + 1, NeighbouringCells(newMiner, true).ChooseRandomValue());
                    }
                }
            }
        }

        private void CreateBoolMap(int x, int y)
        {
            m_boolMap = new bool[x, y];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    m_boolMap[i, j] = true;
                }
            }
        }

        private IEnumerable<Vector2> NeighbouringWalls(Vector2 current)
        {
            return NeighbouringCells(current, false);
        }

        private IEnumerable<Vector2> NeighbouringCells(Vector2 current, bool allowEmptySpace)
        {
            for (int i = -1; i < 2; i += 2)
            {
                var newX = (int)current.x + i;
                if (newX >= 0 &&
                    newX < m_boolMap.GetLength(0) &&
                    (m_boolMap[newX, (int)current.y] || allowEmptySpace))
                {
                    yield return new Vector2(newX, current.y);
                }

                var newY = (int)current.x + i;
                if (newY >= 0 &&
                    newY < m_boolMap.GetLength(1) &&
                    (m_boolMap[(int)current.x, newY] || allowEmptySpace))
                {
                    yield return new Vector2(current.x, newY);
                }
            }
        }
    }

    #endregion MinerSpawningCaveMapGenerator
}