using System.Collections.Generic;
using System.Configuration;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
    [ConfigurationCollection(typeof(SqlServerElement), AddItemName = "database")]
    internal class DatabaseCollection : ConfigurationElementCollection, IEnumerable<DatabaseElement>
    {
        public DatabaseElement this[int index]
        {
            get { return (DatabaseElement)BaseGet(index); }
        }

        internal void Add(DatabaseElement element)
        {
            this.BaseAdd(element);
        }

        IEnumerator<DatabaseElement> IEnumerable<DatabaseElement>.GetEnumerator()
        {
            var elements = new List<DatabaseElement>();
            foreach (DatabaseElement element in this)
            {
                elements.Add(element);
            }
            return elements.GetEnumerator();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new DatabaseElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DatabaseElement)(element)).Name;
        }
    }

}