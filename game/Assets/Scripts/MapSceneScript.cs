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
//			transform.position = new Vector3(m_playerSprite.transform.position.x, m_playerSprite.transform.position.y, transform.position.z);
			yield return new WaitForSeconds(0.25f);
		}

		if (Input.GetMouseButtonUp(0)) { // left click	
			//Get Mouse direction, let's assume it's right for now
            //create laser object

            var destination = Input.mousePosition;

            var laser = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("laser"), m_playerSprite.transform.position, m_playerSprite.transform.rotation));
            var laserScript = laser.GetComponent<LaserScript>();
            //laserScript.Ini
			var direction = transform.TransformDirection (Vector3.right);
			RaycastHit2D hit = Physics2D.Raycast(m_playerSprite.transform.position, Vector3.right);
			if (hit.collider != null) {

				Debug.Log("We have a hit!");
				print(hit.collider.shapeCount);
				//hit.transform.position;
			}
	}
	}
}
