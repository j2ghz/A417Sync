namespace A417Sync.Core.Models
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class Modpack
    {
        public List<string> Addons { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string IP { get; set; }
        [XmlAttribute]
        public int Port { get; set; }
        [XmlAttribute]
        public int Query { get; set; }
        [XmlAttribute]
        public string AdditionalParams { get; set; }
        [XmlAttribute]
        public string Password { get; set; }
    }
}