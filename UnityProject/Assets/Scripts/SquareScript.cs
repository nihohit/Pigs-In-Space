using UnityEngine;
using System.Collections;

public class SquareScript : MonoBehaviour 
{
	public Entity OccupyingEntity { get; set; }
	private static SquareScript[,] s_map;
	private int m_x,m_y;

	public static void Init()
	{
		//HACK
		var xSize = 5;
		var ySize = 5;
		s_map = new SquareScript[xSize, ySize];
		var squareSize = 0.64f;
		var currentPosition = Vector3.zero;
		for (int i = 0; i < xSize; i++) 
		{
			for (int j = 0; j < ySize; j++) 
			{
				var squareGameobject = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("SquareTileResource"), currentPosition, Quaternion.identity));;
				s_map[i,j] = squareGameobject.GetComponent<SquareScript>();
				currentPosition = new Vector3(currentPosition.x, currentPosition.y + squareSize, 0);
			}
			currentPosition = new Vector3(currentPosition.x + squareSize, 0 , 0);
		}
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
}
