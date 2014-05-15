using System.Collections.Generic;
using System.Configuration;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
    [ConfigurationCollection(typeof (AzureSqlDatabaseElement), AddItemName = "database")]
    internal class AzureCollection : ConfigurationElementCollection, IEnumerable<AzureSqlDatabaseElement>
    {
        public AzureSqlDatabaseElement this[int index]
        {
            get { return (AzureSqlDatabaseElement) BaseGet(index); }
        }

        internal void Add(AzureSqlDatabaseElement element)
        {
            this.BaseAdd(element);
        }

        IEnumerator<AzureSqlDatabaseElement> IEnumerable<AzureSqlDatabaseElement>.GetEnumerator()
        {
            var elements = new List<AzureSqlDatabaseElement>();
            foreach (AzureSqlDatabaseElement element in this)
            {
                elements.Add(element);
            }
            return elements.GetEnumerator();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new AzureSqlDatabaseElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AzureSqlDatabaseElement) (element)).Name;
        }
    }
}
