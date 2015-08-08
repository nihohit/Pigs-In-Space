namespace Assets.Scripts.IntersceneCommunication
{
    using Assets.Scripts.Base;
    using Assets.Scripts.Base.JsonParsing;
    using Assets.Scripts.LogicBase;

    #region Configurations

    public class Configurations
    {
        #region fields

        private ConfigurationStorage<MonsterTemplate> m_monsters;
        private ConfigurationStorage<PlayerEquipment> m_equipment;
        private ConfigurationStorage<ActionableItem> m_monsterEquipment;

        #endregion fields  

        #region properties

        public ConfigurationStorage<MonsterTemplate> Monsters
        {
            get
            {
                if (m_monsters == null)
                {
                    m_monsters = new ConfigurationStorage<MonsterTemplate>("monsters");
                }

                return m_monsters;
            }
        }

        public ConfigurationStorage<PlayerEquipment> Equipment
        {
            get
            {
                if (m_equipment == null)
                {
                    m_equipment = new ConfigurationStorage<PlayerEquipment>("equipment");
                }

                return m_equipment;
            }
        }

        public ConfigurationStorage<ActionableItem> MonsterEquipment
        {
            get
            {
                if (m_monsterEquipment == null)
                {
                    m_monsterEquipment = new ConfigurationStorage<ActionableItem>("monsterItems");
                }

                return m_monsterEquipment;
            }
        }

        #endregion properties
    }

    #endregion Configurations
}