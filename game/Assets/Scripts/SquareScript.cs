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

public class SquareScript : MonoBehaviour 
{
	public Entity OccupyingEntity { get; set; }
	private static SquareScript[,] s_map;
	private int m_x,m_y;

	public static void Init(int xSize, int ySize)
	{
        s_map = new SquareScript[xSize, ySize];
		var squareSize = 0.64f;
		var currentPosition = Vector3.zero;
        for (int i = 0; i < xSize; i++) 
		{
            for (int j = 0; j < ySize; j++) 
			{
				var squareGameobject = CreateTile(currentPosition, "SquareTileResource");
				s_map[j,i] = squareGameobject.GetComponent<SquareScript>();
				currentPosition = new Vector3(currentPosition.x, currentPosition.y + squareSize, 0);
			}
			currentPosition = new Vector3(currentPosition.x + squareSize, 0 , 0);
		}
	}

	private static GameObject CreateTile(Vector3 position, string tileResourceName)
	{
		var tile = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("SquareTileResource"), position, Quaternion.identity));
		var tileSpriteRenderer = tile.GetComponent<SpriteRenderer> ();
		tileSpriteRenderer.sprite = SpriteManager7.GetSpriteByName (tileResourceName);
		return tile;

	}

    public static void LoadFromTMX(string filename)
    {
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
										terrain.Add(SpriteManager7.ConvertLayerAndGidToSprintName("Terrain", reader.Value));
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
										entities.Add(SpriteManager7.ConvertLayerAndGidToSprintName("Entities", reader.Value));
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
										markers.Add(SpriteManager7.ConvertLayerAndGidToSprintName("Markers", reader.Value));
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
				var squareGameobject = CreateTile(currentPosition, terrain[j*mapWidth + i]);
				s_map[i, j] = squareGameobject.GetComponent<SquareScript>();
				s_map[i, j].setLocation(i, j);
				currentPosition = new Vector3(currentPosition.x + squareSize, currentPosition.y, 0);
            }
			currentPosition = new Vector3(0, currentPosition.y + squareSize, 0);
		}
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

	private static void PlaceEntity(string spriteName, SquareScript square)
	{

	}

	public static SquareScript GetSquare (int x, int y)
	{
		return s_map[x,y];
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

	public SquareScript GetNextSquare (int x, int y)
	{
		x = Mathf.Min(x + m_x, s_map.GetLength(0)-1);
		y = Mathf.Min(y + m_y, s_map.GetLength(1)-1);
		x = Mathf.Max(x, 0);
		y = Mathf.Max(y, 0);
		return GetSquare(x,y);
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
	public static Sprite Fuel_Cell { get { return GetSprite("Entities_0"); } }
	public static Sprite Tentacle_Monster { get { return GetSprite("Entities_1"); } }	
	public static Sprite Astronaut_Right { get { return GetSprite("Entities_2"); } }
	public static Sprite Astronaut_Front { get { return GetSprite("Entities_3"); } }
	public static Sprite Mouse_Hover { get { return GetSprite("Markers_0"); } }	
	public static Sprite Blueish_Marker { get { return GetSprite("Markers_1"); } }
	public static Sprite Green_Marker { get { return GetSprite("Markers_2"); }}

	public static string ConvertLayerAndGidToSprintName(string layer, string gid)
	{
		var number_gid = Int32.Parse(gid);
		return (number_gid > 0) ? layer + "_" + (number_gid - 1) : "Terrain_0";
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