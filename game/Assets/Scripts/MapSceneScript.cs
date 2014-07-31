using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameState { Ongoing, Won, Lost}
public class MapSceneScript : MonoBehaviour 
{
    private Vector2 CameraMax= new Vector2(15.05f, 11.25f);        // The maximum x and y coordinates the camera can have.
    private Vector2 CameraMin = new Vector2(4.75f, 3.5f);        // The minimum x and y coordinates the camera can have.
    private static Dictionary<Action, Marker> s_Markers = new Dictionary<Action, Marker>();
    private static GameState s_gameState = GameState.Ongoing;
    private const int c_startHealth = 15;
    private const int c_startAttackRange = 5;
    private const int c_startMinDamage = 3;
    private const int c_startMaxDamage = 7;
    private const int c_startOxygen = 200;
    private const int c_startEnergy = 10;
    private const int c_HiveHealth = 20;

    public static void ChangeGameState(GameState state)
    {
        s_gameState = state;
    }

	// Use this for initialization
	void Start () 
	{
		SquareScript.LoadFromTMX(@"Maps\testMap3.tmx");
        var square = SquareScript.GetSquare(5, 5);
        Entity.Player = new PlayerEntity(c_startHealth, c_startAttackRange, c_startMinDamage, c_startMaxDamage,
            square,
            ((GameObject)MonoBehaviour.Instantiate(Resources.Load("PlayerSprite"),
                                                     	square.transform.position, 
                                                 		Quaternion.identity)).GetComponent<SpriteRenderer>(),
            c_startEnergy,
            c_startOxygen);

        var minCameraX = 0f - 0.64f / 2 + camera.orthographicSize * camera.aspect;
        var minCameraY = 0f - 0.64f / 2 + camera.orthographicSize;
        var maxCameraX = minCameraX + 0.64f * SquareScript.Weidth() - 2 * camera.orthographicSize * camera.aspect;
        var maxCameraY = minCameraY + 0.64f * SquareScript.Height() - 2 * camera.orthographicSize;
        CameraMin = new Vector2(minCameraX, minCameraY);
        CameraMax = new Vector2(maxCameraX, maxCameraY);
        square.FogOfWar();
	}

    public static EnemyEntity CreateTentacleMonster(int x, int y)
    {
        return CreateTentacleMonster(SquareScript.GetSquare(x, y));
        
    }

    public static EnemyEntity CreateTentacleMonster(SquareScript square)
    {
        return new EnemyEntity(10, 1, 1, 2,
            square,
            ((GameObject)MonoBehaviour.Instantiate(Resources.Load("TentacleMonster"),
                                                        square.transform.position,
                                                        Quaternion.identity)).GetComponent<SpriteRenderer>(),
            MovementType.Walking);
    }

    internal static Hive CreateHive(int x, int y)
    {
        var square = SquareScript.GetSquare(x, y);
        return new Hive(c_HiveHealth,
            square,
            ((GameObject)MonoBehaviour.Instantiate(Resources.Load("Hive"),
                                                        square.transform.position,
                                                        Quaternion.identity)).GetComponent<SpriteRenderer>());
    }
	
	// Update is called once per frame
	void Update () 
	{
        if (s_gameState == GameState.Ongoing)
        {
            CameraTrackPlayer();
            StartCoroutine(PlayerAction());
        }
    }

