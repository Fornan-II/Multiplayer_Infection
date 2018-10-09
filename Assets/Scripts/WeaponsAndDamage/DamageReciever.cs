using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DamageReciever : MonoBehaviour {

    public int health = 100;
    public TeamBehavior myTeam;
    public roomManager myManager;
    protected bool _isDying = false;

    public int maxHealth { get { return _maxHealth; } }
    protected int _maxHealth;

    private void Start()
    {
        _maxHealth = health;
    }

    private void OnGUI()
    {
        //GUI.Box(new Rect(10, 10, 100, 30), "HP : " + health);
    }

    [PunRPC]
    public void ApplyDamage(int dmg)
    {
        PhotonView pv = gameObject.GetComponent<PhotonView>();
        if(pv)
        {
            if(!pv.IsMine) { return; }
        }
        else
        {
            Debug.LogError("DamageReciever has no PhotonView!");
            return;
        }

        health -= dmg;
        if(health <= 0.0f && !_isDying)
        {
            _isDying = true;
            myTeam.OnDie();
            PhotonNetwork.Destroy(gameObject);
            if (myManager)
            {
                myManager.playerController.TakeControlOf(null);
                myManager.SpawnPlayer();
            }
        }
    }
}
