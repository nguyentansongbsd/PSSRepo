using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Action_Invoice_VNPT_UpdateInvoiceNoBySchedule.Models
{
    [XmlRoot(ElementName = "DSHDon")]
    public class DSHDon
    {
        [XmlElement(ElementName = "HDon")]
        public List<HDon> HDon { get; set; }
    }

    public class HDon
    {
        [XmlElement(ElementName = "KHMSHDon")]
        public string KHMSHDon { get; set; }

        [XmlElement(ElementName = "KHHDon")]
        public string KHHDon { get; set; }

        [XmlElement(ElementName = "SHDon")]
        public int SHDon { get; set; }

        [XmlElement(ElementName = "MCCQThue")]
        public string MCCQThue { get; set; }

        [XmlElement(ElementName = "TThai")]
        public int TThai { get; set; }

        [XmlElement(ElementName = "MTLoi")]
        public string MTLoi { get; set; }

        [XmlElement(ElementName = "Fkey")]
        public string Fkey { get; set; }
    }
}
