using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AxlPlay
{
    public class ShellSound : MonoBehaviour
    {

        public AudioSource AudioSource;
        public AudioClip GetOutSound;
        public AudioClip FallSound;

        private void Awake()
        {
            if (AudioSource && GetOutSound)
            {
                // play sound when spawn shell
                AudioSource.PlayOneShot(GetOutSound);
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            // and play sound when the shell touchs the ground
            if (AudioSource && FallSound && !AudioSource.isPlaying)
            {
                AudioSource.PlayOneShot(FallSound);

            }
        }
    }
}