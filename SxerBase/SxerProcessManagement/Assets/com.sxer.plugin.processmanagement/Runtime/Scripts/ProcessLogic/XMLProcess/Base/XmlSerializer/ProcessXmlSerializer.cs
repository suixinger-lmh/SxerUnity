using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
namespace Sxer.Frame.Plugin.ProcessManagement.XMLProcess
{
    [XmlRoot("ProcessXmlSerializer")]
    [XmlInclude(typeof(XMLProcessBase))]
    public class ProcessXmlSerializer
    {
        [XmlArray("XMLProcessBases")]
        [XmlArrayItem("XMLProcessBase")]
        public List<XMLProcessBase> process;

    }
}