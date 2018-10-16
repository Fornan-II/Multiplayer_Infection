using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomSearcher : MonoBehaviour {

    public Text QueryButtonText;
    public Text RoomStatusText;
    public Text RoomNameField;

    public int gameSceneBuildIndex = 1;

    public enum SearcherState
    {
        QUERY,
        JOIN,
        CREATE
    }
    public SearcherState CurrentState = SearcherState.QUERY;

    protected string _roomName = "";

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
        if (!RoomNameField)
        {
            Debug.LogWarning(name + " does not have RoomNameField assigned!");
        }

        RoomStatusText.text = "";
    }

    public void RoomNameTextOnChange()
    {
        CurrentState = SearcherState.QUERY;
        QueryButtonText.text = "Search for room...";
    }

    public void OnRoomNameFinishedEditing()
    {
        _roomName = RoomNameField.text.Trim();
    }

    public void QueryButtonAction()
    {
        //First remove leading/trailing white space, then make sure roomName isn't an empty field.
        if(_roomName == "")
        {
            RoomNameTextOnChange();
            RoomStatusText.text = "Invalid room name; can not be empty.";
            return;
        }

        //Then, perform button action
        SearcherState StartingState = CurrentState;

        QueryRoom();

        //If state changed during the query, don't try to do anything else yet.
        if (StartingState != CurrentState) { return; }

        if (CurrentState == SearcherState.JOIN) { JoinRoom(); }
        else if(CurrentState == SearcherState.CREATE) { CreateRoom(); }
    }

    protected void QueryRoom()
    {
        roomManager.Self.ForceRefreshRoomList();

        RoomInfo foundRoom = null;
        for(int i = 0; (i < roomManager.Self.RoomList.Count) && (foundRoom == null); i++)
        {
            if(roomManager.Self.RoomList[i].Name == _roomName)
            {
                foundRoom = roomManager.Self.RoomList[i];
            }
        }
        
        if(foundRoom == null)
        {
            CurrentState = SearcherState.CREATE;
            RoomStatusText.text = "No room called " + _roomName + " found.";
            QueryButtonText.text = "Create room";
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
                    if((GameManager.GameState)gameStatusObj == GameManager.GameState.PREGAME)
                    {
                        newStatusText += "Currently waiting for the game to start.";
                        QueryButtonText.text = "Join game";
                    }
                    else
                    {
                        newStatusText += "Game already in progress, can not join.";
                        CurrentState = SearcherState.QUERY;
                        QueryButtonText.text = "Unable to join";
                    }
                }
                else
                {
                    newStatusText += "Currently waiting for the game to start.";
                    QueryButtonText.text = "Join game";
                }
            }
            else
            {
                newStatusText += "Game already in progress, can not join.";
                CurrentState = SearcherState.QUERY;
                QueryButtonText.text = "Unable to join";
            }

            RoomStatusText.text = newStatusText;
        }
    }

    protected void JoinRoom()
    {
        roomManager.Self.JoinRoom(_roomName);
    }

    protected void CreateRoom()
    {
        roomManager.Self.CreateRoom(_roomName, gameSceneBuildIndex);
    }
}
