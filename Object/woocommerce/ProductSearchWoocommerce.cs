using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebservicesSage.Object.woocommerce
{
    public partial class ProductSearchWoocommerce
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("featured")]
        public bool Featured { get; set; }

        [JsonProperty("catalog_visibility")]
        public string CatalogVisibility { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("short_description")]
        public string ShortDescription { get; set; }

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

        [JsonProperty("date_on_sale_to")]
        public object DateOnSaleTo { get; set; }

        [JsonProperty("total_sales")]
        public int TotalSales { get; set; }

        [JsonProperty("tax_status")]
        public string TaxStatus { get; set; }

        [JsonProperty("tax_class")]
        public string TaxClass { get; set; }

        [JsonProperty("backorders")]
        public string Backorders { get; set; }

        [JsonProperty("sold_individually")]
        public bool SoldIndividually { get; set; }

        [JsonProperty("weight")]
        public string Weight { get; set; }

        [JsonProperty("length")]
        public string Length { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("parent_id")]
        public int ParentId { get; set; }

        [JsonProperty("reviews_allowed")]
        public bool ReviewsAllowed { get; set; }

        [JsonProperty("purchase_note")]
        public string PurchaseNote { get; set; }

        [JsonProperty("menu_order")]
        public int MenuOrder { get; set; }

        [JsonProperty("post_password")]
        public string PostPassword { get; set; }

        [JsonProperty("virtual")]
        public bool Virtual { get; set; }

        [JsonProperty("downloadable")]
        public bool Downloadable { get; set; }

        [JsonProperty("shipping_class_id")]
        public int ShippingClassId { get; set; }

        [JsonProperty("image_id")]
        public string ImageId { get; set; }

        [JsonProperty("download_limit")]
        public int DownloadLimit { get; set; }

        [JsonProperty("download_expiry")]
        public int DownloadExpiry { get; set; }

        [JsonProperty("average_rating")]
        public string AverageRating { get; set; }

        [JsonProperty("review_count")]
        public int ReviewCount { get; set; }
    }

    public partial class ProductSearchWoocommerce
    {
        public static ProductSearchWoocommerce FromJson(string json) => JsonConvert.DeserializeObject<ProductSearchWoocommerce>(json, WebservicesSage.Object.Converter.Settings);

    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                ValueConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
