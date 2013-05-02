using System.Collections.Generic;
using System.Configuration;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
    [ConfigurationCollection(typeof (SqlServerElement), AddItemName = "sqlServer")]
    internal class SqlServerCollection : ConfigurationElementCollection, IEnumerable<SqlServerElement>
    {
        public SqlServerElement this[int index]
        {
            get { return (SqlServerElement) BaseGet(index); }
        }

        IEnumerator<SqlServerElement> IEnumerable<SqlServerElement>.GetEnumerator()
        {
            var elements = new List<SqlServerElement>();
            foreach (SqlServerElement element in this)
            {
                elements.Add(element);
            }
            return elements.GetEnumerator();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SqlServerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SqlServerElement) (element)).Name;
        }
    }
}