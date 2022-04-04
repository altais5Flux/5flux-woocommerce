using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebservicesSage.Object.woocommerce
{
    public partial class orderSearchWoocommerce
    {

        [JsonProperty("items")]
        public List<OrderItemsWoocommerce> orderItemsWoocommerce { get; set; }
    }

    public partial class OrderItemsWoocommerce
    {
        [JsonProperty("order_flag")]
        public string OrderFlag { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("parent_id")]
        public int ParentId { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("order_key")]
        public string OrderKey { get; set; }

        [JsonProperty("created_via")]
        public string CreatedVia { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("date_created")]
        public DateCreated DateCreated { get; set; }

        [JsonProperty("date_created_gmt")]
        public object DateCreatedGmt { get; set; }

        [JsonProperty("date_modified")]
        public DateModified DateModified { get; set; }

        [JsonProperty("date_modified_gmt")]
        public object DateModifiedGmt { get; set; }

        [JsonProperty("discount_total")]
        public string DiscountTotal { get; set; }

        [JsonProperty("discount_tax")]
        public string DiscountTax { get; set; }

        [JsonProperty("shipping_total")]
        public string ShippingTotal { get; set; }

        [JsonProperty("shipping_tax")]
        public string ShippingTax { get; set; }

        [JsonProperty("cart_tax")]
        public string CartTax { get; set; }

        [JsonProperty("total")]
        public string Total { get; set; }

        [JsonProperty("total_tax")]
        public string TotalTax { get; set; }

        [JsonProperty("prices_include_tax")]
        public bool PricesIncludeTax { get; set; }

        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }

        [JsonProperty("customer_ip_address")]
        public string CustomerIpAddress { get; set; }

        [JsonProperty("customer_user_agent")]
        public string CustomerUserAgent { get; set; }

        [JsonProperty("customer_note")]
        public string CustomerNote { get; set; }

        [JsonProperty("billing")]
        public Billing Billing { get; set; }

        [JsonProperty("shipping")]
        public Shipping Shipping { get; set; }

        [JsonProperty("payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty("payment_method_title")]
        public string PaymentMethodTitle { get; set; }

        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty("date_paid")]
        public object DatePaid { get; set; }

        [JsonProperty("date_paid_gmt")]
        public object DatePaidGmt { get; set; }

        [JsonProperty("date_completed")]
        public object DateCompleted { get; set; }

        [JsonProperty("date_completed_gmt")]
        public object DateCompletedGmt { get; set; }

        [JsonProperty("cart_hash")]
        public string CartHash { get; set; }

        [JsonProperty("line_items")]
        public List<LineItem> LineItems { get; set; }
    }

    public partial class DateCreated
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("timezone_type")]
        public int TimezoneType { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }
    }

    public partial class DateModified
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("timezone_type")]
        public int TimezoneType { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }
    }

    public partial class  Billing
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("address_1")]
        public string Address1 { get; set; }

        [JsonProperty("address_2")]
        public string Address2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("postcode")]
        public string Postcode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }
    }

    public partial class Shipping
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("address_1")]
        public string Address1 { get; set; }

        [JsonProperty("address_2")]
        public string Address2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("postcode")]
        public string Postcode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }
    }

    public partial class Taxes
    {
        [JsonProperty("total")]
        public List<object> Total { get; set; }

        [JsonProperty("subtotal")]
        public List<object> Subtotal { get; set; }
    }

    public partial class MetaData
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("display_key")]
        public object DisplayKey { get; set; }

        [JsonProperty("display_value")]
        public object DisplayValue { get; set; }
    }

    public partial class LineItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("variation_id")]
        public int VariationId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("tax_class")]
        public string TaxClass { get; set; }

        [JsonProperty("subtotal")]
        public string Subtotal { get; set; }

        [JsonProperty("subtotal_tax")]
        public string SubtotalTax { get; set; }

        [JsonProperty("total")]
        public string Total { get; set; }

        [JsonProperty("total_tax")]
        public string TotalTax { get; set; }

        [JsonProperty("taxes")]
        public Taxes Taxes { get; set; }

        [JsonProperty("meta_data")]
        public List<MetaData> MetaData { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }
    }

    public partial class orderSearchWoocommerce
    {
        public static List<orderSearchWoocommerce> FromJson(string json) => JsonConvert.DeserializeObject<List<orderSearchWoocommerce>>(json, WebservicesSage.Object.Converter.Settings);

    }

}
