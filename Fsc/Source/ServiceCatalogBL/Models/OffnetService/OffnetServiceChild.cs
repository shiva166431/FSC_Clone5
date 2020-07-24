using ServiceCatalog.BL.Web.Offnet;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalog.BL.Models.Offnet
{
    public class OffnetServiceChild
    {

        public long Id { get; set; }
        public string Name { get; set; }
        public int MaxQuantity { get; set; }
        public int MinQuantity { get; set; }

        #region ToWeb
        public OffnetServiceChildWeb ToWeb()
        {
            return new OffnetServiceChildWeb()
            {
                Id = Id,
                Name = Name,
                MaxQuantity = MaxQuantity,
                MinQuantity = MinQuantity
            };
        }
        #endregion
    }
}
