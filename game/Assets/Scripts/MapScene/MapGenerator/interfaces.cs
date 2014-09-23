using Assets.Scripts.LogicBase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region IMonsterPopulator

    public interface IMonsterPopulator
    {
        void PopulateMap(SquareScript[,] map, IEnumerable<MonsterTemplate> monsters);
    }

    #endregion IMonsterPopulator

    #region ITreasurePopulator

    public interface ITreasurePopulator
    {
        void PopulateMap(SquareScript[,] map, IEnumerable<Loot> loot);
    }

    #endregion ITreasurePopulator

    #region ITerrainGenerator

    public interface ITerrainGenerator
    {
        SquareScript[,] GenerateMap(int x, int y, int playerStartX, int playerStartY);
    }

    #endregion ITerrainGenerator
}