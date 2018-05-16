using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : MonoBehaviour {
    public Material LightsOn;
    public Material LightsOff;

    private bool LightState = false;

	void Update () {
		if(Input.GetKeyDown(KeyCode.L))
        {
            LightState = !LightState;
            if(LightState)
            {
                gameObject.GetComponent<Renderer>().material = LightsOn;
            } else
            {
                gameObject.GetComponent<Renderer>().material = LightsOff;
            }

            gameObject.transform.GetChild(0).gameObject.SetActive(LightState);
        }
	}
}
