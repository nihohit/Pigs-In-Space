using Assets.Scripts.Base;

namespace Assets.Scripts.LogicBase
{
    public class MonsterTemplate : EntityTemplate, IIdentifiable<string>
    {
        #region properties

        public EntityTactics Tactics { get; set; }

        public string ActionItem { get; set; }

        public string DestructionItem { get; set; }

        #endregion properties

        #region constructor

        public MonsterTemplate(
            string name,
            double health,
            string actionItem,
            MovementType movementType = MovementType.Walking,
            EntityTactics tactics = EntityTactics.ActInRange,
            string destructionItem = null)
            : base(name, health, movementType)
        {
            Assert.NotNullOrEmpty(actionItem, "{0} action item".FormatWith(name));
            Tactics = tactics;
            ActionItem = actionItem;
            DestructionItem = destructionItem;
        }

        #endregion constructor
    }
}