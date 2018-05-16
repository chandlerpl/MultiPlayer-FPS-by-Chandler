using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AxlPlay
{
    // Only pickup ammo when the player's weapon is compatible with this type of magazine
    public enum WeaponAmmoTypes
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N

    }
    // for magazine on the floor pick up them
    public class AmmoPick : MonoBehaviour
    {
        public int AmmoAmount = 30;
        public WeaponAmmoTypes WeaponType;

        private void OnTriggerEnter(Collider other)
        {

            PlayerController playerC = other.GetComponent<PlayerController>();


            if (playerC == null)
                return;
            if (!playerC.photonView.isMine)
                return;
            if (playerC.CurrentWeapon == null)
                return;

            if (playerC.CurrentWeapon.WeaponType != WeaponType)
                return;
            // when a player is up this ammo box
            // add ammo and desactivate this box
            playerC.CurrentWeapon.ammunition += AmmoAmount;
            gameObject.SetActive(false);
            GameManager.Instance.UpdateAmmoUI();

        }

    }
}