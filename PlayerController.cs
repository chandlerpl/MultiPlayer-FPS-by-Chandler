using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AxlPlay;
using UnityEngine.UI;

public class ItemInInventory
{
    public Item item;
    public PhotonView photonView;
}

[System.Serializable]
public class InteractableSound
{
    public int LayerMask;
    public AudioClip[] Sounds;
}
namespace AxlPlay
{
    public class PlayerController : Photon.PunBehaviour
    {
        public int MaxItemInventory = 2;
        //public Transform ShootBase;
        public float ClimbSpeed = 7f;

        public float TiltRightAngle = -0.1f;
        public float TiltLeftAngle = 0.1f;
        [Header("Grab")]
        public UIEffects GrabIcon;
        public Transform GrabOffset;
        public bool MoveToOffset = true;
        public float GrabFromDistance = 5f;
        [HideInInspector]
        public Item grabbedItem;
        [HideInInspector]

        public Transform itemLastParent;
        private Vector3 itemLastLocalPos;

        [HideInInspector]
        public PlayerStats MyStats;
        [HideInInspector]
        public int Kills;

        [HideInInspector]
        public int Deaths;

        public Weapon[] StartWeapons;
        [HideInInspector]

        public List<Weapon> StartWeaponsSpawned = new List<Weapon>();

        [Header("Camera")]
        public Camera RagdollDieCamera;
        public Camera MinimapCamera;
        public SpriteRenderer MiniMapIcon;

        public string GroundTag = "Ground";

        public Camera PCamera;
        public Transform SpineRot;

        public AudioSource AudioSource;
        public InteractableSound[] FootstepsSounds;
        public InteractableSound[] LandSounds;
        public AudioClip JumpSound;
        private int footstepSoundIndex;

        [HideInInspector]
        public float startFov;
        [Header("References")]
        public Animator CameraAnim;
        public Transform FPSView;
        public Transform weaponRecoil;
        public Transform camKickBack;
        public Transform weaponKickBack;

        [Header("Movement")]
        public float CrouchSpeed = 8f;

        public float CrouchHeight = 1f;
        public Animator Hands;
        public float JumpForce = 7f;
        public float WalkSpeed = 3.5f;
        public float AimingDown_WalkSpeed = 3f;

        public float RunSpeed = 6f;

        public float CrouchWalkSpeed = 2f;

        public float LookSensitivity = 3f;
        public float AimingDown_LookSensitivity = 2f;

        [Header("Model Animations")]
        public Animator ModelAnimator;
        public Transform Model;
        public float ModelRightAngle = 0f;
        public float ModelLeftAngle = 0f;
        public float ModelDefaultAngle = 0f;


        [HideInInspector]
        public bool Team1;

        public Transform WeaponBase;
        public Transform ModelWeaponBase;
        [HideInInspector]
        public Weapon CurrentWeapon;
        [HideInInspector]
        public List<ItemInInventory> Items = new List<ItemInInventory>();



        private PlayerMotor motor;
        [HideInInspector]
        public Rigidbody rigidBody;
        private CapsuleCollider capsuleCollider;



        private float timer;
        private float distToGround;
        private CanSeeObject canSeeObject;
        private Health health;
        public int index;


        private List<GameObject> gameObjectsSeen = new List<GameObject>();

        private bool jumpFlag;
        [HideInInspector]
        public bool isCrouched;
        private float startHeight;
        private CharacterController characterController;
        private Vector3 moveDirection;
        private bool crouching;

        public float Gravity = -15f;
        private float vol;
        [HideInInspector]
        public bool isShooting;
        private bool tiltFlag;
        private bool tiltFlag2;


        private bool inRunPose;
        private bool returningToNormalPose;
        private bool startedToRunPose;


        private bool inJumpPose;
        private bool startedToJumpPose;

        private bool shootRecoilReturn;

        private Vector3 weaponBaseParentInitialPosition;
        private Vector3 weaponBaseParentInitialRotation;
        [HideInInspector]
        public float _yRot;
        [HideInInspector]

        public float _cameraRotationX;

        [Header("HeadBob")]
        public bool HeadBob = true;
        public float headbobSpeed = 1f;
        public float headbobStepCounter;
        public float headbobAmountX = 1f;
        public float headbobAmountY = 1f;
        public float eyeHeightRacio = 0.9f;

        public float WalkDistanceToPlayStep = 0.18f;
        public float StickToGroundForce = 10f;
        public float GravityMultiplier = 2f;

        Vector3 parentLastPos;


        private bool isSighting2D;
        private float timeSighting2D;

        private Vector3 lastPos;

        private bool previousGrounded;
        private bool isGrounded;



        [HideInInspector]
        public string userName;

        private bool statsFlag;

        private bool OnLadder;

        private Vector3 climbDirection;
        private Vector3 lateralMove;
        private Vector3 ladderMovement;
        private float downThreshold;

        private float ladderExitTimer;
        private bool ladderExit;

        RaycastHit hit;
        private bool runKeyDown;
        [HideInInspector]
        public bool finishPicking = true;
        [HideInInspector]
        public bool onTriggerWithWeapon;


        #region ManagedByMasterClient
        [HideInInspector]
        public float afkTime;
        [HideInInspector]
        public float afkLastShootTime;
        [HideInInspector]
        public Vector3 afklastPos;
        [HideInInspector]
        public Quaternion afklastRot;

        #endregion
        public void Reset()
        {
            if (photonView.isMine)
            {
                // delete the player items
                List<int> deleteThings = new List<int>();
                foreach (var item in Items)
                {
                    if (!StartWeaponsSpawned.Contains(item.item.GetComponent<Weapon>()))
                    {
                        deleteThings.Add(item.photonView.viewID);
                    }

                }
                foreach (var itemToDelete in deleteThings)
                {
                    photonView.RPC("DropItem", PhotonTargets.All, itemToDelete);

                }


                // use start item
                photonView.RPC("UseItem", PhotonTargets.AllBuffered, StartWeaponsSpawned[0].photonView.viewID, 0);
            }
            Team1 = false;
            index = 0;
            Kills = 0;
            Deaths = 0;
            // reset player stats
            foreach (var player in MultiplayerGameManager.Instance.Players)
            {
                player.Kills = 0;
                player.Deaths = 0;
            }

            Items = new List<ItemInInventory>();
            gameObjectsSeen = new List<GameObject>();
            CurrentWeapon = null;

            // reset afk variables
            // used to calculate hoy many time they have been in afk mode before kick them
            afklastPos = transform.position;
            afklastRot = transform.rotation;
            afkTime = 0f;

            health.Reset();
        }

        void Awake()
        {
            // set afk variables
            // used to calculate hoy many time they have been in afk mode before kick them

            afklastPos = transform.position;
            afklastRot = transform.rotation;
            afkTime = 0f;

            downThreshold = -0.4f;
            climbDirection = Vector3.up;

            if (photonView.isMine)
            {

                if (Application.isMobilePlatform && GameManager.Instance.MobileUI)
                    GameManager.Instance.MobileUI.gameObject.SetActive(true);
                //      MinimapCamera.transform.SetParent(null);

                // pickup the start weapons
                for (int i = 0; i < StartWeapons.Length; i++)
                {
                    GameObject weaponSpawned = PhotonNetwork.Instantiate(StartWeapons[i].name, Vector3.zero, Quaternion.identity, 0);
                    Weapon weaponScpawned = weaponSpawned.GetComponent<Weapon>();
                    StartWeaponsSpawned.Add(weaponScpawned);
                    photonView.RPC("PickupItem", PhotonTargets.AllBuffered, weaponScpawned.photonView.viewID);

                }
                // use the first start weapon 
                photonView.RPC("UseItem", PhotonTargets.AllBuffered, StartWeaponsSpawned[0].photonView.viewID, 0);
            }
            // get references
            startFov = Camera.main.fieldOfView;
            motor = GetComponent<PlayerMotor>();
            rigidBody = GetComponent<Rigidbody>();

            capsuleCollider = GetComponent<CapsuleCollider>();
            canSeeObject = GetComponent<CanSeeObject>();
            health = GetComponent<Health>();
            weaponBaseParentInitialPosition = WeaponBase.parent.localPosition;
            weaponBaseParentInitialRotation = WeaponBase.parent.localEulerAngles;

            characterController = GetComponent<CharacterController>();
            startHeight = characterController.height;
            lastPos = transform.localPosition;

            if (!photonView.isMine)
            {
                MinimapCamera.gameObject.SetActive(false);
                motor.enabled = false;
                rigidBody.isKinematic = true;

            }
            else
            {
                MultiplayerGameManager.Instance.LocalPlayer = this;
            }
            // calculate distance to ground, used to check if player is grounuded
            if (capsuleCollider)
                distToGround = capsuleCollider.bounds.extents.y;
        }

