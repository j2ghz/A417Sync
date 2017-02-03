namespace A417Sync.Core.Models
{
    using System;
    using System.Xml.Serialization;

    public class File
    {
        [XmlAttribute]
        public DateTime LastChange { get; set; }
        [XmlAttribute]
        public string Path { get; set; }
    }
}