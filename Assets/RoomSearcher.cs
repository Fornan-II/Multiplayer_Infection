using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomSearcher : MonoBehaviour {

    public Text QueryButtonText;
    public Text RoomStatusText;

    public enum SearcherState
    {
        QUERY,
        JOIN,
        CREATE,
        UNABLE_TO_JOIN
    }
    public SearcherState CurrentState = SearcherState.QUERY;

    [HideInInspector]
    public string RoomName = "";

    private void Start()
    {
        if(!QueryButtonText)
        {
            Debug.LogWarning(name + " does not have QueryButtonText assigned!");
        }
        if(!RoomStatusText)
        {
            Debug.LogWarning(name + " does not have RoomStatusText assigned!");
        }
    }

    public void RoomNameTextOnChange()
    {
        CurrentState = SearcherState.QUERY;
        QueryButtonText.text = "Search For Room...";
    }

    public void QueryButtonAction()
    {
        QueryRoom();

        if(CurrentState == SearcherState.JOIN) { JoinRoom(); }
        else if(CurrentState == SearcherState.CREATE) { CreateRoom(); }
    }

    public void QueryRoom()
    {
        RoomInfo foundRoom = null;
        for(int i = 0; (i < roomManager.Self.roomName.Length) && (foundRoom == null); i++)
        {
            if(roomManager.Self.RoomList[i].Name == RoomName)
            {
                foundRoom = roomManager.Self.RoomList[i];
            }
        }
        
        if(foundRoom == null)
        {
            CurrentState = SearcherState.CREATE;
            RoomStatusText.text = "No room of this name found.";
            QueryButtonText.text = "Create Room";
        }
        else
        {
            CurrentState = SearcherState.JOIN;
            string newStatusText = "Room found.\n" + foundRoom.Name + " - " + foundRoom.PlayerCount + " players.\n";
            if(foundRoom.IsOpen)
            {
                object gameStatusObj;
                if(foundRoom.CustomProperties.TryGetValue("enum_CurrentGameState", out gameStatusObj))
                {
                    GameManager.GameState gameStatus = (GameManager.GameState)gameStatusObj;
                    if(gameStatus == GameManager.GameState.PREPARATION_PHASE)
                    {
                        newStatusText += "Currently in preperation phase: zombies spawning soon.";
                    }
                    else
                    {
                        newStatusText += "Currently waiting for the game to start.";
                    }
                }
                else
                {
                    newStatusText += "Currently waiting for the game to start.";
                }

                QueryButtonText.text = "Join Game";
            }
            else
            {
                newStatusText += "Game already in progress, can not join.";
                CurrentState = SearcherState.UNABLE_TO_JOIN;
                QueryButtonText.text = "Unable to Join";
            }

            RoomStatusText.text = newStatusText;
        }
    }

    public void JoinRoom()
    {
        roomManager.Self.JoinRoom(RoomName);
    }

    public void CreateRoom()
    {
        roomManager.Self.CreateRoom(RoomName);
    }
}
