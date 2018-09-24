using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DamageReciever : MonoBehaviour {

    public int health = 100;
    public TeamBehavior myTeam;
    public roomManager myManager;
    protected bool _isDying = false;

    private void FixedUpdate()
    {
        /*if(health <= 0.0f)
        {
            PhotonView pv = gameObject.GetComponent<PhotonView>();
            if(pv)
            {
                pv.RPC("Die", RpcTarget.AllBuffered);
            }
        }*/
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 100, 30), "HP : " + health);
    }

    [PunRPC]
    public void ApplyDamage(int dmg)
    {
        health -= dmg;
        Debug.Log("ouchie");
        if(health <= 0.0f && !_isDying)
        {
            _isDying = true;
            myTeam.OnDie();
            PhotonNetwork.Destroy(gameObject);
            myManager.controlledObject = null;
            myManager.SpawnPlayer();
        }
    }
}
