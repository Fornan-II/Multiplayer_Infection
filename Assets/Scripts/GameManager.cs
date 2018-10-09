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
    public float TimeBeforeZombieSpawn = 30.0f;
    public float MaxGameTime = -1.0f;
    public float AutoGameStartTime = 60.0f;
    protected float _preGameRemainingTime;

    protected float _gameTimeElapsed = -1.0f;

    protected bool _playerIsReady = false;

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

        //Players ready initialization
        ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "bool_PlayerReady", _playerIsReady }
        };
        PhotonNetwork.SetPlayerCustomProperties(newProperties);

        _preGameRemainingTime = AutoGameStartTime;
    }

    protected virtual void Update()
    {
        if(!myRoom.isConnected) { return; }

        //Check to see if this client is the Master Client
        if(PhotonNetwork.IsMasterClient != _IsMasterClient)
        {
            _IsMasterClient = PhotonNetwork.IsMasterClient;
        }

        //Perform MasterClient stuffs
        if(_IsMasterClient)
        {
            MasterClientOperations();
        }

        LocalClientOperations();
    }

    protected virtual void MasterClientOperations()
    {
        //Just to make sure
        if(!PhotonNetwork.IsMasterClient) { return; }

        //If game hasn't started yet, tell the server to get ready to start the game
        if ((PhotonNetwork.CurrentRoom.PlayerCount >= PlayerCountMinimum) && !_gameIsRunning)
        {
            //Room properties
            ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
                {
                    { "bool_GameStarted", _gameIsRunning }
                };

            if (!myRoom.RoomProperties.ContainsKey("double_PreGameCountDownStart"))
            {
                newProperties.Add("double_PreGameCountDownStart", PhotonNetwork.Time);
            }

            myRoom.RoomProperties = newProperties;
        }

        if(_gameIsRunning)
        {
            //Wait until setup time is over then murder some humans
        }
    }

    protected virtual void LocalClientOperations()
    {
        //Get ready to start game if Server says so.
        object preGameStartTime;
        if(myRoom.RoomProperties.TryGetValue("double_PreGameCountDownStart", out preGameStartTime))
        {
            //Run auto-start timer
            float preGameElapsedTime = (float)(PhotonNetwork.Time - (double)preGameStartTime);
            _preGameRemainingTime = AutoGameStartTime - preGameElapsedTime;

            //Check to see if all players are ready
            List<Player> allPlayers = new List<Player>(PhotonNetwork.CurrentRoom.Players.Values);
            bool allPlayersReady = true;
            for (int i = 0; (i < allPlayers.Count) && allPlayersReady; i++)
            {
                object playerReady;
                if (allPlayers[i].CustomProperties.TryGetValue("bool_PlayerReady", out playerReady))
                {
                    if (!(bool)playerReady)
                    {
                        allPlayersReady = false;
                    }
                }
            }

            if (_preGameRemainingTime <= 0.0f || allPlayersReady)
            {
                GameStartSetup();
            }
        }

        if (!_gameIsRunning)
        {
            object gameRunningObj;
            if (myRoom.RoomProperties.TryGetValue("bool_GameStarted", out gameRunningObj))
            {
                if ((bool)gameRunningObj)
                {
                    GameStartSetup();
                }
            }
        }
        else
        {
            //Game is running. If zombies haven't spawned yet, spawn them if enough time has passed.
            if (_IsMasterClient)
            {
                object zombiesSpawnedObj;
                if (!myRoom.RoomProperties.TryGetValue("bool_ZombiesSpawned", out zombiesSpawnedObj) && _gameTimeElapsed > TimeBeforeZombieSpawn)
                {
                    FigureOutWhoToMurderAndMakeIntoZombies();
                }
            }

            object startTimeObj;
            if (myRoom.RoomProperties.TryGetValue("double_StartTime", out startTimeObj))
            {
                _gameTimeElapsed = (float)(PhotonNetwork.Time - (double)startTimeObj);
            }
        }
    }

    public virtual void SetPlayerReady(bool value)
    {
        _playerIsReady = value;
        ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "bool_PlayerReady", _playerIsReady }
        };
        PhotonNetwork.SetPlayerCustomProperties(newProperties);
    }

    protected virtual void GameStartSetup()
    {
        if (_IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "bool_GameStarted", true },
                { "double_StartTime", PhotonNetwork.Time },
                { "double_PreGameCountDownStart", null }
            };
            myRoom.RoomProperties = newProperties;
        }

        _gameIsRunning = true;
        myRoom.myPlayerType = roomManager.PlayerType.HUMAN;
    }

    protected virtual void FigureOutWhoToMurderAndMakeIntoZombies()
    {
        ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "bool_MarkedForDeath", true }
            };
        List<Player> SelectablePlayers = new List<Player>(PhotonNetwork.PlayerList);

        int starterZombieCount = (int)(SelectablePlayers.Count * StarterZombiePercentage);
        if(starterZombieCount < 1) { starterZombieCount = 1; }

        for(int c = 0; (c <= starterZombieCount) && (SelectablePlayers.Count > 1); c++)
        {
            int index = Random.Range(0, SelectablePlayers.Count);
            SelectablePlayers[index].SetCustomProperties(newProperties);
            SelectablePlayers.RemoveAt(index);
        }


        newProperties.Clear();
        newProperties.Add("bool_ZombiesSpawned", true);
        myRoom.RoomProperties = newProperties;
    }

    protected virtual void OnGUI()
    {
        if(!_gameIsRunning)
        {
            int readyPlayerCount = 0;
            foreach(Player p in PhotonNetwork.PlayerList)
            {
                object value;
                if(p.CustomProperties.TryGetValue("bool_PlayerReady", out value))
                {
                    if((bool)value)
                    {
                        readyPlayerCount++;
                    }
                }
            }

            if(myRoom.isConnected)
            {
                //Debug.Log(_preGameRemainingTime);
                if (_preGameRemainingTime < AutoGameStartTime)
                {
                    GUI.Box(new Rect(Screen.width - 130, 10, 120, 43), "Game will start\nin " + (int)_preGameRemainingTime + " seconds.");
                }
                GUI.Box(new Rect(10, 10, 100, 40), PhotonNetwork.CurrentRoom.PlayerCount + " of " + PlayerCountMinimum + " players\n" + readyPlayerCount + " players ready");
                
            }
            else
            {
                GUI.Box(new Rect(10, 10, 100, 25), "0 of " + PlayerCountMinimum + " players");
            }
        }

        if(_gameTimeElapsed > 0.0f)
        {
            int minutes = (int)_gameTimeElapsed / 60;
            int seconds = (int)_gameTimeElapsed % 60;
            string message = minutes + ":";
            if(seconds < 10)
            {
                message += "0" + seconds;
            }
            else
            {
                message += seconds;
            }

            GUI.Box(new Rect(Screen.width - 110, 10, 100, 25), message);
        }
    }
}
