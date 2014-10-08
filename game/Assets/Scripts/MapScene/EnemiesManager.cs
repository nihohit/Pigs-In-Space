using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.MapScene
{
    // contains and activates all enemy entities
    public static class EnemiesManager
    {
        #region fields

        private static readonly List<EnemyEntity> s_activeEntities = new List<EnemyEntity>();
        private static readonly Dictionary<string, int> s_deadMonsters = new Dictionary<string, int>();

        #endregion fields

        #region properties

        public static int KilledTentacles { get { return s_deadMonsters.TryGetOrDefaultValue("TentacleMonster", 0); } }

        public static int KilledHives { get { return s_deadMonsters.TryGetOrDefaultValue("Hive", 0); } }

        public static int KilledSlimes { get { return s_deadMonsters.TryGetOrDefaultValue("Slime", 0); } }

        #endregion properties

        #region public methods

        public static void Clear()
        {
            s_activeEntities.Clear();
            s_deadMonsters.Clear();
        }

        public static void AddEnemy(EnemyEntity enemy)
        {
            if (!s_deadMonsters.ContainsKey(enemy.TypeOfEntity))
            {
                s_deadMonsters[enemy.TypeOfEntity] = 0;
            }
            s_activeEntities.Add(enemy);
        }

        // return an enumerator of all monsters' actions
        public static IEnumerator EnemiesTurn()
        {
            // activate only active entities
            var activeEntities = s_activeEntities.Where(ent => ent.Active).ToList();

            // find how much time to wait after each action
            var timePerMonster = activeEntities.TimePerItem(0.15f, 0.01f);

            // enumerate the actions
            IEnumerator enumerator = new EmptyEnumerator();
            return activeEntities.Aggregate(enumerator, (current, enemy) => enemy.Act(timePerMonster).Join(current));
        }

        internal static void Remove(EnemyEntity enemy)
        {
            s_deadMonsters[enemy.TypeOfEntity]++;
            s_activeEntities.Remove(enemy);
        }

        public static EnemyEntity CreateEnemy(MonsterTemplate template, SquareScript square)
        {
            var monster = new EnemyEntity(template, square);
            AddEnemy(monster);
            return monster;
        }

        #endregion public methods
    }
}