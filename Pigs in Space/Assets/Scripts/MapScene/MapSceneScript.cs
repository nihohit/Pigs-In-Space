using Assets.Scripts.Base;
using Assets.Scripts.IntersceneCommunication;
using Assets.Scripts.LogicBase;
using Assets.Scripts.MapScene.MapGenerator;
using Assets.Scripts.UnityBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.MapScene
{
    using UnityEngine.Events;
    using UnityEngine.EventSystems;

    public enum GameState { Ongoing, Won, Lost }

    public class MapSceneScript : MonoBehaviour
    {
        #region private members

        private static readonly Dictionary<Action, Marker> sr_markers = new Dictionary<Action, Marker>();

        private static readonly ColorBlock sr_regularColorBlock = new ColorBlock
        {
            normalColor = new Color(0.7f, 0.7f, 0.7f),
            highlightedColor = new Color(0, 0, 0),
            pressedColor = new Color(0.7f, 0.7f, 0.7f),
            fadeDuration = 0.1f,
            colorMultiplier = 1,

        };

        private static readonly ColorBlock sr_leftClickColorBlock = new ColorBlock
        {
            normalColor = new Color(1, 0, 0),
            highlightedColor = new Color(0.5f, 0, 0),
            pressedColor = new Color(1, 0, 0),
            fadeDuration = 0.1f,
            colorMultiplier = 1,
        };

        private static readonly ColorBlock sr_rightClickColorBlock = new ColorBlock
        {
            normalColor = new Color(0, 1, 0),
            highlightedColor = new Color(0, 0.5f, 0),
            pressedColor = new Color(0, 1, 0),
            fadeDuration = 0.1f,
            colorMultiplier = 1,
        };

        /// <summary>
        /// List of all the squares with an active effect, each turn all these effects durability is reduced
        /// </summary>
        private static HashSet<SquareScript> s_squaresWithEffect = new HashSet<SquareScript>();

        private static GameState s_gameState = GameState.Ongoing;

        private Vector2 m_cameraMax = new Vector2(0f, 0f);        // The maximum x and y coordinates the camera can have.
        private Vector2 m_cameraMin = new Vector2(0f, 0f);        // The minimum x and y coordinates the camera can have.

        private Text m_healthText;
        private Text m_energyText;
        private Text m_oxygenText;
        private Text m_crystalsText;

        private Button m_leftClickButton;
        private Button m_rightClickButton;
        private GameObject m_endGamePanel;
        private GameObject m_sidebarPanel;

        public const float c_unitsToPixelsRatio = 1f / 100f;
        private bool m_equipmentChange;
        private bool m_startCoRoutine = true;
        private bool m_mouseOverUI = false;

        #endregion private members

        #region properties

        public static bool EscapeMode { get; set; }

        #endregion

        #region public methods

        #region static methods

        public static void EnterEscapeMode()
        {
            EscapeMode = true;
            var escapeGameEvents = sr_markers.Where(entry => entry.Value == Marker.OnEscape).Select(entry => entry.Key);
            foreach (var gameEvent in escapeGameEvents)
            {
                gameEvent();
            }
        }

        public static void SetEvent(Action action, Marker marker)
        {
            sr_markers.Add(action, marker);
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

        #endregion static methods

        public void MouseEnterUI()
        {
            m_mouseOverUI = true;
        }

        public void MouseExitUI()
        {
            m_mouseOverUI = false;
        }

        public static void ChangeGameState(GameState state)
        {
            s_gameState = state;
        }

        public void Init()
        {
            EscapeMode = false;
            ClearData();

            GuiInit();
            MapInit();
            ScreenSizeInit();
            ChangeGameState(GameState.Ongoing);
        }

        public void ToSpaceshipScreen()
        {
            GlobalState.Instance.EndLevel = new EndLevelInfo(Entity.Player.GainedLoot);
            Application.LoadLevel("SpaceShipScene");
        }

        public void StartNewGame()
        {
            GlobalState.Instance.EndGame();
            Application.LoadLevel("StartScene");
        }

        private void GuiInit()
        {
            m_healthText = GameObject.Find("HeartText").GetComponent<Text>();
            m_energyText = GameObject.Find("EnergyText").GetComponent<Text>();
            m_oxygenText = GameObject.Find("OxygenText").GetComponent<Text>();
            m_crystalsText = GameObject.Find("CrystalsText").GetComponent<Text>();
            m_endGamePanel = GameObject.Find("EndGamePanel");
            m_endGamePanel.SetActive(false);
            m_sidebarPanel = GameObject.Find("SidebarPanel");
            var canvas = GameObject.Find("UICanvas");
            var equipmentButtons =
                canvas.GetComponentsInChildren<Button>().Where(button => button.name.Equals("EquipmentButton")).ToList();

            var equipment = GlobalState.Instance.Player.Equipment;

            UnityHelper.SetFunctionalityForFirstItems<Button, PlayerEquipment>(
                equipmentButtons,
                equipment,
                (button, equipmentPiece) =>
                {
                    button.SetButtonFunctionality(SetEquipment(button, equipmentPiece));

                    button.colors = sr_regularColorBlock;
                    var image = button.GetComponent<Image>();
                    image.sprite = GlobalState.Instance.TextureManager.GetTexture(equipmentPiece);
                    button.name = equipmentPiece.Name;
                    button.navigation = new Navigation
                    {
                        mode = Navigation.Mode.None,
                    };
                });

            m_leftClickButton = equipmentButtons.First();
            m_leftClickButton.colors = sr_leftClickColorBlock;

            m_rightClickButton = equipmentButtons.Skip(1).First();
            m_rightClickButton.colors = sr_rightClickColorBlock;
        }

        private UnityAction<BaseEventData> SetEquipment(Button button, PlayerEquipment equipment)
        {
            return (BaseEventData eventData) =>
                {
                    var clickEventData = eventData as PointerEventData;
                    if (clickEventData.button == PointerEventData.InputButton.Left)
                    {
                        Entity.Player.LeftHandEquipment = equipment;
                        m_leftClickButton.colors = sr_regularColorBlock;
                        m_leftClickButton = button;
                        m_leftClickButton.colors = sr_leftClickColorBlock;
                        m_equipmentChange = true;
                    }
                    else if (clickEventData.button == PointerEventData.InputButton.Right)
                    {
                        Entity.Player.RightHandEquipment = equipment;
                        m_rightClickButton.colors = sr_regularColorBlock;
                        m_rightClickButton = button;
                        m_rightClickButton.colors = sr_rightClickColorBlock;
                        m_equipmentChange = true;
                    }
                };
        }

        #endregion public methods

        #region UnityMethods

        private void Awake()
        {
            GetComponent<Camera>().orthographicSize = Screen.height * c_unitsToPixelsRatio;
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

                m_healthText.text = "{0:0.0}".FormatWith(Entity.Player.Health);
                m_energyText.text = "{0:0.0}".FormatWith(Entity.Player.Energy);
                m_oxygenText.text = "{0:0.0}".FormatWith(Entity.Player.Oxygen);
                m_crystalsText.text = "{0}".FormatWith(Entity.Player.GainedLoot.BlueCrystal);
                return;
            }

            if (!m_endGamePanel.activeSelf)
            {
                m_sidebarPanel.SetActive(false);
                m_endGamePanel.SetActive(true);
                m_endGamePanel.transform.FindChild("TentacleText").GetComponent<Text>().text = "{0} killed".FormatWith(EnemiesManager.KilledTentacles);
                m_endGamePanel.transform.FindChild("hiveText").GetComponent<Text>().text = "{0} killed".FormatWith(EnemiesManager.KilledHives);

                m_endGamePanel.transform.FindChild("SlimeText").GetComponent<Text>().text = "{0} killed".FormatWith(EnemiesManager.KilledSlimes);
                var textResult = m_endGamePanel.GetComponentsInChildren<Text>().First(text => text.name.Equals("ResultText"));

                if (s_gameState == GameState.Lost)
                {
                    textResult.text = "You lost";
                    m_endGamePanel.GetComponentsInChildren<Button>().First(button => button.name.Equals("SpaceshipButton")).gameObject.SetActive(false);
                }
                else
                {
                    textResult.text = "You won!";
                }
            }
        }

        #endregion UnityMethods

        #region private methods

        private IEnumerator PlayerAction()
        {
            m_startCoRoutine = false;
            IEnumerator returnedEnumerator = new EmptyEnumerator();
            var x = 0;
            var y = 0;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                y = -1;
            }

            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                y = 1;
            }

            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                x = -1;
            }

            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                x = 1;
            }

            if (x != 0 || y != 0)
            {
                if (Entity.Player.Move(Entity.Player.Location.GetNextSquare(x, y)))
                {
                    returnedEnumerator = this.WaitAndEndTurn(0, 0.1f);
                }
            }

            if (m_equipmentChange)
            {
                m_equipmentChange = false;

                returnedEnumerator = this.WaitAndEndTurn(0, 0.1f);
            }

            if (Input.GetMouseButtonDown(0) && !m_mouseOverUI)
            {
                returnedEnumerator = Entity.Player.LeftHandEquipment.Effect(SquareScript.s_markedSquare, 0.15f);
            }

            if (Input.GetMouseButtonDown(1) && !m_mouseOverUI)
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
            const float c_XSmooth = 8f; // How smoothly the camera catches up with it's target movement in the x axis.
            const float c_YSmooth = 8f; // How smoothly the camera catches up with it's target movement in the y axis.

            // By default the target x and y coordinates of the camera are it's current x and y coordinates.

            // If the player has moved beyond the x margin...
            var playerPosition = Entity.Player.Image.Position;

            // ... the target x coordinate should be a Lerp between the camera's current x position and the player's current x position.
            float targetX = Mathf.Lerp(this.transform.position.x, playerPosition.x, c_XSmooth * Time.deltaTime);

            // The target x coordinates should not be larger than the maximum or smaller than the minimum.
            targetX = Mathf.Clamp(targetX, this.m_cameraMin.x, this.m_cameraMax.x);

            // ... the target y coordinate should be a Lerp between the camera's current y position and the player's current y position.
            float targetY = Mathf.Lerp(this.transform.position.y, playerPosition.y, c_YSmooth * Time.deltaTime);

            // The target y coordinates should not be larger than the maximum or smaller than the minimum.
            targetY = Mathf.Clamp(targetY, this.m_cameraMin.y, this.m_cameraMax.y);

            // Set the camera's position to the target position with the same z component.
            transform.position = new Vector3(targetX, targetY, transform.position.z);
        }

        private void ScreenSizeInit()
        {
            var camera = GetComponent<Camera>();
            const float c_SquareSize = SquareScript.PixelsPerSquare * MapSceneScript.c_unitsToPixelsRatio; // 1f

            var minCameraX = 0f - (c_SquareSize / 2) + (camera.orthographicSize * camera.aspect);
            var maxCameraX = minCameraX + (c_SquareSize * SquareScript.Width) - (2 * (camera.orthographicSize * camera.aspect));
            if (maxCameraX < minCameraX)
            {
                // camera not moving in x axis
                maxCameraX = minCameraX = (maxCameraX + minCameraX) / 2;
            }

            var minCameraY = 0f - (c_SquareSize / 2) + camera.orthographicSize;
            var maxCameraY = minCameraY + (c_SquareSize * SquareScript.Height) - (2 * camera.orthographicSize);
            if (maxCameraY < minCameraY)
            {
                // camera not moving in y axis
                maxCameraY = minCameraY = (maxCameraY + minCameraY) / 2;
            }

            // this makes sure that the sidebar will be added to the length of the screen
            // TODO - this isn't exact. When reaching right, the edge of the last square should be right on the edge of the sidebar.
            var sidebarWidth = m_sidebarPanel.GetComponent<RectTransform>().rect.width;
            var canvasWidth = GameObject.Find("UICanvas").GetComponent<RectTransform>().rect.width;
            var sidebarPartOfScreen = sidebarWidth / canvasWidth;
            var screenWidth = maxCameraX - minCameraX;
            maxCameraX = maxCameraX + (screenWidth * sidebarPartOfScreen);

            this.m_cameraMin = new Vector2(minCameraX, minCameraY);
            this.m_cameraMax = new Vector2(maxCameraX, maxCameraY);
        }

        private void ClearData()
        {
            EnemiesManager.Clear();
            sr_markers.Clear();
            s_squaresWithEffect.Clear();
            SquareScript.Clear();
            MapGenerator.BasePopulator<Loot>.Clear();
            MapGenerator.BasePopulator<MonsterTemplate>.Clear();
        }

        private void MapInit()
        {
            var terrainGenerator = new PerlinNoiseCaveMapGenerator();
            var monsterPopulator = new DistanceBasedMonsterPopulator();
            var treasurePopulator = new DistanceAndDensityBasedTreasurePopulator();
            SquareScript.Init(terrainGenerator, monsterPopulator, treasurePopulator);
            Entity.Player.Location.FogOfWar();
        }

        #endregion private methods
    }

    public enum Marker
    {
        OnEscape,
    }
}