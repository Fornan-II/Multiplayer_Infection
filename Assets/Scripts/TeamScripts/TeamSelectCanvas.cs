using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSelectCanvas : MonoBehaviour {

    public roomManager myRoomManager;

	public void PickHuman()
    {
        if(myRoomManager)
        {
            myRoomManager.myPlayerType = roomManager.PlayerType.HUMAN;
            gameObject.SetActive(false);
        }
    }

    public void PickZombie()
    {
        if (myRoomManager)
        {
            myRoomManager.myPlayerType = roomManager.PlayerType.ZOMBIE;
            gameObject.SetActive(false);
        }
    }
}
