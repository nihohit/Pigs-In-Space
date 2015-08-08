using Assets.Scripts.LogicBase;

namespace Assets.Scripts.IntersceneCommunication
{
    #region EndLevelInfo

    public class EndLevelInfo
    {
        #region properties

        public Loot GainedLoot { get; private set; }

        #endregion properties

        public EndLevelInfo(Loot gainedLoot)
        {
            GainedLoot = gainedLoot;
        }
    }

    #endregion EndLevelInfo
}