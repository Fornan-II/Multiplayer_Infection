using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public PlayerHUD playerHUD;
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
        _controlledPawn.OnTakeControl();

        playerHUD.IsVisible(true);

        return true;
    }

    protected void ReleaseControl()
    {
        _controlledPawn.OnReleasedControl();
        _controlledPawn = null;

        playerHUD.IsVisible(false);
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
