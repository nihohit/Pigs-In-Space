using Assets.Scripts.LogicBase;
using System.Collections.Generic;

namespace Assets.Scripts.MapScene.MapGenerator
{
    #region IMonsterPopulator

    public interface IMonsterPopulator
    {
        void PopulateMap(SquareScript[,] map, IEnumerable<MonsterTemplate> monsters);
    }

    #endregion IMonsterPopulator

    #region MonsterPopulator

    public abstract class MonsterPopulator : BasePopulator<MonsterTemplate>, IMonsterPopulator
    {
        protected abstract PopulationWeighting ChosenWeights { get; }

        public override void PopulateMap(SquareScript[,] map, IEnumerable<MonsterTemplate> monsters)
        {
            PlaceBy(map, monsters);
        }

        protected void PlaceBy(SquareScript[,] map, IEnumerable<MonsterTemplate> items)
        {
            PlaceByConstraint(map, items, ChosenWeights, false);
        }

        protected override void Place(SquareScript square, MonsterTemplate ent)
        {
            EnemiesManager.CreateEnemy(ent, square);
        }
    }

    #endregion MonsterPopulator

    #region UniformCaveMonsterPopulator

    public class UniformMonsterPopulator : MonsterPopulator
    {
        protected override PopulationWeighting ChosenWeights
        {
            get { return PopulationWeighting.None; }
        }
    }

    #endregion UniformCaveMonsterPopulator

    #region DistanceBasedMonsterPopulator

    public class DistanceBasedMonsterPopulator : MonsterPopulator
    {
        protected override PopulationWeighting ChosenWeights
        {
            get { return PopulationWeighting.DistanceFromPlayer; }
        }
    }

    #endregion DistanceBasedMonsterPopulator
}