using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SpaceshipSceneScript : MonoBehaviour
{
    #region private members

    private TextureManager m_textureManager;

    private static GUIStyle s_guiStyle;

    #endregion private members

    #region Unity methods

    // Use this for initialization
    private void Start()
    {
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
        EndLevelInfoGui();
        UpgradesGui();
    }   

    void EndLevelInfoGui()
    {
        var height = Screen.height;
        var width = Screen.width;
        var unit = (float)height / 400;

        width = width / 2;

        // boxing the GUI screen
        var widthDivider = width / 7;
        var heightDivider = height / 7;
        var widthMultiplier = 5 * widthDivider;
        var heightMultiplier = 5 * heightDivider;
        //GUI.BeginGroup(new Rect(widthDivider, heightDivider, widthMultiplier, heightMultiplier));
        //GUI.DrawTexture(new Rect(0, 0, widthMultiplier, heightMultiplier), m_textureManager.GetUIBackground(), ScaleMode.StretchToFill);

        // placing the main message
        s_guiStyle.fontSize = Convert.ToInt32(32 * unit);
        var message = "Spaceship";
        var messageSize = s_guiStyle.CalcSize(new GUIContent(message));
        var messageLength = messageSize.x;
        var messageHeight = messageSize.y;
        var middleWidth = widthMultiplier / 2;
        var accumulatingHeight = messageHeight * 1.1f;
        GUI.Label(new Rect(middleWidth - messageLength / 2, accumulatingHeight, messageLength, messageHeight), message, s_guiStyle);
        accumulatingHeight += messageHeight * 2;

        //sub message
        s_guiStyle.fontSize = Convert.ToInt32(15 * unit);
        message = "This is your spaceship, see your statistics \n and buy weapons here";
        messageSize = s_guiStyle.CalcSize(new GUIContent(message));
        messageLength = messageSize.x;
        messageHeight = messageSize.y;
        GUI.Label(new Rect(middleWidth - messageLength / 2, accumulatingHeight, messageLength, messageHeight), message, s_guiStyle);
        accumulatingHeight += messageHeight * 2;

        // placing the statistics
        s_guiStyle.fontSize = Convert.ToInt32(15 * unit);
        message = "Crystals collected: {0}".FormatWith((int)ShipData.Get.BlueCrystal);
        messageSize = s_guiStyle.CalcSize(new GUIContent(message));
        messageLength = messageSize.x;
        messageHeight = messageSize.y;
        GUI.Label(new Rect(middleWidth - messageLength / 2, accumulatingHeight, messageLength, messageHeight), message, s_guiStyle);
        accumulatingHeight += messageHeight * 2;

        message = "Tentacle monsters killed: {0}".FormatWith((int)ShipData.Get.KilledTentacles);
        messageSize = s_guiStyle.CalcSize(new GUIContent(message));
        messageLength = messageSize.x;
        messageHeight = messageSize.y;
        GUI.Label(new Rect(middleWidth - messageLength / 2, accumulatingHeight, messageLength, messageHeight), message, s_guiStyle);
        accumulatingHeight += messageHeight * 2;

        message = "slimes killed: {0}".FormatWith((int)ShipData.Get.KilledSlimes);
        messageSize = s_guiStyle.CalcSize(new GUIContent(message));
        messageLength = messageSize.x;
        messageHeight = messageSize.y;
        GUI.Label(new Rect(middleWidth - messageLength / 2, accumulatingHeight, messageLength, messageHeight), message, s_guiStyle);
        accumulatingHeight += messageHeight * 2;

        message = "Hives killed: {0}".FormatWith((int)ShipData.Get.KilledHives);
        messageSize = s_guiStyle.CalcSize(new GUIContent(message));
        messageLength = messageSize.x;
        messageHeight = messageSize.y;
        GUI.Label(new Rect(middleWidth - messageLength / 2, accumulatingHeight, messageLength, messageHeight), message, s_guiStyle);
        accumulatingHeight += messageHeight * 2;

        //Next level or quit
        if (GUI.Button(new Rect(middleWidth - 180, accumulatingHeight, 160, 30), "Level1"))
        {
            if (MapSceneScript.GameState == GameState.Lost)
            {
                ShipData.NewGame();
            }
            Application.LoadLevel("MapScene");
        }

        if (GUI.Button(new Rect(middleWidth, accumulatingHeight, 150, 30), "Quit"))
        {
            Application.Quit();
        }
        //GUI.EndGroup();
    }

    void UpgradesGui()
    {
        var height = Screen.height;
        var width = Screen.width;
        var unit = (float)height / 400;

        width = width / 4;

        var widthOffset = width * 2;

        // boxing the GUI screen
        var widthDivider = width / 7;
        var heightDivider = height / 7;
        var widthMultiplier = 5 * widthDivider;
        var heightMultiplier = 5 * heightDivider;
        //GUI.BeginGroup(new Rect(widthDivider, heightDivider, widthMultiplier, heightMultiplier));
        //GUI.DrawTexture(new Rect(0, 0, widthMultiplier, heightMultiplier), m_textureManager.GetUIBackground(), ScaleMode.StretchToFill);

        // placing the main message
        s_guiStyle.fontSize = Convert.ToInt32(32 * unit);
        var message = "Buy stuff";
        var messageSize = s_guiStyle.CalcSize(new GUIContent(message));
        var messageLength = messageSize.x;
        var messageHeight = messageSize.y;        
        var middleWidth = widthMultiplier / 2 + widthOffset;
        var accumulatingHeight = messageHeight * 1.1f;
        GUI.Label(new Rect(middleWidth - messageLength / 2, accumulatingHeight, messageLength, messageHeight), message, s_guiStyle);
        accumulatingHeight += messageHeight * 2;

        var heightSliver = height / 10;
        var aspectRatio = (float)width / height;
        var relativeWidth = heightSliver + s_guiStyle.CalcSize(new GUIContent("8 Letters")).x;

        //Choose weapons to Buy
        foreach (var equipment in ShipData.Get.PossibleEquipment)
        {
            GUI.color = new Color32(128, 128, 128, 196);

 

            if (GUI.Button(new Rect(middleWidth, accumulatingHeight, relativeWidth, heightSliver / 2), m_textureManager.GetTexture(equipment)))
            {
                
                if (Event.current.button == 0)
                {
                    //Entity.Player.LeftHandEquipment = equipment;
                    //m_equipmentChange = true;
                }
                else if (Event.current.button == 1)
                {
                    //Entity.Player.RightHandEquipment = equipment;
                    //m_equipmentChange = true;
                }
            }

            accumulatingHeight += heightSliver / 2;
        }
    }

    #endregion Unity methods

    #region public methods

    #endregion

    #region private methods

    #endregion

}