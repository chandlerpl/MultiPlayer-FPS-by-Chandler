using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AxlPlay
{
    public class Pickup : MonoBehaviour
    {

        public AudioSource AudioSource;
        public AudioClip PickupSound;

        public bool PickupByTrigger = true;
        public SphereCollider TriggerPickup;
        public bool UseAutomatic = true;

        private Item item;
        private void Awake()
        {
            item = GetComponent<Item>();
        }

        public void PickupItem(PlayerController picker)
        {
            if (TriggerPickup)
                TriggerPickup.enabled = false;

            if (AudioSource && PickupSound)
                AudioSource.PlayOneShot(PickupSound);

            picker.photonView.RPC("PickupItem", PhotonTargets.AllBuffered, item.GetComponent<PhotonView>().viewID);
            if (UseAutomatic)
            {
                picker.photonView.RPC("UseItem", PhotonTargets.AllBuffered, item.GetComponent<PhotonView>().viewID, -1);

            }
            else
            {

                SendMessage("Pickuped", SendMessageOptions.DontRequireReceiver);

                gameObject.SetActive(false);
            }
        }


        private void OnTriggerStay(Collider other)
        {

            if (!PickupByTrigger)
                return;
            PlayerController playerC = other.GetComponent<PlayerController>();


            if (playerC == null)
                return;
            if (!playerC.photonView.isMine)
                return;

            if (playerC.CurrentWeapon && GetComponent<Weapon>() && GetComponent<Weapon>().WeaponType == playerC.CurrentWeapon.WeaponType)
            {

                if (!playerC.CurrentWeapon.tookAmmoFrom.Contains(GetComponent<Weapon>()))
                {
                    playerC.CurrentWeapon.tookAmmoFrom.Add(GetComponent<Weapon>());
                    playerC.CurrentWeapon.ammunition += playerC.CurrentWeapon.CartridgeAmmo;
                    GameManager.Instance.UpdateAmmoUI();
                    if (playerC.CurrentWeapon.AudioSource && GameManager.Instance.PickUpAmmoSound)
                        playerC.CurrentWeapon.AudioSource.PlayOneShot(GameManager.Instance.PickUpAmmoSound);

                }
                return;
            }

            if (GameManager.Instance.InteractIcon)

                GameManager.Instance.InteractIcon.DoFadeIn();



            if (!Input.GetButtonDown(InputManager.inputManager.PickUpItemAxis))
                return;

            if (!playerC.finishPicking)
                return;


            if (playerC != null)
            {
                TriggerPickup.enabled = false;

                if (AudioSource && PickupSound)
                    AudioSource.PlayOneShot(PickupSound);

                playerC.photonView.RPC("PickupItem", PhotonTargets.AllBuffered, item.GetComponent<PhotonView>().viewID);
                if (UseAutomatic)
                {
                    playerC.photonView.RPC("UseItem", PhotonTargets.AllBuffered, item.GetComponent<PhotonView>().viewID, -1);

                }
                else
                {

                    SendMessage("Pickuped", SendMessageOptions.DontRequireReceiver);

                    gameObject.SetActive(false);
                }
                playerC.finishPicking = false;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (GameManager.Instance.InteractIcon)
                GameManager.Instance.InteractIcon.DoFadeOut();
        }
    }

}