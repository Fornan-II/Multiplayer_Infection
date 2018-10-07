using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reticule : MonoBehaviour
{
    public Color enemyTargetColor;
    public Color friendlyTargetColor;

    public enum TargetType
    {
        NONE,
        FRIENDLY,
        ENEMY
    }

    protected Color _defaultColor;
    protected Image _myReticule;

    protected virtual void Start()
    {
        _myReticule = gameObject.GetComponent<Image>();
        if(_myReticule)
        {
            _defaultColor = _myReticule.color;
        }
        else
        {
            Debug.LogWarning("Reticule script on " + gameObject.name + " has no Image Component!");
        }
    }

    public virtual void HasTarget(TargetType value)
    {
        if(!_myReticule) { return; }

        if(value == TargetType.ENEMY)
        {
            _myReticule.color = enemyTargetColor;
        }
        else if(value == TargetType.FRIENDLY)
        {
            _myReticule.color = friendlyTargetColor;
        }
        else
        {
            _myReticule.color = _defaultColor;
        }
    }
}
