using Assets.Scripts.Base;

namespace Assets.Scripts.LogicBase
{
    #region EntityTemplate

    public class EntityTemplate : IIdentifiable<string>
    {
        #region properties

        public double Health { get; private set; }

        public string Name { get; protected set; }

        public MovementType MovementType { get; private set; }

        #endregion properties

        #region constructor

        public EntityTemplate(string name, double health, MovementType movementType)
        {
            Assert.Greater(health, 0, "Entity {0}'s Health should be positive.");
            Assert.NotNullOrEmpty(name, "Entity name");
            Name = name;
            Health = health;
            MovementType = movementType;
        }

        #endregion constructor
    }

    #endregion EntityTemplate

    #region PlayerTemplate

    public class PlayerTemplate : EntityTemplate
    {
        public PlayerTemplate(string name, double health)
            : base("Player", health, MovementType.Walking)
        { }
    }

    #endregion PlayerTemplate
}