using UnityEngine;

namespace Assets.Scripts.MapScene
{
    public class GameOverScript : MonoBehaviour
    {
        public string Message { get; set; }

        // Use this for initialization
        private void Start()
        {
        }

        private void OnGUI()
        {
            // Make a background box
            GUI.Box(new Rect(100, 100, 500, 600), Message);
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}