    void OnGUI()
    {       
        if(s_gameState != GameState.Ongoing)
        {
            GUI.BeginGroup(new Rect(320, 265, 384, 256));
            GUI.DrawTexture(new Rect(0, 0, 384, 256), Resources.Load<Texture2D>(@"Sprites/WinLoseMessage"), ScaleMode.StretchToFill);
            var style = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 32,
                normal = new GUIStyleState
                {
                    textColor = Color.white,
                },
            };
            var message = (s_gameState == GameState.Lost) ? "Game Over" : "You Win :)";
            GUI.Label(new Rect(110, 45, 60, 60), message, style);
            style.fontSize = 12;
            GUI.Label(new Rect(176, 127, 30, 30), String.Format("X {0}",(int)Entity.Player.BlueCrystal), style);
            GUI.Label(new Rect(176, 165, 30, 30), String.Format("X {0}",EnemyEntity.KilledEnemies), style);
            GUI.Label(new Rect(176, 205, 30, 30), String.Format("X {0}", (int)Hive.KilledHives), style);
            GUI.EndGroup();
            //// Make the second button.
            //if (GUI.Button(new Rect(20, 70, 80, 20), "Level 2"))
            //{
            //    Application.LoadLevel(2);
            //}
        }
    }

	private IEnumerator PlayerAction ()
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
            if (Entity.Player.Move(Entity.Player.Location.GetNextSquare(x, y)))
            {
                //transform.position = new Vector3(m_playerSprite.transform.position.x, m_playerSprite.transform.position.y, transform.position.z);
                yield return new WaitForSeconds(0.25f);
                Entity.Player.EndTurn();
            }
		}

		if (Input.GetMouseButtonUp(0)) { // left click	
			//Get Mouse direction, let's assume it's right for now
            //create laser object

            var destination = Input.mousePosition;

            if (Entity.Player.ShootLaser(destination))
            {
                yield return new WaitForSeconds(0.25f);
                Entity.Player.EndTurn();
            }
	    }

        if (Input.GetMouseButtonUp(1))
        {
            Entity.Player.MineAsteroid();
        }
	}

    public void UpdatePlayerState(string updatedProperty, double updatedValue)
    {
        var doubleToString = string.Format("{0:N1}", updatedValue);
        var guiText = GameObject.Find(updatedProperty).GetComponent<GUIText>();
        guiText.text = "{0}:{1}".FormatWith(updatedProperty, doubleToString);
        guiText = GameObject.Find("{0}Shadow".FormatWith(updatedProperty)).GetComponent<GUIText>();
        guiText.text = "{0}:{1}".FormatWith(updatedProperty, doubleToString);
    }

    private void CameraTrackPlayer ()
    {
        const float xSmooth = 8f; // How smoothly the camera catches up with it's target movement in the x axis.
        const float ySmooth = 8f; // How smoothly the camera catches up with it's target movement in the y axis.

        // By default the target x and y coordinates of the camera are it's current x and y coordinates.
        float targetX = transform.position.x;
        float targetY = transform.position.y;
 
        // If the player has moved beyond the x margin...
        var playerTransform = Entity.Player.Image.transform;

        // ... the target x coordinate should be a Lerp between the camera's current x position and the player's current x position.
        targetX = Mathf.Lerp(transform.position.x, playerTransform.position.x, xSmooth * Time.deltaTime);
 
        // ... the target y coordinate should be a Lerp between the camera's current y position and the player's current y position.
        targetY = Mathf.Lerp(transform.position.y, playerTransform.position.y, ySmooth * Time.deltaTime);
             
        // The target x and y coordinates should not be larger than the maximum or smaller than the minimum.
        targetX = Mathf.Clamp(targetX, CameraMin.x, CameraMax.x);
        targetY = Mathf.Clamp(targetY, CameraMin.y, CameraMax.y);
 
        // Set the camera's position to the target position with the same z component.
        transform.position = new Vector3(targetX, targetY, transform.position.z);
    }

    public static void EnterEscapeMode()
    {
        Hive.EnterEscapeMode();
        var escapeGameEvents = s_Markers.Where(entry => entry.Value == Marker.OnEscape).Select(entry => entry.Key);
        foreach(var gameEvent in escapeGameEvents)
        {
            gameEvent();
        }
    }

    public static void SetEvent(Action action, Marker marker)
    {
        s_Markers.Add(action, marker);
    }
}

public enum Marker
{
    OnEscape,
}