        private void Start()
        {
            if (photonView.isMine)
            {
                // desactivate the model of the player because it is used only when photonView is not mine in order to other players see my model on network

                foreach (Transform child in Model.transform)
                {

                    child.gameObject.SetActive(false);
                }

            }
            if (!photonView.isMine || MultiplayerGameManager.Instance.finished)
            {

                motor.cam.enabled = false;

                AudioListener audioListener = motor.cam.GetComponent<AudioListener>();
                if (audioListener)
                    DestroyImmediate(audioListener);

            }
            else
                MultiplayerGameManager.Instance.AssignTeam(photonView.viewID);


            if (!PhotonNetwork.isMasterClient)
            {
                if (photonView.isMine)
                {

                    foreach (PhotonPlayer player in PhotonNetwork.playerList)
                    {
                        if (player.NickName == PhotonNetwork.player.NickName)
                        {
                            MultiplayerGameManager.Instance.photonView.RPC("NewPlayerSpawned", PhotonTargets.MasterClient, player);
                            break;
                        }
                    }

                }
            }
            else
            {

                MultiplayerGameManager.Instance.Players.Add(this);
                MultiplayerGameManager.Instance.PlayerIDs.Add(photonView.viewID);

            }
            if (photonView.isMine)
            {

                userName = PhotonNetwork.player.NickName;
                // get user name to use in player stats script, when showing player list
                photonView.RPC("GetUserName", PhotonTargets.OthersBuffered, PhotonNetwork.player.NickName);

            }

            StartCoroutine(SetUIStats());
        }

        [PunRPC]
        void GetUserName(string _name)
        {
            userName = _name;
        }
        // instantiate objects in player list
        public IEnumerator SetUIStats()
        {
            yield return new WaitForSeconds(5f);

            if (MultiplayerGameManager.Instance.GameMode == MultiplayerGameManager.GameModes.TeamDeathmatch)
            {
                if (Team1)
                {
                    GameObject instantiated = Instantiate(MultiplayerGameManager.Instance.PlayerStatsUI, MultiplayerGameManager.Instance.PlayerStatsTeam1Grid.transform);
                    instantiated.GetComponent<Image>().color = MultiplayerGameManager.Instance.PlayerStatsUITeam1Color;
                    MyStats = instantiated.GetComponent<PlayerStats>();
                    MyStats.Owner = this;

                    MultiplayerGameManager.Instance.Team1Stats.Add(instantiated);
                }
                else
                {
                    GameObject instantiated = Instantiate(MultiplayerGameManager.Instance.PlayerStatsUI, MultiplayerGameManager.Instance.PlayerStatsTeam2Grid.transform);
                    instantiated.GetComponent<Image>().color = MultiplayerGameManager.Instance.PlayerStatsUITeam2Color;

                    MyStats = instantiated.GetComponent<PlayerStats>();
                    MyStats.Owner = this;

                    MultiplayerGameManager.Instance.Team2Stats.Add(instantiated);
                }
            }
            else
            {
                GameObject instantiated = Instantiate(MultiplayerGameManager.Instance.PlayerStatsUI, MultiplayerGameManager.Instance.PlayerStatsFreeForAllGrid.transform);
                instantiated.GetComponent<Image>().color = MultiplayerGameManager.Instance.PlayerStatsUIFreeForAllColor;

                MyStats = instantiated.GetComponent<PlayerStats>();
                MyStats.Owner = this;

                MultiplayerGameManager.Instance.Team1Stats.Add(instantiated);
            }

            for (int i = 0; i < MultiplayerGameManager.Instance.Team1Stats.ToArray().Length; i += 2)
            {
                if (MultiplayerGameManager.Instance.GameMode == MultiplayerGameManager.GameModes.TeamDeathmatch)
                    MultiplayerGameManager.Instance.Team1Stats[i].GetComponent<Image>().color = MultiplayerGameManager.Instance.PlayerStatsUITeam1ColorVariant;
                else
                    MultiplayerGameManager.Instance.Team1Stats[i].GetComponent<Image>().color = MultiplayerGameManager.Instance.PlayerStatsUIFreeForAllColorVariant;

            }
            for (int i = 0; i < MultiplayerGameManager.Instance.Team2Stats.ToArray().Length; i += 2)
            {
                if (MultiplayerGameManager.Instance.GameMode == MultiplayerGameManager.GameModes.TeamDeathmatch)

                    MultiplayerGameManager.Instance.Team2Stats[i].GetComponent<Image>().color = MultiplayerGameManager.Instance.PlayerStatsUITeam2ColorVariant;

            }

        }

        // from master client
        [PunRPC]
        void SetMyTeam(bool team1, int viewID)
        {
            if (viewID == photonView.viewID)
            {
                Team1 = team1;
                if (photonView.viewID != MultiplayerGameManager.Instance.LocalPlayer.photonView.viewID)
                {
                    if (team1 == MultiplayerGameManager.Instance.LocalPlayer.Team1 && MultiplayerGameManager.Instance.GameMode != MultiplayerGameManager.GameModes.FreeForAll)
                    {
                        MiniMapIcon.color = Color.blue;
                    }
                    else
                    {
                        if (!MultiplayerGameManager.Instance.ShowAlwaysEnemiesInMiniMap)
                            MiniMapIcon.gameObject.SetActive(false);
                        MiniMapIcon.color = Color.red;

                    }
                }
                if (team1)
                {
                    if (!MultiplayerGameManager.Instance.Team1.Contains(this))
                    {
                        MultiplayerGameManager.Instance.Team1.Add(this);
                    }
                }
                else
                {
                    if (!MultiplayerGameManager.Instance.Team2.Contains(this))
                    {
                        MultiplayerGameManager.Instance.Team2.Add(this);

                    }
                }
            }
            else
            {
                PlayerController player = PhotonView.Find(viewID).GetComponent<PlayerController>();

                if (team1)
                {
                    if (!MultiplayerGameManager.Instance.Team1.Contains(player))
                    {
                        MultiplayerGameManager.Instance.Team1.Add(player);
                    }
                }
                else
                {
                    if (!MultiplayerGameManager.Instance.Team2.Contains(player))
                    {
                        MultiplayerGameManager.Instance.Team2.Add(player);

                    }
                }

            }

        }


