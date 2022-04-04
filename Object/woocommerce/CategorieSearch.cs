using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebservicesSage.Object.woocommerce
{
    public partial class CategorieSearch
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

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("menu_order")]
        public int MenuOrder { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }
    }

    public partial class Links
    {
        [JsonProperty("self")]
        public List<Self> Self { get; set; }

        [JsonProperty("collection")]
        public List<Collection> Collection { get; set; }
    }

    public partial class Self
    {
        [JsonProperty("href")]
        public string Href { get; set; }
    }

    public partial class Collection
    {
        [JsonProperty("href")]
        public string Href { get; set; }
    }

    public class Image
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

        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("alt")]
        public string Alt { get; set; }
    }

    public partial class CategorieSearch
    {
        public static CategorieSearch FromJson(string json) => JsonConvert.DeserializeObject<CategorieSearch>(json, WebservicesSage.Object.Converter.Settings);

    }

}
