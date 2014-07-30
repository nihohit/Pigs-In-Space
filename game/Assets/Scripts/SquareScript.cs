using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml;
using UnityEngine;

public enum Traversability { Walkable, Flyable, Blocking }

public class SquareScript : MonoBehaviour
{
    #region fields

    private static SquareScript[,] s_map;
	private int m_x,m_y;
    private Loot m_droppedLoot;
    private SpriteRenderer m_lootRenderer;
    private static SpriteRenderer s_squareMarker;
    public static SquareScript s_markedSquare;

    #endregion

    #region properties

    public Traversability TraversingCondition { get; set; }

    public Entity OccupyingEntity { get; set; }

    #endregion

    #region public methods

    public static void LoadFromTMX(string filename)
    {
        s_squareMarker = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("squareSelectionBox"), Vector2.zero, Quaternion.identity)).GetComponent<SpriteRenderer>();
        //Unity does not support .Net 3.5 or higher, until we find a way to do that ...
        //var mapWidth = ((IEnumerable)tiledMapXmlRoot.XPathEvaluate("/@width")).Cast<XAttribute>().Select(a => Int32.Parse(a.Value)).First();
        //var mapHeight = ((IEnumerable)tiledMapXmlRoot.XPathEvaluate("/@height")).Cast<XAttribute>().Select(a => Int32.Parse(a.Value)).First();
        //var tileNames = ((IEnumerable)tiledMapXmlRoot.XPathEvaluate("layer/data/tile/@gid")).Cast<XAttribute>().Select(a => "GameTiles_" + (Int32.Parse(a.Value) - 1)).ToList();

        var mapWidth = 0;
        var mapHeight = 0;
		var terrain = new List<string> ();
		var entities = new List<string> ();
		var markers = new List<string> ();
        XmlTextReader reader = new XmlTextReader(filename);
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element: // The node is an element.
                    if (reader.Name == "map")
                    {
                        while (reader.MoveToNextAttribute()) // Read the attributes.
                        {
                            if (reader.Name == "width")
                            {
                                mapWidth = Int32.Parse(reader.Value);
                            }
                            if (reader.Name == "height")
                            {
                                mapHeight = Int32.Parse(reader.Value);
                            }
                        }
                    }
							
				else if (reader.Name == "layer" && reader.MoveToNextAttribute() && reader.Name == "name" && reader.Value == "Terrain") // Terrain layer
				{
					while (reader.Read())
					{
						switch (reader.NodeType)
						{
						case XmlNodeType.Element: // The node is an element.
							if (reader.Name == "tile")
							{
								while (reader.MoveToNextAttribute()) // Read the attributes.
								{
									if (reader.Name == "gid")
									{
										terrain.Add(reader.Value);
									}
								}
							}

							else if (reader.Name == "layer" && reader.MoveToNextAttribute() && reader.Name == "name" && reader.Value == "Entities") // Entities layer
							{
								while (reader.Read())
								{
									switch (reader.NodeType)
									{
									case XmlNodeType.Element: // The node is an element.
										if (reader.Name == "tile")
										{
											while (reader.MoveToNextAttribute()) // Read the attributes.
											{
												if (reader.Name == "gid")
												{
													entities.Add(reader.Value);
												}
											}
										}

										else if (reader.Name == "layer" && reader.MoveToNextAttribute() && reader.Name == "name" && reader.Value == "Markers") // Entities layer
										{
											while (reader.Read())
											{
												switch (reader.NodeType)
												{
												case XmlNodeType.Element: // The node is an element.
													if (reader.Name == "tile")
													{
														while (reader.MoveToNextAttribute()) // Read the attributes.
														{
															if (reader.Name == "gid")
															{
																markers.Add(reader.Value);
															}
														}
													}

													break;
													
												case XmlNodeType.Text: //Display the text in each element.
													break;
													
												case XmlNodeType.EndElement: //Display the end of the element.
													break;
												}
											}
										}
										break;
										
									case XmlNodeType.Text: //Display the text in each element.
										break;
										
									case XmlNodeType.EndElement: //Display the end of the element.
										break;
                                            }
                                        }
                                    }

                                    break;

                                case XmlNodeType.Text: //Display the text in each element.
                                    break;

                                case XmlNodeType.EndElement: //Display the end of the element.
                                    break;
                            }
                        }
                    }

                    break;

                case XmlNodeType.Text: //Display the text in each element.
                    break;

                case XmlNodeType.EndElement: //Display the end of the element.
                    break;
            }
        }

        s_map = new SquareScript[mapWidth, mapHeight];
        var squareSize = 0.64f;
        var currentPosition = Vector3.zero;
        for (int j = mapHeight - 1; j >= 0; j--) // invert y axis
        {
            for (int i = 0; i < mapWidth; i++)
            {
                TmxManager.HandleTerrain(terrain[j * mapWidth + i], i, j, currentPosition);
                TmxManager.HandleEntity(entities[j * mapWidth + i], i, j);
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
            m_lootRenderer = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("Crystals"),
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
        GameObject.Destroy(m_lootRenderer.gameObject);
        return loot;
    }

    #endregion

    #region private methods

    private static GameObject CreateTile(Vector3 position, string tileResourceName)
	{
		var tile = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("SquareTileResource"), position, Quaternion.identity));
		TileManager.SetTileFromLayerAndGid(tile, tileResourceName);
		return tile;
	}

	// convert gid number to prefab name
	private static string GetPrefabName(int gid)
	{
		const string tilePrefabPrefix = "tiles7_";
		const string EmptyPrefab = "tiles7_6";
		if (gid >= 33 && gid <= 36)
		{
			// random rock tile
			return tilePrefabPrefix + UnityEngine.Random.Range(32, 35);
		}
		if (gid > 0)
		{
			return tilePrefabPrefix + (gid - 1);
		}
		return EmptyPrefab;
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

public class TileManager
{
	public static void EmptyTile(GameObject tile){SetTile(tile, SpriteManager7.Empty, Traversability.Walkable);}
	public static void Rock_Bottom_Right_Corner(GameObject tile){SetTile(tile, SpriteManager7.Rock_Bottom_Right_Corner, Traversability.Blocking);}
	public static void Rock_Bottom_Left_Corner(GameObject tile){SetTile(tile, SpriteManager7.Rock_Bottom_Left_Corner, Traversability.Blocking);}
	public static void Rock_Top_Right_Corner(GameObject tile){SetTile(tile, SpriteManager7.Rock_Top_Right_Corner, Traversability.Blocking);}
	public static void Rock_Top_Left_Corner(GameObject tile){SetTile(tile, SpriteManager7.Rock_Top_Left_Corner, Traversability.Blocking);}
	public static void Rock_Side_Bottom(GameObject tile){SetTile(tile, SpriteManager7.Rock_Side_Bottom, Traversability.Blocking);}
	public static void Rock_Side_Left(GameObject tile){SetTile(tile, SpriteManager7.Rock_Side_Left, Traversability.Blocking);}
	public static void Rock_Side_Top(GameObject tile){SetTile(tile, SpriteManager7.Rock_Side_Top, Traversability.Blocking);}
	public static void Rock_Side_Right(GameObject tile){SetTile(tile, SpriteManager7.Rock_Side_Right, Traversability.Blocking);}
	public static void Rock_Crater(GameObject tile){SetTile(tile, SpriteManager7.Rock_Crater, Traversability.Blocking);}
    public static void Rock_Crystal(GameObject tile) { SetTile(tile, SpriteManager7.Rock_Crystal, Traversability.Blocking); }
	public static void Spaceship_Top_Left(GameObject tile){SetTile(tile, SpriteManager7.Spaceship_Top_Left, Traversability.Blocking);}
	public static void Spaceship_Top_Right(GameObject tile){SetTile(tile, SpriteManager7.Spaceship_Top_Right, Traversability.Blocking);}
	public static void Spaceship_Bottom_Left(GameObject tile){SetTile(tile, SpriteManager7.Spaceship_Bottom_Left, Traversability.Blocking);}
	public static void Spaceship_Bottom_Right(GameObject tile){SetTile(tile, SpriteManager7.Spaceship_Bottom_Right, Traversability.Blocking);}
	public static void Fuel_Cell(GameObject tile){SetTile(tile, SpriteManager7.Fuel_Cell, Traversability.Walkable);}
	public static void Tentacle_Monster(GameObject tile){SetTile(tile, SpriteManager7.Tentacle_Monster, Traversability.Blocking);}
	public static void Astornaut(GameObject tile){SetTile(tile, SpriteManager7.Astronaut_Front, Traversability.Blocking);}
	
	public static void Rock_Full(GameObject tile)
	{
		switch (UnityEngine.Random.Range(0, 4)) 
		{
			case 0: 
				SetTile(tile, SpriteManager7.Rock_Full1, Traversability.Blocking);
			break;
			case 1: 
				SetTile(tile, SpriteManager7.Rock_Full2, Traversability.Blocking);
			break;
			case 2: 
				SetTile(tile, SpriteManager7.Rock_Full3, Traversability.Blocking);
			break;
			case 3: 
				SetTile(tile, SpriteManager7.Rock_Full4, Traversability.Blocking);
			break;
		}
	}

	public static void SetTile(GameObject tile, Sprite sprite, Traversability traversability)
	{
		var sr = tile.GetComponent<SpriteRenderer> ();
		sr.sprite = sprite;
		var script = tile.GetComponent<SquareScript>();
		script.TraversingCondition = traversability;
	}

	public static void SetTileFromLayerAndGid(GameObject tile, string layerAndGid)
	{
		switch (layerAndGid) 
		{
		case "Terrain_0" : EmptyTile(tile); break;
		case "Terrain_1" : Rock_Bottom_Right_Corner(tile); break;
		case "Terrain_2" : Rock_Bottom_Left_Corner(tile); break;
		case "Terrain_3" : Rock_Top_Right_Corner(tile); break;
		case "Terrain_4" : Rock_Top_Left_Corner(tile); break;
		case "Terrain_5" : 
		case "Terrain_6" : 
		case "Terrain_7" : 
		case "Terrain_8" : Rock_Full(tile); break;
		case "Terrain_9" : Rock_Side_Bottom(tile); break;
		case "Terrain_10" : Rock_Side_Left(tile); break;
		case "Terrain_11" : Rock_Side_Top(tile); break;
		case "Terrain_12" : Rock_Side_Right(tile); break;
		case "Terrain_13" : Rock_Crater(tile); break;
		case "Terrain_14" : Spaceship_Top_Left(tile); break;
		case "Terrain_15" : Spaceship_Top_Right(tile); break;
		case "Terrain_16" : Spaceship_Bottom_Left(tile); break;
		case "Terrain_17" : Spaceship_Bottom_Right(tile); break;
        case "Terrain_18": Rock_Crystal(tile); break;
		case "Entities_0" : EmptyTile(tile); break;
		case "Entities_33" : Fuel_Cell(tile); break;
		case "Entities_34" : Tentacle_Monster(tile); break;
		case "Entities_35" : 
		case "Entities_36" : Astornaut(tile); break;
		}
	}
}

public class SpriteManager7 
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

	public static Sprite Empty { get { return GetSprite("Terrain_0"); } }
	public static Sprite Rock_Bottom_Right_Corner { get { return GetSprite("Terrain_1"); } }
	public static Sprite Rock_Bottom_Left_Corner { get { return GetSprite("Terrain_2"); } }
	public static Sprite Rock_Top_Right_Corner { get { return GetSprite("Terrain_3"); } }
	public static Sprite Rock_Top_Left_Corner { get { return GetSprite("Terrain_4"); } }
	public static Sprite Rock_Full1 { get { return GetSprite("Terrain_5"); } }
	public static Sprite Rock_Full2 { get { return GetSprite("Terrain_6"); } }
	public static Sprite Rock_Full3 { get { return GetSprite("Terrain_7"); } }
	public static Sprite Rock_Full4 { get { return GetSprite("Terrain_8"); } }
	public static Sprite Rock_Side_Bottom { get { return GetSprite("Terrain_9"); } }
	public static Sprite Rock_Side_Left { get { return GetSprite("Terrain_10"); } }
	public static Sprite Rock_Side_Top { get { return GetSprite("Terrain_11"); } }
	public static Sprite Rock_Side_Right { get { return GetSprite("Terrain_12"); } }
	public static Sprite Rock_Crater { get { return GetSprite("Terrain_13"); } }
	public static Sprite Spaceship_Top_Left { get { return GetSprite("Terrain_14"); } }
	public static Sprite Spaceship_Top_Right { get { return GetSprite("Terrain_15"); } }
	public static Sprite Spaceship_Bottom_Left { get { return GetSprite("Terrain_16"); } }
	public static Sprite Spaceship_Bottom_Right { get { return GetSprite("Terrain_17"); } }
    public static Sprite Rock_Crystal { get { return GetSprite("Terrain_18"); } }
	public static Sprite Fuel_Cell { get { return GetSprite("Entities_0"); } }
	public static Sprite Tentacle_Monster { get { return GetSprite("Entities_1"); } }	
	public static Sprite Astronaut_Right { get { return GetSprite("Entities_2"); } }
	public static Sprite Astronaut_Front { get { return GetSprite("Entities_3"); } }
    public static Sprite Crystal { get { return GetSprite("Entities_4"); } }
	public static Sprite Mouse_Hover { get { return GetSprite("Markers_0"); } }	
	public static Sprite Blueish_Marker { get { return GetSprite("Markers_1"); } }
	public static Sprite Green_Marker { get { return GetSprite("Markers_2"); }}
    public static Sprite Tentacle_Marker { get { return GetSprite("Markers_3"); } }

	public static string ConvertLayerAndGidToSprintName(string layer, string gid)
	{
		var number_gid = Int32.Parse(gid);
		switch (layer) 
		{
			case "Terrain":
			return (number_gid > 0) ? layer + "_" + (number_gid - 1) : "Terrain_0";

			case "Entities":
			return (number_gid > 0) ? layer + "_" + (number_gid - 33) : "Terrain_0";
		}
		return "Terrain_0";

	}

	public static Sprite GetSpriteByLayerAndGid(string layer, string gid)
	{
		return GetSprite(ConvertLayerAndGidToSprintName(layer, gid));
	}

	public static Sprite GetSpriteByName(string spriteName)
	{
		return GetSprite(spriteName);
	}
}

