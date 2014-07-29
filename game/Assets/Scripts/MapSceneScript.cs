using UnityEngine;
using System.Collections;

public class MapSceneScript : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
	{
        //SquareScript.Init(5,5);
		SquareScript.LoadFromTMX(@"Maps\testMap1.tmx");
        var square = SquareScript.GetSquare(5, 5);
		Entity.Player = new PlayerEntity (10, 5, 3, 5,
            square,
            ((GameObject)MonoBehaviour.Instantiate(Resources.Load("PlayerSprite"),
                                                                     square.transform.position, 
		                                                         Quaternion.identity)).GetComponent<SpriteRenderer>(),
            10,
            10);
        var enemy = CreateEnemy(1, 1);
	}

    private EnemyEntity CreateEnemy(int x, int y)
    {
        var square = SquareScript.GetSquare(x, y);
        return new EnemyEntity(10, 1, 1, 2,
            square,
            ((GameObject)MonoBehaviour.Instantiate(Resources.Load("PlayerSprite"),
                                                                square.transform.position,
                                                                Quaternion.identity)).GetComponent<SpriteRenderer>(),
            MovementType.Walking);
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
            Entity.Player.Move(Entity.Player.Location.GetNextSquare(x, y));
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
