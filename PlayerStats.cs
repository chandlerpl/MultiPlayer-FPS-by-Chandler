using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// the player info in the list ( how many kills and deaths you have)
namespace AxlPlay
{
    public class PlayerStats : Photon.PunBehaviour
    {


        [HideInInspector]
        public PlayerController Owner;

        public Text KillsText;
        public Text DeathsKills;

        public Text NameText;
        public GameObject KickBt;

        void Start()
        {
            if (Owner)
            {
                // only master client can kick a player
                if (PhotonNetwork.isMasterClient && !Owner.photonView.isMine)
                    KickBt.gameObject.SetActive(true);
                else
                    KickBt.gameObject.SetActive(false);

            }
            else
                KickBt.gameObject.SetActive(false);
        }

        void Update()
        {
            // if player is not anymore in the room destroy player stats
            if (Owner == null)
            {

                Destroy(this.gameObject);
                return;
            }


            if (Owner.photonView.isMine)
            {
                NameText.text = PhotonNetwork.player.NickName;


            }
            else
            {
                NameText.text = Owner.userName;
            }

            KillsText.text = Owner.Kills.ToString();
            DeathsKills.text = Owner.Deaths.ToString();
        }
        // kick player ( when click on it )
        public void KickPlayer()
        {
            if (!Owner || !PhotonNetwork.isMasterClient)
                return;


            foreach (var player in PhotonNetwork.playerList)
            {
                if (player.NickName == Owner.userName)
                {
                    PhotonNetwork.CloseConnection(player);
                    break;
                }
            }
            //       PhotonNetwork.CloseConnection(PhotonPlayer.Find(Owner.photonView.viewID));
        }
    }
}