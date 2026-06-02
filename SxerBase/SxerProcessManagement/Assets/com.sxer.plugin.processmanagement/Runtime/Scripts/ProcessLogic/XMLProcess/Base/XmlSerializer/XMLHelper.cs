using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace Sxer.Frame.Plugin.ProcessManagement.XMLProcess.Tool
{
    public static class XMLHelper
    {
        #region xml 使用System.Xml.Serialization 通过文件流方式加载xml文件

        /// <summary>
        /// XML反序列化
        /// </summary>
        /// <typeparam name="T">需要对类和属性进行标记</typeparam>
        /// <returns></returns>
        public static T XMLDeserialize<T>(string path, Type[] types)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T), types);
                FileStream fs = new FileStream(path, FileMode.Open);
                return (T)serializer.Deserialize(fs);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                Debug.Log(" XML反序列化出错 " + ex.Data + ", 错误信息 : " + ex);
#endif
                return default(T);
            }
        }
        /// <summary>
        /// XML反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static T XMLDeserialize<T>(byte[] bytes, Type[] types)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T), types);
                Stream stream = new MemoryStream(bytes);
                return (T)serializer.Deserialize(stream);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                Debug.Log(" XML反序列化出错 " + ex.Data + ", 错误信息 : " + ex);
#endif
                return default(T);
            }
        }

        #endregion
    }
}

