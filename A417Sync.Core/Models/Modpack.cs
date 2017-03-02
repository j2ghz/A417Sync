namespace A417Sync.Core.Models
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class Modpack
    {
        [XmlAttribute]
        public string AdditionalParams { get; set; } = string.Empty;

        public List<string> Addons { get; set; }

        [XmlAttribute]
        public string IP { get; set; } = string.Empty;

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Password { get; set; } = string.Empty;

        [XmlAttribute]
        public int Port { get; set; }

        [XmlAttribute]
        public int Query { get; set; }
    }
}