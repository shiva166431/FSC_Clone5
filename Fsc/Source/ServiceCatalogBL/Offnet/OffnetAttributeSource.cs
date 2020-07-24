using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalog.BL.Offnet
{
    public class OffnetAttributeSource
    {
        public long OffnetServiceId { get; set; }
        public long PartnerOrderId { get; set; }
        public int Version { get; set; }
        public string AttributeName { get; set; }
        public string RequestSource { get; set; }
        public string AttributeSource { get; set; }
        public string SolutionTarget { get; set; }

    }
}
