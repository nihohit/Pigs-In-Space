using Assets.Scripts.Base;
using UnityEngine;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region PerlinNoiseCaveMapGenerator

    public class PerlinNoiseCaveMapGenerator : BaseTerrainGenerator
    {
        #region fields

        private const float c_leapSize = 0.213f;
        private const float c_randomRange = 100000f;
        private const float c_wallThreshold = 0.51f;

        private float m_xSeed, m_ySeed;

        #endregion fields

        public PerlinNoiseCaveMapGenerator()
        {
            m_xSeed = UnityEngine.Random.Range(0, c_randomRange);
            m_ySeed = UnityEngine.Random.Range(0, c_randomRange);
        }

        protected override bool WallOnSpot(int x, int y)
        {
            var xUpdate = m_xSeed + x * c_leapSize;
            var yUpdate = m_ySeed + y * c_leapSize;
            var perlinNoise = Mathf.PerlinNoise(xUpdate, yUpdate);
            return perlinNoise > c_wallThreshold;
        }
    }

    #endregion PerlinNoiseCaveMapGenerator
}