        void Update()
        {


            if (MultiplayerGameManager.Instance.finished)
            {
                //Apply movement
                motor.Move(Vector3.zero);

                //Apply rotation
                motor.Rotate(Vector3.zero);

                //Apply camera rotation
                motor.RotateCamera(0f);
                return;
            }
            if (!photonView.isMine)
            {
                // detect if this player is in afk

                timer += Time.deltaTime;

                afkLastShootTime += Time.deltaTime;
                SpineRot.transform.localEulerAngles = PCamera.transform.localEulerAngles;

                if (PhotonNetwork.isMasterClient)
                {
                    if (PhotonNetwork.GetPing() > MultiplayerGameManager.Instance.MaxPing)
                    {
                        MultiplayerGameManager.Instance.KickPlayer(this, "Ping too high");

                    }

                    bool notAfk = false;
                    if (Vector3.Distance(transform.position, afklastPos) > 0.1f)
                    {
                        notAfk = true;
                        afklastPos = transform.position;
                    }
                    if (Quaternion.Angle(transform.rotation, afklastRot) > 0.1f)
                    {
                        notAfk = true;
                        afklastRot = transform.rotation;
                    }

                    // has shooted
                    if (afkLastShootTime > timer)
                    {
                        notAfk = true;
                        afkLastShootTime = timer;
                    }

                    if (!notAfk)
                    {
                        afkTime += Time.deltaTime;
                        if (afkTime >= MultiplayerGameManager.Instance.MaxTimeAfk)
                        {
                            MultiplayerGameManager.Instance.KickPlayer(this, "c");
                        }
                    }
                    else
                    {
                        afkTime = 0f;
                    }
                }

                return;
            }




            if (health.isDead)
                return;

            // check if isn't holding breath because is same key usually
            if (InputManager.inputManager.GetButtonDown(InputManager.inputManager.RunAxis) && timeSighting2D == 0f)
            {
                runKeyDown = !runKeyDown;
            }
            if (GetIsIdle() || CurrentWeapon && CurrentWeapon.isAimingDown)
                runKeyDown = false;

            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction);
            //   Debug.Log(LayerMask.LayerToName(8));
            RaycastHit[] hits = Physics.RaycastAll(ray, GameManager.Instance.PickUpDistance);
            //     RaycastHit hit = hits[0];
            bool hitted = false;
            if (hits.Length > 0)
            {
                for (int x = 0; x < hits.Length; x++)
                {
                    if (hits[x].transform != transform && hits[x].transform.root != transform && hits[x].transform.parent != transform)
                    {
                        if (hits[x].transform.GetComponent<Item>() && !hits[x].transform.GetComponent<Weapon>() || hits[x].transform.GetComponent<Weapon>() && !hits[x].transform.GetComponent<Weapon>().Owner && !hits[x].transform.GetComponent<Weapon>().AIOwner)
                        {
                            if (CurrentWeapon && hits[x].transform.GetComponent<Weapon>() && hits[x].transform.GetComponent<Weapon>().WeaponType == CurrentWeapon.WeaponType)
                            {
                                hitted = false;
                            }
                            else
                            {
                                hit = hits[x];
                                hitted = true;
                            }
                        }
                    }

                }
                if (hitted)
                {

                    if (hit.transform.GetComponent<Pickup>())
                    {
                        if (GameManager.Instance.InteractIcon)
                            GameManager.Instance.InteractIcon.DoFadeIn();

                        if (InputManager.inputManager.GetButtonDown(InputManager.inputManager.PickUpItemAxis))
                        {
                            Pickup pickScript = hit.transform.GetComponent<Pickup>();
                            pickScript.PickupItem(this);
                        }
                    }
                }
                else
                {
                    if (GameManager.Instance.InteractIcon && !onTriggerWithWeapon)
                        GameManager.Instance.InteractIcon.DoFadeOut();
                }

            }
            else
            {
                if (GameManager.Instance.InteractIcon)
                    GameManager.Instance.InteractIcon.DoFadeOut();
            }
            // check if player left the ladder
            if (ladderExit)
            {


                ladderExitTimer += Time.deltaTime;

                if (OnLadder)
                {
                    ladderExitTimer = 0f;
                    ladderExit = false;
                }
                if (ladderExitTimer >= 1f)
                {
                    ladderExitTimer = 0f;
                    ladderExit = false;

                    OnLadder = false;

                    if (CurrentWeapon)
                    {
                        photonView.RPC("UseItem", PhotonTargets.All, CurrentWeapon.photonView.viewID, index);
                    }

                }
            }
            // tilt 
            if (Input.GetKey(KeyCode.E))
            {
                FPSView.localRotation = Quaternion.Lerp(FPSView.transform.localRotation, new Quaternion(FPSView.transform.localRotation.x, FPSView.transform.localRotation.y, TiltRightAngle, FPSView.transform.localRotation.w), Time.deltaTime * 7f);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                FPSView.transform.localRotation = Quaternion.Lerp(FPSView.transform.localRotation, new Quaternion(FPSView.transform.localRotation.x, FPSView.transform.localRotation.y, TiltLeftAngle, FPSView.transform.localRotation.w), Time.deltaTime * 7f);
            }
            else
            {
                FPSView.transform.localRotation = Quaternion.Lerp(FPSView.transform.localRotation, new Quaternion(FPSView.transform.localRotation.x, FPSView.transform.localRotation.y, 0, FPSView.transform.localRotation.w), Time.deltaTime * 7f);
            }

            // still grab system doesn't work in online mode, soon releasing
            if (GrabOffset && PhotonNetwork.offlineMode)
            {

                RaycastHit grabHit;
                Physics.Raycast(transform.position, transform.up, out grabHit);

                RaycastHit[] grabhits = Physics.RaycastAll(ray, GrabFromDistance);
                //     RaycastHit hit = hits[0];
                bool grabHitted = false;
                if (grabhits.Length > 0)
                {
                    for (int x = 0; x < grabhits.Length; x++)
                    {
                        if (grabhits[x].transform != transform && grabhits[x].transform.root != transform && grabhits[x].transform.parent != transform)
                        {
                            grabHit = grabhits[x];
                            grabHitted = true;
                        }

                    }

                    if (grabHitted)
                    {
                        // check if there's something front to me
                        //    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, GrabFromDistance))
                        //   {
                        // show grab icon
                        if (GrabIcon)
                            GrabIcon.DoFadeIn();

                        if (InputManager.inputManager.GetButtonDown(InputManager.inputManager.GrabKey.ToString()))
                        {
                            // grab item
                            grabbedItem = grabHit.transform.GetComponent<Item>();
                            // check if this item can be dragged
                            if (grabbedItem && grabbedItem.Dragable)
                            {
                                Rigidbody currentDraggingItemRg = grabbedItem.GetComponent<Rigidbody>();


                                if (currentDraggingItemRg)
                                    currentDraggingItemRg.isKinematic = true;
                                itemLastLocalPos = grabbedItem.transform.localPosition;
                                itemLastParent = grabbedItem.transform.parent;


                                grabbedItem.transform.SetParent(GrabOffset);
                                if (MoveToOffset)
                                {
                                    grabbedItem.transform.localPosition = Vector3.zero;

                                    grabbedItem.transform.localRotation = Quaternion.identity;
                                }

                            }
                            else
                            {
                                grabbedItem = null;
                            }
                        }
                    }
                    // if there isn't something fron to me
                    else
                    {

                        if (GrabIcon)
                            GrabIcon.DoFadeOut();
                    }
                    if (InputManager.inputManager.GetButton(InputManager.inputManager.GrabKey.ToString()))
                    {
                        // make lerp rotation effect
                        if (grabbedItem)
                            grabbedItem.transform.localRotation = Quaternion.Lerp(grabbedItem.transform.localRotation, new Quaternion(Camera.main.transform.localRotation.x, transform.localRotation.y, grabbedItem.transform.localRotation.z, grabbedItem.transform.localRotation.w), Time.deltaTime * 7f);
                    }
                    // drop item
                    if (InputManager.inputManager.GetButtonUp(InputManager.inputManager.GrabKey.ToString()))
                    {
                        if (grabbedItem)
                        {


                            grabbedItem.transform.SetParent(itemLastParent);

                            Rigidbody currentDraggingItemRg = grabbedItem.GetComponent<Rigidbody>();
                            if (currentDraggingItemRg)
                                currentDraggingItemRg.isKinematic = false;

                            grabbedItem = null;
                        }
                    }
                }


            }

