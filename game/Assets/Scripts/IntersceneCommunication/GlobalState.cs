using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;
namespace Assets.Scripts.IntersceneCommunication
{
    public class GlobalState
    {
        public EndLevelInfo EndLevel { get; set; }

        public PlayerState Player { get; private set; }

        public Configurations Configurations { get; private set; }

        public TextureManager TextureManager { get; private set; }

        public static GlobalState Instance
        {
            get
            {
                return Singleton<GlobalState>.Instance;
            }
        }

        private GlobalState()
        {
            Configurations = new Configurations();
            Player = new PlayerState();
            Player.Equipment.AddRange(Configurations.Equipment.GetAllConfigurations().ChooseRandomValues(5));
            TextureManager = new TextureManager();
        }
    }
}