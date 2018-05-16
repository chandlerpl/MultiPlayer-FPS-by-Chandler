using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AxlPlay
{
    public class HealthBody : MonoBehaviour
    {

        public enum BodyParts
        {
            Head,
            Arms,
            Legs,
            Torso,
            Hips
        }

        public BodyParts BodyPart;
        [HideInInspector]
        public Health healthScript;
        private void Awake()
        {
            healthScript = transform.root.GetComponent<Health>();

        }

    }
}