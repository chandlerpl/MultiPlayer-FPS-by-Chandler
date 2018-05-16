using UnityEngine;
using System.Collections;
using AxlPlay;
using UnityEngine.AI;

namespace AxlPlay
{
    // Wander using Unity NavMesh
    [RequireComponent(typeof(NavMeshAgent))]
    [AddComponentMenu("Easy AI/Wander")]
    public class Wander : MonoBehaviour
    {
        [Tooltip("How far ahead of the current position to look ahead for a wander")]
        public float wanderDistance = 20;
        [Tooltip("The amount that the agent rotates direction")]
        public float wanderRate = 2;
        [Tooltip("The Agent speed.")]
        public float AgentSpeed = 3.5f;

        [HideInInspector]
        public Vector3 destination;
        private NavMeshAgent _agent;
        // init FSM
        public enum States
        {
            Idle,
            Wander,
            Finish
        }
        public StateMachine<States> fsm;
        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = AgentSpeed;
            //Initialize State Machine Engine		
            fsm = StateMachine<States>.Initialize(this);
        }
        void Start()
        {
            fsm.ChangeState(States.Idle);
        }
        void Idle_Enter()
        {

            //Debug.Log("Wander => Idle_Enter");
        }
        void Wander_Enter()
        {
            //Debug.Log("Wander => Wander_Enter");
            _agent.isStopped = false;
            destination = Target();
            SetDestination(destination);
        }
        void Wander_Update()
        {
            if (HasArrived(destination))
            {
                destination = Target();
                SetDestination(destination);
            }
        }
        void Finish_Enter()
        {
            
            _agent.velocity = Vector3.zero;

            _agent.isStopped = true;
            //Debug.Log("Wander => Finish_Enter");
        }
        private Vector3 Target()
        {
            // point in a new random direction and then multiply that by the wander distance
            var direction = transform.forward + Random.insideUnitSphere * wanderRate;
            var returnThe = transform.position + direction.normalized * wanderDistance;
            returnThe.y = 0f;
            return returnThe;
        }
        private bool SetDestination(Vector3 target)
        {
            if (_agent.destination == target)
            {
                return true;
            }
            if (_agent.SetDestination(target))
            {
                return true;
            }
            return false;
        }
        /*
            private bool HasArrived()
            {
                return !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.001f;
            }
        */
        private bool HasArrived(Vector3 position)
        {
            position.y = 0;
            if (Vector3.Distance(transform.position, position) < 1f)
                return true;
            return false;
        }

    }
}
