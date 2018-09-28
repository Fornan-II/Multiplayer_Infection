using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

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
        return true;
    }

    protected void ReleaseControl()
    {
        _controlledPawn.OnReleasedControl();
        _controlledPawn = null;
    }

    protected void Update()
    {
        HandleInput();
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
