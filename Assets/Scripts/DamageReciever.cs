using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DamageReciever : MonoBehaviour {

    public int health = 100;

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
        if(health <= 0.0f)
        {
            PhotonView pv = gameObject.GetComponent<PhotonView>();
            if (pv)
            {
                pv.RPC("Die", RpcTarget.AllBuffered);
                if(pv.IsMine)
                {
                    roomManager rm = GameObject.FindGameObjectWithTag("Room").GetComponent<roomManager>();
                    if (rm)
                    {
                        rm.SpawnPlayer();
                    }
                }
            }
        }
    }

    [PunRPC]
    public void Die()
    {
        Destroy(gameObject);

        
        //PhotonNetwork.Disconnect();
        //UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
