using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BGGStats.Helper
{
    public static class Extensions
    {
        public static string TextAttribute(this XmlNode node, string attribute)
        {
            return node.Attributes[attribute] != null ? node.Attributes[attribute].InnerText.Trim() : null;
        }
    }
}