public class Marker
{

}

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
            case "0": TileManager.EmptyTile(tile); break;
            case "1": TileManager.EmptyTile(tile); break;
            case "2": TileManager.Rock_Bottom_Right_Corner(tile); break;
            case "3": TileManager.Rock_Bottom_Left_Corner(tile); break;
            case "4": TileManager.Rock_Top_Right_Corner(tile); break;
            case "5": TileManager.Rock_Top_Left_Corner(tile); break;
            case "6":
            case "7":
            case "8":
            case "9": TileManager.Rock_Full(tile); break;
            case "10": TileManager.Rock_Side_Bottom(tile); break;
            case "11": TileManager.Rock_Side_Left(tile); break;
            case "12": TileManager.Rock_Side_Top(tile); break;
            case "13": TileManager.Rock_Side_Right(tile); break;
            case "14": TileManager.Rock_Crater(tile); break;
            case "15": TileManager.Spaceship_Top_Left(tile); break;
            case "16": TileManager.Spaceship_Top_Right(tile); break;
            case "17": TileManager.Spaceship_Bottom_Left(tile); break;
            case "18": TileManager.Spaceship_Bottom_Right(tile); break;
            case "19": TileManager.Rock_Crystal(tile); break;
            default: TileManager.EmptyTile(tile); break;
        }
    }

    public static void HandleEntity(string gid, int x, int y)
    {
        switch (gid)
        {
            case "33": break;
            case "34": MapSceneScript.CreateEnemy(x, y); break;
            case "35": break;
            case "36": break;
        }
    }
}
