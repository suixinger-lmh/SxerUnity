using Sxer.Plugin.EventSystem.Cmd;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JSCMD_Test : JsCommandHandler
{
    public override string CommandId => typeof(JSCMD_Test).FullName;



    protected override void ExecuteAction(JsCmdArg arg)
    {
        Debug.Log($"[JSCMD_Test] Ö´ÐÐ¡£²ÎÊý: {arg.jsonStr}");
    }

    
}
