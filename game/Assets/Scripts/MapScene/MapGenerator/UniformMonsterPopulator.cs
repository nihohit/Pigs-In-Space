using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region UniformCaveMonsterPopulator

    public class UniformMonsterPopulator : IMonsterPopulator
    {
        private IEnumerable<MonsterTemplate> m_templates;

        public UniformMonsterPopulator(IEnumerable<MonsterTemplate> enemyTemplates)
        {
            m_templates = enemyTemplates;
        }

        public void PopulateMap(SquareScript[,] map, IEnumerable<MonsterTemplate> monsters)
        {
            monsters = monsters.Shuffle();
            var freeTiles = GetFreeTiles(map).Shuffle();
            Assert.Greater(freeTiles.Count(), monsters.Count(), "There should be more free tiles than monsters");
            var freeTilesEnumerator = freeTiles.GetEnumerator();

            foreach (var monster in monsters)
            {
                freeTilesEnumerator.MoveNext();
                EnemiesManager.CreateEnemy(monster, freeTilesEnumerator.Current);
            }
        }

        private IEnumerable<SquareScript> GetFreeTiles(SquareScript[,] map)
        {
            return map.ToEnumerable().Where(square => square.TraversingCondition == Traversability.Walkable);
        }
    }

    #endregion UniformCaveMonsterPopulator
}