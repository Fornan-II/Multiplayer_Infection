using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour {

    public advancedMoveScript myMoveScript;
    public lookScript myLookScript;
    public WeaponHolder myWeaponHolder;

    public virtual void OnTakeControl()
    {
        if (myLookScript)
        {
            myLookScript.lockState = true;
        }
    }

    public virtual void OnReleasedControl()
    {
        if (myLookScript)
        {
            myLookScript.lockState = false;
        }
    }

    public void Movement(Vector2 input)
    {
        if (myMoveScript)
        {
            myMoveScript.MoveHorizontal(input.x);
            myMoveScript.MoveVertical(input.y);
        }
    }

    public void Rotation(Vector2 input)
    {
        if(myLookScript)
        {
            myLookScript.MouseInput = input;
        }
    }

    public void Attack(bool input)
    {
        if (myWeaponHolder)
        {
            myWeaponHolder.Use(input);
        }

        if (myLookScript && input)
        {
            myLookScript.lockState = true;
        }
    }

    public void Jump(bool input)
    {
        if (myMoveScript)
        {
            myMoveScript.Ability1(input);
        }
    }

    public void Sprint(bool input)
    {
        if (myMoveScript)
        {
            myMoveScript.Ability2(input);
        }
    }

    public void Crouch(bool input)
    {
        if (myMoveScript)
        {
            myMoveScript.Ability3(input);
        }
    }

    public void Cancel(bool input)
    {
        if (myLookScript && input)
        {
            myLookScript.lockState = false;
        }
    }
}
