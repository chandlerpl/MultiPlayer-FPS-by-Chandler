using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AxlPlay
{
    // for muzzle flash effects or bullet holes, destroy on time or on particle system stop
    public class DesactivateEffect : Photon.PunBehaviour
    {

        public bool DoDestroy;
        public bool DesactivateInTime = false;
        public bool DesactivateInParticleStop = false;
        public bool DesactivateInAnimStop = false;


        public float TimeToDesactivate = 0f;


        private bool activated;
        private ParticleSystem pSystem;

        private float timer;
        private Animation animation;
        private void Awake()
        {
            pSystem = GetComponent<ParticleSystem>();
            animation = GetComponent<Animation>();
        }
        private void OnEnable()
        {
            activated = true;
        }
        private void Update()
        {
            if (activated)
            {
                if (DesactivateInParticleStop && !pSystem.isPlaying)
                {
                    if (DoDestroy)
                    {

                        Destroy(gameObject);
                        return;
                    }

                    activated = false;
                    gameObject.SetActive(false);
                }
                if (DesactivateInTime)
                {

                    timer += Time.deltaTime;

                    if (timer >= TimeToDesactivate)
                    {
                        if (DoDestroy)
                        {
                            Destroy(gameObject);
                            return;
                        }

                        timer = 0f;
                        activated = false;
                        gameObject.SetActive(false);


                    }
                }
                if (DesactivateInAnimStop)
                {
                    if (!animation.isPlaying)
                        gameObject.SetActive(false);

                }
            }
        }
    }
}