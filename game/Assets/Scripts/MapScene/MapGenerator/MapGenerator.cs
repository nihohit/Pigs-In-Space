using Assets.Scripts.Base;
using UnityEngine;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region CaveMapGenerator

    public class CaveMapGenerator
    {
        #region fields

        private const float c_leapSize = 0.213f;
        private const float c_randomRange = 100000f;
        private const float c_wallThreshold = 0.51f;

        #endregion fields

        #region public methods

        public SquareScript[,] GenerateMap(int x, int y)
        {
            var xSeed = UnityEngine.Random.Range(0, c_randomRange);
            var ySeed = UnityEngine.Random.Range(0, c_randomRange);
            var squares = new SquareScript[x, y];
            var squareSize = SquareScript.PixelsPerSquare * MapSceneScript.UnitsToPixelsRatio; // 1f
            var currentPosition = Vector3.zero;

            for (int j = y - 1; j >= 0; j--) // invert y axis
            {
                for (int i = 0; i < x; i++)
                {
                    var xUpdate = xSeed + i * c_leapSize;
                    var yUpdate = ySeed + j * c_leapSize;
                    var perlinNoise = Mathf.PerlinNoise(xUpdate, yUpdate);
                    Debug.Log("{0},{1}:{2}".FormatWith(xUpdate, yUpdate, perlinNoise));
                    squares[i, j] = createSquare(perlinNoise, i, j, currentPosition);
                    currentPosition = new Vector3(currentPosition.x + squareSize, currentPosition.y, 0);
                }
                currentPosition = new Vector3(0, currentPosition.y + squareSize, 0);
            }

            for (int i = 1; i < 10; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    squares[i, j].TerrainType = TerrainType.Empty;
                }
            }

            return squares;
        }

        private SquareScript createSquare(float perlinNoise, int x, int y, Vector3 location)
        {
            var tile = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("SquareTileResource"), location, Quaternion.identity));
            var script = tile.GetComponent<SquareScript>();
            script.setLocation(x, y);

            if (perlinNoise > c_wallThreshold)
            {
                script.TerrainType = TerrainType.Rock_Full;
            }
            else
            {
                script.TerrainType = TerrainType.Empty;
            }

            return script;
        }

        #endregion public methods
    }

    #endregion CaveMapGenerator
}