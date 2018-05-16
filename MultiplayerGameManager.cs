using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon;
using AxlPlay;
using System;
using System.Linq;
namespace AxlPlay
{

    public class MultiplayerGameManager : Photon.PunBehaviour
    {
        //    public bool TESTKICK;
        public enum GameModes
        {
            TeamDeathmatch,
            FreeForAll
        }

        public float MaxTimeAfk = 90f;
        public UIEffects[] FadeInOnReturn;
        public UIEffects[] FadeOutOnReturn;
        [HideInInspector]
        public List<GameObject> Team1Stats = new List<GameObject>();
        [HideInInspector]

        public List<GameObject> Team2Stats = new List<GameObject>();

        public GameObject PlayerStatsUI;

        public UIEffects PlayerStatsList;
        public GameObject PlayerStatsTeam1Grid;
        public GameObject PlayerStatsFreeForAllGrid;

        public GameObject PlayerStatsTeam2Grid;
        public Color PlayerStatsUITeam1Color;
        public Color PlayerStatsUITeam1ColorVariant;
        public Color PlayerStatsUITeam2Color;
        public Color PlayerStatsUITeam2ColorVariant;
        public Color PlayerStatsUIFreeForAllColor;

        public Color PlayerStatsUIFreeForAllColorVariant;


        public bool ShowAlwaysEnemiesInMiniMap = false;
        public float ShowTimeEnemyInMinimapWhenShoot = 2f;
        [HideInInspector]
        public PlayerController LocalPlayer;

        public Text ErrorCreatingRoom;

        public UIEffects KickPanel;
        public Text KickMessage;

        public Text MaxPlayersTxt;
        public Text MaxTimeTxt;
        public Text MaxPingTxt;

        public Text MaxKillsTxt;

        public UIEffects[] FadeInOnHost;

        //[HideInInspector]
        public bool finished;
        public UIEffects OnMatchFinished;

        public Text Timer;
        public Text Team1ScoreTxt;
        public Text Team2ScoreTxt;

        public UIEffects[] FadeOutOnPlay;
        public UIEffects[] FadeInOnPlay;


        public UIEffects[] FadeOutOnPlaySP;
        public UIEffects[] FadeInOnPlaySP;

        public Text ErrorName;
        public static MultiplayerGameManager Instance;


        public CanSeeObject[] Team1SpawnPoints;
        public CanSeeObject[] Team2SpawnPoints;

        public GameObject Player;
        public GameObject[] DesactivateOnStartGame;

        [HideInInspector]
        public int Team1Score;
        [HideInInspector]
        public int Team2Score;
        [HideInInspector]
        public List<PlayerController> Players = new List<PlayerController>();
        [HideInInspector]

        public List<int> PlayerIDs = new List<int>();
        [HideInInspector]
        public List<PlayerController> Team1 = new List<PlayerController>();
        [HideInInspector]

        public List<PlayerController> Team2 = new List<PlayerController>();
        [HideInInspector]
        public int KillsForMatch;
        [HideInInspector]

        public int MinsForMatch;

        [HideInInspector]
        public float MaxPing;
        [HideInInspector]
        public GameModes GameMode;

        private int errorRoomIndex;
        private float timer;
        private bool clickConnect;


        void Awake()
        {
            Instance = this;

        }

        void Start()
        {

        }
        void Update()
        {
            if (!PhotonNetwork.offlineMode)
            {
                if (PhotonNetwork.inRoom && !finished)
                {
                    TimeSpan t = TimeSpan.FromMinutes(timer);

                    string answer = string.Format("{0:D2}:{1:D2}",
                                    t.Hours,
                                    t.Minutes);

                    timer -= Time.deltaTime;


                    Timer.text = answer;

                    if (PhotonNetwork.isMasterClient)
                    {

                        if (timer <= 0)
                        {
                            // call finish method
                            photonView.RPC("FinishGame", PhotonTargets.AllBuffered, null);
                        }
                        if (GameMode != GameModes.FreeForAll)
                        {
                            if (Team1Score >= KillsForMatch || Team2Score >= KillsForMatch)
                            {
                                // call finish method
                                photonView.RPC("FinishGame", PhotonTargets.AllBuffered, null);

                            }
                        }
                        else
                        {

                            var playerListWithoutMe = new List<PlayerController>(Players);
                            playerListWithoutMe.Remove(LocalPlayer);

                            var playerList = playerListWithoutMe.OrderByDescending(player => player.Kills).ToList();
                            if (playerList.Count > 0 && playerList[0].Kills >= KillsForMatch)
                            {
                                photonView.RPC("FinishGame", PhotonTargets.AllBuffered, null);

                            }

                        }
                    }
                }

                if (Players.Count > 0 || PlayerIDs.Count > 0)
                {

                    List<int> indexToRemove = new List<int>();

                    for (int i = 0; i < Players.ToArray().Length; i++)
                    {
                        if (Players[i] == null)
                        {
                            indexToRemove.Add(i);
                        }
                    }
                    foreach (var index in indexToRemove)
                    {

                        Players.Remove(Players[index]);
                        if (PlayerIDs.Count > 0)
                            PlayerIDs.Remove(PlayerIDs[index]);
                    }

                }

            }
        }


