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

        if(!_gameIsRunning)
        {
            object gameRunningObj;
            if (myRoom.RoomProperties.TryGetValue("bool_GameStarted", out gameRunningObj))
            {
                if ((bool)gameRunningObj)
                {
                    _gameIsRunning = true;
                }
            }
        }
        else
        {
            if(_IsMasterClient)
            {
                
            }

            object startTimeObj;
            if(myRoom.RoomProperties.TryGetValue("double_StartTime", out startTimeObj))
            {
                _gameTimeElapsed = (float)(PhotonNetwork.Time - (double)startTimeObj);
            }
        }
    }

    protected virtual void MasterClientOperations()
    {
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
    }

    /*protected virtual IEnumerator GameStartCountDown()
    {
        if (!_gameStartCountdownStarted)
        {
            _gameStartCountdownStarted = true;

            

            _preGameRemainingTime = AutoGameStartTime;
            bool allPlayersReady = false;

            //Wait for the time to go past or for all players to ready up.
            while((_preGameRemainingTime > 0) && !allPlayersReady)
            {
                List<Player> allPlayers = new List<Player>(PhotonNetwork.CurrentRoom.Players.Values);
                allPlayersReady = true;
                for(int i = 0; (i < allPlayers.Count) && allPlayersReady; i++)
                {
                    object playerReady;
                    Debug.Log("index " + i + "\nAllReady = " + allPlayersReady);
                    if(allPlayers[i].CustomProperties.TryGetValue("bool_PlayerReady", out playerReady))
                    {
                        if(!(bool)playerReady)
                        {
                            allPlayersReady = false;
                        }
                    }
                }

                Debug.Log("Game countdown " + _preGameRemainingTime);

                yield return null;
                _preGameRemainingTime -= Time.deltaTime;
            }

            Debug.Log("Game started!");
            //Game Start
            GameStartSetup();
        }
    }*/

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
                GUI.Box(new Rect(10, 10, 100, 43), PhotonNetwork.CurrentRoom.PlayerCount + " of " + PlayerCountMinimum + " players\n" + readyPlayerCount + " players ready");
                
            }
            else
            {
                GUI.Box(new Rect(10, 10, 100, 29), "0 of " + PlayerCountMinimum + " players");
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

            GUI.Box(new Rect(Screen.width - 110, 10, 100, 29), message);
        }
    }
}
