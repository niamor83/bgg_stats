using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public static List<KeyValuePair<TKey, TValue>> AddOrUpdate<TKey, TValue>(this List<KeyValuePair<TKey, TValue>> dictionary, TKey key, TValue value)
        {
            if (dictionary.Exists(k => k.Key.Equals(key)))
            {
                //Assume that there is only on entry...
                dictionary.Remove(dictionary.Single(k => k.Key.Equals(key)));
            }
            dictionary.Add(new KeyValuePair<TKey, TValue>(key, value));

            return dictionary;
        }

        public static ObservableCollection<KeyValuePair<TKey, TValue>> AddOrUpdate<TKey, TValue>(this ObservableCollection<KeyValuePair<TKey, TValue>> dictionary, TKey key, TValue value)
        {
            //TODO : Replace the count by something like "Exist"
            if (dictionary.Count(k => k.Key.Equals(key)) > 0)
            {
                //Assume that there is only on entry...
                dictionary.Remove(dictionary.Single(k => k.Key.Equals(key)));
            }
            dictionary.Add(new KeyValuePair<TKey, TValue>(key, value));

            return dictionary;
        }
    }
}
