using Sxer.Plugin.EventSystem.Cmd;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// JS指令特有的参数封装
/// </summary>
public class JsCmdArg : CommandHandlerArg
{
    public string jsonStr;
    public JsCmdArg(string json) => this.jsonStr = json;
}

[Serializable]
/// <summary>
/// 针对 JS 字符串的指令处理基类
/// </summary>
public abstract class JsCommandHandler : CommandHandlerBase<JsCmdArg>
{

    

    // 提供业务层的通用 JSON 方法
    protected string GetCommonJsonInf(string jsonstr)
    {
        return "";
        //需要litjson
        //try
        //{
        //    JsonData jd = JsonMapper.ToObject(jsonstr);
        //    return (string)jd["inf"];
        //}
        //catch
        //{
        //    return string.Empty;
        //}
    }
}