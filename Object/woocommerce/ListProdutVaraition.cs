using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebservicesSage.Object.woocommerce
{
    public partial class ListProdutVaraition
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("date_created")]
        public DateTime DateCreated { get; set; }

        [JsonProperty("date_created_gmt")]
        public DateTime DateCreatedGmt { get; set; }

        [JsonProperty("date_modified")]
        public DateTime DateModified { get; set; }

        [JsonProperty("date_modified_gmt")]
        public DateTime DateModifiedGmt { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("permalink")]
        public string Permalink { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("regular_price")]
        public string RegularPrice { get; set; }

        [JsonProperty("sale_price")]
        public string SalePrice { get; set; }

        [JsonProperty("date_on_sale_from")]
        public object DateOnSaleFrom { get; set; }

        [JsonProperty("date_on_sale_from_gmt")]
        public object DateOnSaleFromGmt { get; set; }

        [JsonProperty("date_on_sale_to")]
        public object DateOnSaleTo { get; set; }

        [JsonProperty("date_on_sale_to_gmt")]
        public object DateOnSaleToGmt { get; set; }


    }

    public partial class ListProdutVaraition
    {
        public static List<ListProdutVaraition> FromJson(string json) => JsonConvert.DeserializeObject<List<ListProdutVaraition>>(json, WebservicesSage.Object.CustomerSearch.Converter.Settings);
    }



}
