using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamHuman : TeamBehavior {

    public override void OnDie()
    {
        base.OnDie();
        if(playerSpawner)
        {
            playerSpawner.myPlayerType = NetworkedPlayerSpawner.PlayerType.ZOMBIE;
        }
    }
}
