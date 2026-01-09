using System.Collections.Generic;

namespace X3D
{
    public class X3DNode
    {
        public string Name;
        public Dictionary<string, string> Attributes = new Dictionary<string, string>();
        public List<X3DNode> Children = new List<X3DNode>();
        public string InnerText = null; // For text nodes
        public List<string> Comments = new List<string>(); // For comments

        public X3DNode(string name)
        {
            Name = name;
        }
    }
}
