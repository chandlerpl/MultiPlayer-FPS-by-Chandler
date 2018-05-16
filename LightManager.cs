using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour {
    public static LightManager instance;
    public GameObject mainLight;
    public bool isStormActive = false;
    public float introduceFog = 1f;
    [SerializeField]
    public List<VolumetricDirectionalLightChanges> lightChanges = new List<VolumetricDirectionalLightChanges>();
    
    public GameObject deathParticle;
    
    public bool IsStormActive
    {
        get { return isStormActive; }
        set { IsStormActive = value; }
    }
    
    void Start () {
		if(instance != null)
        {
            Debug.Log("There is already an instance of GameManager in the scene.");
            return;
        }
        instance = this;
        
        //StartCoroutine(ChangeStormState());
	}

    private void Update()
    {
        
    }
}
