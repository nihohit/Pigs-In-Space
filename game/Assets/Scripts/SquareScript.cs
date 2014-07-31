using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public enum Traversability { Walkable, Flyable, Blocking }

#region SquareScript

public class SquareScript : MonoBehaviour
{
    #region fields

    private static SquareScript[,] s_map;
	private int m_x,m_y;
    private Loot m_droppedLoot;
    private SpriteRenderer m_fogOfWar;
    private TerrainType m_terrainType;
    private static SpriteRenderer s_squareMarker;
    public static SquareScript s_markedSquare;

    #endregion

    #region properties

    public Entity OccupyingEntity { get; set; }


    public SpriteRenderer LootRenderer { get; private set; }

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

    #endregion

    #region public methods

    void Awake()
    {
        m_fogOfWar = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("FogOfWar"),
                                                                     transform.position,
                                                                 Quaternion.identity)).GetComponent<SpriteRenderer>();
        m_fogOfWar.enabled = false;
    }

    public static void LoadFromTMX(string filename)
    {
        s_squareMarker = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("squareSelectionBox"), Vector2.zero, Quaternion.identity)).GetComponent<SpriteRenderer>();

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
                Debug.Log(i + " , " + j);
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
            LootRenderer = ((GameObject)MonoBehaviour.Instantiate(Resources.Load(prefabName),
                                                                     transform.position,
                                                                 Quaternion.identity)).GetComponent<SpriteRenderer>();
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
        if(m_droppedLoot == null)
        {
            return null;
        }

        var loot = m_droppedLoot;
        m_droppedLoot = null;
        GameObject.Destroy(LootRenderer.gameObject);
        return loot;
    }

    public IEnumerable<SquareScript> GetNeighbours()
    {
        List<SquareScript> neighbours = new List<SquareScript>();

        if(m_x > 0) neighbours.Add(GetSquare(m_x - 1, m_y));
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

        foreach(var tempSquare in SeenFrom())
        {
            tempSquare.Visible(true);
        }
    }

    #endregion

    #region private methods

    private void Visible(bool visible)
    {
        m_fogOfWar.enabled = !visible;
        if (OccupyingEntity != null)
        {
            OccupyingEntity.Image.enabled = visible;
            OccupyingEntity.SetActive(visible);
        }
        if (LootRenderer != null)
        {
            LootRenderer.enabled = visible;
        }
    }

    private IEnumerable<SquareScript> SeenFrom()
    {
        List<SquareScript> list = new List<SquareScript>();

        int range = 10;
        var amountOfSquaresToCheck = range * 4;
        var angleSlice = 360.0f / amountOfSquaresToCheck;
        for (float angle = 0; angle < 360; angle += angleSlice)
        {
            list.AddRange(FindVisibleSquares(angle));
        }

        return list;
    }

    private IEnumerable<SquareScript> FindVisibleSquares(float angle)
    {
        var layerMask = 1 << LayerMask.NameToLayer("Ground");

        // return all colliders that the ray passes through
        var rayHits = Physics2D.RaycastAll(transform.position, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), layerMask);
        foreach (var rayHit in rayHits)
        {
            yield return rayHit.collider.gameObject.GetComponent<SquareScript>();
            if (Blocking(rayHit.collider.gameObject.GetComponent<SquareScript>()))
            {
                yield break;
            }
        }
    }

    private bool Blocking(SquareScript square)
    {
        return square != null &&
            square.TraversingCondition == Traversability.Blocking;
    }

	// Use this for initialization
	void Start () 
	{
		var location = s_map.GetCoordinates (this);
		m_x = (int)location.x;
		m_y = (int)location.y;
	}
	
	// Update is called once per frame
	void Update () 
	{

    }

    void OnMouseOver()
    {
		s_squareMarker.transform.position = transform.position;
        s_markedSquare = this;
    }

    #endregion
	
}

#endregion

