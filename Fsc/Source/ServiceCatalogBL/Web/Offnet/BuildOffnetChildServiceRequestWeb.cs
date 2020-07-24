using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalog.BL.Web.Offnet
{
    public class BuildOffnetChildServiceRequestWeb
    {
        public OffnetServiceKeyWeb ParentService { get; set; }
        public string Name { get; set; }
    }
}
