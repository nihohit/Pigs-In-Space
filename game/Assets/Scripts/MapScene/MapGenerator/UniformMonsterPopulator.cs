using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region UniformCaveMonsterPopulator

    public class UniformMonsterPopulator : BasePopulator, IMonsterPopulator
    {
        public void PopulateMap(SquareScript[,] map, IEnumerable<MonsterTemplate> monsters)
        {
            monsters = monsters.Shuffle();
            var freeTiles = GetFreeTiles(map).Shuffle();
            Assert.Greater(freeTiles.Count(), monsters.Count(), "There should be more free tiles than monsters");
            var freeTilesEnumerator = freeTiles.GetEnumerator();

            foreach (var monster in monsters)
            {
                freeTilesEnumerator.MoveNext();
                PlaceEnemy(freeTilesEnumerator.Current, monster);
            }
        }
    }

    #endregion UniformCaveMonsterPopulator
}