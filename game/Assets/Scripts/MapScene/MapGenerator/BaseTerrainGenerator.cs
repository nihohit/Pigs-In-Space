using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region BaseTerrainGenerator

    public abstract class BaseTerrainGenerator : ITerrainGenerator
    {
        protected abstract bool WallOnSpot(int x, int y);

        public virtual SquareScript[,] GenerateMap(int width, int height)
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

            if (WallOnSpot(x, y))
            {
                script.TerrainType = TerrainType.Rock_Full;
            }
            else
            {
                script.TerrainType = TerrainType.Empty;
            }

            return script;
        }
    }

    #endregion BaseTerrainGenerator
}