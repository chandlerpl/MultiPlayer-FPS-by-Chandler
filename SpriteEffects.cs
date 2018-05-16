using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AxlPlay
{
    public class SpriteEffects : MonoBehaviour
    {

        public float speedIn = 0.09f;
        public float speedOut = 0.09f;
        public bool UseDeltaTime;

        private SpriteRenderer spriteRenderer;
        private bool FadeIn;
        private bool FadeOut;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        void Start()
        {

        }

        void Update()
        {

            Color spriteColor = spriteRenderer.color;
            if (FadeIn)
            {
                if (UseDeltaTime)
                {
                    spriteColor.a += speedIn * Time.deltaTime;
                }
                else
                {

                    spriteColor.a += speedIn;
                }

            }

            if (FadeOut)
            {
                if (UseDeltaTime)
                {
                    spriteColor.a -= speedOut * Time.deltaTime;

                }
                else
                {
                    spriteColor.a -= speedOut;
                }
            }
            spriteRenderer.color = spriteColor;
        }

        public void DoFadeIn()
        {

            FadeIn = true;
            FadeOut = false;
        }
        public void DoFadeOut()
        {

            FadeOut = true;
            FadeIn = false;
        }
    }
}