using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleActivation : MonoBehaviour {

    public float generalForce = 20f;
    public float dustForce = 10f;
    public float splinterForce = 40f;
    public GameObject dust;
    public GameObject splinters;
    
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.relativeVelocity.sqrMagnitude > dustForce)
        {
            dust.SetActive(true);
        }
        if (collision.relativeVelocity.sqrMagnitude > splinterForce)
        {
            splinters.SetActive(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.relativeVelocity.sqrMagnitude > dustForce)
        {
            dust.SetActive(true);
        }
        if (collision.relativeVelocity.sqrMagnitude > splinterForce)
        {
            splinters.SetActive(true);
        }
    }

    private void OnJointBreak(float breakForce)
    {
        Debug.Log("Break force: " + breakForce);
        GetComponent<Rigidbody>().isKinematic = false;
    }
}
