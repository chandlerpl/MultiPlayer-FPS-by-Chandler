using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AxlPlay
{
    // to play sounds in specific parts of animation ( when insert magazine, remove magazine, pull spring)
    public class AnimationSounds : MonoBehaviour
    {


        [HideInInspector]
        public Weapon weapon;
        // called by animation events

        public void InsertMagazine()
        {
            if (weapon == null)
                return;
            if (weapon.AudioSource && weapon.MagazineOnSound)
                weapon.AudioSource.PlayOneShot(weapon.MagazineOnSound);

        }
        public void RemoveMagazine()
        {
            if (weapon == null)
                return;
            if (weapon.AudioSource && weapon.MagazineOffSound)
                weapon.AudioSource.PlayOneShot(weapon.MagazineOffSound);

        }
        public void PullSpring()
        {
            if (weapon == null)
                return;
            if (weapon.AudioSource && weapon.PullSpringSound)
                weapon.AudioSource.PlayOneShot(weapon.PullSpringSound);

        }
    }
}