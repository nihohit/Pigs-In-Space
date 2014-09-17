using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.MapScene.MapGenerator;
using Assets.Scripts.UnityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace Assets.Scripts.MapScene
{
    public enum Traversability { Walkable, Flyable, Blocking }

    public enum Opacity { Blocking, SeeThrough }

    public enum FogCoverType { None, Partial, Full }

    #region SquareScript

    public class SquareScript : MonoBehaviour
    {
        #region fields

        private static SquareScript[,] s_map;
        private int m_x, m_y;
        private Loot m_droppedLoot;
        private TerrainType m_terrainType;
        private FogOfWarType m_fogOfWarType;
        public static SquareScript s_markedSquare;
        public const int PixelsPerSquare = 64;

        private static IUnityMarker s_attackMarker;
        private static IUnityMarker s_squareMarker;
        public IUnityMarker m_lootMarker;
        private IUnityMarker m_fogOfWar;

        /// <summary>
        /// Unity marker for effect
        /// </summary>
        private IUnityMarker m_squareEffect;

        /// <summary>
        /// The Ground effec object
        /// </summary>
        private GroundEffect m_groundEffect;

        #endregion fields

        #region properties

        public static int Width { get { return s_map.GetLength(0); } }

        public static int Height { get { return s_map.GetLength(1); } }

        public Entity OccupyingEntity { get; set; }

        public TerrainType TerrainType
        {
            get
            {
                return m_terrainType;
            }
            set
            {
                m_terrainType = value;
                gameObject.GetComponent<SpriteRenderer>().sprite = value.Sprite;
            }
        }

        public FogOfWarType FogOfWarType
        {
            get
            {
                return m_fogOfWarType;
            }
            set
            {
                m_fogOfWarType = value;
                m_fogOfWar.Renderer.sprite = value.Sprite;
            }
        }

        public bool Visible
        {
            get
            {
                return !m_fogOfWar.Visible;
            }
            private set
            {
                m_fogOfWar.Visible = !value;
                if (OccupyingEntity != null)
                {
                    OccupyingEntity.Image.Visible = value;
                    OccupyingEntity.Active = value;
                }
                if (m_lootMarker != null)
                {
                    m_lootMarker.Visible = value;
                }
            }
        }

        /// <summary>
        /// setting the groundeffect also replace sprite in squareEffect unityMarker
        /// </summary>
        public GroundEffect GroundEffect
        {
            get { return m_groundEffect; }
            set
            {
                if (value.EffectType != GroundEffectType.None)
                {
                    m_squareEffect.Renderer.sprite = value.Sprite;
                    m_squareEffect.Visible = true;
                }
                else
                {
                    m_squareEffect.Renderer.sprite = value.Sprite;
                    m_squareEffect.Visible = false;
                }
                m_groundEffect = value;
            }
        }

        public Traversability TraversingCondition { get { return TerrainType.TraversingCondition; } }

        public Opacity Opacity { get { return TerrainType.Opacity; } }

        public FogCoverType TileFogCoverType { get { return FogOfWarType.FogCoverType; } }

        #endregion properties

        #region public methods

        public static void Clear()
        {
            s_map = null;
        }

        public static void Init(ITerrainGenerator terrainGenerator, IMonsterPopulator monsterPopulator)
        {
            s_map = AddWallsAndLandingArea(terrainGenerator.GenerateMap(40, 40));
            monsterPopulator.PopulateMap(s_map);
            CorrectSquaresImages();
            InitMarkers();
            InitFog();
        }

        private static SquareScript[,] AddWallsAndLandingArea(SquareScript[,] squares)
        {
            for (int i = 0; i < squares.GetLength(0); i++)
            {
                squares[i, 0].TerrainType = TerrainType.Rock_Full;
                squares[i, squares.GetLength(1) - 1].TerrainType = TerrainType.Rock_Full;
            }

            for (int i = 0; i < squares.GetLength(1); i++)
            {
                squares[0, i].TerrainType = TerrainType.Rock_Full;
                squares[squares.GetLength(0) - 1, i].TerrainType = TerrainType.Rock_Full;
            }

            for (int i = 1; i < 10; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    squares[i, j].TerrainType = TerrainType.Empty;
                }
            }

            return squares;
        }

        public void AddLoot(Loot loot)
        {
            if (m_droppedLoot == null)
            {
                m_droppedLoot = loot;
                var prefabName = (m_droppedLoot.FuelCell) ? "FuelCell" : "Crystals";
                m_lootMarker = ((GameObject)MonoBehaviour.Instantiate(Resources.Load(prefabName),
                                                                         transform.position,
                                                                     Quaternion.identity)).GetComponent<MarkerScript>();
            }
            else
            {
                m_droppedLoot.AddLoot(loot);
            }
        }

        public static SquareScript GetSquare(int x, int y)
        {
            return s_map[x, y];
        }

        public void setLocation(int x, int y)
        {
            m_y = y;
            m_x = x;
        }

        public Vector2 getWorldLocation()
        {
            return this.gameObject.transform.position;
        }

        public SquareScript GetNextSquare(int x, int y)
        {
            x = Mathf.Min(x + m_x, s_map.GetLength(0) - 1);
            y = Mathf.Min(y + m_y, s_map.GetLength(1) - 1);
            x = Mathf.Max(x, 0);
            y = Mathf.Max(y, 0);
            return GetSquare(x, y);
        }

        public Loot TakeLoot()
        {
            if (m_droppedLoot == null)
            {
                return null;
            }

            var loot = m_droppedLoot;
            m_droppedLoot = null;
            m_lootMarker.DestroyGameObject();
            m_lootMarker = null;
            return loot;
        }

        public IEnumerable<SquareScript> GetNeighbours()
        {
            return GetNeighbours(false);
        }

        public IEnumerable<SquareScript> GetNeighbours(bool diagonals)
        {
            List<SquareScript> neighbours = new List<SquareScript>();

            if (m_x > 0) neighbours.Add(GetSquare(m_x - 1, m_y));
            if (m_x < s_map.GetLength(0)) neighbours.Add(GetSquare(m_x + 1, m_y));
            if (m_y > 0) neighbours.Add(GetSquare(m_x, m_y - 1));
            if (m_y < s_map.GetLength(1)) neighbours.Add(GetSquare(m_x, m_y + 1));
            if (diagonals)
            {
                if ((m_x > 0) && (m_y > 0)) neighbours.Add(GetSquare(m_x - 1, m_y - 1));
                if ((m_x < s_map.GetLength(0)) && (m_y > 0)) neighbours.Add(GetSquare(m_x + 1, m_y - 1));
                if ((m_x > 0) && (m_y < s_map.GetLength(1))) neighbours.Add(GetSquare(m_x - 1, m_y + 1));
                if ((m_x < s_map.GetLength(0)) && (m_y < s_map.GetLength(1))) neighbours.Add(GetSquare(m_x + 1, m_y + 1));
            }

            return neighbours;
        }

        public static void InitFog()
        {
            foreach (var square in s_map)
            {
                square.Visible = false;
                square.FogOfWarType = FogOfWarType.Full;
            }
        }

        public void FogOfWar()
        {
            // darken all previously seen squares
            if (Entity.Player.LastSeen != null)
            {
                foreach (var square in Entity.Player.LastSeen)
                {
                    square.Visible = false;
                    square.FogOfWarType = FogOfWarType.Full;
                }
            }

            var seen = SeenFrom();
            foreach (var tempSquare in seen)
            {
                tempSquare.Visible = true;
            }

            // Go over all fog covered tiles and fix fog in corners
            foreach (SquareScript tempSquare in s_map)
            {
                if (!seen.Contains(tempSquare))
                {
                    var upperTileHasFow = tempSquare.GetNextSquare(0, -1).m_fogOfWar.Visible;
                    var lowerTileHasFow = tempSquare.GetNextSquare(0, 1).m_fogOfWar.Visible;
                    var leftTileHasFow = tempSquare.GetNextSquare(-1, 0).m_fogOfWar.Visible;
                    var rightTileHasFow = tempSquare.GetNextSquare(1, 0).m_fogOfWar.Visible;

                    if (!leftTileHasFow && !lowerTileHasFow && rightTileHasFow && upperTileHasFow) tempSquare.FogOfWarType = FogOfWarType.Top_Right_Corner;
                    else if (leftTileHasFow && !lowerTileHasFow && !rightTileHasFow && upperTileHasFow) tempSquare.FogOfWarType = FogOfWarType.Top_Left_Corner;
                    else if (leftTileHasFow && lowerTileHasFow && !rightTileHasFow && !upperTileHasFow) tempSquare.FogOfWarType = FogOfWarType.Bottom_Left_Corner;
                    else if (!leftTileHasFow && lowerTileHasFow && rightTileHasFow && !upperTileHasFow) tempSquare.FogOfWarType = FogOfWarType.Bottom_Right_Corner;
                    else tempSquare.FogOfWarType = FogOfWarType.Full;
                }
            }

            // Go over all visible tiles and cover corners
            var soonToBeInvisible = new List<SquareScript>();
            foreach (SquareScript tempSquare in seen)
            {
                var upperTileHasFow = !tempSquare.GetNextSquare(0, -1).Visible && tempSquare.GetNextSquare(0, -1).TileFogCoverType == FogCoverType.Full;
                var lowerTileHasFow = !tempSquare.GetNextSquare(0, 1).Visible && tempSquare.GetNextSquare(0, 1).TileFogCoverType == FogCoverType.Full;
                var leftTileHasFow = !tempSquare.GetNextSquare(-1, 0).Visible && tempSquare.GetNextSquare(-1, 0).TileFogCoverType == FogCoverType.Full;
                var rightTileHasFow = !tempSquare.GetNextSquare(1, 0).Visible && tempSquare.GetNextSquare(1, 0).TileFogCoverType == FogCoverType.Full;

                if (!leftTileHasFow && !lowerTileHasFow && rightTileHasFow && upperTileHasFow) { tempSquare.m_fogOfWar.Renderer.sprite = SpriteManager.Fog_Top_Right_Corner; soonToBeInvisible.Add(tempSquare); }
                else if (leftTileHasFow && !lowerTileHasFow && !rightTileHasFow && upperTileHasFow) { tempSquare.m_fogOfWar.Renderer.sprite = SpriteManager.Fog_Top_Left_Corner; soonToBeInvisible.Add(tempSquare); }
                else if (leftTileHasFow && lowerTileHasFow && !rightTileHasFow && !upperTileHasFow) { tempSquare.m_fogOfWar.Renderer.sprite = SpriteManager.Fog_Bottom_Left_Corner; soonToBeInvisible.Add(tempSquare); }
                else if (!leftTileHasFow && lowerTileHasFow && rightTileHasFow && !upperTileHasFow) { tempSquare.m_fogOfWar.Renderer.sprite = SpriteManager.Fog_Bottom_Right_Corner; soonToBeInvisible.Add(tempSquare); }
            }

            foreach (SquareScript tempSquare in soonToBeInvisible)
            {
                tempSquare.Visible = false;
            }
            seen.ExceptWith(soonToBeInvisible);
            Entity.Player.LastSeen = seen;
        }

        #endregion public methods

        #region private methods

        private static void CorrectSquaresImages()
        {
            //throw new NotImplementedException();
        }

        private static void InitMarkers()
        {
            s_squareMarker = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("squareSelectionBox"), new Vector2(1000000, 1000000), Quaternion.identity)).GetComponent<MarkerScript>();
            s_attackMarker = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("AttackMark"), new Vector2(1000000, 1000000), Quaternion.identity)).GetComponent<MarkerScript>();
        }

        private void Awake()
        {
            m_fogOfWar = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("FogOfWar"), transform.position, Quaternion.identity)).GetComponent<MarkerScript>();
            FogOfWarType = FogOfWarType.Full;

            m_squareEffect = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("EmptyMarker"), transform.position, Quaternion.identity)).GetComponent<MarkerScript>();
            GroundEffect = GroundEffect.NoEffect;

            Visible = true;
        }

        private HashSet<SquareScript> SeenFrom()
        {
            HashSet<SquareScript> seenSquaresSet = new HashSet<SquareScript>();

            int range = 32;
            var amountOfSquaresToCheck = range * 8;
            var angleSlice = 360.0f / amountOfSquaresToCheck;
            for (float angle = 0; angle < 360; angle += angleSlice)
            {
                seenSquaresSet.UnionWith(FindVisibleSquares(angle, range));
            }

            return seenSquaresSet;
        }

        private IEnumerable<SquareScript> FindVisibleSquares(float angle, int leftInRange)
        {
            var layerMask = 1 << LayerMask.NameToLayer("Ground");

            // return all colliders that the ray passes through
            var rayHits = Physics2D.RaycastAll(transform.position, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), layerMask);
            foreach (var rayHit in rayHits)
            {
                if (leftInRange > 0)
                {
                    yield return rayHit.collider.gameObject.GetComponent<SquareScript>();
                    if (Blocking(rayHit.collider.gameObject.GetComponent<SquareScript>()))
                    {
                        yield break;
                    }
                }
                leftInRange--;
            }
        }

        private bool Blocking(SquareScript square)
        {
            return square != null && square.Opacity == Opacity.Blocking;
        }

        // Use this for initialization
        private void Start()
        {
            var location = s_map.GetCoordinates(this);
            m_x = (int)location.x;
            m_y = (int)location.y;
        }

        // Update is called once per frame
        private void Update()
        {
        }

        private void OnMouseOver()
        {
            if (OccupyingEntity != null && !(OccupyingEntity is PlayerEntity) && !m_fogOfWar.Visible)
            {
                s_attackMarker.Mark(transform.position);

                s_markedSquare = this;
            }
            else
            {
                s_squareMarker.Mark(transform.position);
                s_markedSquare = this;
            }
        }

        private void OnMouseExit()
        {
            s_squareMarker.Unmark();
            s_attackMarker.Unmark();
        }

        #endregion private methods
    }

    #endregion SquareScript

    #region TerrainType

    public class TerrainType
    {
        private List<Sprite> m_sprites;

        public Traversability TraversingCondition { get; private set; }

        public Opacity Opacity { get; private set; }

        public Sprite Sprite
        {
            get { return m_sprites[UnityEngine.Random.Range(0, m_sprites.Count - 1)]; }
        }

        public TerrainType(List<Sprite> sprites, Traversability traversingCondition, Opacity opacity)
        {
            m_sprites = sprites;
            TraversingCondition = traversingCondition;
            Opacity = opacity;
        }

        public TerrainType(Sprite sprite, Traversability traversingCondition, Opacity opacity)
        {
            m_sprites = new List<Sprite> { sprite };
            TraversingCondition = traversingCondition;
            Opacity = opacity;
        }

        public static TerrainType Empty = new TerrainType(SpriteManager.Empty, Traversability.Walkable, Opacity.SeeThrough);
        public static TerrainType Rock_Full = new TerrainType(new List<Sprite> { SpriteManager.Rock_Full1, SpriteManager.Rock_Full2, SpriteManager.Rock_Full3, SpriteManager.Rock_Full4 }, Traversability.Blocking, Opacity.Blocking);
        public static TerrainType Rock_Bottom_Right_Corner = new TerrainType(SpriteManager.Rock_Bottom_Right_Corner, Traversability.Blocking, Opacity.SeeThrough);
        public static TerrainType Rock_Bottom_Left_Corner = new TerrainType(SpriteManager.Rock_Bottom_Left_Corner, Traversability.Blocking, Opacity.SeeThrough);
        public static TerrainType Rock_Top_Right_Corner = new TerrainType(SpriteManager.Rock_Top_Right_Corner, Traversability.Blocking, Opacity.SeeThrough);
        public static TerrainType Rock_Top_Left_Corner = new TerrainType(SpriteManager.Rock_Top_Left_Corner, Traversability.Blocking, Opacity.SeeThrough);
        public static TerrainType Rock_Side_Bottom = new TerrainType(SpriteManager.Rock_Side_Bottom, Traversability.Blocking, Opacity.SeeThrough);
        public static TerrainType Rock_Side_Left = new TerrainType(SpriteManager.Rock_Side_Left, Traversability.Blocking, Opacity.SeeThrough);
        public static TerrainType Rock_Side_Top = new TerrainType(SpriteManager.Rock_Side_Top, Traversability.Blocking, Opacity.SeeThrough);
        public static TerrainType Rock_Side_Right = new TerrainType(SpriteManager.Rock_Side_Right, Traversability.Blocking, Opacity.SeeThrough);
        public static TerrainType Rock_Crater = new TerrainType(SpriteManager.Rock_Crater, Traversability.Blocking, Opacity.Blocking);
        public static TerrainType Rock_Crystal = new TerrainType(SpriteManager.Rock_Crystal, Traversability.Blocking, Opacity.Blocking);
        public static TerrainType Spaceship_Top_Left = new TerrainType(SpriteManager.Spaceship_Top_Left, Traversability.Blocking, Opacity.SeeThrough);
        public static TerrainType Spaceship_Top_Right = new TerrainType(SpriteManager.Spaceship_Top_Right, Traversability.Blocking, Opacity.SeeThrough);
        public static TerrainType Spaceship_Bottom_Left = new TerrainType(SpriteManager.Spaceship_Bottom_Left, Traversability.Blocking, Opacity.SeeThrough);
        public static TerrainType Spaceship_Bottom_Right = new TerrainType(SpriteManager.Spaceship_Bottom_Right, Traversability.Blocking, Opacity.SeeThrough);
        public static TerrainType Fuel_Cell = new TerrainType(SpriteManager.Fuel_Cell, Traversability.Walkable, Opacity.SeeThrough);
        //public static TerrainType Tentacle_Monster= new TerrainType(SpriteManager.Tentacle_Monster, Traversability.Blocking);
        //public static TerrainType Astornaut= new TerrainType(SpriteManager.Astronaut_Front, Traversability.Blocking);
    }

    #endregion TerrainType

    #region FogOfWarType

    public class FogOfWarType
    {
        public FogCoverType FogCoverType { get; private set; }

        public Sprite Sprite { get; private set; }

        public FogOfWarType(Sprite sprite, FogCoverType fogCoverType)
        {
            Sprite = sprite;
            FogCoverType = fogCoverType;
        }

        public static FogOfWarType None = new FogOfWarType(SpriteManager.Empty, FogCoverType.None);
        public static FogOfWarType Full = new FogOfWarType(SpriteManager.Fog_Full, FogCoverType.Full);
        public static FogOfWarType Bottom_Right_Corner = new FogOfWarType(SpriteManager.Fog_Bottom_Right_Corner, FogCoverType.Partial);
        public static FogOfWarType Bottom_Left_Corner = new FogOfWarType(SpriteManager.Fog_Bottom_Left_Corner, FogCoverType.Partial);
        public static FogOfWarType Top_Right_Corner = new FogOfWarType(SpriteManager.Fog_Top_Right_Corner, FogCoverType.Partial);
        public static FogOfWarType Top_Left_Corner = new FogOfWarType(SpriteManager.Fog_Top_Left_Corner, FogCoverType.Partial);
    }

    #endregion FogOfWarType

    #region SpriteManager

    public class SpriteManager
    {
        private static Dictionary<string, Sprite> s_sprites;

        private static Sprite GetSprite(string spriteName)
        {
            if (s_sprites == null)
            {
                s_sprites = Resources.LoadAll<Sprite>("Sprites").ToDictionary(sprite => sprite.name);
            }
            return s_sprites.Get(spriteName, "Square sprites dictionary");
        }

        public static Sprite Empty = GetSprite("Terrain_0");
        public static Sprite Rock_Bottom_Right_Corner = GetSprite("Terrain_1");
        public static Sprite Rock_Bottom_Left_Corner = GetSprite("Terrain_2");
        public static Sprite Rock_Top_Right_Corner = GetSprite("Terrain_3");
        public static Sprite Rock_Top_Left_Corner = GetSprite("Terrain_4");
        public static Sprite Rock_Full1 = GetSprite("Terrain_5");
        public static Sprite Rock_Full2 = GetSprite("Terrain_6");
        public static Sprite Rock_Full3 = GetSprite("Terrain_7");
        public static Sprite Rock_Full4 = GetSprite("Terrain_8");
        public static Sprite Rock_Side_Bottom = GetSprite("Terrain_9");
        public static Sprite Rock_Side_Left = GetSprite("Terrain_10");
        public static Sprite Rock_Side_Top = GetSprite("Terrain_11");
        public static Sprite Rock_Side_Right = GetSprite("Terrain_12");
        public static Sprite Rock_Crater = GetSprite("Terrain_13");
        public static Sprite Spaceship_Top_Left = GetSprite("Terrain_14");
        public static Sprite Spaceship_Top_Right = GetSprite("Terrain_15");
        public static Sprite Spaceship_Bottom_Left = GetSprite("Terrain_16");
        public static Sprite Spaceship_Bottom_Right = GetSprite("Terrain_17");
        public static Sprite Rock_Crystal = GetSprite("Terrain_18");
        public static Sprite Fog_Full = GetSprite("Terrain_19");
        public static Sprite Fog_Side_Right = GetSprite("Terrain_20");
        public static Sprite Fog_Side_Bottom = GetSprite("Terrain_21");
        public static Sprite Fog_Side_Left = GetSprite("Terrain_22");
        public static Sprite Fog_Side_Top = GetSprite("Terrain_23");
        public static Sprite Fog_Top_Right_Corner = GetSprite("Terrain_24");
        public static Sprite Fog_Bottom_Right_Corner = GetSprite("Terrain_25");
        public static Sprite Fog_Bottom_Left_Corner = GetSprite("Terrain_26");
        public static Sprite Fog_Top_Left_Corner = GetSprite("Terrain_27");
        public static Sprite Fuel_Cell = GetSprite("Entities_0");
        public static Sprite Tentacle_Monster = GetSprite("Entities_1");
        public static Sprite Astronaut_Right = GetSprite("Entities_2");
        public static Sprite Astronaut_Front = GetSprite("Entities_3");
        public static Sprite Crystal = GetSprite("Entities_4");
        public static Sprite Mouse_Hover = GetSprite("Markers_0");
        public static Sprite Blueish_Marker = GetSprite("Markers_1");
        public static Sprite Green_Marker = GetSprite("Markers_2");
        public static Sprite Tentacle_Marker = GetSprite("Markers_3");
        public static Sprite CardiacIcon = GetSprite("Markers_4");
        public static Sprite LightningIcon = GetSprite("Markers_5");
        public static Sprite OxygenTank = GetSprite("Markers_6");
        public static Sprite EmptyMarker = GetSprite("Markers_8");
        public static Sprite Acid = GetSprite("Markers_9");
    }

    #endregion SpriteManager

    #region TmxManager

    public class TmxManager
    {
        public static void HandleTerrain(string gid, int x, int y, Vector3 universalLocation)
        {
            var tile = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("SquareTileResource"), universalLocation, Quaternion.identity));
            var script = tile.GetComponent<SquareScript>();
            script.setLocation(x, y);
            //SquareScript.SetSquare(script, x, y);
            switch (gid)
            {
                case "0": script.TerrainType = TerrainType.Empty; break;
                case "1": script.TerrainType = TerrainType.Empty; break;
                case "2": script.TerrainType = TerrainType.Rock_Bottom_Right_Corner; break;
                case "3": script.TerrainType = TerrainType.Rock_Bottom_Left_Corner; break;
                case "4": script.TerrainType = TerrainType.Rock_Top_Right_Corner; break;
                case "5": script.TerrainType = TerrainType.Rock_Top_Left_Corner; break;
                case "6":
                case "7":
                case "8":
                case "9": script.TerrainType = TerrainType.Rock_Full; break;
                case "10": script.TerrainType = TerrainType.Rock_Side_Bottom; break;
                case "11": script.TerrainType = TerrainType.Rock_Side_Left; break;
                case "12": script.TerrainType = TerrainType.Rock_Side_Top; break;
                case "13": script.TerrainType = TerrainType.Rock_Side_Right; break;
                case "14": script.TerrainType = TerrainType.Rock_Crater; break;
                case "15": script.TerrainType = TerrainType.Spaceship_Top_Left; break;
                case "16": script.TerrainType = TerrainType.Spaceship_Top_Right; break;
                case "17": script.TerrainType = TerrainType.Spaceship_Bottom_Left; break;
                case "18": script.TerrainType = TerrainType.Spaceship_Bottom_Right; break;
                case "19": script.TerrainType = TerrainType.Rock_Crystal; break;
                default: script.TerrainType = TerrainType.Empty; break;
            }
        }

        public static void HandleEntity(string gid, int x, int y)
        {
            switch (gid)
            {
                case "33": SquareScript.GetSquare(x, y).AddLoot(new Loot(16, true)); break;
                case "34": EnemiesManager.CreateTentacleMonster(x, y); break;
                case "35": break;
                case "36": break;
                case "37": SquareScript.GetSquare(x, y).AddLoot(new Loot(UnityEngine.Random.Range(0, 10), false)); break;
                case "38": EnemiesManager.CreateHive(x, y); break;
                case "39": EnemiesManager.CreateSlime(x, y); break;
            }
        }

        public static void HandleMarker(string gid, int x, int y)
        {
            switch (gid)
            {
                case "68": MapSceneScript.SetEvent(() => EnemiesManager.CreateTentacleMonster(x, y), Marker.OnEscape); break;
                case "74": MapSceneScript.AddGroundEffect(GroundEffect.StandardAcid, x, y); break;
            }
        }
    }

    #endregion TmxManager
}