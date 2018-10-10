using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour {

    public PlayerHUD playerHUD;
    public Reticule playerReticule;
    public Pawn ControlledPawn { get { return _controlledPawn; } }
    protected Pawn _controlledPawn;

	public bool TakeControlOf(Pawn pawn)
    {
        if (pawn == null)
        {
            if (_controlledPawn)
            {
                ReleaseControl();
            }
            return true;
        }
        if(_controlledPawn)
        {
            return false;
        }

        _controlledPawn = pawn;
        _controlledPawn.OnTakeControl(this);

        playerHUD.IsVisible(true);
        playerReticule.gameObject.SetActive(true);

        return true;
    }

    protected void ReleaseControl()
    {
        if(_controlledPawn)
        {
            _controlledPawn.OnReleasedControl();
            _controlledPawn = null;
        }

        playerHUD.IsVisible(false);
        playerReticule.gameObject.SetActive(false);
    }

    public bool MurderPawn()
    {
        if(!ControlledPawn) { return false; }

        PhotonView pv = ControlledPawn.GetComponent<PhotonView>();
        if(!pv) { return false; }
        pv.RPC("ApplyDamage", RpcTarget.All, 666);
        return true;
    }

    protected void Update()
    {
        HandleInput();

        if(_controlledPawn)
        {
            playerHUD.SetMaxHealth(_controlledPawn.MaxHealth);
            playerHUD.UpdateHealthUI(_controlledPawn.Health);
        }
    }

    protected virtual void HandleInput()
    {
        if(!_controlledPawn) { return; }

        _controlledPawn.Movement(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
        _controlledPawn.Rotation(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")));
        _controlledPawn.Attack(Input.GetButton("Fire1"));
        _controlledPawn.Jump(Input.GetButton("Jump"));
        _controlledPawn.Sprint(Input.GetButton("Fire5"));
        _controlledPawn.Crouch(Input.GetButton("Fire6"));
        _controlledPawn.Cancel(Input.GetButton("Cancel"));
    }
}
