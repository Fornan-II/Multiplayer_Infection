using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DebugListRooms : MonoBehaviour {

	public void ListRooms()
    {
        int c = PhotonNetwork.CountOfRooms;
        string msg = "Currently " + c + " room";
        if(c != 1)
        {
            msg += "s";
        }
        msg += " active.";

        Debug.Log(msg);
    }
}
