using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JSCMD_Init : JsCommandHandler
{
    public override string CommandId => "001";
    public override string CmdDesc => "ceshi INIT";

    protected override void ExecuteAction(JsCmdArg arg)
    {
        Debug.Log($"[JSCMD_Init] Ö´ÐÐ¡£²ÎÊý: {arg.jsonStr}");
    }

}
