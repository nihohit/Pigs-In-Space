using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using Assets.scripts.UnityBase;

public enum Traversability { Walkable, Flyable, Blocking }
public enum Opacity { Blocking, SeeThrough}

#region SquareScript
public class SquareScript : MonoBehaviour
{
    #region fields

    private static SquareScript[,] s_map;
    private int m_x, m_y;
    private Loot m_droppedLoot;
    private IUnityMarker m_fogOfWar;
    private TerrainType m_terrainType;
    private static IUnityMarker s_squareMarker;
    private static IUnityMarker s_attackMarker;
    public static SquareScript s_markedSquare;

    #endregion fields

    #region properties

    public Entity OccupyingEntity { get; set; }

    public IUnityMarker LootMarker { get; private set; }

    public TerrainType TerrainType
    {
        get
        {
            return m_terrainType;
        }
        set
        {
            m_terrainType = value;
            var sr = gameObject.GetComponent<SpriteRenderer>();
            sr.sprite = value.Sprite;
        }
    }

    public Traversability TraversingCondition { get { return TerrainType.TraversingCondition; } }

    public Opacity Opacity { get { return TerrainType.Opacity; } }

    #endregion

    #region public methods

    private void Awake()
    {
        m_fogOfWar = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("FogOfWar"),
                                                                     transform.position,
                                                                 Quaternion.identity)).GetComponent<MarkerScript>();
        m_fogOfWar.Unmark();
    }

    public static void LoadFromTMX(string filename)
    {
        s_squareMarker = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("squareSelectionBox"), new Vector2(1000000, 1000000), Quaternion.identity)).GetComponent<MarkerScript>();
        s_attackMarker = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("AttackMark"), new Vector2(1000000, 1000000), Quaternion.identity)).GetComponent<MarkerScript>();

        var root = new XmlDocument();
        root.Load(filename);
        var mapWidth = Int32.Parse(root.SelectSingleNode(@"map/@width").Value);
        var mapHeight = Int32.Parse(root.SelectSingleNode(@"map/@height").Value);
        var terrain = root.SelectNodes(@"map/layer[@name='Terrain']/data/tile").Cast<XmlNode>().Select(node => node.Attributes["gid"].Value).ToList();
        var entities = root.SelectNodes(@"map/layer[@name='Entities']/data/tile").Cast<XmlNode>().Select(node => node.Attributes["gid"].Value).ToList();
        var markers = root.SelectNodes(@"map/layer[@name='Markers']/data/tile").Cast<XmlNode>().Select(node => node.Attributes["gid"].Value).ToList();

        s_map = new SquareScript[mapWidth, mapHeight];
        var squareSize = 0.64f;
        var currentPosition = Vector3.zero;
        for (int j = mapHeight - 1; j >= 0; j--) // invert y axis
        {
            for (int i = 0; i < mapWidth; i++)
            {
                TmxManager.HandleTerrain(terrain[j * mapWidth + i], i, j, currentPosition);
                TmxManager.HandleEntity(entities[j * mapWidth + i], i, j);
                TmxManager.HandleMarker(markers[j * mapWidth + i], i, j);
                currentPosition = new Vector3(currentPosition.x + squareSize, currentPosition.y, 0);
            }
            currentPosition = new Vector3(0, currentPosition.y + squareSize, 0);
        }
    }

    public void AddLoot(Loot loot)
    {
        if (m_droppedLoot == null)
        {
            m_droppedLoot = loot;
            var prefabName = (m_droppedLoot.FuelCell) ? "FuelCell" : "Crystals";
            LootMarker = ((GameObject)MonoBehaviour.Instantiate(Resources.Load(prefabName),
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

    public static void SetSquare(SquareScript square, int x, int y)
    {
        s_map[x, y] = square;
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

    public static int Weidth()
    {
        return s_map.GetLength(0);
    }

    public static int Height()
    {
        return s_map.GetLength(1);
    }

    internal Loot TakeLoot()
    {
        if (m_droppedLoot == null)
        {
            return null;
        }

        var loot = m_droppedLoot;
        m_droppedLoot = null;
        LootMarker.DestroyGameObject();
        LootMarker = null;
        return loot;
    }

    public IEnumerable<SquareScript> GetNeighbours()
    {
        List<SquareScript> neighbours = new List<SquareScript>();

        if (m_x > 0) neighbours.Add(GetSquare(m_x - 1, m_y));
        if (m_x < s_map.GetLength(0)) neighbours.Add(GetSquare(m_x + 1, m_y));
        if (m_y > 0) neighbours.Add(GetSquare(m_x, m_y - 1));
        if (m_y < s_map.GetLength(1)) neighbours.Add(GetSquare(m_x, m_y + 1));

        return neighbours;
    }

    public void FogOfWar()
    {
        foreach (SquareScript tempSquare in s_map)
        {
            tempSquare.Visible(false);
        }

        foreach (var tempSquare in SeenFrom())
        {
            tempSquare.Visible(true);
        }
    }

    #endregion public methods

    #region private methods

    private void Visible(bool visible)
    {
        m_fogOfWar.Visible(!visible);
        if (OccupyingEntity != null)
        {
            OccupyingEntity.Image.Visible(visible);
            OccupyingEntity.SetActive(visible);
        }
        if (LootMarker != null)
        {
            LootMarker.Visible(visible);
        }
    }

    private IEnumerable<SquareScript> SeenFrom()
    {
        List<SquareScript> list = new List<SquareScript>();

        int range = 16;
        var amountOfSquaresToCheck = range * 8;
        var angleSlice = 360.0f / amountOfSquaresToCheck;
        for (float angle = 0; angle < 360; angle += angleSlice)
        {
            list.AddRange(FindVisibleSquares(angle, range));
        }

        return list;
    }

    private IEnumerable<SquareScript> FindVisibleSquares(float angle, int leftInRange)
    {
        var layerMask = 1 << LayerMask.NameToLayer("Ground");

        // return all colliders that the ray passes through
        var rayHits = Physics2D.RaycastAll(transform.position, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), layerMask);
        foreach (var rayHit in rayHits)
        {
            if(leftInRange > 0)
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
        if (OccupyingEntity != null && !(OccupyingEntity is PlayerEntity))
        {
            s_attackMarker.Position = transform.position;
            s_markedSquare = this;
        }
        else
        {
            s_squareMarker.Position = transform.position;
            s_markedSquare = this;
        }
    }

    private void OnMouseExit()
    {
        s_squareMarker.Position = new Vector2(1000000, 1000000);
        s_attackMarker.Position = new Vector2(1000000, 1000000);
    }

    #endregion Private methods
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
        return s_sprites[spriteName];
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
        SquareScript.SetSquare(script, x, y);
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
            case "34": Entity.CreateTentacleMonster(x, y); break;
            case "35": break;
            case "36": break;
            case "37": SquareScript.GetSquare(x, y).AddLoot(new Loot(UnityEngine.Random.Range(0, 10), false)); break;
            case "38": Entity.CreateHive(x, y); break;
        }
    }

    public static void HandleMarker(string gid, int x, int y)
    {
        switch (gid)
        {
            case "68": MapSceneScript.SetEvent(() => Entity.CreateTentacleMonster(x, y), Marker.OnEscape); break;
        }
    }
}

#endregion TmxManager