        [PunRPC]
        void FinishGame()
        {
            finished = true;
            timer = MinsForMatch * 60f;

            OnMatchFinished.DoFadeIn();

            if (Application.isMobilePlatform && GameManager.Instance.MobileUI)
                GameManager.Instance.MobileUI.gameObject.SetActive(false);

            StartCoroutine(WaitForPlayAgain());
        }


        IEnumerator WaitForPlayAgain()
        {

            yield return new WaitForSeconds(5f);

            OnMatchFinished.DoFadeOut();
            PlayAgain();


        }
        // only for master client
        // when master client changes the players time afk doesn't update
        public void KickPlayer(PlayerController playerKick, string kickMessage)
        {
            if (!PhotonNetwork.isMasterClient)
                return;

            foreach (var player in PhotonNetwork.playerList)
            {
                if (player.NickName == playerKick.userName)
                {

                    PhotonNetwork.CloseConnection(player);
                    if (KickPanel && KickMessage)
                    {
                        KickPanel.DoFadeIn();
                        KickMessage.text = kickMessage;
                    }

                    return;
                }
            }
        }

        [PunRPC]
        public void PlayAgain()
        {

            if (Application.isMobilePlatform && GameManager.Instance.MobileUI)
                GameManager.Instance.MobileUI.gameObject.SetActive(true);

            GameManager.Instance.StopUsingWeapon();
            Team1Score = 0;
            Team2Score = 0;

            Team1 = new List<PlayerController>();
            Team2 = new List<PlayerController>();
            UpdateTeamScoresUI();


            if (PhotonNetwork.isMasterClient)
            {
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                {
                    PhotonNetwork.RemoveRPCs(PhotonNetwork.playerList[i]);
                }
            }
            PlayerController[] playerControllers = FindObjectsOfType(typeof(PlayerController)) as PlayerController[];
            Pickup[] pickups = Resources.FindObjectsOfTypeAll(typeof(Pickup)) as Pickup[];

            foreach (Pickup pickup in pickups)
            {
                pickup.enabled = true;
                pickup.TriggerPickup.enabled = true;
            }

            foreach (PlayerController playerController in playerControllers)
            {
                if (playerController.CurrentWeapon != null)
                    playerController.CurrentWeapon.Reset();

                playerController.Reset();
                DetermineTeamForNewPlayer(playerController.photonView.viewID, false);

            }

            finished = false;

        }

        private void OnApplicationQuit()
        {
            if (PhotonNetwork.connectedAndReady && PhotonNetwork.isMasterClient)
            {
                photonView.RPC("GetHostData", PhotonTargets.Others);
            }
        }

        public Transform GetSpawnPoint(bool _team1)
        {
            if (_team1)
            {
                for (int i = 0; i < Team1SpawnPoints.Length; i++)
                {
                    if (!CanSeeEnemy(Team1SpawnPoints[i], _team1))
                    {

                        return Team1SpawnPoints[i].transform;
                    }
                }
                for (int i = 0; i < Team2SpawnPoints.Length; i++)
                {
                    if (!CanSeeEnemy(Team2SpawnPoints[i], _team1))
                    {

                        return Team2SpawnPoints[i].transform;
                    }
                }

                return Team1SpawnPoints[UnityEngine.Random.Range(0, Team1SpawnPoints.Length)].transform;
            }
            else
            {

                for (int i = 0; i < Team2SpawnPoints.Length; i++)
                {
                    if (!CanSeeEnemy(Team2SpawnPoints[i], _team1))
                    {

                        return Team2SpawnPoints[i].transform;
                    }
                }

                for (int i = 0; i < Team1SpawnPoints.Length; i++)
                {
                    if (!CanSeeEnemy(Team1SpawnPoints[i], _team1))
                    {

                        return Team1SpawnPoints[i].transform;
                    }
                }
                return Team2SpawnPoints[UnityEngine.Random.Range(0, Team2SpawnPoints.Length)].transform;


            }
        }

