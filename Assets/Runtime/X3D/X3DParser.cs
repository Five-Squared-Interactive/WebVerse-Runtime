using System.Xml;
using System.IO;
using UnityEngine;

namespace X3D
{
    public class X3DParser
    {
        /// <summary>
        /// Parse an X3D document from a file path.
        /// </summary>
        public static X3DNode ParseFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"File not found: {filePath}");
                return null;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            XmlElement root = doc.DocumentElement;
            return ParseNode(root);
        }

        /// <summary>
        /// Parse an X3D document from an XML string.
        /// </summary>
        public static X3DNode Parse(string x3dContent)
        {
            if (string.IsNullOrEmpty(x3dContent))
            {
                Debug.LogError("[X3DParser] Content is null or empty");
                return null;
            }
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(x3dContent);
                XmlElement root = doc.DocumentElement;
                return ParseNode(root);
            }
            catch (XmlException ex)
            {
                Debug.LogError($"[X3DParser] XML parsing error: {ex.Message}");
                return null;
            }
        }

        private static X3DNode ParseNode(XmlNode xmlNode)
        {
            X3DNode node = new X3DNode(xmlNode.Name);
            // Parse all attributes, including DEF/USE and others
            if (xmlNode.Attributes != null)
            {
                foreach (XmlAttribute attr in xmlNode.Attributes)
                {
                    node.Attributes[attr.Name] = attr.Value;
                }
            }
            foreach (XmlNode child in xmlNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    node.Children.Add(ParseNode(child));
                }
                else if (child.NodeType == XmlNodeType.Text || child.NodeType == XmlNodeType.CDATA)
                {
                    // Store text content (e.g., for Script, Metadata, or field values)
                    if (!string.IsNullOrWhiteSpace(child.Value))
                        node.InnerText = (node.InnerText ?? "") + child.Value;
                }
                else if (child.NodeType == XmlNodeType.Comment)
                {
                    node.Comments.Add(child.Value);
                }
            }
            return node;
        }
    }
}