            if (InputManager.inputManager.GetButtonDown(InputManager.inputManager.ShowPlayerListKey))
            {
                // show player list
                MultiplayerGameManager.Instance.PlayerStatsList.DoFadeIn();
            }
            if (InputManager.inputManager.GetButtonUp(InputManager.inputManager.ShowPlayerListKey))
            {
                MultiplayerGameManager.Instance.PlayerStatsList.DoFadeOut();
            }
            // show sniper sight 2D
            if (isSighting2D)
            {
                if (InputManager.inputManager.GetButton(InputManager.inputManager.HoldBreathSniper) && CurrentWeapon.MaxTimeSight2DToExplote > 0f && !CameraAnim.GetCurrentAnimatorStateInfo(0).IsName("ExploteBreath"))
                {
                    timeSighting2D += Time.deltaTime;
                    CameraAnim.Play("Holding");
                    if (timeSighting2D >= CurrentWeapon.MaxTimeSight2DToExplote)
                    {
                        // explote breath when holding breath a lot time

                        timeSighting2D = 0f;
                        CameraAnim.SetFloat("ExplotionForce", 1f);

                        CameraAnim.Play("ExploteBreath");
                    }
                }
                float scrollWheel = InputManager.inputManager.GetAxis("Mouse ScrollWheel");
                // zoom
                if (scrollWheel < 0)
                {
                    Camera.main.fieldOfView -= CurrentWeapon.Sight2DZoomSpeed * Time.deltaTime;
                    if (Camera.main.fieldOfView < CurrentWeapon.Sight2DCameraFov)
                    {
                        Camera.main.fieldOfView = CurrentWeapon.Sight2DCameraFov;
                    }
                }
                else if (scrollWheel > 0)
                {
                    Camera.main.fieldOfView += CurrentWeapon.Sight2DZoomSpeed * Time.deltaTime;
                    if (Camera.main.fieldOfView > CurrentWeapon.Sight2DMaxCameraFov)
                    {
                        Camera.main.fieldOfView = CurrentWeapon.Sight2DMaxCameraFov;
                    }
                }
                else
                {
                    if (timeSighting2D > 0)
                    {
                        CameraAnim.SetFloat("ExplotionForce", Mathf.Clamp01(timeSighting2D));
                        CameraAnim.Play("ExploteBreath");

                        timeSighting2D = 0f;
                    }
                }
            }


            if (CurrentWeapon != null)
            {
                // return kick back weapon and camera to original pos
                camKickBack.localRotation = Quaternion.Lerp(camKickBack.localRotation, Quaternion.identity, Time.deltaTime * CurrentWeapon.returnSpeed);
                weaponKickBack.localRotation = Quaternion.Lerp(weaponKickBack.localRotation, Quaternion.identity, Time.deltaTime * CurrentWeapon.returnSpeed);
            }
            // head bob effect
            if (HeadBob)
            {
                if (IsGrounded())

                    headbobStepCounter += Vector3.Distance(parentLastPos, transform.position) * headbobSpeed;

                Vector3 newCameraPos = Camera.main.transform.localPosition;
                newCameraPos.x = Mathf.Sin(headbobStepCounter) * headbobAmountX;
                newCameraPos.y = (Mathf.Cos(headbobStepCounter * 2) * headbobAmountY * -1) + (Camera.main.transform.localScale.y * eyeHeightRacio) - (Camera.main.transform.localScale.y / 2);

                Camera.main.transform.localPosition = newCameraPos;

                parentLastPos = transform.position;

            }
            // play player effects sounds
            // Run, walk, jump,land
            if (AudioSource && !AudioSource.isPlaying)
            {

                if (IsGrounded())
                {

                    if (FootstepsSounds.Length > 0)
                    {
                        if (GetIsRun())
                        {
                            if (!CurrentWeapon || CurrentWeapon && !CurrentWeapon.animationC.IsPlaying("Land"))
                                // when running add more pitch, it sounds faster
                                AudioSource.pitch = 1.5f;
                        }
                        else
                        {
                            AudioSource.pitch = 1f;
                        }

                        // check if walked distance to play footStep sound
                        if (GetIsWalk() || GetIsRun() && Vector3.Distance(transform.localPosition, lastPos) > WalkDistanceToPlayStep)
                        {
                            lastPos = transform.localPosition;
                            RaycastHit hit;
                            if (Physics.Raycast(transform.position, -Vector3.up, out hit, distToGround + 0.1f))
                            {
                                if (FootstepsSounds.Length > 0)
                                {
                                    foreach (var footStep in FootstepsSounds)
                                    {
                                        if (footStep.LayerMask == hit.transform.gameObject.layer)
                                        {
                                            AudioSource.clip = footStep.Sounds[footstepSoundIndex];
                                            break;
                                        }
                                    }
                                }
                                AudioSource.Play();
                            }
                            if (footstepSoundIndex < FootstepsSounds.Length)
                            {

                                footstepSoundIndex++;

                            }
                            if (footstepSoundIndex > FootstepsSounds.Length - 1)
                            {
                                footstepSoundIndex = 0;
                            }
                        }
                    }
                    if (!previousGrounded)
                    {
                        if (CurrentWeapon && CurrentWeapon.animationC && CurrentWeapon.LandClip)
                        {
                            CurrentWeapon.animationC.Play("Land");

                        }

                        if (LandSounds.Length > 0)
                        {
                            // play land effect
                            RaycastHit hit;
                            if (Physics.Raycast(transform.position, -Vector3.up, out hit, distToGround + 0.1f))
                            {
                                if (LandSounds.Length > 0)
                                {
                                    foreach (var landSound in LandSounds)
                                    {
                                        if (landSound.LayerMask == hit.transform.gameObject.layer)
                                        {

                                            AudioSource.PlayOneShot(landSound.Sounds[0]);
                                            break;
                                        }
                                    }
                                }
                            }
                        }


                    }
                    else
                    {
                        AudioSource.pitch = 1f;
                    }

                }
            }
            previousGrounded = IsGrounded();

            // text above head if see enemy near
            if (canSeeObject.CanSee())
            {

                TextMesh nameNear = (TextMesh)canSeeObject.CanSee().GetComponentInChildren(typeof(TextMesh), true);
                if (nameNear != null)
                {
                    GameObject goSeen = nameNear.gameObject;
                    PlayerController playerSeen = canSeeObject.CanSee().GetComponent<PlayerController>();


                    if (playerSeen)
                    {
                        if (playerSeen.Team1 == Team1 && MultiplayerGameManager.Instance.GameMode != MultiplayerGameManager.GameModes.FreeForAll)
                        {
                            nameNear.color = Color.blue;
                        }
                        else
                        {
                            nameNear.color = Color.red;

                        }
                    }
                    if (gameObjectsSeen.Count > 0)
                    {
                        foreach (var go in gameObjectsSeen.ToList())
                        {
                            if (go == null)
                                continue;
                            if (go != goSeen)
                            {
                                go.SetActive(false);
                                gameObjectsSeen.Remove(go);
                            }
                        }
                    }
                    goSeen.SetActive(true);
                    goSeen.transform.LookAt(transform);

                    if (!gameObjectsSeen.Contains(goSeen))
                        gameObjectsSeen.Add(goSeen);
                }

            }
            else if (gameObjectsSeen.Count > 0)
            {
                foreach (var go in gameObjectsSeen.ToList())
                {
                    if (go == null)
                        continue;
                    go.SetActive(false);
                    gameObjectsSeen.Remove(go);

                }
            }


            timer += Time.deltaTime;

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            //Calculate rotation as a 3D vector (turning around)
            _yRot = InputManager.inputManager.GetAxisRaw("Mouse X");

            float _lookSensitivity = LookSensitivity;
            if (CurrentWeapon && CurrentWeapon.isAimingDown)
                _lookSensitivity = AimingDown_LookSensitivity;

            if (CurrentWeapon && CurrentWeapon.isAimingDown && CurrentWeapon.Sight2D != null)
                _lookSensitivity = CurrentWeapon.Sight2DLookSensitivity;

            Vector3 _rotation = new Vector3(0f, _yRot, 0f) * _lookSensitivity;

            //Apply rotation
            motor.Rotate(_rotation);
            //Calculate camera rotation as a 3D vector (turning around)
            float _xRot = InputManager.inputManager.GetAxisRaw("Mouse Y");

            _cameraRotationX = _xRot * _lookSensitivity;

            //Apply camera rotation
            motor.RotateCamera(_cameraRotationX);

            // Calculate the thrusterforce based on player InputManager.inputManager
            Vector3 _thrusterForce = Vector3.zero;


            if (InputManager.inputManager.GetButtonDown(InputManager.inputManager.Crouch) && IsGrounded() && PhotonNetwork.offlineMode)
            {
                // crouch
                isCrouched = !isCrouched;
            }



            // inventory

