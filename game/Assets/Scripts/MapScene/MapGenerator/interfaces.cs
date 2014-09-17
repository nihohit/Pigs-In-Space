using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region IMonsterPopulator

    public interface IMonsterPopulator
    {
        void PopulateMap(SquareScript[,] map);
    }

    #endregion IMonsterPopulator

    #region ITerrainGenerator

    public interface ITerrainGenerator
    {
        SquareScript[,] GenerateMap(int x, int y);
    }

    #endregion ITerrainGenerator
}