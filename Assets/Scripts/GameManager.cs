using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviour {

    public roomManager myRoom;

    [SerializeField]
    protected bool _IsMasterClient;

    public int PlayerCountMinimum = 2;

    [Range(0.0f, 1.0f)]
    public float StarterZombiePercentage = 0.25f;
    public float ZombieSpawnTime = 30.0f;
    public float MaxGameTime = -1.0f;
    public float AutoGameStartTime = 60.0f;

    protected bool _playerIsReady = false;

    protected bool _gameStartCountdownStarted = false;
    protected bool _gameIsRunning = false;
    public bool GameIsRunning { get { return _gameIsRunning; } }

    protected virtual void Start()
    {
        if(!myRoom)
        {
            myRoom = FindObjectOfType<roomManager>();

            if(!myRoom)
            {
                Debug.LogError("No roomManager in scene!");
            }
        }
    }

    protected virtual void Update()
    {
        if(!myRoom.isConnected) { return; }

        if(PhotonNetwork.IsMasterClient != _IsMasterClient)
        {
            _IsMasterClient = PhotonNetwork.IsMasterClient;
        }

        if(_IsMasterClient)
        {
            if((PhotonNetwork.CurrentRoom.PlayerCount >= PlayerCountMinimum) && !_gameIsRunning)
            {
                //Room properties
                ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable();
                newProperties.Add("GameReady", true);
                newProperties.Add("GameStarted", _gameIsRunning);
                myRoom.RoomProperties = newProperties;

                StartCoroutine(GameStartCountDown());
            }
            else if(!_gameIsRunning && _gameStartCountdownStarted)
            {
                ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable();
                newProperties.Add("GameReady", true);
                myRoom.RoomProperties = newProperties;
            }
        }

        if(!_gameIsRunning)
        {
            object gameReady;
            if(myRoom.RoomProperties.TryGetValue("GameReady", out gameReady))
            {
                if((bool)gameReady && !_gameStartCountdownStarted)
                {
                    StartCoroutine(GameStartCountDown());
                }
                else if(_gameStartCountdownStarted)
                {
                    StopCoroutine(GameStartCountDown());
                }
            }
        }

        if(!_gameIsRunning)
        {
            object value;
            if(myRoom.RoomProperties.TryGetValue("GameStarted", out value))
            {
                if((bool)value)
                {
                    _gameIsRunning = true;
                }
            }
        }
    }

    protected virtual IEnumerator GameStartCountDown()
    {
        if (!_gameStartCountdownStarted)
        {
            _gameStartCountdownStarted = true;

            //Players ready initialization
            ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable();
            newProperties.Add("PlayerReady", _playerIsReady);
            PhotonNetwork.SetPlayerCustomProperties(newProperties);

            float elapsedTime = 0.0f;
            bool allPlayersReady = false;

            //Wait for the time to go past or for all players to ready up.
            while((elapsedTime < AutoGameStartTime) && !allPlayersReady)
            {
                List<Player> allPlayers = new List<Player>(PhotonNetwork.CurrentRoom.Players.Values);
                allPlayersReady = true;
                for(int i = 0; (i < allPlayers.Count) && allPlayersReady; i++)
                {
                    object playerReady;
                    Debug.Log("index " + i + "\nAllReady = " + allPlayersReady);
                    if(allPlayers[i].CustomProperties.TryGetValue("PlayerReady", out playerReady))
                    {
                        if(!(bool)playerReady)
                        {
                            allPlayersReady = false;
                        }
                    }
                }

                Debug.Log("Game countdown " + elapsedTime);

                yield return null;
                elapsedTime += Time.deltaTime;
            }

            Debug.Log("Game started!");
            _gameIsRunning = true;
            //Game Start
            newProperties.Clear();
            newProperties.Add("GameStarted", true);
            myRoom.RoomProperties = newProperties;
        }
    }

    public void SetPlayerReady(bool value)
    {
        _playerIsReady = value;
        ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable();
        newProperties.Add("PlayerReady", _playerIsReady);
        PhotonNetwork.SetPlayerCustomProperties(newProperties);
    }

    private void OnGUI()
    {
        if(!_gameIsRunning)
        {
            int readyPlayerCount = 0;
            foreach(Player p in PhotonNetwork.PlayerList)
            {
                object value;
                if(p.CustomProperties.TryGetValue("PlayerReady", out value))
                {
                    if((bool)value)
                    {
                        readyPlayerCount++;
                    }
                }
            }

            if(myRoom.isConnected)
            {
                GUI.Box(new Rect(10, 10, 100, 40), PhotonNetwork.CurrentRoom.PlayerCount + " of " + PlayerCountMinimum + " players\n" + readyPlayerCount + " players ready");
            }
            else
            {
                GUI.Box(new Rect(10, 10, 100, 25), "0 of " + PlayerCountMinimum + " players");
            }
        }
    }
}
