using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebservicesSage.Object.Categories
{
    public partial class CategorieSearchWoocommerce
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("parent")]
        public int Parent { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("display")]
        public string Display { get; set; }

        [JsonProperty("menu_order")]
        public int MenuOrder { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        

    }

    public partial class CategorieSearchWoocommerce
    {
        public static List<CategorieSearchWoocommerce> FromJson(string json) => JsonConvert.DeserializeObject<List<CategorieSearchWoocommerce>>(json, WebservicesSage.Object.Converter.Settings);

    }

    


}
