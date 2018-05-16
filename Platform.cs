using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AxlPlay
{
    public class Platform : MonoBehaviour
    {

        public float ArrivedDistance = 0.1f;
        public float Speed = 5f;

        public Vector3 EndPos;
        private Vector3 startPos;

        private Vector3 nextPos;
        private void Awake()
        {
            startPos = transform.position;
        }
        private void Start()
        {
            nextPos = EndPos;
        }

        private void Update()
        {
            float step = Speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, nextPos, step);

            if (Vector3.Distance(transform.position, nextPos) < ArrivedDistance)
            {
                if (nextPos != startPos)
                    nextPos = startPos;
                else
                    nextPos = EndPos;
            }

        }

        private void OnTriggerEnter(Collider collision)
        {

            if (collision.transform.tag == "Player")
            {

                collision.transform.parent = transform;

            }
        }
        private void OnTriggerExit(Collider collision)
        {
            if (collision.transform.tag == "Player")
            {
                collision.transform.parent = null;

            }
        }
    }

}