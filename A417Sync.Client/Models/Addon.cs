namespace A417Sync.Core.Models
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class Addon
    {
        public List<File> Files { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
    }
}