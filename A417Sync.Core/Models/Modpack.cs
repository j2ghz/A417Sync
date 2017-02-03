namespace A417Sync.Core.Models
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class Modpack
    {
        public List<string> Addons { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
    }
}