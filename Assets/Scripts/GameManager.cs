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

    protected float _gameTimeElapsed = 0.0f;

    protected bool _playerIsReady = false;
    protected bool _playerInitialized = false;
    public enum GameState
    {
        PREGAME,
        PREPARATION_PHASE,
        GAME_RUNNING,
        GAME_END
    }
    protected GameState _currentGameState = GameState.PREGAME;

    public GameState CurrentGameState { get { return _currentGameState; } }

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

        Debug.Log("Current Game State: " + _currentGameState);

        //Check to see if this client is the Master Client
        if (PhotonNetwork.IsMasterClient != _IsMasterClient)
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
        if ((PhotonNetwork.CurrentRoom.PlayerCount >= PlayerCountMinimum) && _currentGameState == GameState.PREGAME)
        {
            //Room properties
            ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
                {
                    { "enum_CurrentGameState", _currentGameState }
                };

            if (!myRoom.RoomProperties.ContainsKey("double_PreGameCountDownStart"))
            {
                newProperties.Add("double_PreGameCountDownStart", PhotonNetwork.Time);
            }

            myRoom.RoomProperties = newProperties;
        }

        if(_currentGameState == GameState.PREPARATION_PHASE)
        {
            //Game is running. If zombies haven't spawned yet, spawn them if enough time has passed.
            if (_gameTimeElapsed > TimeBeforeZombieSpawn)
            {
                FigureOutWhoToMurderAndMakeIntoZombies();
            }
        }

        if(_currentGameState == GameState.PREPARATION_PHASE || _currentGameState == GameState.GAME_RUNNING)
        {
            bool humansRemain = false;
            bool isHumanInitialized = false;
            for (int i = 0; (i < PhotonNetwork.PlayerList.Length) && !humansRemain; i++)
            {
                object isHuman;
                if (PhotonNetwork.PlayerList[i].CustomProperties.TryGetValue("bool_IsHuman", out isHuman))
                {
                    isHumanInitialized = true;
                    if ((bool)isHuman)
                    {
                        humansRemain = true;
                    }
                }
            }
            if (isHumanInitialized && !humansRemain)
            {
                EndGame();
            }
        }
    }

    protected virtual void LocalClientOperations()
    {
        //Update Game state
        object gameStateObj;
        if (myRoom.RoomProperties.TryGetValue("enum_CurrentGameState", out gameStateObj))
        {
            if ((GameState)gameStateObj != _currentGameState)
            {
                _currentGameState = (GameState)gameStateObj;
            }
        }

        if(_currentGameState == GameState.PREGAME)
        {
            //Get ready to start game if Server says so.
            object preGameStartTime;
            if (myRoom.RoomProperties.TryGetValue("double_PreGameCountDownStart", out preGameStartTime))
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

        if(_currentGameState == GameState.PREPARATION_PHASE)
        {
            if(!_playerInitialized)
            {
                GameStartSetup();
            }
        }

        if(_currentGameState == GameState.PREPARATION_PHASE || _currentGameState == GameState.GAME_RUNNING)
        {
            object startTimeObj;
            if (myRoom.RoomProperties.TryGetValue("double_StartTime", out startTimeObj))
            {
                _gameTimeElapsed = (float)(PhotonNetwork.Time - (double)startTimeObj);
            }
        }

        if (_currentGameState == GameState.GAME_RUNNING)
        {
            if(myRoom.myPlayerType == roomManager.PlayerType.HUMAN)
            {
                object IsHuman;
                if(PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("bool_IsHuman", out IsHuman))
                {
                    if(!(bool)IsHuman)
                    {
                        myRoom.playerController.MurderPawn();
                    }
                }
            }
        }

        if(_currentGameState == GameState.GAME_END)
        {
            if(myRoom.isConnected)
            {
                myRoom.LeaveRoom();
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
                { "enum_CurrentGameState", GameState.PREPARATION_PHASE },
                { "double_PreGameCountDownStart", null }
            };

            if(!myRoom.RoomProperties.ContainsKey("double_StartTime"))
            {
                newProperties.Add("double_StartTime", PhotonNetwork.Time);
            }

            myRoom.RoomProperties = newProperties;
        }

        _playerInitialized = true;
        _currentGameState = GameState.PREPARATION_PHASE;
        myRoom.myPlayerType = roomManager.PlayerType.HUMAN;
    }

    protected virtual void EndGame()
    {
        if (_IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "enum_CurrentGameState", GameState.GAME_END }
            };
            myRoom.RoomProperties = newProperties;
            myRoom.RoomCanBeJoined = false;
        }
    }

    protected virtual void FigureOutWhoToMurderAndMakeIntoZombies()
    {
        if(!_IsMasterClient) { return; }

        ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "bool_IsHuman", false }
            };
        List<Player> SelectablePlayers = new List<Player>(PhotonNetwork.PlayerList);

        int starterZombieCount = (int)(SelectablePlayers.Count * StarterZombiePercentage);
        if(starterZombieCount < 1) { starterZombieCount = 1; }

        //Remove players who are already zombies
        for (int i = 0; i < SelectablePlayers.Count; i++)
        {
            object isHuman;
            if (SelectablePlayers[i].CustomProperties.TryGetValue("bool_IsHuman", out isHuman))
            {
                if(!(bool)isHuman)
                {
                    SelectablePlayers.RemoveAt(i);
                    i--;
                    starterZombieCount--;
                }
            }
        }

        //Pick a few players to make into zombies
        for (int c = 0; (c < starterZombieCount) && (SelectablePlayers.Count > 1); c++)
        {
            Debug.Log("Picking player for zombie (" + starterZombieCount + " starter zombies)");
            int index = Random.Range(0, SelectablePlayers.Count);
            SelectablePlayers[index].SetCustomProperties(newProperties);
            SelectablePlayers.RemoveAt(index);
        }

        newProperties.Clear();
        newProperties.Add("bool_IsHuman", true);
        //Mark the rest of the players as humans
        foreach(Player p in SelectablePlayers)
        {
            p.SetCustomProperties(newProperties);
        }

        newProperties.Clear();
        newProperties.Add("enum_CurrentGameState", GameState.GAME_RUNNING);
        myRoom.RoomProperties = newProperties;
        //Make the room no longer joinable
        myRoom.RoomCanBeJoined = false;
    }

    protected virtual void OnGUI()
    {
        if(_currentGameState == GameState.PREGAME)
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

        if(_currentGameState == GameState.PREPARATION_PHASE || _currentGameState == GameState.GAME_RUNNING)
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
