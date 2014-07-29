using UnityEngine;
using System.Collections;

public class MapSceneScript : MonoBehaviour 
{
	private SpriteRenderer m_playerSprite;
	private SquareScript m_currentSquare;

	// Use this for initialization
	void Start () 
	{
        //SquareScript.Init(5,5);
		SquareScript.LoadFromTMX(@"Maps\testMap1.tmx");
		m_currentSquare = SquareScript.GetSquare(2,2);
		//instantiate a player sprite, and save the sprite renderer
		m_playerSprite = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("PlayerSprite"), 
		                                                         m_currentSquare.transform.position, 
		                                                         Quaternion.identity)).GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		StartCoroutine(Move ());
    }

	private IEnumerator Move ()
	{
		var x = 0;
		var y = 0;
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			y = -1;
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			y = 1;
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			x = -1;
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			x = 1;
		}
		if(x != 0 || y != 0)
		{
			m_currentSquare = m_currentSquare.GetNextSquare(x,y);
			m_playerSprite.transform.position = m_currentSquare.transform.position;
			yield return new WaitForSeconds(0.25f);
		}
	}
}