        GameObject CanSeeEnemy(CanSeeObject canSeeObject, bool _Team1)
        {
            if (!canSeeObject.CanSee())
                return null;
            PlayerController pC = canSeeObject.CanSee().GetComponent<PlayerController>();
            if (pC != null)
            {
                if (pC.Team1 != _Team1)
                    return pC.gameObject;
            }
            else
            {
                AIPlayer aIP = canSeeObject.CanSee().GetComponent<AIPlayer>();
                if (aIP == null)
                    return null;

                if (aIP.Team1 != _Team1)
                    return aIP.gameObject;
            }
            return null;

        }
        [PunRPC]
        // from host that is disconectting to all player
        void GetHostData(List<int> _playerIDs)
        {
            PlayerIDs = _playerIDs;
        }
        public void Play(Text name)
        {
            PhotonNetwork.offlineMode = false;

            if (name.text.Length < 3)
            {

                ErrorName.text = "You have to type a name with more than 3 characters";
                ErrorName.gameObject.SetActive(true);
                return;
            }
            else
            {
                ErrorName.text = "Error";

                ErrorName.gameObject.SetActive(false);

            }
            foreach (var fadeOut in FadeOutOnPlay)
            {
                fadeOut.DoFadeOut();
            }
            foreach (var fadeIn in FadeInOnPlay)
            {
                fadeIn.DoFadeIn();
            }
            clickConnect = true;

            // if photon is !connected
            if (!PhotonNetwork.connectedAndReady)
            {

                PhotonNetwork.ConnectUsingSettings("1.0");

            }
            else
            {

                OnJoinedLobby();
            }

            // PhotonNetwork.ConnectUsingSettings("1.0");
            PhotonNetwork.player.NickName = name.text;
        }
        public void PlaySP()
        {

            PhotonNetwork.offlineMode = true;
            PhotonNetwork.CreateRoom("Offline");
            foreach (var fadeOut in FadeOutOnPlaySP)
            {
                fadeOut.DoFadeOut();
            }
            foreach (var fadeIn in FadeInOnPlaySP)
            {
                fadeIn.DoFadeIn();
            }


        }
        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            // clicked to connect
            if (clickConnect)
            {
                ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "mode", GameMode } };
                PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, byte.Parse("0"));
            }
            // kicked and trying to reconnect
            else
            {

            }
            clickConnect = false;
        }


        public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            base.OnPhotonPlayerConnected(newPlayer);

            if (PhotonNetwork.isMasterClient)
            {


                photonView.RPC("SendPlayersConnected", newPlayer, PlayerIDs.ToArray());
                if (GameMode != GameModes.FreeForAll)
                    photonView.RPC("SendTeamScores", newPlayer, Team1Score, Team2Score);
                photonView.RPC("SendTimerCount", newPlayer, timer, MinsForMatch);

            }

        }
        // from new player connected to master client
        // it's made so because it needs to be completely spawned before run this code
        [PunRPC]
        void NewPlayerSpawned(PhotonPlayer newPlayer)
        {
            List<int> killsList = new List<int>();
            List<int> deathsList = new List<int>();

            foreach (var _player in Players)
            {
                killsList.Add(_player.Kills);
                deathsList.Add(_player.Deaths);
            }

            photonView.RPC("SendPlayerScores", newPlayer, PlayerIDs.ToArray(), killsList.ToArray(), deathsList.ToArray());

        }
        public void ResumeGame()
        {
            Time.timeScale = 1f;
            if (Application.isMobilePlatform)
                GameManager.Instance.MobileUI.SetActive(true);

        }
        public void PauseGame()
        {
            Cursor.visible = true;
            Cursor.lockState = true ? CursorLockMode.Locked : CursorLockMode.None;


            if (PhotonNetwork.offlineMode)
            {
                Time.timeScale = 0f;
            }
            else
            {
                GameManager.Instance.MobileUI.SetActive(false);
            }
        }
        public void LeftRoom()
        {
            PhotonNetwork.LeaveRoom();
        }
        [PunRPC]
        void SendTimerCount(float _timer, int _minsForMatch)
        {
            MinsForMatch = _minsForMatch;
            timer = _timer;
        }


        // from master client to the new client
        [PunRPC]
        void SendPlayersConnected(int[] viewIDs)
        {

            for (int i = 0; i < viewIDs.Length; i++)
            {
                GameObject newPlayer = PhotonView.Find(viewIDs[i]).gameObject;
                if (!Players.Contains(newPlayer.GetComponent<PlayerController>()))
                    Players.Add(newPlayer.GetComponent<PlayerController>());
                PlayerIDs.Add(viewIDs[i]);
            }

            Players.Add(LocalPlayer);

            PlayerIDs.Add(LocalPlayer.photonView.viewID);





            // wait for sync in another clients

            StartCoroutine(WaitToSyncNames());
        }
        IEnumerator WaitToSyncNames()
        {
            yield return new WaitForSeconds(5f);
            SendRPCSetPlayerNames();

        }
        public override void OnDisconnectedFromPhoton()
        {
            base.OnDisconnectedFromPhoton();

            Players = new List<PlayerController>();

        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            base.OnPhotonRandomJoinFailed(codeAndMsg);
            foreach (var uiEff in FadeInOnHost)
            {
                uiEff.DoFadeIn();
            }
        }
        public void CreateGame()
        {
            if (string.IsNullOrEmpty(MaxPlayersTxt.text))

                ErrorCreatingRoom.text = "Type a max player amount";

            else if (string.IsNullOrEmpty(MaxTimeTxt.text))
                ErrorCreatingRoom.text = "Type a max time amount";
            else
            {
                ErrorCreatingRoom.text = "";

            }
            CreateRoom("MyMatch");

        }

        void CreateRoom(string _roomName)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = byte.Parse(MaxPlayersTxt.text);
            roomOptions.CustomRoomPropertiesForLobby = new string[] { "mode" };
            roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "mode", GameMode } };

            MinsForMatch = int.Parse(MaxTimeTxt.text);
            KillsForMatch = int.Parse(MaxKillsTxt.text);
            MaxPing = int.Parse(MaxPingTxt.text);

            timer = MinsForMatch * 60;

            PhotonNetwork.CreateRoom(_roomName, roomOptions, null);
            foreach (var uiEff in FadeInOnHost)
            {
                uiEff.DoFadeOut();
            }
        }
        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            foreach (var c in DesactivateOnStartGame)
            {
                c.gameObject.SetActive(false);
            }

            bool _team1 = DetermineTeamForNextPlayer();

            if (_team1)
            {
                Vector3 spawnPos = GetSpawnPoint(true).transform.position;
                PhotonNetwork.Instantiate(Player.name, spawnPos, Quaternion.identity, 0);
                
            }
            else
            {
                PhotonNetwork.Instantiate(Player.name, GetSpawnPoint(false).transform.position, Quaternion.identity, 0);
            }

            PlayerController[] playerControllers = FindObjectsOfType(typeof(PlayerController)) as PlayerController[];

        }
        // from client (player controller)
        public void AssignTeam(int viewId)
        {

            photonView.RPC("DetermineTeamForNewPlayer", PhotonTargets.MasterClient, viewId, true);
        }

        [PunRPC]
        // from master client to client. when a new player connects
        void SendTeamScores(int team1Score, int team2Score)
        {
            Team1Score = team1Score;
            Team2Score = team2Score;
            UpdateTeamScoresUI();

        }
        [PunRPC]
        // from master client to client. when a new player connects

        void SendPlayerScores(int[] viewIDs, int[] kills, int[] deaths)
        {
            for (int i = 0; i < viewIDs.Length; i++)
            {
                if (viewIDs[i] == LocalPlayer.photonView.viewID)
                    continue;

                PlayerController newPlayer = PhotonView.Find(viewIDs[i]).gameObject.GetComponent<PlayerController>();
                newPlayer.Kills = kills[i];
                newPlayer.Deaths = deaths[i];


            }
            if (MultiplayerGameManager.Instance.GameMode == MultiplayerGameManager.GameModes.FreeForAll)
            {

                var playerList = Players.OrderByDescending(player => player.Kills).ToList();
                MultiplayerGameManager.Instance.Team2Score = playerList[0].Kills;
                MultiplayerGameManager.Instance.UpdateTeamScoresUI();
            }
        }

        public void UpdateTeamScoresUI()
        {

            Team1ScoreTxt.text = Team1Score.ToString();
            Team2ScoreTxt.text = Team2Score.ToString();
        }
        // in host
        [PunRPC]
        public void DetermineTeamForNewPlayer(int viewID, bool sendRpc = true)
        {

            PlayerController player = PhotonView.Find(viewID).GetComponent<PlayerController>();
            if (Team1.Count == Team2.Count)
            {
                PlayerToTeam(player, true, viewID, sendRpc);
                return;
            }
            else
            {
                if (Team1.Count - Team2.Count < 0)
                {
                    PlayerToTeam(player, true, viewID, sendRpc);
                    return;

                }
                else
                {
                    PlayerToTeam(player, false, viewID, sendRpc);
                    return;

                }
            }
        }

        // true team 1 false team 2
        bool DetermineTeamForNextPlayer()
        {
            if (Team1.Count == Team2.Count)
            {
                return true;
            }
            else
            {
                if (Team1.Count - Team2.Count < 0)
                {
                    return true;

                }
                else
                {
                    return false;

                }
            }
        }

        public void SetGameMode(int _mode)
        {
            GameMode = (GameModes)_mode;
        }
        // from host to clients
        void PlayerToTeam(PlayerController player, bool team1, int viewID, bool sendRpc)
        {

            if (team1)
            {
                Team1.Add(player);
                player.Team1 = true;
            }
            else
            {
                Team2.Add(player);
                player.Team1 = false;
            }

            if (sendRpc)
                player.photonView.RPC("SetMyTeam", PhotonTargets.AllBuffered, team1, viewID);
        }
        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            GameManager.Instance.DamageIndicator.DoFadeOut();

            AIManager.Instance.Reset();
            GameManager.Instance.BloodSplash.DoFadeOut();

            if (PhotonNetwork.offlineMode)
            {
                Time.timeScale = 1f;
                PhotonNetwork.Destroy(LocalPlayer.photonView);
                Weapon[] weapon = FindObjectsOfType(typeof(Weapon)) as Weapon[];
                foreach (var wp in weapon)
                {
                    wp.Reset();
                }
            }

            foreach (var fadeOut in FadeOutOnReturn)
            {
                fadeOut.DoFadeOut();
            }
            foreach (var fadeIn in FadeInOnReturn)
            {
                fadeIn.DoFadeIn();
            }
            foreach (var activate in DesactivateOnStartGame)
            {
                activate.gameObject.SetActive(true);
            }


            GameManager.Instance.StopUsingWeapon();

            if (Application.isMobilePlatform && GameManager.Instance.MobileUI)
                GameManager.Instance.MobileUI.gameObject.SetActive(false);

            Team1Score = 0;
            Team2Score = 0;

            MaxTimeTxt.text = MinsForMatch.ToString();

            Team1 = new List<PlayerController>();
            Team1Stats = new List<GameObject>();
            Team2Stats = new List<GameObject>();
            PlayerIDs = new List<int>();
            Team2 = new List<PlayerController>();
            Players = new List<PlayerController>();
            GameManager.Instance.PlayerWeapon = null;
            LocalPlayer = null;

        }

        public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {


            base.OnPhotonCreateRoomFailed(codeAndMsg);

            CreateRoom("MyMatch" + errorRoomIndex++);

        }
        public void SendRPCSetPlayerNames()
        {
            photonView.RPC("SetPlayersNames", PhotonTargets.All);

        }
        [PunRPC]
        void SetPlayersNames()
        {
            if (Players[0].GetComponentInChildren<TextMesh>() != null)
                Players[0].GetComponentInChildren<TextMesh>().text = "";

            for (int i = 0; i < PhotonNetwork.otherPlayers.Length; i++)
            {
                if (Players.ToArray().Length >= PhotonNetwork.otherPlayers.Length)
                    Players[i + 1].GetComponentInChildren<TextMesh>().text = PhotonNetwork.otherPlayers[i].NickName;

            }

        }

    }
}