using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SmartNotes.Models
{
    public class XmlSerialization
    {
        //Serialize to xml
        public static string ToXml(List<Note> value)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Note>));
            StringBuilder stringBuilder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = true,
                OmitXmlDeclaration = true
            };


            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                serializer.Serialize(xmlWriter, value);
            }
            return stringBuilder.ToString();
        }

        //Deserialize from xml
        public static List<Note> FromXml(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Note>));
            List<Note> value;
            using (StringReader stringReader = new StringReader(xml))
            {
                object deserialized = serializer.Deserialize(stringReader);
                value = (List<Note>)deserialized;
            }
            return value;
        }
    }
}
