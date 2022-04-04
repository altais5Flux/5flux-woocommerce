using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebservicesSage.Object.woocommerce
{
    public class GammeEnums
    {
        public string Intitule { get; set; }
        public List<string> enums { get; set; }

        public GammeEnums(string intitule,string enums)
        {
            Intitule = intitule;
            this.enums.Add(enums);
        }
    }

}
