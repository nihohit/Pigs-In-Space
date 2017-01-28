using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using Assets.Scripts.IntersceneCommunication;

public class StartScreenScript : MonoBehaviour 
{
    private Text oxygenText;
    private Slider oxygenSlider;

    private Text healthText;
    private Slider healthSlider;

    private Text energyText;
    private Slider energySlider;

    private Text crystalsText;
    private Slider crystalsSlider;

    public void UpdateOxygen()
    {
        oxygenText.text = "Oxygen: " + oxygenSlider.value;
    }

    public void UpdateHealth()
    {
        healthText.text = "Health: " + healthSlider.value;
    }

    public void UpdateEnergy()
    {
        energyText.text = "Energy: " + energySlider.value;
    }

    public void UpdateCrystals()
    {
        crystalsText.text = "Starting crystals: " + crystalsSlider.value;
    }

    public void StartGame()
    {
        GlobalState.Instance.StartNewPlayer((int)healthSlider.value, (int)energySlider.value, (int)oxygenSlider.value, (int)crystalsSlider.value);
        Application.LoadLevel("SpaceshipScene");
    }

	// Use this for initialization
	void Start () 
    {
        oxygenSlider = GameObject.Find("OxygenSlider").GetComponent<Slider>();
        oxygenText = oxygenSlider.GetComponentInChildren<Text>();
        UpdateOxygen();

        healthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();
        healthText = healthSlider.GetComponentInChildren<Text>();
        UpdateHealth();

        energySlider = GameObject.Find("EnergySlider").GetComponent<Slider>();
        energyText = energySlider.GetComponentInChildren<Text>();
        UpdateEnergy();

        crystalsSlider = GameObject.Find("CrystalSlider").GetComponent<Slider>();
        crystalsText = crystalsSlider.GetComponentInChildren<Text>();
        UpdateCrystals();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
