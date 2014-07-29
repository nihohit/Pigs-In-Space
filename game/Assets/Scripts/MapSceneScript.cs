using UnityEngine;
using System.Collections;

public class MapSceneScript : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
	{
        //SquareScript.Init(5,5);
		SquareScript.LoadFromTMX(@"Maps\testMap1.tmx");
		Entity.Player = new PlayerEntity ();
		Entity.Player.Location = SquareScript.GetSquare(2,2);
		//instantiate a player sprite, and save the sprite renderer
		Entity.Player.Image = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("PlayerSprite"), 
		                                                             Entity.Player.Location.transform.position, 
		                                                         Quaternion.identity)).GetComponent<SpriteRenderer>();
        var enemy = CreateEnemy(0, 0);
	}

    private EnemyEntity CreateEnemy(int p1, int p2)
    {
        var enemy = new EnemyEntity();
        enemy.Location = SquareScript.GetSquare(0, 0);
        enemy.Image = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("PlayerSprite"),
                                                                     Entity.Player.Location.transform.position,
                                                                 Quaternion.identity)).GetComponent<SpriteRenderer>();
        enemy.Health = 10;
        enemy.MaxDamage = 2;
        enemy.MinDamage = 1;
        enemy.AttackRange = 1;
        return enemy;
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
			Entity.Player.MoveTo(Entity.Player.Location.GetNextSquare(x,y));
//			transform.position = new Vector3(m_playerSprite.transform.position.x, m_playerSprite.transform.position.y, transform.position.z);
			yield return new WaitForSeconds(0.25f);
		}

		if (Input.GetMouseButtonUp(1)) {
			//Get Mouse direction, let's assume it's right for now
			var direction = transform.TransformDirection (Vector3.right);
			RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up);
			if (hit.collider != null) {
				print("We have a hit!");
				//hit.transform.position;
			}
	}
	}
}
