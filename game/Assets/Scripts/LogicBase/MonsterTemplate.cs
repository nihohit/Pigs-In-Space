using Assets.Scripts.Base;
using System.Collections.Generic;

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

    public sealed class MonsterTemplateStorage : ConfigurationStorage<MonsterTemplate>
    {
        public MonsterTemplateStorage(string filename)
            : base(filename)
        { }

        protected override JSONParser<MonsterTemplate> GetParser()
        {
            return new MonsterJSONParser();
        }

        private sealed class MonsterJSONParser : JSONParser<MonsterTemplate>
        {
            protected override MonsterTemplate ConvertToObject(Dictionary<string, object> item)
            {
                return new MonsterTemplate(
                    TryGetValueAndFail<string>(item, "Name"),
                    TryGetValueAndFail<float>(item, "Health"),
                    (MovementType)TryGetValueOrSetDefaultValue<int>(item, "MovementType", 1),
                    (EntityTactics)TryGetValueOrSetDefaultValue<int>(item, "Tactics", 1),
                    TryGetValueAndFail<string>(item, "ActionItem"),
                    TryGetValueOrSetDefaultValue<string>(item, "DestructionItem", null));
            }
        }
    }

    #endregion MonsterTemplateStorage
}