#region TerrainType
public class TerrainType
{
    private List<Sprite> m_sprites;
    public Traversability TraversingCondition { get; private set; }
    public Sprite Sprite 
    {
        get { return m_sprites[UnityEngine.Random.Range(0, m_sprites.Count - 1)]; }
    }

    public TerrainType(List<Sprite> sprites, Traversability traversingCondition)
    {
        m_sprites = sprites;
        TraversingCondition = traversingCondition;
    }

    public TerrainType(Sprite sprite, Traversability traversingCondition)
    {
        m_sprites = new List<Sprite>{sprite};
        TraversingCondition = traversingCondition;
    }

    public static TerrainType Empty = new TerrainType( SpriteManager.Empty, Traversability.Walkable);
    public static TerrainType Rock_Full = new TerrainType(new List<Sprite> { SpriteManager.Rock_Full1, SpriteManager.Rock_Full2, SpriteManager.Rock_Full3, SpriteManager.Rock_Full4 }, Traversability.Blocking);
    public static TerrainType Rock_Bottom_Right_Corner= new TerrainType(SpriteManager.Rock_Bottom_Right_Corner, Traversability.Blocking);
    public static TerrainType Rock_Bottom_Left_Corner= new TerrainType(SpriteManager.Rock_Bottom_Left_Corner, Traversability.Blocking);
    public static TerrainType Rock_Top_Right_Corner= new TerrainType(SpriteManager.Rock_Top_Right_Corner, Traversability.Blocking);
    public static TerrainType Rock_Top_Left_Corner= new TerrainType(SpriteManager.Rock_Top_Left_Corner, Traversability.Blocking);
    public static TerrainType Rock_Side_Bottom= new TerrainType(SpriteManager.Rock_Side_Bottom, Traversability.Blocking);
    public static TerrainType Rock_Side_Left= new TerrainType(SpriteManager.Rock_Side_Left, Traversability.Blocking);
    public static TerrainType Rock_Side_Top= new TerrainType(SpriteManager.Rock_Side_Top, Traversability.Blocking);
    public static TerrainType Rock_Side_Right= new TerrainType(SpriteManager.Rock_Side_Right, Traversability.Blocking);
    public static TerrainType Rock_Crater= new TerrainType(SpriteManager.Rock_Crater, Traversability.Blocking);
    public static TerrainType Rock_Crystal= new TerrainType(SpriteManager.Rock_Crystal, Traversability.Blocking);
    public static TerrainType Spaceship_Top_Left= new TerrainType(SpriteManager.Spaceship_Top_Left, Traversability.Blocking);
    public static TerrainType Spaceship_Top_Right= new TerrainType(SpriteManager.Spaceship_Top_Right, Traversability.Blocking);
    public static TerrainType Spaceship_Bottom_Left= new TerrainType(SpriteManager.Spaceship_Bottom_Left, Traversability.Blocking);
    public static TerrainType Spaceship_Bottom_Right= new TerrainType(SpriteManager.Spaceship_Bottom_Right, Traversability.Blocking);
    public static TerrainType Fuel_Cell= new TerrainType(SpriteManager.Fuel_Cell, Traversability.Walkable);
    public static TerrainType Tentacle_Monster= new TerrainType(SpriteManager.Tentacle_Monster, Traversability.Blocking);
    public static TerrainType Astornaut= new TerrainType(SpriteManager.Astronaut_Front, Traversability.Blocking);	
}
#endregion

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

#endregion

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
            case "34": MapSceneScript.CreateTentacleMonster(x, y); break;
            case "35": break;
            case "36": break;
            case "37": SquareScript.GetSquare(x, y).AddLoot(new Loot(UnityEngine.Random.Range(0, 10), false)); break;
            case "38": MapSceneScript.CreateHive(x, y); break;
        }
    }

    public static void HandleMarker(string gid, int x, int y)
    {
        switch (gid)
        {
            case "68": MapSceneScript.SetEvent(() => MapSceneScript.CreateTentacleMonster(x, y), Marker.OnEscape); break;
        }
    }
}

#endregion
