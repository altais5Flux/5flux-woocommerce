using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebservicesSage.Object.woocommerce
{
    class ShippingAddressWoocommerce
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }

        public ShippingAddressWoocommerce()
        {

        }
    }
}
