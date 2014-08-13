﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameState { Ongoing, Won, Lost }

public class MapSceneScript : MonoBehaviour
{
    private Vector2 CameraMax = new Vector2(0f, 0f);        // The maximum x and y coordinates the camera can have.
    private Vector2 CameraMin = new Vector2(0f, 0f);        // The minimum x and y coordinates the camera can have.
    private TextureManager m_textureManager;
    private static Dictionary<Action, Marker> s_Markers = new Dictionary<Action, Marker>();
    private static GameState s_gameState = GameState.Ongoing;

    private static GUIStyle s_guiStyle = new GUIStyle
    {
        fontStyle = FontStyle.Bold,
        fontSize = 12,
        normal = new GUIStyleState
        {
            textColor = Color.white,
        },
    };

    public const float UnitsToPixelsRatio = 1f / 100f;
    private bool m_mouseOnUI;

    public static void ChangeGameState(GameState state)
    {
        s_gameState = state;
    }

    public void Awake()
    {
        camera.orthographicSize = (Screen.height * UnitsToPixelsRatio);
    }

    // Use this for initialization
    private void Start()
    {
        m_textureManager = new TextureManager();
        SquareScript.LoadFromTMX(@"Maps\testMap3.tmx");
        Entity.Player = Entity.CreatePlayerEntity(5, 5);

        var squareSize = SquareScript.PixelsPerSquare * MapSceneScript.UnitsToPixelsRatio; // 1f

        var minCameraX = 0f - squareSize / 2 + camera.orthographicSize * camera.aspect;
        var maxCameraX = minCameraX + squareSize * SquareScript.Weidth() - 2 * camera.orthographicSize * camera.aspect;
        if (maxCameraX < minCameraX)
        {
            // camera not moving in x axis
            maxCameraX = minCameraX = (maxCameraX + minCameraX) / 2;
        }

        var minCameraY = 0f - squareSize / 2 + camera.orthographicSize;
        var maxCameraY = minCameraY + squareSize * SquareScript.Height() - 2 * camera.orthographicSize;
        if (maxCameraY < minCameraY)
        {
            // camera not moving in y axis
            maxCameraY = minCameraY = (maxCameraY + minCameraY) / 2;
        }

        CameraMin = new Vector2(minCameraX, minCameraY);
        CameraMax = new Vector2(maxCameraX, maxCameraY);

        SquareScript.InitFog();
        Entity.Player.Location.FogOfWar();
    }

    // Update is called once per frame
    private void Update()
    {
        if (s_gameState == GameState.Ongoing)
        {
            CameraTrackPlayer();
            StartCoroutine(PlayerAction());
        }
    }

    private void OnGUI()
    {
        // load game ending message
        if (s_gameState != GameState.Ongoing)
        {
            GUI.BeginGroup(new Rect(320, 265, 384, 256));
            GUI.DrawTexture(new Rect(0, 0, 384, 256), m_textureManager.GetUIBackground(), ScaleMode.StretchToFill);
            s_guiStyle.fontSize = 32;
            var message = (s_gameState == GameState.Lost) ? "Game Over" : "You Win :)";
            GUI.Label(new Rect(110, 45, 60, 60), message, s_guiStyle);
            s_guiStyle.fontSize = 12;
            GUI.Label(new Rect(176, 127, 30, 30), String.Format("X {0}", (int)Entity.Player.BlueCrystal), s_guiStyle);
            GUI.Label(new Rect(176, 165, 30, 30), String.Format("X {0}", EnemyEntity.KilledEnemies), s_guiStyle);
            GUI.Label(new Rect(176, 205, 30, 30), String.Format("X {0}", (int)Hive.KilledHives), s_guiStyle);
            GUI.EndGroup();
        }

        if (Entity.Player != null)
        {
            // define the area of the UI
            var heightSliver = Screen.height / 7f;
            var relativeWidth = heightSliver * 4 / 3;
            var oneSliver = Screen.width - relativeWidth;
            var twoSlivers = Screen.width - 2 * relativeWidth;
            var currentHeight = 16f;
            Rect UIArea = new Rect(twoSlivers, 0, 2 * relativeWidth, Screen.height);

            //load the UI background
            GUI.DrawTexture(UIArea, Resources.Load<Texture2D>(@"Sprites/PlayerStateDisplay"), ScaleMode.StretchToFill);

            //every
            m_mouseOnUI = false;
            if (UIArea.Contains(Input.mousePosition))
            {
                m_mouseOnUI = true;
            }

            // display stats
            DrawSpriteToGUI(SpriteManager.CardiacIcon, new Rect(twoSlivers + 10, currentHeight, 32, 32));
            GUI.Label(new Rect(twoSlivers + 52, 24, 30, 30), String.Format("X {0}", (int)Entity.Player.Health), s_guiStyle);

            DrawSpriteToGUI(SpriteManager.LightningIcon, new Rect(oneSliver + 10, currentHeight, 32, 32));
            GUI.Label(new Rect(oneSliver + 52, 24, 30, 30), String.Format("X {0}", (int)Entity.Player.Energy), s_guiStyle);

            currentHeight += heightSliver;
            DrawSpriteToGUI(SpriteManager.OxygenTank, new Rect(twoSlivers + 10, currentHeight, 32, 32));
            GUI.Label(new Rect(twoSlivers + 52, currentHeight + 8, 30, 30), String.Format("X {0}", (int)Entity.Player.Oxygen), s_guiStyle);

            DrawSpriteToGUI(SpriteManager.Crystal, new Rect(oneSliver + 10, currentHeight, 32, 32));
            GUI.Label(new Rect(oneSliver + 52, currentHeight + 8, 30, 30), String.Format("X {0}", (int)Entity.Player.BlueCrystal), s_guiStyle);

            // display choosable equipment
            bool goDown = false;
            foreach (var equipment in Entity.Player.Equipment)
            {
                goDown = !goDown;
                float widthPosition = 0;
                if (goDown)
                {
                    currentHeight += heightSliver;
                    widthPosition = twoSlivers;
                }
                else
                {
                    widthPosition = oneSliver;
                }
                GUI.color = Color.gray;

                if (equipment == Entity.Player.LeftHandEquipment)
                {
                    GUI.color = Color.blue;
                }
                if (equipment == Entity.Player.RightHandEquipment)
                {
                    GUI.color = Color.red;
                }

                if (GUI.Button(new Rect(widthPosition, currentHeight, relativeWidth, heightSliver), m_textureManager.GetTexture(equipment)))
                {
                    if (Event.current.button == 0)
                        Entity.Player.LeftHandEquipment = equipment;
                    else if (Event.current.button == 1)
                        Entity.Player.RightHandEquipment = equipment;
                }
            }
        }
    }