            if (Items.Count > 1 && !OnLadder)
            {
                if (CurrentWeapon && CurrentWeapon.Sight2D && !CurrentWeapon.isAimingDown || CurrentWeapon && !CurrentWeapon.Sight2D || !CurrentWeapon)
                {
                    // change items with scroll wheel or ps4 mapping
                    var d = InputManager.inputManager.GetAxisRaw("Mouse ScrollWheel");
                    if (InputManager.inputManager.GetButtonDown(InputManager.inputManager.ChangeItemAxis))
                        d = 1f;

                    if (d > 0f)
                    {

                        // scroll up
                        if (index == Items.ToArray().Length - 1)
                            index = 0;
                        else
                        {
                            index++;
                        }
                        photonView.RPC("UseItem", PhotonTargets.AllBuffered, Items[index].item.GetComponent<PhotonView>().viewID, index);

                    }
                    else if (d < 0f)
                    {

                        // scroll down
                        if (index <= 0)
                            index = Items.ToArray().Length - 1;
                        else
                        {
                            index--;
                        }
                        photonView.RPC("UseItem", PhotonTargets.AllBuffered, Items[index].item.GetComponent<PhotonView>().viewID, index);

                    }
                }
            }
            //--
            if (CurrentWeapon != null && CurrentWeapon.hasReloaded)
            {

                if (CurrentWeapon.SwayWeapon)
                {
                    // sway weapon

                    float movementX = -InputManager.inputManager.GetAxis(InputManager.inputManager.TurnAroundX) * CurrentWeapon.Sway_Amount;
                    float movementY = -InputManager.inputManager.GetAxis(InputManager.inputManager.TurnAroundY) * CurrentWeapon.Sway_Amount;

                    movementX = Mathf.Clamp(movementX, -CurrentWeapon.Sway_MaxAmount, CurrentWeapon.Sway_MaxAmount);
                    movementY = Mathf.Clamp(movementY, -CurrentWeapon.Sway_MaxAmount, CurrentWeapon.Sway_MaxAmount);

                    Vector3 finalPosition = new Vector3(movementX, movementY, 0);
                    // tilt weapon when moving into that direction
                    if (!CurrentWeapon.isAimingDown)
                    {
                        var tiltAroundZ = -InputManager.inputManager.GetAxis(InputManager.inputManager.MoveHorizontal) * CurrentWeapon.TiltX;
                        var tiltAroundX = -InputManager.inputManager.GetAxis(InputManager.inputManager.MoveVertical) * CurrentWeapon.TiltY;
                        var target = Quaternion.Euler(tiltAroundX, 0, tiltAroundZ);
                        CurrentWeapon.transform.localRotation = Quaternion.Slerp(CurrentWeapon.transform.localRotation, target, Time.deltaTime * 2f);
                    }
                    else
                    {
                        CurrentWeapon.transform.localRotation = Quaternion.Slerp(CurrentWeapon.transform.localRotation, Quaternion.identity, Time.deltaTime * 2f);

                    }


                    Vector3 newPos = Vector3.zero;
                    Vector3 newRot = Vector3.zero;

                    CurrentWeapon.transform.localPosition = Vector3.Lerp(CurrentWeapon.transform.localPosition, finalPosition + CurrentWeapon.initialPosition, Time.deltaTime * CurrentWeapon.Sway_SmoothAmount);

                }

                if (InputManager.inputManager.GetButtonDown("Fire2"))
                {
                    // when aim down reset position

                    CurrentWeapon.animationC.Stop();
                    CurrentWeapon.transform.localPosition = Vector3.zero;
                    CurrentWeapon.transform.parent.parent.localPosition = Vector3.zero;

                }

                if (InputManager.inputManager.GetButton("Fire2"))
                    AimingDownSights();
                else
                    AimingUpSights();

            }

            if (CurrentWeapon != null)
            {

                if (!GetIsRun())
                {

                    if (InputManager.inputManager.GetButton("Fire1") && timer > CurrentWeapon.ShootInterval)
                    {
                        // shoot weapon
                        if (CurrentWeapon.hasReloaded)
                        {
                            timer = 0f;
                            isShooting = true;

                            ModelAnimator.SetTrigger("Shoot");

                            photonView.RPC("Shoot", PhotonTargets.All, null);

                        }

                        else if (CurrentWeapon.ammunition <= 0)
                        {
                            timer = 0f;
                            if (CurrentWeapon.AudioSource && CurrentWeapon.NoAmmoSound)
                                CurrentWeapon.AudioSource.PlayOneShot(CurrentWeapon.NoAmmoSound);
                        }
                    }
                    else
                    {
                        isShooting = false;
                    }

                }
                if (!CurrentWeapon.hasReloaded)
                {
                    AimingUpSights();

                }
            }

        }
        float LoopPos(float current, float targetA, float targetB, float speed, ref bool _flag)

        {

            if (!_flag)
            {

                current = Mathf.LerpUnclamped(current, targetA, Time.deltaTime * speed);
                if (Mathf.Abs(current - targetA) < 0.01f)
                    _flag = !_flag;
            }
            else
            {

                current = Mathf.LerpUnclamped(current, targetB, Time.deltaTime * speed);
                if (Mathf.Abs(current - targetB) < 0.01f)
                    _flag = !_flag;
            }
            return current;

        }


        Vector3 LoopRot(Vector3 current, Vector3 targetA, Vector3 targetB, float speed, ref bool _flag)
        {

            current.x = (current.x > 180) ? current.x - 360 : current.x;
            current.y = (current.y > 180) ? current.y - 360 : current.y;
            current.z = (current.z > 180) ? current.z - 360 : current.z;


            if (!_flag)
            {

                current = Vector3.MoveTowards(current, targetA, Time.deltaTime * speed);
                if (Vector3.Distance(current, targetA) < 0.01f)
                    _flag = !_flag;
            }
            else
            {

                current = Vector3.MoveTowards(current, targetB, Time.deltaTime * speed);
                if (Vector3.Distance(current, targetB) < 0.01f)
                    _flag = !_flag;
            }
            return current;
        }
        Vector3 LoopPos(Vector3 current, Vector3 targetA, Vector3 targetB, float speed, ref bool _flag)

        {


            if (!_flag)
            {

                current = Vector3.MoveTowards(current, targetA, Time.deltaTime * speed);
                if (Vector3.Distance(current, targetA) < 0.01f)
                    _flag = !_flag;
            }
            else
            {

                current = Vector3.MoveTowards(current, targetB, Time.deltaTime * speed);
                if (Vector3.Distance(current, targetB) < 0.01f)
                    _flag = !_flag;
            }
            return current;

        }

