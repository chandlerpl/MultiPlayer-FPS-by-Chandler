using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AxlPlay
{
    // Instantiate AIs
    public class AIManager : MonoBehaviour
    {

        public AIPlayer[] AIPrefabsTeam1;
        public AIPlayer[] AIPrefabsTeam2;



        public int AIPlayersTeam1;
        // enemies
        public int AIPlayersTeam2;


        public Transform[] PatrolPointsTeam1;
        public Transform[] PatrolPointsTeam2;

        private List<AIPlayer> PlayersTeam1 = new List<AIPlayer>();
        private List<AIPlayer> PlayersTeam2 = new List<AIPlayer>();


        private bool gameStarted;
        public static AIManager Instance;

        private Queue<AIPlayer> aiPrefabsTeam1Stack = new Queue<AIPlayer>();
        private Queue<AIPlayer> aiPrefabsTeam2Stack = new Queue<AIPlayer>();

        // for offline mode destroy ai
        public void Reset()
        {
            gameStarted = false;

            if (PhotonNetwork.offlineMode)
            {
                foreach (var player1 in PlayersTeam1)
                {
                    PhotonNetwork.Destroy(player1.photonView);
                }
                foreach (var player2 in PlayersTeam2)
                {
                    PhotonNetwork.Destroy(player2.photonView);
                }
            }
            PlayersTeam1 = new List<AIPlayer>();
            PlayersTeam2 = new List<AIPlayer>();

        }
        void Awake()
        {
            Instance = this;
            aiPrefabsTeam1Stack = new Queue<AIPlayer>(AIPrefabsTeam1);
            aiPrefabsTeam2Stack = new Queue<AIPlayer>(AIPrefabsTeam2);

        }
        void Start()
        {
        }

        void Update()
        {
            // Still AI doesn't work in multiplayer mode, soon releasing.
            if (!PhotonNetwork.offlineMode)
                return;

            // flag

            if (!gameStarted && MultiplayerGameManager.Instance.LocalPlayer != null)
            {
                gameStarted = true;
                // instantiate AI 
                for (int i = 0; i < AIPlayersTeam1; i++)
                {
                    GameObject player = PhotonNetwork.Instantiate(aiPrefabsTeam1Stack.Peek().name, MultiplayerGameManager.Instance.Team1SpawnPoints[Random.Range(0, MultiplayerGameManager.Instance.Team1SpawnPoints.Length)].transform.position, Quaternion.identity, 0);
                    AIPlayer playerScript = player.GetComponent<AIPlayer>();
                    playerScript.Team1 = true;
                    PlayersTeam1.Add(playerScript);

                }

                for (int i = 0; i < AIPlayersTeam2; i++)
                {
                    GameObject player = PhotonNetwork.Instantiate(aiPrefabsTeam2Stack.Peek().name, MultiplayerGameManager.Instance.Team2SpawnPoints[Random.Range(0, MultiplayerGameManager.Instance.Team2SpawnPoints.Length)].transform.position, Quaternion.identity, 0);
                    AIPlayer playerScript = player.GetComponent<AIPlayer>();
                    PlayersTeam2.Add(playerScript);

                }
            }

        }
    }
}