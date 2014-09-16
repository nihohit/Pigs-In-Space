using Assets.Scripts.Base;
using Assets.Scripts.IntersceneCommunication;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.MapScene
{
    public enum GameState { Ongoing, Won, Lost }

    public class MapSceneScript : MonoBehaviour
    {
        #region private members

        private const int c_playStartPositionX = 5;
        private const int c_playStartPositionY = 5;

        private Vector2 CameraMax = new Vector2(0f, 0f);        // The maximum x and y coordinates the camera can have.
        private Vector2 CameraMin = new Vector2(0f, 0f);        // The minimum x and y coordinates the camera can have.
        private TextureManager m_textureManager;
        private static Dictionary<Action, Marker> s_Markers = new Dictionary<Action, Marker>();
        private static GUIStyle s_guiStyle;

        /// <summary>
        /// List of all the squares with an active effect, each turn all these effects durability is reduced
        /// </summary>
        private static HashSet<SquareScript> s_squaresWithEffect = new HashSet<SquareScript>();

        private static GameState s_gameState = GameState.Ongoing;
        public const float UnitsToPixelsRatio = 1f / 100f;
        private bool m_mouseOnUI;
        private bool m_equipmentChange;
        private bool m_startCoRoutine = true;
        private bool m_displayGUI = true;

        #endregion private members

        public static bool EscapeMode { get; set; }

        #region public methods

        public static void ChangeGameState(GameState state)
        {
            s_gameState = state;
        }

        public void Init()
        {
            m_textureManager = new TextureManager();
            ActionableItem.Init(m_textureManager);

            SquareScript.Init();
            ScreenSizeInit();
            GUIStyleInit();
            PlayerInit();
            ChangeGameState(GameState.Ongoing);
        }

        public static void EnterEscapeMode()
        {
            EscapeMode = true;
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

        /// <summary>
        /// Reduce durability for all active effects
        /// </summary>
        public static void ReduceEffectsDuration()
        {
            var toBeRemoved = new List<SquareScript>();
            foreach (var squareWithEffect in s_squaresWithEffect)
            {
                if (squareWithEffect.OccupyingEntity != null)
                {
                    squareWithEffect.OccupyingEntity.ApplyGroundEffects();
                }
                squareWithEffect.GroundEffect.Duration--;
                if (squareWithEffect.GroundEffect.Duration <= 0)
                {
                    toBeRemoved.Add(squareWithEffect);
                    squareWithEffect.GroundEffect = GroundEffect.NoEffect;
                }
            }
            s_squaresWithEffect = s_squaresWithEffect.Except(toBeRemoved).ToHashSet();
        }

        /// <summary>
        /// Add a effect at given location
        /// </summary>
        public static void AddGroundEffect(GroundEffect effect, int x, int y)
        {
            AddGroundEffect(effect, SquareScript.GetSquare(x, y));
            AddGroundEffect(effect, SquareScript.GetSquare(x, y));
        }

        /// <summary>
        /// Add a effect at given location
        /// </summary>
        public static void AddGroundEffect(GroundEffect effect, SquareScript square)
        {
            if (square.TraversingCondition == Traversability.Walkable)
            {
                square.GroundEffect = effect;
                s_squaresWithEffect.Add(square);
            }
        }

        #endregion public methods

        #region UnityMethods

        private void Awake()
        {
            camera.orthographicSize = (Screen.height * UnitsToPixelsRatio);
        }

        // Use this for initialization
        private void Start()
        {
            Init();
        }

        // Update is called once per frame
        private void Update()
        {
            if (s_gameState == GameState.Ongoing)
            {
                CameraTrackPlayer();
                if (m_startCoRoutine)
                {
                    StartCoroutine(PlayerAction());
                }
            }
        }

        private void OnGUI()
        {
            var height = Screen.height;
            var width = Screen.width;
            var unit = (float)height / 400;

            // load game ending message
            if (s_gameState != GameState.Ongoing && m_displayGUI)
            {
                // boxing the GUI screen
                var widthDivider = width / 7;
                var heightDivider = height / 7;
                var widthMultiplier = 5 * widthDivider;
                var heightMultiplier = 5 * heightDivider;
                GUI.BeginGroup(new Rect(widthDivider, heightDivider, widthMultiplier, heightMultiplier));
                GUI.DrawTexture(new Rect(0, 0, widthMultiplier, heightMultiplier), m_textureManager.GetUIBackground(), ScaleMode.StretchToFill);

                // placing the main message
                s_guiStyle.fontSize = Convert.ToInt32(32 * unit);
                var message = (s_gameState == GameState.Lost) ? "Game Over" : "You Win :)";
                var messageSize = s_guiStyle.CalcSize(new GUIContent(message));
                var messageLength = messageSize.x;
                var messageHeight = messageSize.y;
                var middleWidth = widthMultiplier / 2;
                var accumulatingHeight = messageHeight * 1.1f;
                GUI.Label(new Rect(middleWidth - messageLength / 2, accumulatingHeight, messageLength, messageHeight), message, s_guiStyle);
                accumulatingHeight += messageHeight * 2;

                // placing the statistics
                s_guiStyle.fontSize = Convert.ToInt32(15 * unit);
                message = "Crystals collected: {0}".FormatWith((int)Entity.Player.GainedLoot.BlueCrystal);
                messageSize = s_guiStyle.CalcSize(new GUIContent(message));
                messageLength = messageSize.x;
                messageHeight = messageSize.y;
                GUI.Label(new Rect(middleWidth - messageLength / 2, accumulatingHeight, messageLength, messageHeight), message, s_guiStyle);
                accumulatingHeight += messageHeight * 2;

                message = "Tentacle monsters killed: {0}".FormatWith((int)EnemiesManager.KilledTentacles);
                messageSize = s_guiStyle.CalcSize(new GUIContent(message));
                messageLength = messageSize.x;
                messageHeight = messageSize.y;
                GUI.Label(new Rect(middleWidth - messageLength / 2, accumulatingHeight, messageLength, messageHeight), message, s_guiStyle);
                accumulatingHeight += messageHeight * 2;

                message = "slimes killed: {0}".FormatWith((int)EnemiesManager.KilledSlimes);
                messageSize = s_guiStyle.CalcSize(new GUIContent(message));
                messageLength = messageSize.x;
                messageHeight = messageSize.y;
                GUI.Label(new Rect(middleWidth - messageLength / 2, accumulatingHeight, messageLength, messageHeight), message, s_guiStyle);
                accumulatingHeight += messageHeight * 2;

                message = "Hives killed: {0}".FormatWith((int)EnemiesManager.KilledHives);
                messageSize = s_guiStyle.CalcSize(new GUIContent(message));
                messageLength = messageSize.x;
                messageHeight = messageSize.y;
                GUI.Label(new Rect(middleWidth - messageLength / 2, accumulatingHeight, messageLength, messageHeight), message, s_guiStyle);
                accumulatingHeight += messageHeight * 2;

                if (GUI.Button(new Rect(middleWidth - 80, accumulatingHeight, 160, 30), "Spaceship"))
                {
                    m_displayGUI = false;
                    GlobalState.EndLevel = new EndLevelInfo(Entity.Player.GainedLoot);
                    ClearData();
                    Application.LoadLevel("SpaceShipScene");
                }
                GUI.EndGroup();
            }

            if (Entity.Player != null)
            {
                // define the area of the UI
                var heightSliver = height / 10;
                var relativeWidth = heightSliver + s_guiStyle.CalcSize(new GUIContent("8 Letters")).x;
                var oneSliver = Screen.width - relativeWidth;
                Rect UIArea = new Rect(oneSliver, 0, 2 * relativeWidth, Screen.height);

                // check whether mouse is over UI
                m_mouseOnUI = false;
                if (UIArea.Contains(Input.mousePosition))
                {
                    m_mouseOnUI = true;
                }

                // display stats
                var heightSize = 24 * unit;
                var currentHeight = heightSize / 2;
                DrawSpriteToGUI(SpriteManager.CardiacIcon, new Rect(oneSliver, heightSize / 2, heightSliver, heightSliver));
                GUI.Label(new Rect(oneSliver + heightSliver, heightSize, relativeWidth, heightSliver), "X {0}".FormatWith((int)Entity.Player.Health), s_guiStyle);

                currentHeight += heightSliver;
                DrawSpriteToGUI(SpriteManager.LightningIcon, new Rect(oneSliver, currentHeight, heightSliver, heightSliver));
                GUI.Label(new Rect(oneSliver + heightSliver, currentHeight + heightSliver / 3, relativeWidth, heightSliver), "X {0}".FormatWith((int)Entity.Player.Energy), s_guiStyle);

                currentHeight += heightSliver;
                DrawSpriteToGUI(SpriteManager.OxygenTank, new Rect(oneSliver, currentHeight, heightSliver, heightSliver));
                GUI.Label(new Rect(oneSliver + heightSliver, currentHeight + heightSliver / 3, relativeWidth, heightSliver), "X {0}".FormatWith((int)Entity.Player.Oxygen), s_guiStyle);

                currentHeight += heightSliver;
                DrawSpriteToGUI(SpriteManager.Crystal, new Rect(oneSliver, currentHeight, heightSliver, heightSliver));
                GUI.Label(new Rect(oneSliver + heightSliver, currentHeight + heightSliver / 3, relativeWidth, heightSliver), "X {0}".FormatWith((int)Entity.Player.GainedLoot.BlueCrystal), s_guiStyle);

                // separate status and weapons
                // display choosable equipment
                currentHeight += heightSliver;
                foreach (var equipment in Entity.Player.Equipment)
                {
                    GUI.color = new Color32(128, 128, 128, 196);

                    if (equipment.Equals(Entity.Player.LeftHandEquipment))
                    {
                        GUI.color = new Color32(128, 128, 255, 255);
                    }
                    if (equipment.Equals(Entity.Player.RightHandEquipment))
                    {
                        GUI.color = new Color32(255, 128, 128, 255);
                    }

                    if (GUI.Button(new Rect(oneSliver, currentHeight, relativeWidth, heightSliver / 2), m_textureManager.GetTexture(equipment)))
                    {
                        if (Event.current.button == 0)
                        {
                            Entity.Player.LeftHandEquipment = equipment;
                            m_equipmentChange = true;
                        }
                        else if (Event.current.button == 1)
                        {
                            Entity.Player.RightHandEquipment = equipment;
                            m_equipmentChange = true;
                        }
                    }

                    currentHeight += heightSliver / 2;
                }
            }
        }

        #endregion UnityMethods

        #region private methods

        private void DrawSpriteToGUI(Sprite sprite, Rect position)
        {
            Texture t = sprite.texture;
            Rect tr = sprite.textureRect;
            Rect r = new Rect(tr.x / t.width, tr.y / t.height, tr.width / t.width, tr.height / t.height);
            GUI.DrawTextureWithTexCoords(position, t, r);
        }

        private IEnumerator PlayerAction()
        {
            m_startCoRoutine = false;
            IEnumerator returnedEnumerator = new EmptyEnumerator();
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
                    returnedEnumerator = this.WaitAndEndTurn(0, 0.1f);
                }
            }

            if (m_equipmentChange)
            {
                m_equipmentChange = false;

                returnedEnumerator = this.WaitAndEndTurn(0, 0.1f);
            }

            if (Input.GetMouseButtonUp(0) && !m_mouseOnUI)
            {
                returnedEnumerator = Entity.Player.LeftHandEquipment.Effect(SquareScript.s_markedSquare, 0.15f);
            }

            if (Input.GetMouseButtonUp(1) && !m_mouseOnUI)
            {
                returnedEnumerator = Entity.Player.RightHandEquipment.Effect(SquareScript.s_markedSquare, 0.15f);
            }

            while (returnedEnumerator.MoveNext())
            {
                yield return returnedEnumerator.Current;
            }
            m_startCoRoutine = true;
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

        private void ScreenSizeInit()
        {
            var squareSize = SquareScript.PixelsPerSquare * MapSceneScript.UnitsToPixelsRatio; // 1f

            var minCameraX = 0f - squareSize / 2 + camera.orthographicSize * camera.aspect;
            var maxCameraX = minCameraX + squareSize * SquareScript.Width - 2 * camera.orthographicSize * camera.aspect;
            if (maxCameraX < minCameraX)
            {
                // camera not moving in x axis
                maxCameraX = minCameraX = (maxCameraX + minCameraX) / 2;
            }

            var minCameraY = 0f - squareSize / 2 + camera.orthographicSize;
            var maxCameraY = minCameraY + squareSize * SquareScript.Height - 2 * camera.orthographicSize;
            if (maxCameraY < minCameraY)
            {
                // camera not moving in y axis
                maxCameraY = minCameraY = (maxCameraY + minCameraY) / 2;
            }

            CameraMin = new Vector2(minCameraX, minCameraY);
            CameraMax = new Vector2(maxCameraX, maxCameraY);
        }

        private void GUIStyleInit()
        {
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

        private void ClearData()
        {
            EnemiesManager.Clear();
            s_Markers.Clear();
            s_squaresWithEffect.Clear();
            SquareScript.Clear();
        }

        private void PlayerInit()
        {
            Entity.CreatePlayerEntity(c_playStartPositionX, c_playStartPositionY);
            Entity.Player.Location.FogOfWar();
        }

        #endregion private methods
    }

    public enum Marker
    {
        OnEscape,
    }
}