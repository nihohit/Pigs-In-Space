using Assets.Scripts.Base;

namespace Assets.Scripts.LogicBase
{
    #region MonsterTemplate

    public class MonsterTemplate : EntityTemplate, IIdentifiable
    {
        #region properties

        public EntityTactics Tactics { get; set; }

        public string ActionItem { get; set; }

        public string DestructionItem { get; set; }

        #endregion properties

        #region constructor

        public MonsterTemplate(string name, double health, MovementType movementType,
            EntityTactics tactics, string actionItem, string destructionItem)
            : base(name, health, movementType)
        {
            Assert.NotNullOrEmpty(actionItem, "{0} action item".FormatWith(name));
            Tactics = tactics;
            ActionItem = actionItem;
            DestructionItem = destructionItem;
        }

        #endregion constructor
    }

    #endregion MonsterTemplate

    #region MonsterTemplateStorage

    public sealed class MonsterTemplateStorage : ConfigurationStorage<MonsterTemplate, MonsterTemplateStorage>
    {
        public MonsterTemplateStorage()
            : base("monsters")
        { }

        protected override JSONParser<MonsterTemplate> GetParser()
        {
            return new MonsterJSONParser();
        }

        private sealed class MonsterJSONParser : JSONParser<MonsterTemplate>
        {
            protected override MonsterTemplate ConvertCurrentItemToObject()
            {
                return new MonsterTemplate(
                    TryGetValueAndFail<string>("Name"),
                    TryGetValueAndFail<float>("Health"),
                    TryGetValueOrSetDefaultValue<MovementType>("MovementType", MovementType.Walking),
                    TryGetValueOrSetDefaultValue<EntityTactics>("Tactics", EntityTactics.ActInRange),
                    TryGetValueAndFail<string>("ActionItem"),
                    TryGetValueOrSetDefaultValue<string>("DestructionItem", null));
            }
        }
    }

    #endregion MonsterTemplateStorage
}