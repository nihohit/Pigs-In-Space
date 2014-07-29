﻿using System;
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
	private static Dictionary<string, Sprite> s_sprites;
    private Loot m_droppedLoot;
    private SpriteRenderer m_lootRenderer;
    private static SpriteRenderer s_squareMarker;

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
        var tileNames = new List<string>();
        XmlTextReader reader = new XmlTextReader(filename);
        s_sprites = Resources.LoadAll<Sprite>("Sprites").ToDictionary(sprite => sprite.name);
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
                                                tileNames.Add(GetPrefabName(Int32.Parse(reader.Value)));
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

                var squareGameobject = CreateTile(currentPosition, tileNames[j * mapWidth + i]);
                s_map[i, j] = squareGameobject.GetComponent<SquareScript>();
                s_map[i, j].setLocation(i, j);
                currentPosition = new Vector3(currentPosition.x + squareSize, currentPosition.y, 0);
                if (i == 0 || j == 0 || j == mapHeight - 1 || i == mapWidth - 1)
                {
                    s_map[i, j].TraversingCondition = Traversability.Blocking;
                }
                else
                {
                    s_map[i, j].TraversingCondition = Traversability.Walkable;
                }
            }
            currentPosition = new Vector3(0, currentPosition.y + squareSize, 0);
        }
    }

    public void AddLoot(Loot loot)
    {
        if (m_droppedLoot == null)
        {
            m_droppedLoot = loot;
            m_lootRenderer = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("BlueCrystal"),
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
		var tileSpriteRenderer = tile.GetComponent<SpriteRenderer> ();
		tileSpriteRenderer.sprite = s_sprites [tileResourceName];
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
    }

    #endregion
}
