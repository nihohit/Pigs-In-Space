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
        SquareScript.GetSquare(10, 10).AddLoot(new Loot{
            BlueCrystal = 10
        });

        SquareScript.GetSquare(8, 2).AddLoot(new Loot
        {
            BlueCrystal = 5
        });
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

    public void UpdatePlayerState(string updatedProperty, double updatedValue)
    {
        var doubleToString = string.Format("{0:N1}", updatedValue);
        var child = transform.FindChild(updatedProperty).GetComponent<GUIText>();
        child.text = "{0}:{1}".FormatWith(updatedProperty, doubleToString);
        child = transform.FindChild("{0}Shadow".FormatWith(updatedProperty)).GetComponent<GUIText>();
        child.text = "{0}:{1}".FormatWith(updatedProperty, doubleToString);
    }
}
