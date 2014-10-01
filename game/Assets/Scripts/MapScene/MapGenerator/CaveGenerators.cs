using Assets.Scripts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region BaseTerrainGenerator

    public abstract class BaseCaveGenerator : BaseTerrainGenerator
    {
        protected override void FillMapToRequiredSize(int expectedAmount)
        {
            var mapFiller = new MinerSpawningCaveMapGenerator();
            mapFiller.MineTheMap(m_boolMap, expectedAmount);
        }

        protected override SquareScript CreateSquare(int x, int y, Vector3 location)
        {
            var tile = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("SquareTileResource"), location, Quaternion.identity));
            var script = tile.GetComponent<SquareScript>();
            script.setLocation(x, y);

            script.TerrainType = m_boolMap[x, y] ? GetTerrainType(x, y) : TerrainType.Empty;

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
    }

    #endregion BaseTerrainGenerator

    #region MinerSpawningCaveMapGenerator

    /// <summary>
    /// implements Random Walk algorithm with random generation of additional random walkers
    /// </summary>
    public class MinerSpawningCaveMapGenerator : BaseCaveGenerator
    {
        #region fields

        private int m_expectedFreeSpace, m_currentFreeSpace;

        private readonly float c_requiredSizeOfMap = SimpleConfigurationHandler.GetFloatProperty("required map ratio", FileAccessor.TerrainGeneration);

        private readonly List<Vector2> m_currentMiners = new List<Vector2>();

        private readonly double c_chanceToCreateNewMiner = SimpleConfigurationHandler.GetFloatProperty("chance to spawn random walker", FileAccessor.TerrainGeneration);

        #endregion fields

        /// <summary>
        /// This method is called when a given map needs to be expanded.
        /// </summary>
        /// <param name="boolMap"></param>
        /// <param name="expectedFreeSpace"></param>
        public void MineTheMap(bool[,] boolMap, int expectedFreeSpace)
        {
            m_boolMap = boolMap;
            m_expectedFreeSpace = expectedFreeSpace;
            m_currentFreeSpace = boolMap.ToEnumerable().Count(cell => !cell);

            m_currentMiners.Add(FindRandomEmptySpot());

            MineTheMap();
        }

        /// <summary>
        /// shuffles all the squares in the map into a random order,
        /// and will return the coordiantes of the first empty square it finds.
        /// </summary>
        /// <returns></returns>
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
                // choose the next miner, remove it from the list and find its neighbouring walls
                minerIndex++;
                if (minerIndex >= m_currentMiners.Count)
                {
                    minerIndex = 0;
                }
                var miner = m_currentMiners.ElementAt(minerIndex);
                m_currentMiners.RemoveAt(minerIndex);
                var wallNeighbours = NeighbouringWalls(miner);

                if (!wallNeighbours.Any())
                {
                    // if there are no miners, create a random one
                    if (!m_currentMiners.Any())
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
                    // create a new walker on a random neighbour
                    var newMiner = wallNeighbours.ChooseRandomValue();
                    m_currentMiners.Insert(minerIndex, newMiner);
                    m_boolMap[(int)newMiner.x, (int)newMiner.y] = false;
                    m_currentFreeSpace++;

                    // randomly create new walkers
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
                var newY = (int)current.x + i;
                if (newY >= 0 &&
                    newY < m_boolMap.GetLength(1) &&
                    (m_boolMap[(int)current.x, newY] || allowEmptySpace))
                {
                    yield return new Vector2(current.x, newY);
                }

                var newX = (int)current.x + i;
                if (newX >= 0 &&
                    newX < m_boolMap.GetLength(0) &&
                    (m_boolMap[newX, (int)current.y] || allowEmptySpace))
                {
                    yield return new Vector2(newX, current.y);
                }
            }
        }
    }

    #endregion MinerSpawningCaveMapGenerator

    #region CellularAutomataCaveMapGenerator

    /// <summary>
    /// using the B678_S345678 cellular automata
    /// </summary>
    public class CellularAutomataCaveMapGenerator : BaseCaveGenerator
    {
        private readonly double c_initialChanceOfWall = SimpleConfigurationHandler.GetFloatProperty("initial chance of wall", FileAccessor.TerrainGeneration);

        protected override void CreateInitialMap(int x, int y)
        {
            GenerateArray(x, y);

            IterateOnArray(x, y);
        }

        private void GenerateArray(int x, int y)
        {
            m_boolMap = new bool[x, y];
            // generate random values
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    m_boolMap[i, j] = c_initialChanceOfWall.ProbabilityCheck();
                }
            }
        }

        /// <summary>
        /// Iterate the automata, until it reaches a stable condition, or until 100 iterations are over
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
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

        //B678
        private bool Born(int x, int y)
        {
            return m_boolMap.GetNeighbours(x, y, true).Count() > 5;
        }

        //S345678
        private bool Survive(int x, int y)
        {
            return m_boolMap.GetNeighbours(x, y, true).Count() > 2;
        }
    }

    #endregion CellularAutomataCaveMapGenerator

    #region PerlinNoiseCaveMapGenerator

    /// <summary>
    /// Uses Unity's Perlin Noise function to generate a map
    /// </summary>
    public class PerlinNoiseCaveMapGenerator : BaseCaveGenerator
    {
        #region fields

        private readonly float c_leapSize = SimpleConfigurationHandler.GetFloatProperty("perlin noise leap size", FileAccessor.TerrainGeneration);
        private readonly float c_randomRange = SimpleConfigurationHandler.GetFloatProperty("noise random range", FileAccessor.TerrainGeneration);
        private readonly float c_wallThreshold = SimpleConfigurationHandler.GetFloatProperty("perlin noise wall threshold", FileAccessor.TerrainGeneration);

        private float m_xSeed, m_ySeed;

        #endregion fields

        protected override void CreateInitialMap(int x, int y)
        {
            m_xSeed = UnityEngine.Random.Range(0, c_randomRange);
            m_ySeed = UnityEngine.Random.Range(0, c_randomRange);
            m_boolMap = new bool[x, y];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    m_boolMap[i, j] = WallOnSpot(i, j);
                }
            }
        }

        /// <summary>
        /// checks if the noise in that spot is above the wall threshold
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool WallOnSpot(int x, int y)
        {
            var xUpdate = m_xSeed + x * c_leapSize;
            var yUpdate = m_ySeed + y * c_leapSize;
            var perlinNoise = Mathf.PerlinNoise(xUpdate, yUpdate);
            return perlinNoise > c_wallThreshold;
        }
    }

    #endregion PerlinNoiseCaveMapGenerator
}