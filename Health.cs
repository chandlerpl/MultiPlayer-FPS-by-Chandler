using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace AxlPlay
{
    public class Health : Photon.PunBehaviour
    {

        public GameObject BloodEffect;
        public bool FallDamage;
        public float FallDamageDistance = 5f;
        public bool AutoHealing;
        public float HealingSpeed = 1f;
        // public bool TESTDAMAGE;
        public float currentHealth = 100f;
        public float RespawnDelay = 3f;

        [HideInInspector]

        public float startCurrentHealth;

        private bool player;
        [HideInInspector]
        public bool isDead;
        private GameManager gameManager;
        private PlayerController playerController;

        private Vector3 spawnedPosition;

        private float timer;
        private bool damaged;
        private Transform damager;


        private float lastPositionY = 0f;
        private float fallDistance = 0f;

        public float CurrentHealth
        {
            get
            {
                return currentHealth;
            }

            set
            {
                currentHealth = value;

            }
        }
      
        public void Reset()
        {

            startCurrentHealth = CurrentHealth;
            isDead = false;
            transform.position = spawnedPosition;

        }
        void Awake()
        {
            // get references
            gameManager = GameManager.Instance;
            gameManager.GetPlayerHealth(this);

            playerController = GetComponent<PlayerController>();
            player = playerController != null;
            spawnedPosition = transform.position;
        }
        void Start()
        {
            // set health
            startCurrentHealth = CurrentHealth;
            gameManager.BloodSplash.DoFadeIn();

        }

        void Update()
        {
            if (player && photonView.isMine)
            {
                // fall damage
                if (lastPositionY > playerController.transform.position.y)
                {
                    fallDistance += lastPositionY - playerController.transform.position.y;

                }
                lastPositionY = playerController.transform.position.y;

                if (fallDistance >= FallDamageDistance && playerController.IsGrounded())
                {
                    TakeDamage(fallDistance * 5f, Vector3.zero, -1, false);
                    ApplyNormal();
                }

                if (fallDistance <= FallDamageDistance && playerController.IsGrounded())
                {
                    ApplyNormal();
                }

            }
            // put damage indicator arrow
            if (damaged && damager)
            {
                timer += Time.deltaTime;
                DamageIndicatorMove();
                if (timer >= 5f)
                {
                    gameManager.DamageIndicator.DoFadeOut();
                    damager = null;
                    timer = 0f;
                    damaged = false;
                }
            }

            if (photonView.isMine)
            {

                // text health amount
                if (playerController)
                {
                    gameManager.HealthUI.text = ((int)CurrentHealth).ToString();
                }

                if (!isDead)
                {
                    if (CurrentHealth < startCurrentHealth)
                    {
                        if (gameManager.BloodSplash)
                        {
                            // auto healing

                            CurrentHealth += HealingSpeed * Time.deltaTime;


                        }
                    }
                    else if (CurrentHealth > startCurrentHealth)

                    {
                        // health to start health
                        CurrentHealth = startCurrentHealth;

                    }

                }
            }



        }
        void ApplyNormal()
        {
            fallDistance = 0;
            lastPositionY = 0;
        }
        public void SendTakeDamageRPC(float damage, Vector3 hitPoint, int viewID)
        {
            photonView.RPC("TakeDamage", PhotonTargets.AllBuffered, damage, hitPoint, viewID, true);

        }
        // damage arrow
        void DamageIndicatorMove()
        {

            Vector3 damagerPos = damager.transform.position;
            Vector3 dir = Vector3.zero;
            if (Camera.main != null)
                dir = Camera.main.WorldToScreenPoint(damagerPos);
            Vector3 pointing = Vector3.zero;
            pointing.z = Mathf.Atan2((gameManager.DamageIndicator.transform.position.y - dir.y), (gameManager.DamageIndicator.transform.position.x - dir.x)) * Mathf.Rad2Deg - 90;
            pointing.z = -pointing.z;
            var targetDir = Quaternion.Euler(pointing);
            gameManager.DamageIndicator.transform.rotation = targetDir;

        }

        [PunRPC]
        void TakeDamage(float damage, Vector3 hitPoint, int viewIdDamager, bool comeInRpc = true)
        {

            if (!photonView.isMine)
            {
                if (BloodEffect)
                {
                    // put blood effect particle in player model
                    BloodEffect.transform.position = hitPoint;
                    BloodEffect.SetActive(true);
                }
            }


            PhotonView damagerView = PhotonView.Find(viewIdDamager);
            if (!playerController && damagerView)
            {
                if (BloodEffect)
                {
                    // put blood effect particle in AI model

                    BloodEffect.transform.position = hitPoint;
                    BloodEffect.SetActive(true);
                }
                GetComponent<AIPlayer>().GotHitBy(damagerView.gameObject);
            }
            if (photonView.isMine)
            {
                if (CurrentHealth > 0)
                {
                    CurrentHealth -= damage;


                    if (playerController)
                    {
                        // someone damaged me and it is not fall damage 
                        if (damagerView)
                        {
                            damager = damagerView.transform;


                            gameManager.DamageIndicator.DoFadeIn();
                        }
                        else
                        {
                            damager = null;
                        }
                        // fade in blood overlay screen
                        Color imageColor = gameManager.BloodSplash.GetComponent<Image>().color;
                        imageColor.a = 255 - CurrentHealth;
                        gameManager.BloodSplash.GetComponent<Image>().color = imageColor;
                        damaged = true;


                    }

                }
                if (playerController && !isDead)

                    gameManager.BloodSplash.DoFadeIn();



                if (CurrentHealth <= 0 && !isDead)
                {
                    // die
                    photonView.RPC("Die", PhotonTargets.All);
                }
            }

        }

        // use pun rpc when called by fall damage
        [PunRPC]

        IEnumerator Die()
        {


            if (!PhotonNetwork.offlineMode && MultiplayerGameManager.Instance.GameMode != MultiplayerGameManager.GameModes.FreeForAll)
            {
                if (damager || !photonView.isMine)
                {
                    // network team scores
                    if (playerController.Team1)
                    {
                        MultiplayerGameManager.Instance.Team2Score++;
                    }
                    else
                    {
                        MultiplayerGameManager.Instance.Team1Score++;

                    }
                }

            }

            MultiplayerGameManager.Instance.UpdateTeamScoresUI();


            isDead = true;
            if (playerController)
            {
                // add deaths and kills in players
                playerController.Deaths++;


                if (photonView.isMine)
                {
                    if (playerController.grabbedItem)
                    {


                        playerController.grabbedItem.transform.SetParent(playerController.itemLastParent);

                        Rigidbody currentDraggingItemRg = playerController.grabbedItem.GetComponent<Rigidbody>();
                        if (currentDraggingItemRg)
                            currentDraggingItemRg.isKinematic = false;

                        playerController.grabbedItem = null;
                    }

                    if (damager && damager.GetComponent<PlayerController>())
                    {


                        damager.GetComponent<PlayerController>().Kills++;

                        // if free for all show different scores, my kills and the kills of the top player
                        if (MultiplayerGameManager.Instance.GameMode == MultiplayerGameManager.GameModes.FreeForAll)
                        {

                            var playerListWithoutMe = new List<PlayerController>(MultiplayerGameManager.Instance.Players);
                            playerListWithoutMe.Remove(MultiplayerGameManager.Instance.LocalPlayer);

                            var playerList = playerListWithoutMe.OrderByDescending(player => player.Kills).ToList();
                            MultiplayerGameManager.Instance.Team2Score = playerList[0].Kills;
                            MultiplayerGameManager.Instance.UpdateTeamScoresUI();
                        }
                        if (damager && damager.GetComponent<AIPlayer>())
                        {
                            damager.GetComponent<AIPlayer>().fsm.ChangeState(AIPlayer.States.Search);
                        }


                        damager.GetComponent<PlayerController>().photonView.RPC("KilledSomebody", PhotonTargets.Others);

                    }
                    // KILL CAM
                    // activate my model for kill cam
                    playerController.Model.gameObject.SetActive(true);
                    GameManager.Instance.FadeWhenSight2D.SetActive(false);
                    if (playerController.CurrentWeapon && playerController.CurrentWeapon.Sight2D)
                    {
                        playerController.CurrentWeapon.Sight2D.DoFadeOutInmmediately();
                        playerController.CurrentWeapon.Sight2D.gameObject.SetActive(false);


                    }
                    // fade out damage indicator
                    gameManager.DamageIndicator.DoFadeOut();

                    playerController.rigidBody.isKinematic = true;

                    foreach (Transform child in playerController.Model.transform)
                    {

                        child.gameObject.SetActive(true);
                    }
                    playerController.RagdollDieCamera.gameObject.SetActive(true);
                    playerController.PCamera.gameObject.SetActive(false);
                    yield return new WaitForSeconds(0.2f);
                    playerController.ModelAnimator.enabled = false;

                }
                else
                {
                    // THROW WEAPON (SO OTHER PEOPLE CAN TAKE MY WEAPON)
                    if (playerController.CurrentWeapon)
                    {

                        GameObject newWeaponClone = PhotonNetwork.Instantiate(playerController.CurrentWeapon.gameObject.name, playerController.CurrentWeapon.transform.position, playerController.CurrentWeapon.transform.rotation, 0);
                        Rigidbody newWpRigidBody = newWeaponClone.GetComponent<Rigidbody>();

                        newWpRigidBody.isKinematic = false;


                        newWpRigidBody.GetComponent<Pickup>().enabled = true;
                        newWpRigidBody.GetComponent<BoxCollider>().enabled = true;

                        newWpRigidBody.AddForce(new Vector3(1, 1) * 5f, ForceMode.Impulse);
                        newWpRigidBody.AddTorque(new Vector3(1, 1) * 5f, ForceMode.Impulse);
                        newWpRigidBody.GetComponent<Pickup>().TriggerPickup.enabled = true;
                        newWpRigidBody.GetComponent<Pickup>().PickupByTrigger = true;

                        playerController.CurrentWeapon.gameObject.SetActive(false);
                        photonView.RPC("ThrowWeaponInOthers", PhotonTargets.Others, newWeaponClone.GetComponent<PhotonView>().viewID);
                    }

                    playerController.rigidBody.isKinematic = true;
                    yield return new WaitForSeconds(0.01f);

                    playerController.ModelAnimator.enabled = false;
                }



            }
            else
            {

                AIPlayer aiPlayer = GetComponent<AIPlayer>();
                // THROW WEAPON IN AI (SO OTHER PEOPLE CAN TAKE MY WEAPON)

                if (aiPlayer.PlayerWeapon)
                {
                    GameObject newWeaponClone = PhotonNetwork.Instantiate(aiPlayer.PlayerWeapon.gameObject.name, aiPlayer.PlayerWeapon.transform.position, aiPlayer.PlayerWeapon.transform.rotation, 0);
                    Rigidbody newWpRigidBody = newWeaponClone.GetComponent<Rigidbody>();

                    newWpRigidBody.isKinematic = false;
                    newWpRigidBody.GetComponent<BoxCollider>().enabled = true;
                    newWpRigidBody.AddForce(new Vector3(1, 1) * 5f, ForceMode.Impulse);
                    newWpRigidBody.AddTorque(new Vector3(1, 1) * 5f, ForceMode.Impulse);
                    newWpRigidBody.GetComponent<Pickup>().enabled = true;


                    newWpRigidBody.GetComponent<Pickup>().TriggerPickup.enabled = true;
                    newWpRigidBody.GetComponent<Pickup>().PickupByTrigger = true;

                    aiPlayer.PlayerWeapon.gameObject.SetActive(false);


                }

                aiPlayer.rigidBody.isKinematic = true;
                aiPlayer.fsm.ChangeState(AIPlayer.States.Idle, AxlPlay.StateTransition.Overwrite);
                yield return new WaitForSeconds(0.01f);

                aiPlayer.Model.enabled = false;

            }


            photonView.RPC("Respawn", PhotonTargets.All);
        }
        [PunRPC]
        // throw weapon rpc function
        void ThrowWeaponInOthers(int viewIdWeapon)

        {
            PhotonView itemToThrow = PhotonView.Find(viewIdWeapon);
            Rigidbody newWpRigidBody = itemToThrow.GetComponent<Rigidbody>();

            newWpRigidBody.isKinematic = false;
            newWpRigidBody.GetComponent<BoxCollider>().enabled = true;

            newWpRigidBody.GetComponent<Pickup>().enabled = true;
            newWpRigidBody.GetComponent<Pickup>().TriggerPickup.enabled = true;
            newWpRigidBody.GetComponent<Pickup>().PickupByTrigger = true;

        }
        [PunRPC]
        // respawn player 
        public IEnumerator Respawn()
        {
            GameManager.Instance.BloodSplash.DoFadeOutAtSpeed(0.09f);

            yield return new WaitForSeconds(RespawnDelay);



            isDead = false;
            CurrentHealth = startCurrentHealth;
            if (playerController)
            {
                if (photonView.isMine)
                {

                    if (playerController.CurrentWeapon)
                    {
                        playerController.CurrentWeapon.ammunition = playerController.CurrentWeapon.StartAmmunition;
                        playerController.CurrentWeapon.cartridgeAmmo = playerController.CurrentWeapon.CartridgeAmmo;

                        GameManager.Instance.UpdateAmmoUI();
                    }

                    playerController.Model.gameObject.SetActive(false);


                    GameManager.Instance.FadeWhenSight2D.SetActive(true);
                    if (playerController.CurrentWeapon && playerController.CurrentWeapon.Sight2D)
                    {
                        playerController.CurrentWeapon.Sight2D.DoFadeOutInmmediately();
                        playerController.CurrentWeapon.Sight2D.gameObject.SetActive(true);



                    }
                    playerController.rigidBody.isKinematic = false;
                    GameManager.Instance.BloodSplash.DoFadeOutInmmediately();

                    GameManager.Instance.BloodSplash.gameObject.SetActive(true);

                    foreach (Transform child in playerController.Model.transform)
                    {

                        child.gameObject.SetActive(false);
                    }
                    playerController.RagdollDieCamera.gameObject.SetActive(false);
                    playerController.PCamera.gameObject.SetActive(true);

                    playerController.ModelAnimator.enabled = true;

                    // delete items
                    List<int> deleteThings = new List<int>();
                    foreach (var item in playerController.Items)
                    {
                        if (!playerController.StartWeaponsSpawned.Contains(item.item.GetComponent<Weapon>()))
                        {
                            deleteThings.Add(item.photonView.viewID);
                        }

                    }
                    foreach (var itemToDelete in deleteThings)
                    {
                        playerController.photonView.RPC("DropItem", PhotonTargets.All, itemToDelete);

                    }
                    // use start item
                    playerController.photonView.RPC("UseItem", PhotonTargets.AllBuffered, playerController.StartWeaponsSpawned[0].photonView.viewID, 0);

                }
                else
                {
                    playerController.rigidBody.isKinematic = false;

                    playerController.ModelAnimator.enabled = true;

                    if (playerController.CurrentWeapon)
                    {
                        playerController.CurrentWeapon.gameObject.SetActive(true);
                        playerController.CurrentWeapon.GetComponent<BoxCollider>().enabled = false;

                    }
                }
                // set position to spawn point
                transform.position = MultiplayerGameManager.Instance.GetSpawnPoint(playerController.Team1).position;

            }
            // AI Player
            else
            {
                // set position to spawn point

                transform.position = MultiplayerGameManager.Instance.GetSpawnPoint(GetComponent<AIPlayer>().Team1).position;

                AIPlayer aiPlayer = GetComponent<AIPlayer>();
                if (aiPlayer.PlayerWeapon)
                {
                    aiPlayer.PlayerWeapon.gameObject.SetActive(true);
                    aiPlayer.PlayerWeapon.GetComponent<BoxCollider>().enabled = false;

                }


                aiPlayer.Model.enabled = true;
                aiPlayer.rigidBody.isKinematic = false;

                aiPlayer.agent.stoppingDistance = aiPlayer.startArrivedDistance;
                aiPlayer.fsm.ChangeState(AIPlayer.States.GoStartPos, AxlPlay.StateTransition.Overwrite);

            }

        }

    }
}