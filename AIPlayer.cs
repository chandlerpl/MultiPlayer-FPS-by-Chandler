using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using AxlPlay;

namespace AxlPlay
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Wander))]
    [RequireComponent(typeof(FieldOfView))]
    [RequireComponent(typeof(SetDestination))]
    public class AIPlayer : Photon.PunBehaviour
    {

        public GameObject MiniMapIcon;
        public Animator Model;
        public float MinDistanceToShoot = 20f;
        public float MaxDistanceToShoot = 10f;
        public Weapon PlayerWeapon;

        [HideInInspector]
        public bool Team1;


        public enum States
        {
            Idle,
            GoStartPos,
            Search,
            Attack,
            ChaseEnemy
        }
        public StateMachine<States> fsm;

        private SetDestination setDestination;
        private FieldOfView fieldOfView;
        private Wander wander;
        [HideInInspector]
        public NavMeshAgent agent;

        [HideInInspector]
        public float startArrivedDistance;

        private float timer;
        [HideInInspector]
        public Rigidbody rigidBody;

        RaycastHit hit;

        private void Awake()
        {
            // get references
            PlayerWeapon.AIOwner = this;
            fsm = StateMachine<States>.Initialize(this);
            rigidBody = GetComponent<Rigidbody>();
            setDestination = GetComponent<SetDestination>();
            startArrivedDistance = setDestination.arrivedDistance;
            fieldOfView = GetComponent<FieldOfView>();
            wander = GetComponent<Wander>();
            agent = GetComponent<NavMeshAgent>();
            PlayerWeapon.AIOwner = this;
        }
        void Start()
        {
            // initialize fsm
            fsm.ChangeState(States.GoStartPos);
        }
        private void Update()
        {
            //   Debug.Log(fsm.State + " wander state " + wander.fsm.State + " set destination " + setDestination.fsm.State + " stop distance " + agent.stoppingDistance);
        }
        void Idle_Enter()
        {
            Model.SetFloat("Z", 0f);
            agent.stoppingDistance = startArrivedDistance;
            setDestination.fsm.ChangeState(SetDestination.States.ArrivedEvent);
            wander.fsm.ChangeState(Wander.States.Finish);

        }
        void GoStartPos_Enter()
        {
            // go to start pos in scene
            Model.SetFloat("Z", 1f);
            agent.stoppingDistance = startArrivedDistance;
            Transform closestPoint = null;
            if (Team1)
                closestPoint = GetClosestObject(AIManager.Instance.PatrolPointsTeam1);
            else
                closestPoint = GetClosestObject(AIManager.Instance.PatrolPointsTeam2);

            setDestination.target = closestPoint.gameObject;
            setDestination.fsm.ChangeState(SetDestination.States.goDestination);
        }
        void GoStartPos_Update()
        {
            if (CanSeeEnemy())
            {
                fsm.ChangeState(States.Attack, StateTransition.Overwrite);
            }
            else
            {
                if (setDestination.hasArrived)
                {
                    fsm.ChangeState(States.Search, StateTransition.Overwrite);

                }
            }
        }
        void Search_Enter()
        {
            // if you kill the player start searching again and reset target
            setDestination.fsm.ChangeState(SetDestination.States.ArrivedEvent);
            setDestination.target = null;
            // search a enemy
            agent.stoppingDistance = startArrivedDistance;

            Model.SetFloat("Z", 1f);

            wander.fsm.ChangeState(Wander.States.Wander);
        }
        void Search_Update()
        {
            if (CanSeeEnemy())
            {
                wander.fsm.ChangeState(Wander.States.Finish, StateTransition.Overwrite);

                fsm.ChangeState(States.Attack, StateTransition.Safe);
            }
        }
        void Search_Finish()
        {
            wander.fsm.ChangeState(Wander.States.Finish, StateTransition.Overwrite);

        }
        void Attack_Enter()
        {
            // attack enemy seen
            setDestination.target = CanSeeEnemy();
            agent.stoppingDistance = MinDistanceToShoot;

            setDestination.fsm.ChangeState(SetDestination.States.goDestination, StateTransition.Overwrite);

        }
        void Attack_Update()
        {

            timer += Time.deltaTime;

            if (CanSeeEnemy())
            {
                var targetRotation = Quaternion.LookRotation(CanSeeEnemy().transform.position - transform.position);

                // Smoothly rotate towards the target point.
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);


                if (setDestination.hasArrived && CanSeeEnemy())
                {
                    // player got far, get near
                    if (Vector3.Distance(transform.position, CanSeeEnemy().transform.position) > MinDistanceToShoot)
                    {
                        agent.stoppingDistance = MinDistanceToShoot;

                        Model.SetFloat("Z", 1f);
                        setDestination.fsm.ChangeState(SetDestination.States.goDestination, StateTransition.Overwrite);


                    }
                    // get near and shoot
                    else if (Vector3.Distance(transform.position, CanSeeEnemy().transform.position) > MaxDistanceToShoot)
                    {
                        agent.stoppingDistance = MaxDistanceToShoot;

                        Model.SetFloat("Z", 1f);

                        setDestination.fsm.ChangeState(SetDestination.States.goDestination, StateTransition.Overwrite);

                        // start shooting 
                        if (timer > PlayerWeapon.ShootInterval && PlayerWeapon.hasReloaded)
                        {
                            timer = 0f;
                            Shoot();

                        }
                    }
                    // only shoot
                    else
                    {
                        setDestination.fsm.ChangeState(SetDestination.States.ArrivedEvent, StateTransition.Overwrite);

                        Model.SetFloat("Z", 0f);

                        // start shooting and
                        if (timer > PlayerWeapon.ShootInterval && PlayerWeapon.hasReloaded)
                        {
                            timer = 0f;
                            Shoot();

                        }
                    }


                }



            }
            // if cant see enemy, search again
            else
            {
                fsm.ChangeState(States.Search, StateTransition.Overwrite);
            }

        }
        [PunRPC]
        void Shoot()
        {
            Model.SetTrigger("Shoot");

            if (PlayerWeapon.AudioSource && PlayerWeapon.ShootSound)
                PlayerWeapon.AudioSource.PlayOneShot(PlayerWeapon.ShootSound);

            if (PlayerWeapon.MuzzleEffect)
            {
                PlayerWeapon.MuzzleEffect.transform.SetParent(PlayerWeapon.ShootBase.transform);
                PlayerWeapon.MuzzleEffect.transform.localPosition = Vector3.zero;
                PlayerWeapon.MuzzleEffect.gameObject.SetActive(true);

            }

            if (!MultiplayerGameManager.Instance.ShowAlwaysEnemiesInMiniMap)
            {
                MiniMapIcon.gameObject.SetActive(true);
                StartCoroutine(DesactivateMiniMap(MultiplayerGameManager.Instance.ShowTimeEnemyInMinimapWhenShoot));

            }
            for (int i = 0; i < PlayerWeapon.WeaponShoots; i++)
            {

                float recoilX = 0f;
                float recoilY = 0f;
                if (PlayerWeapon.WeaponShoots == 1 && !PlayerWeapon.isAimingDown || PlayerWeapon.WeaponShoots > 1 && i != 0)
                {

                    recoilX = Random.Range(-PlayerWeapon.MaxAimRecoilX, PlayerWeapon.MaxAimRecoilX);
                    recoilY = Random.Range(-PlayerWeapon.MaxAimRecoilY, PlayerWeapon.MaxAimRecoilY);

                }

                Vector3 fromPosition = new Vector3(transform.position.x, transform.position.y + 1.2f, transform.position.z);
                Ray ray = new Ray(fromPosition, new Vector3(transform.forward.x + recoilX, transform.forward.y + recoilY, transform.forward.z));
                Debug.DrawRay(ray.origin, ray.direction);
                RaycastHit[] hits = Physics.RaycastAll(ray, PlayerWeapon.DistanceShoot);
                bool hitted = false;
                if (hits.Length > 0)
                {

                    for (int x = 0; x < hits.Length; x++)
                    {
                        if (hits[x].transform != transform && hits[x].transform.root != transform && hits[x].transform.parent != transform)
                        {
                            if (!hits[x].transform.GetComponent<AIPlayer>())
                            {
                                hit = hits[x];
                                hitted = true;
                            }
                            else
                                hitted = false;
                        }

                    }

                    if (hitted)
                    {

                        foreach (var effect in PlayerWeapon.EffectsInHit)
                        {
                            if (hit.transform.gameObject.layer == effect.LayerMask)
                            {
                                if (PlayerWeapon.AudioSource && effect.Sound)
                                    PlayerWeapon.AudioSource.PlayOneShot(effect.Sound);

                                GameObject objSpawned = Instantiate(effect.Go, hit.point, Quaternion.Euler(hit.normal));
                                objSpawned.transform.localPosition = new Vector3(objSpawned.transform.localPosition.x, objSpawned.transform.localPosition.y + effect.OffsetY, objSpawned.transform.localPosition.z);

                                InstantiateEffectWithOffset(effect.Go.name, hit.point, hit.normal, effect.OffsetY);
                            }
                        }

                        if (i == 0)
                        {
                            PlayerController playerController = hit.transform.GetComponent<PlayerController>();
                            if (playerController != null && Team1 == playerController.Team1)
                            {
                                PlayerWeapon.cartridgeAmmo--;
                                return;
                            }
                        }
                        Health health = hit.transform.GetComponent<Health>();

                        if (health)
                        {

                            health.SendTakeDamageRPC(PlayerWeapon.DamageBody, hit.point, photonView.viewID);

                        }
                        if (!health)
                        {
                            HealthBody healthBody = hit.transform.GetComponent<HealthBody>();
                            if (healthBody)
                            {
                                if (healthBody.BodyPart == HealthBody.BodyParts.Head)
                                    healthBody.healthScript.SendTakeDamageRPC(PlayerWeapon.DamageHeadShot, hit.point, photonView.viewID);
                                if (healthBody.BodyPart == HealthBody.BodyParts.Arms)
                                    healthBody.healthScript.SendTakeDamageRPC(PlayerWeapon.DamageArms, hit.point, photonView.viewID);
                                if (healthBody.BodyPart == HealthBody.BodyParts.Legs)
                                    healthBody.healthScript.SendTakeDamageRPC(PlayerWeapon.DamageLegs, hit.point, photonView.viewID);
                                if (healthBody.BodyPart == HealthBody.BodyParts.Torso)
                                    healthBody.healthScript.SendTakeDamageRPC(PlayerWeapon.DamageTorso, hit.point, photonView.viewID);
                                if (healthBody.BodyPart == HealthBody.BodyParts.Hips)
                                    healthBody.healthScript.SendTakeDamageRPC(PlayerWeapon.DamageHips, hit.point, photonView.viewID);
                            }
                        }
                    }
                }
            }
            PlayerWeapon.cartridgeAmmo--;

        }
        // if someone hits me turn to him and attack
        public void GotHitBy(GameObject _damager)
        {
            if (fsm.State == States.GoStartPos || fsm.State == States.Search)
            {
                // chase enemy
                setDestination.target = _damager;

                fsm.ChangeState(States.ChaseEnemy);

            }
        }

        // turn to enemy that hit me
        void ChaseEnemy_Enter()
        {
            wander.fsm.ChangeState(Wander.States.Finish, StateTransition.Overwrite);
            agent.velocity = Vector3.zero;
            agent.stoppingDistance = 1f;
            agent.isStopped = false;
            setDestination.fsm.ChangeState(SetDestination.States.goDestination);

        }
        void ChaseEnemy_Update()
        {
            var targetRotation = Quaternion.LookRotation(setDestination.target.transform.position - transform.position);

            // Smoothly rotate towards the target point.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
            if (CanSeeEnemy())
            {
                fsm.ChangeState(States.Attack);

            }
            if (setDestination.fsm.State == SetDestination.States.ArrivedEvent)
            {
                setDestination.fsm.ChangeState(SetDestination.States.goDestination);
                agent.isStopped = false;

            }

        }
        // to instantiate effects like bullet holes with offset
        [PunRPC]
        void InstantiateEffectWithOffset(string effectName, Vector3 pos, Vector3 normal, float offsetY)
        {
            GameObject objSpawned = (GameObject)Instantiate(Resources.Load(effectName), pos, Quaternion.Euler(normal));
            objSpawned.transform.localPosition = new Vector3(objSpawned.transform.localPosition.x, objSpawned.transform.localPosition.y + offsetY, objSpawned.transform.localPosition.z);

        }
        // desactivate my icon in minimap after shoot
        IEnumerator DesactivateMiniMap(float inTime)
        {
            yield return new WaitForSeconds(inTime);
            MiniMapIcon.gameObject.SetActive(false);

        }
        // check if is seeing a enemy to attack 
        GameObject CanSeeEnemy()
        {
            if (fieldOfView.visibleTargets.Count == 0)
                return null;


            PlayerController pC = fieldOfView.visibleTargets[0].GetComponent<PlayerController>();
            if (pC != null)
            {
                if (pC.Team1 != Team1)
                    return pC.gameObject;
            }
            else
            {
                AIPlayer aIP = fieldOfView.visibleTargets[0].GetComponent<AIPlayer>();
                if (!aIP)
                    return null;
                if (aIP.Team1 != Team1)
                    return aIP.gameObject;
            }
            return null;

        }

        Transform GetClosestObject(Transform[] objects)
        {

            Transform closestObject = null;
            foreach (Transform obj in objects)
            {
                if (!closestObject)
                {
                    closestObject = obj;
                }
                //compares distances
                if (Vector3.Distance(transform.position, obj.transform.position) <= Vector3.Distance(transform.position, closestObject.transform.position))
                {
                    closestObject = obj;
                }
            }
            return closestObject;

        }
    }
}