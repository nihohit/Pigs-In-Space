using Assets.Scripts.IntersceneCommunication;
using Assets.Scripts.UnityBase;
using UnityEngine;

namespace Assets.Scripts.SpaceshipScene
{
    using Assets.Scripts.LogicBase;
    using System;

    public class SpaceshipSceneScript : MonoBehaviour
    {
        #region private members

        private TextureManager m_textureManager;

        private EndLevelInfo m_endLevelInfo;

        #endregion private members

        #region public methods

        public void QuitGame()
        {
            Application.Quit();
        }

        public void NextLevel()
        {
            Application.LoadLevel("MapScene");
        }

        #endregion public methods

        #region Unity methods

        // Use this for initialization
        private void Start()
        {
            m_endLevelInfo = GlobalState.EndLevel;
            GlobalState.EndLevel = null;
            m_textureManager = new TextureManager();
        }

        // Update is called once per frame
        private void Update()
        {
        }

        #endregion Unity methods

        #region private methods

        private Action UpgradeWeapon(PlayerEquipment toTeUpgraded, PlayerEquipment result, Loot Cost)
        {
            return () =>
                {
                    GlobalState.Player.Equipment.Remove(toTeUpgraded);
                    GlobalState.Player.Equipment.Add(result);
                    GlobalState.Player.Loot.RemoveIfEnough(Cost);
                };
        }

        #endregion private methods
    }
}