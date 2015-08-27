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
            TextureManager = new TextureManager();
        }

        public void StartNewPlayer(int health, int energy, int oxygen, int crystals)
        {
            Player = new PlayerState(health, energy, oxygen, crystals);
            Player.Equipment.Add(Configurations.Equipment.GetConfiguration("pistol"));
        }

        public void EndGame()
        {
            Player = null;
        }
    }
}