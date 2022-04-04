using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebservicesSage.Object.Categories
{
    public partial class CreatedCategorie
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

        
    }

    public partial class CreatedCategorie
    {
        public static CreatedCategorie FromJson(string json) => JsonConvert.DeserializeObject<CreatedCategorie>(json, WebservicesSage.Object.Converter.Settings);

    }
}
