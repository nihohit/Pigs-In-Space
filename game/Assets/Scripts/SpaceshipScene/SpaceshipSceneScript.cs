using Assets.Scripts.IntersceneCommunication;
using Assets.Scripts.UnityBase;
using UnityEngine;

namespace Assets.Scripts.SpaceshipScene
{
    public class SpaceshipSceneScript : MonoBehaviour
    {
        #region private members

        private TextureManager m_textureManager;

        private EndLevelInfo m_endLevelInfo;

        private static GUIStyle s_guiStyle;

        #endregion private members

        #region Unity methods

        // Use this for initialization
        private void Start()
        {
            m_endLevelInfo = GlobalState.EndLevel;
            GlobalState.EndLevel = null;
            m_textureManager = new TextureManager();
            s_guiStyle = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12,
                normal = new GUIStyleState
                {
                    textColor = Color.white,
                },
            };
        }

        // Update is called once per frame
        private void Update()
        {
        }

        private void OnGUI()
        {
            GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), m_textureManager.GetUIBackground(), ScaleMode.StretchToFill);
            s_guiStyle.fontSize = 32;
            var message = "Space ship!";
            GUI.Label(new Rect(110, 45, 60, 60), message, s_guiStyle);
            s_guiStyle.fontSize = 12;

            //Next level or quit
            if (GUI.Button(new Rect(110, 350, 150, 30), "Level1"))
            {
                Application.LoadLevel("MapScene");
            }

            if (GUI.Button(new Rect(280, 350, 150, 30), "Quit"))
            {
                Application.Quit();
            }
            GUI.EndGroup();
        }

        #endregion Unity methods
    }
}