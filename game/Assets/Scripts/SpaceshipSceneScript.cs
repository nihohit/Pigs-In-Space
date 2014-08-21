using UnityEngine;
using System.Collections;

public class SpaceshipSceneScript : MonoBehaviour
{

    #region private members
    private TextureManager m_textureManager;

    private static GUIStyle s_guiStyle;

    #endregion 

    #region Unity methods

    // Use this for initialization
	void Start () {
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
	void Update () {
        //print("spaceship update");
        //Debug.Log("Update spaceship");
	}

    private void OnGUI()
    {
        
        GUI.BeginGroup(new Rect(0, 0, 384, 300));
        GUI.DrawTexture(new Rect(0, 0, 384, 256), m_textureManager.GetUIBackground(), ScaleMode.StretchToFill);
        s_guiStyle.fontSize = 32;
        var message = "Space ship!";
        GUI.Label(new Rect(110, 45, 60, 60), message, s_guiStyle);
        s_guiStyle.fontSize = 12;
        //GUI.Label(new Rect(176, 127, 30, 30), String.Format("X {0}", (int)Entity.Player.BlueCrystal), s_guiStyle);
        //GUI.Label(new Rect(176, 165, 30, 30), String.Format("X {0}", EnemyEntity.KilledEnemies), s_guiStyle);
        //GUI.Label(new Rect(176, 205, 30, 30), String.Format("X {0}", (int)Hive.KilledHives), s_guiStyle);

        if (GUI.Button(new Rect(110, 230, 150, 30), "Level1"))
        {
            Application.LoadLevel("MapScene");
        }
        GUI.EndGroup();
    }
    #endregion 
}
