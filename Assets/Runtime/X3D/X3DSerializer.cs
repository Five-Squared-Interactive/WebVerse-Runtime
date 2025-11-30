using System.Xml;
using System.IO;
using UnityEngine;

namespace X3D
{
    public class X3DSerializer
    {
        public static void WriteToFile(X3DNode rootNode, string filePath)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement rootElement = doc.CreateElement(rootNode.Name);
            WriteNode(doc, rootElement, rootNode);
            doc.AppendChild(rootElement);
            doc.Save(filePath);
            Debug.Log($"X3D exported to {filePath}");
        }

        private static void WriteNode(XmlDocument doc, XmlElement xmlElement, X3DNode node)
        {
            foreach (var attr in node.Attributes)
            {
                xmlElement.SetAttribute(attr.Key, attr.Value);
            }
            foreach (var child in node.Children)
            {
                XmlElement childElement = doc.CreateElement(child.Name);
                WriteNode(doc, childElement, child);
                xmlElement.AppendChild(childElement);
            }
        }
    }
}
