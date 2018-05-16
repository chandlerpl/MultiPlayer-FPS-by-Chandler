using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BindRigidbodies : MonoBehaviour {

    public LayerMask rigidbodyLayer;
    public float breakForce = Mathf.Infinity;
    public float breakTorque = Mathf.Infinity;

    private void Awake()
    {
        var cols = Physics.OverlapSphere(transform.position, transform.localScale.x / 2, rigidbodyLayer);

        for(int i = 0; i < cols.Length - 1; i++)
        {
            FixedJoint joint = cols[i].gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = cols[i + 1].gameObject.GetComponent<Rigidbody>();
            joint.breakTorque = breakTorque;
            joint.breakForce = breakForce;
        }

        Destroy(gameObject);
    }
}
