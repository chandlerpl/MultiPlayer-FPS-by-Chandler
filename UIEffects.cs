using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AxlPlay
{
    public class UIEffects : MonoBehaviour
    {
        public bool FadeOutAutomatically = false;
        public float speedIn = 0.09f;
        public float speedOut = 0.09f;
        public bool UseDeltaTime;
        [HideInInspector]
        public CanvasGroup canvasGroup;
        private bool FadeIn;
        private bool FadeOut;
        private float startSpeedOut;
        void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            startSpeedOut = speedOut;
        }

        void Update()
        {
            if (FadeIn)
            {
                if (UseDeltaTime)
                    canvasGroup.alpha += speedIn * Time.deltaTime;
                else

                    canvasGroup.alpha += speedIn;

                if(canvasGroup.alpha > 0.99 && FadeOutAutomatically)
                {
                    DoFadeOut();
                }

            }

            if (FadeOut)
            {
                if (UseDeltaTime)

                    canvasGroup.alpha -= speedOut * Time.deltaTime;
                else
                    canvasGroup.alpha -= speedOut;

                if(canvasGroup.alpha < 0.01f)
                {
                    speedOut = startSpeedOut;
                } 
            }
        }

        public void DoFadeIn()
        {
            if (canvasGroup)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
            FadeIn = true;
            FadeOut = false;
        }
        public void DoFadeOut()
        {
            if (canvasGroup)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            FadeOut = true;
            FadeIn = false;
        }
        public void DoFadeOutAtSpeed(float speed)
        {
            if (canvasGroup)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            speedOut = speed;

            FadeOut = true;
            FadeIn = false;
        }
        public void DoFadeOutInmmediately()
        {
            if (canvasGroup)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            FadeOut = false;
            FadeIn = false;

            canvasGroup.alpha = 0f;
        }
    }
}