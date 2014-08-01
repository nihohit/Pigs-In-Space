using UnityEngine;

public class GameOverScript : MonoBehaviour
{
    public string Message { get; set; }

    // Use this for initialization
    void Start()
    {
    }

    void OnGUI()
    {
        // Make a background box
        GUI.Box(new Rect(100, 100, 500, 600), Message);
    }

    // Update is called once per frame
    void Update()
    {
    }
}