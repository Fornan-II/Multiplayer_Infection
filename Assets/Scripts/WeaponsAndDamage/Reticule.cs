using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reticule : MonoBehaviour
{
    public Color hasTargetColor;

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

    public virtual void HasTarget(bool value)
    {
        if(!_myReticule) { return; }

        if(value)
        {
            _myReticule.color = hasTargetColor;
        }
        else
        {
            _myReticule.color = _defaultColor;
        }
    }
}