    private void DrawSpriteToGUI(Sprite sprite, Rect position)
    {
        Texture t = sprite.texture;
        Rect tr = sprite.textureRect;
        Rect r = new Rect(tr.x / t.width, tr.y / t.height, tr.width / t.width, tr.height / t.height);
        GUI.DrawTextureWithTexCoords(position, t, r);
    }

    private IEnumerator PlayerAction()
    {
        var x = 0;
        var y = 0;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            y = -1;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            y = 1;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            x = -1;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            x = 1;
        }
        if (x != 0 || y != 0)
        {
            if (Entity.Player.Move(Entity.Player.Location.GetNextSquare(x, y)))
            {
                //transform.position = new Vector3(m_playerSprite.transform.position.x, m_playerSprite.transform.position.y, transform.position.z);
                yield return new WaitForSeconds(0.25f);
                Entity.Player.EndTurn(0);
            }
        }

        if (Input.GetMouseButtonUp(0) && !m_mouseOnUI)
        { // left click
            //Get Mouse direction, let's assume it's right for now
            //create laser object
            var enumerator = Entity.Player.LeftHandEquipment.Effect(SquareScript.s_markedSquare);
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        if (Input.GetMouseButtonUp(1) && !m_mouseOnUI)
        {
            var enumerator = Entity.Player.RightHandEquipment.Effect(SquareScript.s_markedSquare);
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }

    private void CameraTrackPlayer()
    {
        const float xSmooth = 8f; // How smoothly the camera catches up with it's target movement in the x axis.
        const float ySmooth = 8f; // How smoothly the camera catches up with it's target movement in the y axis.

        // By default the target x and y coordinates of the camera are it's current x and y coordinates.
        float targetX = transform.position.x;
        float targetY = transform.position.y;

        // If the player has moved beyond the x margin...
        var playerPosition = Entity.Player.Image.Position;

        // ... the target x coordinate should be a Lerp between the camera's current x position and the player's current x position.
        targetX = Mathf.Lerp(transform.position.x, playerPosition.x, xSmooth * Time.deltaTime);

        // The target x coordinates should not be larger than the maximum or smaller than the minimum.
        targetX = Mathf.Clamp(targetX, CameraMin.x, CameraMax.x);

        // ... the target y coordinate should be a Lerp between the camera's current y position and the player's current y position.
        targetY = Mathf.Lerp(transform.position.y, playerPosition.y, ySmooth * Time.deltaTime);

        // The target y coordinates should not be larger than the maximum or smaller than the minimum.
        targetY = Mathf.Clamp(targetY, CameraMin.y, CameraMax.y);

        // Set the camera's position to the target position with the same z component.
        transform.position = new Vector3(targetX, targetY, transform.position.z);
    }

    public static void EnterEscapeMode()
    {
        Hive.EnterEscapeMode();
        var escapeGameEvents = s_Markers.Where(entry => entry.Value == Marker.OnEscape).Select(entry => entry.Key);
        foreach (var gameEvent in escapeGameEvents)
        {
            gameEvent();
        }
    }

    public static void SetEvent(Action action, Marker marker)
    {
        s_Markers.Add(action, marker);
    }
}

public enum Marker
{
    OnEscape,
}