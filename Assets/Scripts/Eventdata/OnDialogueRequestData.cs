using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDialogueRequestData 
{
    public string sequenceName;
    public Action action;
    public OnDialogueRequestData(string sequenceName, Action action = null)
    { 
    this.sequenceName = sequenceName;
        this.action = action;
    }
}
