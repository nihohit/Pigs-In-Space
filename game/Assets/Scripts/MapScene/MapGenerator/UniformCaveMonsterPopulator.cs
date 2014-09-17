using Assets.Scripts.LogicBase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region UniformCaveMonsterPopulator

    public class UniformCaveMonsterPopulator : IMonsterPopulator
    {
        private IEnumerable<MonsterTemplate> m_templates;

        public UniformCaveMonsterPopulator(IEnumerable<MonsterTemplate> enemyTemplates)
        {
            m_templates = enemyTemplates;
        }

        public void PopulateMap(SquareScript[,] map)
        {
            //throw new NotImplementedException();
        }
    }

    #endregion UniformCaveMonsterPopulator
}