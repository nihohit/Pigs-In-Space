using UnityEngine;
using System.Collections;

public class GameOverScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}


    void OnGUI()
    {
        // Make a background box
        GUI.Box(new Rect(100, 100, 500, 600), "Game over");

        //// Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
        //if (GUI.Button(new Rect(20, 40, 80, 20), "StartOver"))
        //{
        //    //Application.LoadLevel(1);
        //}

    
    }

	// Update is called once per frame
	void Update () {
	
	}
}