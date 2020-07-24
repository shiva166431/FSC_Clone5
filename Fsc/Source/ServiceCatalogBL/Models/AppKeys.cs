using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalog.BL.Models
{
    internal class AppKeys
    {
        public string Secret { get; set; }
        public string Key { get; private set; }       
        public AppKeys()
        {           
            Key = "APPKEY4886520200424171449524104524";
            Secret = "wj0XuAi3OplYtSTQhh1I5hVj8fhCQZhKD6ngGqX+csLexmaT+IjGtm8wjnZglqqPrA569BU5+bgEINDcKED+wQ==";
        }
    }
}
