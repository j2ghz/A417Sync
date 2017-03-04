namespace A417Sync.Core.Models
{
    using System;
    using System.Xml.Serialization;

    public class File
    {
        [XmlAttribute]
        public long LastChange { get; set; }

        [XmlAttribute]
        public string Path { get; set; }

        [XmlAttribute]
        public long Size { get; set; }
    }
}