        public float GetCurrentCrosshairState()
        {
            if (!CurrentWeapon)
                return 0f;
            if (isCrouched)
                return CurrentWeapon.CrosshairCrouchPrecision;
            if (GetIsIdle())
                return CurrentWeapon.CrosshairIdlePrecision;
            if (GetIsWalk())
                return CurrentWeapon.CrosshairWalkPrecision;
            if (GetIsRun())
                return CurrentWeapon.CrosshairRunPrecision;

            return CurrentWeapon.CrosshairIdlePrecision;
        }
        private void FixedUpdate()
        {


            float h = characterController.height;
            float speed = WalkSpeed;

            if (CurrentWeapon && CurrentWeapon.isAimingDown)
            {
                speed = AimingDown_WalkSpeed;
            }
            if (isCrouched)
            {
                speed = CrouchWalkSpeed;


            }
            float fpHeight = startHeight;
            if (isCrouched)
                // crouch
                fpHeight = characterController.height * 0.5f;


            float lastFPHeight = characterController.height;
            // set character controller height
            characterController.height = Mathf.Lerp(characterController.height, fpHeight, 10f * Time.deltaTime);
            float fixedVerticalPosition = transform.position.y + (characterController.height - lastFPHeight) / 2;
            // and fixed position to make the crouch smooth
            transform.position = new Vector3(transform.position.x, fixedVerticalPosition, transform.position.z);
            // set the speed respect to the state of the player (crouched , slower speed, run, faster speed)
            if (GetIsRun())
            {
                speed = RunSpeed;
            }
            if (photonView.isMine)
                GameManager.Instance.ExpandCrosshair(GetCurrentCrosshairState());


            float _xMov = InputManager.inputManager.GetAxis(InputManager.inputManager.MoveHorizontal);
            float _zMov = InputManager.inputManager.GetAxis(InputManager.inputManager.MoveVertical);


            if (photonView.isMine)
            {
                // fix animation angles in 3d model

                if (_xMov > 0 && _zMov > 0)
                {
                    Model.transform.localEulerAngles = new Vector3(Model.transform.localEulerAngles.x, ModelRightAngle, Model.transform.localEulerAngles.z);
                }
                else if (_xMov < 0 && _zMov > 0)
                {
                    Model.transform.localEulerAngles = new Vector3(Model.transform.localEulerAngles.x, ModelLeftAngle, Model.transform.localEulerAngles.z);

                }
                else if (_xMov > 0 && _zMov < 0)
                {
                    Model.transform.localEulerAngles = new Vector3(Model.transform.localEulerAngles.x, ModelRightAngle, Model.transform.localEulerAngles.z);
                }
                else if (_xMov < 0 && _zMov < 0)
                {
                    Model.transform.localEulerAngles = new Vector3(Model.transform.localEulerAngles.x, ModelLeftAngle, Model.transform.localEulerAngles.z);

                }
                else
                {
                    Model.transform.localEulerAngles = new Vector3(Model.transform.localEulerAngles.x, ModelDefaultAngle, Model.transform.localEulerAngles.z);

                }

            }

            float xAnimator = _xMov;
            float zAnimator = _zMov;

            if (!GetIsRun())
            {
                if (xAnimator > 0.5f)
                {

                    xAnimator = 0.5f;
                }
                if (xAnimator < -0.5f)
                {

                    xAnimator = -0.5f;
                }

                if (zAnimator > 0.5f)
                {

                    zAnimator = 0.5f;
                }
                if (zAnimator < -0.5f)
                {

                    zAnimator = -0.5f;
                }
            }

            ModelAnimator.SetFloat("X", xAnimator);
            ModelAnimator.SetFloat("Z", zAnimator);

            if (CurrentWeapon && photonView.isMine)
            {
                Recoil();
                if (CurrentWeapon.recoil > 0)
                {
                    GameManager.Instance.RecoilCrosshair();
                }
                else
                {
                    GameManager.Instance.UnRecoilCrosshair();
                }
            }
            if (!OnLadder)
            {

                Vector3 input = new Vector3(InputManager.inputManager.GetAxis(InputManager.inputManager.MoveHorizontal), InputManager.inputManager.GetAxis(InputManager.inputManager.MoveVertical), 0);
                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = transform.forward * input.y + transform.right * input.x;

                // get a normal for the surface that is being touched to move along it
                RaycastHit hitInfo;
                Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
                                   characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
                desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;


                moveDirection.x = desiredMove.x * speed;
                moveDirection.z = desiredMove.z * speed;
                if (characterController.isGrounded)
                {
                    moveDirection.y = -StickToGroundForce;

                }
                else
                {
                    moveDirection += Physics.gravity * GravityMultiplier * Time.fixedDeltaTime;

                }
            }
            else
            {
                var cameraRotation = Camera.main.transform.forward.y;
                if (OnLadder)
                {
                    Vector3 verticalMove;
                    verticalMove = climbDirection.normalized;
                    verticalMove *= InputManager.inputManager.GetAxis(InputManager.inputManager.MoveVertical);
                    verticalMove *= (cameraRotation > downThreshold) ? 1 : -1;
                    lateralMove = new Vector3(InputManager.inputManager.GetAxis(InputManager.inputManager.MoveHorizontal), 0, InputManager.inputManager.GetAxis(InputManager.inputManager.MoveVertical));
                    lateralMove = transform.TransformDirection(lateralMove);
                    ladderMovement = verticalMove + lateralMove;
                    characterController.Move(ladderMovement * ClimbSpeed * Time.deltaTime);
                    if (InputManager.inputManager.GetButtonDown(InputManager.inputManager.Jump))
                    {
                        OnLadder = false;
                    }
                }

            }
            if (InputManager.inputManager.GetButtonDown(InputManager.inputManager.Jump) && IsGrounded() && !OnLadder && !isCrouched)
            {
                if (CurrentWeapon == null || CurrentWeapon && !CurrentWeapon.isAimingDown)
                {

                    moveDirection.y = JumpForce;

                    jumpFlag = true;
                    ModelAnimator.SetBool("isJumping", true);
                    if (CurrentWeapon && CurrentWeapon.animationC)
                        CurrentWeapon.animationC.Stop();
                    if (CurrentWeapon && CurrentWeapon.animationC && CurrentWeapon.JumpClip)
                    {
                        CurrentWeapon.animationC.Play("Jump");
                    }
                }
            }
            if (!OnLadder)
                characterController.Move(moveDirection * Time.fixedDeltaTime);

            if (!IsGrounded() && jumpFlag)
            {
                // if jumped and its not anymore in the ground
                jumpFlag = false;
            }
            if (!jumpFlag && Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.2f))
            {
                // set jump animattor
                ModelAnimator.SetBool("isJumping", false);

            }

        }
        public void ApplyJumpForce()
        {
            if (AudioSource && JumpSound)
                AudioSource.PlayOneShot(JumpSound);
            rigidBody.AddForce(new Vector3(0, JumpForce, 0), ForceMode.Impulse);
        }

        IEnumerator DesactivateMiniMap(float inTime)
        {
            yield return new WaitForSeconds(inTime);
            MiniMapIcon.gameObject.SetActive(false);

        }
        #region Weapon 
        [PunRPC]
        public void Shoot()
        {

            CurrentWeapon.recoil += CurrentWeapon.RecoilForce;
            CurrentWeapon.WeaponKick();

            if (CurrentWeapon.AudioSource && CurrentWeapon.ShootSound)
                CurrentWeapon.AudioSource.PlayOneShot(CurrentWeapon.ShootSound);

            if (CurrentWeapon.MuzzleEffect)
            {
                if (CurrentWeapon && CurrentWeapon.Sight2D && !CurrentWeapon.isAimingDown || CurrentWeapon && !CurrentWeapon.Sight2D || !CurrentWeapon)
                {
                    CurrentWeapon.MuzzleEffect.transform.SetParent(CurrentWeapon.ShootBase.transform);
                    CurrentWeapon.MuzzleEffect.transform.localPosition = Vector3.zero;
                    CurrentWeapon.MuzzleEffect.gameObject.SetActive(true);
                }
            }
            if (!photonView.isMine)
            {
                if (!MultiplayerGameManager.Instance.ShowAlwaysEnemiesInMiniMap)
                {
                    MiniMapIcon.gameObject.SetActive(true);
                    StartCoroutine(DesactivateMiniMap(MultiplayerGameManager.Instance.ShowTimeEnemyInMinimapWhenShoot));

                }
                timer = 0f;
                return;

            }
            if (CurrentWeapon.ShellPrefab)
            {
                // spawn shells
                GameObject _shell = Instantiate(CurrentWeapon.ShellPrefab);
                _shell.GetComponent<Rigidbody>().isKinematic = false;


                _shell.transform.parent = null;
                _shell.transform.position = CurrentWeapon.ShellPosBase.position;

                _shell.SetActive(true);

                _shell.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(CurrentWeapon.shellForce + Random.Range(0, CurrentWeapon.shellRandomForce), 0, 0), ForceMode.Impulse);
                _shell.GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(CurrentWeapon.shellTorqueX + Random.Range(-CurrentWeapon.shellRandomTorque, CurrentWeapon.shellRandomTorque), CurrentWeapon.shellTorqueY + Random.Range(-CurrentWeapon.shellRandomTorque, CurrentWeapon.shellRandomTorque), 0), ForceMode.Impulse);
            }
            for (int i = 0; i < CurrentWeapon.WeaponShoots; i++)
            {
                float recoilX = 0f;
                float recoilY = 0f;
                if (CurrentWeapon.WeaponShoots == 1 && !CurrentWeapon.isAimingDown || CurrentWeapon.WeaponShoots > 1 && i != 0)
                {

                    recoilX = Random.Range(-CurrentWeapon.MaxAimRecoilX, CurrentWeapon.MaxAimRecoilX);
                    recoilY = Random.Range(-CurrentWeapon.MaxAimRecoilY, CurrentWeapon.MaxAimRecoilY);

                }

                Ray ray = new Ray(Camera.main.transform.position, new Vector3(Camera.main.transform.forward.x + recoilX, Camera.main.transform.forward.y + recoilY, Camera.main.transform.forward.z));
                Debug.DrawRay(ray.origin, ray.direction);
                //   Debug.Log(LayerMask.LayerToName(8));
                RaycastHit[] hits = Physics.RaycastAll(ray, CurrentWeapon.DistanceShoot);
                //     RaycastHit hit = hits[0];
                bool hitted = false;
                if (hits.Length > 0)
                {
                    for (int x = 0; x < hits.Length; x++)
                    {
                        if (hits[x].transform != transform && hits[x].transform.root != transform && hits[x].transform.parent != transform)
                        {
                            hit = hits[x];
                            hitted = true;
                        }

                    }

                    if (hitted)
                    {
                        foreach (var effect in CurrentWeapon.EffectsInHit)
                        {


                            if (hit.transform.gameObject.layer == effect.LayerMask)
                            {
                                if (CurrentWeapon.AudioSource && effect.Sound)
                                    CurrentWeapon.AudioSource.PlayOneShot(effect.Sound);


                                GameObject objSpawned = Instantiate(effect.Go, hit.point, Quaternion.Euler(hit.normal));
                                objSpawned.transform.localPosition = new Vector3(objSpawned.transform.localPosition.x, objSpawned.transform.localPosition.y + effect.OffsetY, objSpawned.transform.localPosition.z);

                                photonView.RPC("InstantiateEffectWithOffset", PhotonTargets.Others, effect.Go.name, hit.point, hit.normal, effect.OffsetY);
                            }
                        }

                        if (i == 0 && MultiplayerGameManager.Instance.GameMode != MultiplayerGameManager.GameModes.FreeForAll || hit.transform == transform)
                        {
                            PlayerController playerController = hit.transform.GetComponent<PlayerController>();
                            if (playerController != null && Team1 == playerController.Team1)
                            {
                                CurrentWeapon.cartridgeAmmo--;
                                GameManager.Instance.UpdateAmmoUI();
                                return;
                            }
                        }

                        Health health = hit.transform.GetComponent<Health>();
                        // if target has health, damage him
                        if (health)
                        {
                            GameManager.Instance.WeaponHit();

                            health.SendTakeDamageRPC(CurrentWeapon.DamageBody, hit.point, photonView.viewID);

                        }
                        if (!health)
                        {
                            HealthBody healthBody = hit.transform.GetComponent<HealthBody>();
                            if (healthBody)
                            {
                                GameManager.Instance.WeaponHit();

                                if (healthBody.BodyPart == HealthBody.BodyParts.Head)
                                    healthBody.healthScript.SendTakeDamageRPC(CurrentWeapon.DamageHeadShot, hit.point, photonView.viewID);
                                if (healthBody.BodyPart == HealthBody.BodyParts.Arms)
                                    healthBody.healthScript.SendTakeDamageRPC(CurrentWeapon.DamageArms, hit.point, photonView.viewID);
                                if (healthBody.BodyPart == HealthBody.BodyParts.Legs)
                                    healthBody.healthScript.SendTakeDamageRPC(CurrentWeapon.DamageLegs, hit.point, photonView.viewID);
                                if (healthBody.BodyPart == HealthBody.BodyParts.Torso)
                                    healthBody.healthScript.SendTakeDamageRPC(CurrentWeapon.DamageTorso, hit.point, photonView.viewID);
                                if (healthBody.BodyPart == HealthBody.BodyParts.Hips)
                                    healthBody.healthScript.SendTakeDamageRPC(CurrentWeapon.DamageHips, hit.point, photonView.viewID);
                            }
                        }
                    }

                }
            }
            CurrentWeapon.cartridgeAmmo--;
            GameManager.Instance.UpdateAmmoUI();
        }
        [PunRPC]
        // if I've killed somebody set my stats and the player scores
        public void KilledSomebody()
        {

            Kills++;
            GameManager.Instance.KillPopUp.gameObject.SetActive(true);


            if (MultiplayerGameManager.Instance.GameMode == MultiplayerGameManager.GameModes.FreeForAll)
            {

                MultiplayerGameManager.Instance.Team1Score = Kills;

                var playerListWithoutMe = new List<PlayerController>(MultiplayerGameManager.Instance.Players);
                playerListWithoutMe.Remove(this);

                var playerList = playerListWithoutMe.OrderByDescending(player => player.Kills).ToList();
                MultiplayerGameManager.Instance.Team2Score = playerList[0].Kills;
                MultiplayerGameManager.Instance.UpdateTeamScoresUI();
            }

        }
        [PunRPC]
        void InstantiateEffectWithOffset(string effectName, Vector3 pos, Vector3 normal, float offsetY)
        {
            GameObject objSpawned = (GameObject)Instantiate(Resources.Load(effectName), pos, Quaternion.Euler(normal));
            objSpawned.transform.localPosition = new Vector3(objSpawned.transform.localPosition.x, objSpawned.transform.localPosition.y + offsetY, objSpawned.transform.localPosition.z);

        }
        //[PunRPC]
        // aiming down weapon
        void AimingDownSights()
        {


            if (photonView.isMine)
            {
                CurrentWeapon.isAimingDown = true;
                GameManager.Instance.FadeOutCrosshair();
            }
            CurrentWeapon.transform.parent.localPosition = Vector3.MoveTowards(CurrentWeapon.transform.parent.localPosition, CurrentWeapon.aimPosition, Time.deltaTime * CurrentWeapon.adsSpeed);

            if (CurrentWeapon.Sight2D == null)
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, CurrentWeapon.FovAds, CurrentWeapon.FovAdsSpeed * Time.deltaTime);
            if (Vector3.Distance(CurrentWeapon.transform.parent.localPosition, CurrentWeapon.aimPosition) < CurrentWeapon.Sight2DMinDistanceToShow)
            {
                if (CurrentWeapon.Sight2D != null && !isSighting2D)
                {
                    CurrentWeapon.Sight2D.DoFadeIn();
                    isSighting2D = true;
                    CameraAnim.enabled = true;
                    CurrentWeapon.DesactivateModel();
                    GameManager.Instance.FadeWhenSight2D.SetActive(false);
                    Camera.main.fieldOfView = CurrentWeapon.Sight2DCameraFov;

                }

            }
        }
        // we check if is grounded with on collision enter because it's 100% accurate more than a raycast
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(GroundTag))
            {
                isGrounded = true;
            }

        }

        void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag(GroundTag))
            {

                isGrounded = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // check if player is triggering with a ladder
            if (other.gameObject.CompareTag("Ladder"))
            {

                if (CurrentWeapon && !ladderExit)
                {
                    CurrentWeapon.gameObject.SetActive(true);
                    CurrentWeapon.StartCoroutine(CurrentWeapon.StopUsing());

                }
                OnLadder = true;

            }
            if (other.gameObject.GetComponent<Weapon>())
            {
                onTriggerWithWeapon = true;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            // exit from a ladder
            if (other.gameObject.CompareTag("Ladder"))
            {

                OnLadder = false;

                ladderExit = true;

            }
            if (other.gameObject.GetComponent<Weapon>())
            {
                onTriggerWithWeapon = false;
            }
        }
        // [PunRPC]
        // weapon aiming up
        void AimingUpSights()
        {

            if (CurrentWeapon == null)
                return;
            if (CurrentWeapon.animationC.IsPlaying("SwitchIn") || CurrentWeapon.animationC.IsPlaying("SwitchOut"))
                return;

            if (photonView.isMine)
            {
                CurrentWeapon.isAimingDown = false;
                GameManager.Instance.FadeInCrosshair();
            }

            if (!GetIsRun())
            {
                CurrentWeapon.transform.parent.localPosition = Vector3.MoveTowards(CurrentWeapon.transform.parent.localPosition, CurrentWeapon.transform.parent.GetComponent<WeaponBaseData>().weaponBaseInitialPosition, Time.deltaTime * CurrentWeapon.adsSpeed);

            }

            if (CurrentWeapon.Sight2D == null)
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, startFov, CurrentWeapon.FovAdsSpeed * Time.deltaTime);

            if (CurrentWeapon.Sight2D != null && isSighting2D)
            {
                CurrentWeapon.Sight2D.DoFadeOut();


                CameraAnim.enabled = false;

                isSighting2D = false;


                CurrentWeapon.ActivateModel();
                GameManager.Instance.FadeWhenSight2D.SetActive(true);
                Camera.main.fieldOfView = startFov;


            }




        }
        [PunRPC]
        // recoil weapon by script
        void Recoil()
        {

            if (CurrentWeapon == null)
                return;

            if (CurrentWeapon.recoil > 0f)
            {
                if (photonView.isMine)
                    GameManager.Instance.RecoilCrosshair();
                Quaternion maxRecoil = Quaternion.Euler(CurrentWeapon.maxRecoil_x, CurrentWeapon.maxRecoil_y, 0f);

                // Dampen towards the target rotation
                CurrentWeapon.transform.localRotation = Quaternion.Slerp(CurrentWeapon.transform.localRotation, maxRecoil, Time.deltaTime * CurrentWeapon.recoilSpeed);

                CurrentWeapon.recoil -= Time.deltaTime;
            }
            else
            {
                if (photonView.isMine)
                    GameManager.Instance.UnRecoilCrosshair();

                CurrentWeapon.recoil = 0f;
                // Dampen towards the target rotation
                CurrentWeapon.transform.localRotation = Quaternion.Slerp(CurrentWeapon.transform.localRotation, Quaternion.identity, Time.deltaTime * CurrentWeapon.recoilSpeed / 2);
            }

        }
        #endregion

        [PunRPC]
        // pickup item 

        public void PickupItem(int idItem)
        {

            PhotonView itemGo = PhotonView.Find(idItem);

            if (itemGo != null)
            {

                Item item = itemGo.GetComponent<Item>();
                // pickup  disable script because if it is enabled when you are on the trigger with the weapon in the hands of another player you pick up it

                if (item.GetComponent<Pickup>())
                {
                    item.GetComponent<Pickup>().enabled = false;
                    item.GetComponent<Pickup>().TriggerPickup.enabled = false;

                }
                // desactivate box collider because if it is activated it makes collision force with another colliders
                if (item.GetComponent<BoxCollider>() && !item.GetComponent<BoxCollider>().isTrigger)
                {
                    item.GetComponent<BoxCollider>().enabled = false;

                }
                // and set kinematic
                if (item.GetComponent<Rigidbody>())
                    item.GetComponent<Rigidbody>().isKinematic = true;

                Weapon weapon = item.GetComponent<Weapon>();
                if (weapon != null)
                    weapon.Owner = this;


                ItemInInventory newItemInventory = new ItemInInventory();
                newItemInventory.item = item;
                newItemInventory.photonView = itemGo;
                if (Items.Count >= MaxItemInventory)
                {
                    CurrentWeapon.photonView.RPC("StopUsingDrop", PhotonTargets.All);
                    CurrentWeapon.photonView.RPC("SetPositionNetworked", PhotonTargets.All, weapon.transform.position, weapon.transform.rotation);
                    CurrentWeapon = null;
                    if (!Items.Contains(newItemInventory))

                        Items.Add(newItemInventory);
                }
                else
                {
                    if (!Items.Contains(newItemInventory))

                        Items.Add(newItemInventory);

                }
            }


        }

        [PunRPC]
        public IEnumerator UseItem(int itemID, int newIndex)
        {

            PhotonView itemGo = PhotonView.Find(itemID);

            Item item = itemGo.GetComponent<Item>();

            // if is not mine only set some features and break coroutine
            if (!photonView.isMine || MultiplayerGameManager.Instance.finished)
            {
                if (CurrentWeapon != null && item.gameObject != CurrentWeapon.gameObject)
                    CurrentWeapon.StartCoroutine(CurrentWeapon.StopUsing());

                item.gameObject.SetActive(true);
                item.SendMessage("Use", this, SendMessageOptions.DontRequireReceiver);

                yield break;
            }

            if (CurrentWeapon != null && item.gameObject != CurrentWeapon.gameObject)
            {
                CurrentWeapon.gameObject.SetActive(true);
                CurrentWeapon.StartCoroutine(CurrentWeapon.StopUsing());
                // wait while playing animation of switch out
                yield return new WaitWhile(() => !CurrentWeapon.switchedOut);
            }
            if (newIndex != -1)
                index = newIndex;
            else
            {
                for (int i = 0; i < Items.ToArray().Length; i++)
                {
                    if (Items[i].item == item)
                        index = i;
                }
            }
            if (item.GetComponent<Rigidbody>())
            {
                item.GetComponent<Rigidbody>().isKinematic = true;
            }
            item.gameObject.SetActive(true);
            item.SendMessage("Use", this, SendMessageOptions.DontRequireReceiver);

        }
        [PunRPC]
        public void DropItem()
        {
            if (photonView.isMine)
            {

                Items.RemoveAll(x => x.item == CurrentWeapon.GetComponent<Item>());

                index = 0;
            }
            if (Items.Count > 0)
            {
                UseItem(Items[index].item.GetComponent<PhotonView>().viewID, index);
            }
            else
            {
                CurrentWeapon.photonView.RPC("StopUsing", PhotonTargets.AllBuffered, null);
                CurrentWeapon = null;
            }
        }

        [PunRPC]
        public void DropItem(int viewID)
        {
            PhotonView weaponToDrop = PhotonView.Find(viewID);
            if (photonView.isMine)
            {

                Items.RemoveAll(x => x.item == weaponToDrop.GetComponent<Item>());

                index = 0;
            }
            if (Items.Count > 0)
            {
                UseItem(Items[index].item.GetComponent<PhotonView>().viewID, index);
            }

        }
        // it's made so because if you want to modify is grounded method you only have to modify the return value 
        public bool IsGrounded()
        {
            if (characterController)
                return characterController.isGrounded;
            else
                return isGrounded;

        }

        #region WeaponAnimator
        public bool GetIsIdle()
        {
            if (!IsGrounded())
                return false;
            if (InputManager.inputManager.GetAxisRaw(InputManager.inputManager.MoveHorizontal) != 0 || InputManager.inputManager.GetAxisRaw(InputManager.inputManager.MoveVertical) != 0)
                return false;
            else
                return true;

        }
        public bool GetIsWalk()
        {
            if (!IsGrounded())
                return false;
            if (InputManager.inputManager.GetAxisRaw(InputManager.inputManager.MoveHorizontal) != 0 || InputManager.inputManager.GetAxisRaw(InputManager.inputManager.MoveVertical) != 0)
            {
                if (!GetIsRun())
                    return true;

            }

            return false;

        }
        public bool GetIsRun()
        {
            if (!IsGrounded())
                return false;
            if (CurrentWeapon != null && !CurrentWeapon.hasReloaded)
                return false;
            if (CurrentWeapon != null && CurrentWeapon.animationC)
            {
                if (CurrentWeapon.animationC.IsPlaying("SwitchIn") || CurrentWeapon.animationC.IsPlaying("SwitchOut"))
                    return false;
            }
            if (InputManager.inputManager.GetAxisRaw(InputManager.inputManager.MoveHorizontal) != 0 || InputManager.inputManager.GetAxisRaw(InputManager.inputManager.MoveVertical) != 0)
            {
                if (Application.isMobilePlatform && runKeyDown || !Application.isMobilePlatform && InputManager.inputManager.GetButton(InputManager.inputManager.RunAxis))
                {
                    if (CurrentWeapon == null || CurrentWeapon != null && !CurrentWeapon.isAimingDown)
                        return true;
                }
            }
            return false;
        }

        public bool GetIsShoot()
        {
            if (GetIsRun())
                return false;
            if (isShooting)
            {

                return true;
            }
            return false;

        }
        #endregion
    }
}