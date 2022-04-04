using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebservicesSage.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections;
using WebservicesSage.Object.woocommerce;
using WebservicesSage.Cotnroller;

namespace WebservicesSage.Object
{
    class Product
    {

        public List<int> website_ids { get; set; }
        public String status { get; set; }
        public Double price { get; set; }
        public int visibility { get; set; }
        public int stock_quantity { get; set; }
        public string type_id { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public string stock_status { get; set; }
        public bool is_in_stock { get; set; }
        public double stock { get; set; }
        public List<CustomAttribute> CustomAttributes { get; set; }
        public List<Attributes> attributes { get; set; }
        public List<Wholesale_quantity_discount_rule_mapping> wholesale_quantity_discount_rule_mapping { get; set; }
        public List<Categories> categories { get; set; }
        public List<AttributesVariation> attributesVariation { get; set; }
        public Product()
        {

        }
        public partial class CustomAttribute
        {
            [JsonProperty("attribute_code")]
            public string AttributeCode { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }
        }

        public partial class Categories
        {
            public int id { get; set; }
        }

        public partial class AttributesVariation
        {
            public string name { get; set; }
            public string option { get; set; }
        }

        public partial class Attributes
        {
            public string name { get; set; }
            public string type { get; set; }
            public Boolean visible { get; set; }
            public Boolean variation { get; set; }
            public List<String> options { get; set; }
        }

        public partial class Wholesale_quantity_discount_rule_mapping
        {
            public string wholesale_role { get; set; }
            public string start_qty { get; set; }
            public string price_type { get; set; }
            public double wholesale_price { get; set; }
        }

        /*public object GroupedProduct(Article article, Conditionnement conditionnement, ProductSearchCriteria productMagento = null)
        {
            if (productMagento.TotalCount > 0)
            {
                status = 2;
            }
            var product = new
            {
                product = new
                {
                    sku = conditionnement.Reference.ToString(),
                    name = article.Designation + conditionnement.Enumere,
                    attribute_set_id = int.Parse(UtilsConfig.Attribute_set_id.ToString()),
                    type_id = "grouped",
                    //status = status,
                    visibility = 4
                }
            };
            return product;
        }*/
        public object SimpleGroupedProduct(Article article, Conditionnement conditionnement)
        {

            var items = new
            {
                entity = new
                {
                    sku = conditionnement.Reference.ToString(),
                    link_type = "associated",
                    linked_product_sku = article.Reference.ToString(),
                    linked_product_type = "simple",
                    position = 0,
                    extension_attributes = new
                    {
                        qty = conditionnement.Quantity
                    }
                }
            };
            return items;
        }
        public object PrixCatTarif(Article article, PrixCatTarif prixCatTarif)
        {
            var prices = new
            {
                prices = new
                {
                    price = prixCatTarif.Price,
                    price_type = "fixed",
                    website_id = 0,
                    sku = article.Reference,
                    quantity = 1,
                    customer_groupe = prixCatTarif.CategorieTarifaire
                }
            };
            return prices;
        }
        public object PrixRemise(Article article, PrixRemise prixRemise)
        {
            if (prixRemise.reduction_type.Equals("amount"))
            {
                ArrayList price = new ArrayList();
                var prices = new
                {
                    price = prixRemise.Price,
                    price_type = "fixed",
                    website_id = 0,
                    sku = article.Reference,
                    quantity = prixRemise.Born_Sup,
                    customer_group = prixRemise.CategorieTarifaire
                };
                price.Add(prices);
                var priceRemise = new
                {
                    prices = price
                };
                return priceRemise;
            }/*
            else
            {
                var prices = new
                {
                    prices = new
                    {
                        price = prixRemise.RemisePercentage * 100,
                        price_type = "discount",
                        website_id = 0,
                        sku = article.Reference,
                        quantity = prixRemise.Born_Sup,
                        customer_groupe = prixRemise.CategorieTarifaire
                    }
                };
                return prices;
            }*/
            return null;
        }
        public object PrixRemisePercentage(Article article, PrixRemise prixRemise)
        {
            ArrayList price = new ArrayList();
            var prices = new
            {
                price = prixRemise.RemisePercentage * 100,
                price_type = "discount",
                website_id = 0,
                sku = article.Reference,
                quantity = prixRemise.Born_Sup,
                customer_group = prixRemise.CategorieTarifaire
            };
            price.Add(prices);
            var priceRemise = new
            {
                prices = price
            };
            return priceRemise;
        }

        public object SimpleConditionnementProductPricejson(Article article, Conditionnement conditionnement, ProductSearchWoocommerce productWoocommerce = null)
        {
            is_in_stock = false;
            CustomAttributes = new List<CustomAttribute>();
            attributes = new List<Attributes>();
            categories = new List<Categories>();

            website_ids = new List<int>();
            website_ids.Add(int.Parse(UtilsConfig.Store.ToString()));

            name = article.Designation;
            stock = article.Stock;
            price = article.PrixVente;
            if (stock > 0)
            {
                stock_status = "instock";
            }
            else
            {
                stock_status = "outofstock";
            }

            sku = article.Reference;

            Categories cat = new Categories();
            cat.id = int.Parse(UtilsConfig.Category);
            categories.Add(cat);

            if (article.Stock > 0)
            {
                status = "publish";
            }
            else
            {
                status = "pending";
            }

            wholesale_quantity_discount_rule_mapping = new List<Wholesale_quantity_discount_rule_mapping>();

            foreach (PrixRemise item in article.prixRemises)
            {
                if (item.RemisePercentage != 0)
                {
                    Wholesale_quantity_discount_rule_mapping prixRemise = new Wholesale_quantity_discount_rule_mapping();
                    prixRemise.wholesale_role = "customer";
                    prixRemise.start_qty = item.Born_Sup.ToString();
                    prixRemise.price_type = "percent-price";
                    prixRemise.wholesale_price = item.RemisePercentage;

                    wholesale_quantity_discount_rule_mapping.Add(prixRemise);
                }
                else
                {

                    Wholesale_quantity_discount_rule_mapping prixRemise = new Wholesale_quantity_discount_rule_mapping();
                    prixRemise.wholesale_role = "customer";
                    prixRemise.start_qty = item.Born_Sup.ToString();
                    prixRemise.price_type = "fixed-price";
                    prixRemise.wholesale_price = item.Price;

                    wholesale_quantity_discount_rule_mapping.Add(prixRemise);

                }
            }


            /*if (article.prixCatTarifs.Count > 0)
            {
                if (article.prixCatTarifs[0].Price > 0)
                {
                    price = article.prixCatTarifs[0].Price;
                }
            }
            if (!(price > 0))
            {
                price = article.PrixVente;
            }

            stock = article.Stock;
            name = article.Designation;*/

            /*if (!String.IsNullOrEmpty(article.Ecotaxe))
            {

                var value = new
                {
                    website_id = int.Parse(UtilsConfig.Store.ToString()),
                    country = "FR",
                    state = 0,
                    value = Double.Parse(article.Ecotaxe),
                    website_value = Double.Parse(article.Ecotaxe)
                };
                CustomAttribute ecotax = new CustomAttribute();
                ecotax.AttributeCode = "fpt_tax";
                ecotax.Value = value.ToString();
                CustomAttributes.Add(ecotax);
            }
            if (productWoocommerce.StockQuantity > 0)
            {
                status = 1;
            }
            else
            {
                status = 2;
            }*/
            return new
            {
                name = name + " " + conditionnement.Enumere,
                sku = sku + "|" + conditionnement.Enumere,
                regular_price = (price * int.Parse(conditionnement.Quantity)).ToString(),
                type = "simple",
                manage_stock = true,
                stock_quantity = (int)article.Stock,
                stock_status = stock_status,
                categories = categories,
                wholesale_price = new {
                    customer = price * int.Parse(conditionnement.Quantity)
                },
                wholesale_visibility_filter = "",
                wholesale_quantity_discount_rule_mapping = wholesale_quantity_discount_rule_mapping

            };

        }

        public object CategorieArticleParent(String nom, string parent)
        {
            return new
            {
                name = nom,
                slug = nom.ToLower(),
                parent = parent
            };
        }

        public object CategorieArticle(String nom,string slug)
        {
            return new
            {
                name = nom,
                slug = slug
            };
        }

        public object SimpleConditionnementProductjson(Article article, Conditionnement conditionnement, ProductSearchWoocommerce productWoocommerce = null)
        {
            is_in_stock = false;
            CustomAttributes = new List<CustomAttribute>();
            attributes = new List<Attributes>();
            categories = new List<Categories>();

            website_ids = new List<int>();
            website_ids.Add(int.Parse(UtilsConfig.Store.ToString()));

            name = article.Designation;
            stock = article.Stock;
            price = article.PrixVente;
            if (stock > 0)
            {
                stock_status = "instock";
            }
            else
            {
                stock_status = "outofstock";
            }

            sku = article.Reference;

            Categories cat = new Categories();
            cat.id = ControllerCategorie.SendCustumCategories(article.Famille, article.slugFamille);
            categories.Add(cat);

            if (article.Stock > 0)
            {
                status = "publish";
            }
            else
            {
                status = "pending";
            }


            /*if (article.prixCatTarifs.Count > 0)
            {
                if (article.prixCatTarifs[0].Price > 0)
                {
                    price = article.prixCatTarifs[0].Price;
                }
            }
            if (!(price > 0))
            {
                price = article.PrixVente;
            }

            stock = article.Stock;
            name = article.Designation;*/

            /*if (!String.IsNullOrEmpty(article.Ecotaxe))
            {

                var value = new
                {
                    website_id = int.Parse(UtilsConfig.Store.ToString()),
                    country = "FR",
                    state = 0,
                    value = Double.Parse(article.Ecotaxe),
                    website_value = Double.Parse(article.Ecotaxe)
                };
                CustomAttribute ecotax = new CustomAttribute();
                ecotax.AttributeCode = "fpt_tax";
                ecotax.Value = value.ToString();
                CustomAttributes.Add(ecotax);
            }
            if (productWoocommerce.StockQuantity > 0)
            {
                status = 1;
            }
            else
            {
                status = 2;
            }*/
            return new
            {

                name = name + " " + conditionnement.Enumere,
                sku = sku + "|" + conditionnement.Enumere,
                regular_price = (price * int.Parse(conditionnement.Quantity)).ToString(),
                type = "simple",
                manage_stock = true,
                stock_quantity = (int)article.Stock,
                stock_status = stock_status,
                categories = categories

            };

        }

        public object SimpleProductPricejson(Article article, Gamme gamme = null, string value_index = null, string value_index2 = null, ProductSearchWoocommerce productWoocommerce = null)
        {
            is_in_stock = false;

            categories = new List<Categories>();

            website_ids = new List<int>();
            website_ids.Add(int.Parse(UtilsConfig.Store.ToString()));

            name = article.Designation;
            stock = article.Stock;
            if (stock > 0)
            {
                stock_status = "instock";
            }
            else
            {
                stock_status = "outofstock";
            }

            sku = article.Reference;

            Categories cat = new Categories();
            cat.id = int.Parse(UtilsConfig.Category);
            categories.Add(cat);
            if (article.Stock > 0)
            {
                status = "publish";
            }
            else
            {
                status = "pending";
            }

            wholesale_quantity_discount_rule_mapping = new List<Wholesale_quantity_discount_rule_mapping>();

            foreach (PrixRemise item in article.prixRemises)
            {

                if (item.RemisePercentage != 0)
                {
                    Wholesale_quantity_discount_rule_mapping prixRemise = new Wholesale_quantity_discount_rule_mapping();
                    prixRemise.wholesale_role = "customer";
                    prixRemise.start_qty = item.Born_Sup.ToString();
                    prixRemise.price_type = "percent-price";
                    prixRemise.wholesale_price = item.RemisePercentage;

                    wholesale_quantity_discount_rule_mapping.Add(prixRemise);
                }
                else
                {

                    Wholesale_quantity_discount_rule_mapping prixRemise = new Wholesale_quantity_discount_rule_mapping();
                    prixRemise.wholesale_role = "customer";
                    prixRemise.start_qty = item.Born_Sup.ToString();
                    prixRemise.price_type = "fixed-price";
                    prixRemise.wholesale_price = item.Price;

                    wholesale_quantity_discount_rule_mapping.Add(prixRemise);

                }
            }


            return new
            {
                name = article.Designation,
                sku = sku,
                regular_price = (article.PrixVente).ToString(),
                manage_stock = true,
                stock_quantity = (int)article.Stock,
                stock_status = stock_status,
                type = "simple",
                categories = categories,
                wholesale_price = new {
                    customer = article.PrixVente
                },
                wholesale_visibility_filter = "",
                wholesale_quantity_discount_rule_mapping = wholesale_quantity_discount_rule_mapping
            };

        }
        public object SimpleProductjson(Article article, Gamme gamme = null, string value_index = null, string value_index2 = null, ProductSearchWoocommerce productWoocommerce = null)
        {
            is_in_stock = false;

            categories = new List<Categories>();

            website_ids = new List<int>();
            website_ids.Add(int.Parse(UtilsConfig.Store.ToString()));

            name = article.Designation;
            stock = article.Stock;
            if (stock > 0)
            {
                stock_status = "instock";
            }
            else
            {
                stock_status = "outofstock";
            }

            sku = article.Reference;

            Categories cat = new Categories();
            cat.id = ControllerCategorie.SendCustumCategories(article.Famille,article.slugFamille);
            categories.Add(cat);
            if (article.Stock > 0)
            {
                status = "publish";
            }
            else
            {
                status = "pending";
            }




            return new
            {
                name = article.Designation,
                sku = sku,
                regular_price = (article.PrixVente).ToString(),
                manage_stock = true,
                stock_quantity = (int)article.Stock,
                stock_status = stock_status,
                type = "simple",
                categories = categories,
            };

        }
        public object BundleProductjson(Article article, Gamme gamme = null, string value_index = null, string value_index2 = null, ProductSearchCriteria productMagento = null)
        {
            is_in_stock = false;

            CustomAttribute custom_attribute = new CustomAttribute();
            CustomAttribute custom_attribute1 = new CustomAttribute();
            CustomAttributes = new List<CustomAttribute>();

            website_ids = new List<int>();
            website_ids.Add(int.Parse(UtilsConfig.Store.ToString()));
            if (article.Sommeil)
            {
                //status = 2;
            }
            else
            {
                //status = 1;
            }
            if (article.Stock > 0)
            {
                is_in_stock = true;
            }
            else
            {
                is_in_stock = false;
            }
            sku = article.Reference;
            price = article.PrixVente;
            stock = article.Stock;
            name = article.Designation;
            CustomAttribute sku_type = new CustomAttribute();
            sku_type.AttributeCode = "sku_type";
            sku_type.Value = "1";
            CustomAttributes.Add(sku_type);

            CustomAttribute price_type = new CustomAttribute();
            price_type.AttributeCode = "price_type";
            price_type.Value = "0";
            CustomAttributes.Add(price_type);

            CustomAttribute price_view = new CustomAttribute();
            sku_type.AttributeCode = "price_view";
            sku_type.Value = "0";
            CustomAttributes.Add(price_view);

            if (!String.IsNullOrEmpty(article.Ecotaxe))
            {

                var value = new
                {
                    website_id = int.Parse(UtilsConfig.Store.ToString()),
                    country = "FR",
                    state = 0,
                    value = Double.Parse(article.Ecotaxe),
                    website_value = Double.Parse(article.Ecotaxe)
                };
                CustomAttribute ecotax = new CustomAttribute();
                ecotax.AttributeCode = "fpt_tax";
                ecotax.Value = value.ToString();
                CustomAttributes.Add(ecotax);
            }
            /*if (productMagento.TotalCount > 0)
            {
                status = 2;
            }*/
            var product = new
            {
                product = new
                {
                    name = name,
                    sku = sku,
                    price = price,
                    status = status,
                    attribute_set_id = int.Parse(UtilsConfig.Attribute_set_id.ToString()),
                    visibility = 4,
                    type_id = "bundle",
                    weight = article.Poid,
                    extension_attributes = new
                    {
                        website_ids = website_ids,


                        stock_item = new
                        {
                            qty = 0,
                            //min_qty get value from infolibre
                            min_qty = 1,
                            is_in_stock = true
                        }
                    },
                    custom_attributes = CustomAttributes
                    /*new
                    {
                        attribute_code = custom_attribute.AttributeCode.ToString(),
                        value = custom_attribute.Value.ToString()
                    }*/
                },
                saveOptions = true

            };
            return product;
        }

        public object ProductVariationPrice(Article article, PrixGamme gamme)
        {
            attributesVariation = new List<AttributesVariation>();
            AttributesVariation attribute1 = new AttributesVariation();
            attribute1.name = gamme.Gamme1_Intitule;
            attribute1.option = gamme.Gamme1_Value;
            attributesVariation.Add(attribute1);

            if (!String.IsNullOrEmpty(gamme.Gamme2_Intitule))
            {
                AttributesVariation attribute2 = new AttributesVariation();
                attribute2.name = gamme.Gamme2_Intitule;
                attribute2.option = gamme.Gamme2_Value;
                attributesVariation.Add(attribute2);
            }

            wholesale_quantity_discount_rule_mapping = new List<Wholesale_quantity_discount_rule_mapping>();

            
            foreach (PrixRemise item in article.prixRemises)
            {
                if(item.RemisePercentage != 0) { 
                    Wholesale_quantity_discount_rule_mapping prixRemise = new Wholesale_quantity_discount_rule_mapping();
                    prixRemise.wholesale_role = "customer";
                    prixRemise.start_qty = item.Born_Sup.ToString();
                    prixRemise.price_type = "percent-price";
                    prixRemise.wholesale_price = item.RemisePercentage;

                    wholesale_quantity_discount_rule_mapping.Add(prixRemise);
                }
                else
                {

                    Wholesale_quantity_discount_rule_mapping prixRemise = new Wholesale_quantity_discount_rule_mapping();
                    prixRemise.wholesale_role = "customer";
                    prixRemise.start_qty = item.Born_Sup.ToString();
                    prixRemise.price_type = "fixed-price";
                    prixRemise.wholesale_price = item.Price;

                    wholesale_quantity_discount_rule_mapping.Add(prixRemise);

                }
            }

            foreach (Gamme gammeStock in article.Gammes)
            {
                if (gammeStock.Value_Intitule.Equals(gamme.Gamme1_Value) && gammeStock.Value_Intitule2 == gamme.Gamme2_Value)
                {
                    if (gammeStock.Stock > 0)
                    {
                        stock_status = "instock";
                        stock = gammeStock.Stock;
                    }
                    else
                    {
                        stock_status = "outofstock";
                    }

                }
            }


            return new
            {
                manage_stock = true,
                stock_quantity = stock,
                stock_status = stock_status,
                regular_price = gamme.Price.ToString(),
                attributes = attributesVariation,
                wholesale_price = new {
                    customer = gamme.Price
                },
                wholesale_quantity_discount_rule_mapping = wholesale_quantity_discount_rule_mapping
            };

        }

        public object ProductVariation(Article article, PrixGamme gamme)
        {
            attributesVariation = new List<AttributesVariation>();
            AttributesVariation attribute1 = new AttributesVariation();
            attribute1.name = gamme.Gamme1_Intitule;
            attribute1.option = gamme.Gamme1_Value;
            attributesVariation.Add(attribute1);

            if (!String.IsNullOrEmpty(gamme.Gamme2_Intitule))
            {
                AttributesVariation attribute2 = new AttributesVariation();
                attribute2.name = gamme.Gamme2_Intitule;
                attribute2.option = gamme.Gamme2_Value;
                attributesVariation.Add(attribute2);
            }

          
            foreach(Gamme gammeStock in article.Gammes)
            {
                if(gammeStock.Value_Intitule.Equals(gamme.Gamme1_Value) && gammeStock.Value_Intitule2 == gamme.Gamme2_Value)
                {
                    if (gammeStock.Stock > 0)
                    {
                        stock_status = "instock";
                        stock = gammeStock.Stock;
                    }
                    else
                    {
                        stock_status = "outofstock";
                    }

                }
            }

            


            return new
            {
                manage_stock = true,
                stock_status = stock_status,
                stock_quantity = stock,
                regular_price = gamme.Price.ToString(),
                attributes = attributesVariation,
            };

        }

        public object ConfigurableProductjsonPrice(Article article, ProductSearchWoocommerce ProductWoocommerce = null)
        {

            is_in_stock = false;
            CustomAttributes = new List<CustomAttribute>();
            attributes = new List<Attributes>();
            categories = new List<Categories>();

            website_ids = new List<int>();
            website_ids.Add(int.Parse(UtilsConfig.Store.ToString()));

            name = article.Designation;
            stock = article.Stock;
            if (stock > 0)
            {
                stock_status = "instock";
            }
            else
            {
                stock_status = "outofstock";
            }

            sku = article.Reference;

            Categories cat = new Categories();
            cat.id = int.Parse(UtilsConfig.Category);
            categories.Add(cat);

            attributes = new List<Attributes>();

            if (!article.IsDoubleGamme)
            {

                Attributes attribute = new Attributes();
                attribute.name = article.Gammes[0].Intitule;
                attribute.type = "select";
                attribute.visible = true;
                attribute.variation = true;
                attribute.options = article.enumsGammes1;

                attributes = new List<Attributes>();
                attributes.Add(attribute);

            }
            else
            {
                Attributes attribute1 = new Attributes();
                attribute1.name = article.Gammes[0].Intitule;
                attribute1.type = "select";
                attribute1.visible = true;
                attribute1.variation = true;
                attribute1.options = article.enumsGammes1;

                Attributes attribute2 = new Attributes();
                attribute2.name = article.Gammes[0].Intitule2;
                attribute2.type = "select";
                attribute2.visible = true;
                attribute2.variation = true;
                attribute2.options = article.enumsGammes2;


                attributes = new List<Attributes>();
                attributes.Add(attribute1);
                attributes.Add(attribute2);


            }

            if (article.Stock > 0)
            {
                status = "publish";
            }
            else
            {
                status = "pending";
            }




            return new
            {

                name = name,
                sku = sku,
                regular_price = article.PrixVente.ToString(),
                manage_stock = true,
                stock_quantity = stock,
                stock_status = stock_status,
                type = "variable",
                categories = categories,
                attributes = attributes,



            };

        }


        public object ConfigurableProductjson(Article article, ProductSearchWoocommerce ProductWoocommerce = null)
        {

            is_in_stock = false;
            CustomAttributes = new List<CustomAttribute>();
            attributes = new List<Attributes>();
            categories = new List<Categories>();

            website_ids = new List<int>();
            website_ids.Add(int.Parse(UtilsConfig.Store.ToString()));

            name = article.Designation;
            stock = article.Stock;
            if (stock > 0)
            {
                stock_status = "instock";
            }
            else
            {
                stock_status = "outofstock";
            }

            sku = article.Reference;

            Categories cat = new Categories();
            cat.id = ControllerCategorie.SendCustumCategories(article.Famille,article.slugFamille);
            categories.Add(cat);

            attributes = new List<Attributes>();

            if (!article.IsDoubleGamme)
            {

                Attributes attribute = new Attributes();
                attribute.name = article.Gammes[0].Intitule;
                attribute.type = "select";
                attribute.visible = true;
                attribute.variation = true;
                attribute.options = article.enumsGammes1;

                attributes = new List<Attributes>();
                attributes.Add(attribute);

            }
            else
            {
                Attributes attribute1 = new Attributes();
                attribute1.name = article.Gammes[0].Intitule;
                attribute1.type = "select";
                attribute1.visible = true;
                attribute1.variation = true;
                attribute1.options = article.enumsGammes1;

                Attributes attribute2 = new Attributes();
                attribute2.name = article.Gammes[0].Intitule2;
                attribute2.type = "select";
                attribute2.visible = true;
                attribute2.variation = true;
                attribute2.options = article.enumsGammes2;


                attributes = new List<Attributes>();
                attributes.Add(attribute1);
                attributes.Add(attribute2);


            }

            if (article.Stock > 0)
            {
                status = "publish";
            }
            else
            {
                status = "pending";
            }


            return new
            {

                name = name,
                sku = sku,
                regular_price = article.PrixVente.ToString(),
                type = "variable",
                categories = categories,
                attributes = attributes


            };

        }
        public object CustomProductStock(Article article, Gamme gamme = null, ProductSearchCriteria productMagento = null)
        {
            if (gamme != null)
            {
                if (gamme.Reference != null)
                {
                    sku = gamme.Reference;
                }
                else
                {
                    if (article.IsDoubleGamme)
                    {
                        sku = gamme.Value_Intitule + gamme.Value_Intitule2;
                    }
                    else
                    {
                        sku = gamme.Value_Intitule;
                    }
                }
                stock = 0;
                if (article.IsDoubleGamme)
                {
                    name = article.Designation + " " + gamme.Value_Intitule + " " + gamme.Value_Intitule2;
                }
                else
                {
                    name = article.Designation + " " + gamme.Value_Intitule;
                }
                if (gamme.Sommeil)
                {
                    // status = 2;
                }
                else
                {
                    if (gamme.Stock > 0)
                    {
                        stock = gamme.Stock;
                        //status = 1;
                        is_in_stock = true;
                    }
                }

            }
            /*if (productMagento.TotalCount > 0)
            {
                status = 2;
            }*/
            var CustomProductStock = new
            {
                product = new
                {
                    name = name,
                    sku = sku,
                    status = status,
                    attribute_set_id = int.Parse(UtilsConfig.Attribute_set_id.ToString()),
                    visibility = 4,
                    type_id = "simple",
                    weight = article.Poid,
                    extension_attributes = new
                    {
                        website_ids = website_ids,

                        stock_item = new
                        {
                            qty = stock,
                            is_in_stock = true
                        }
                    }
                },
                saveOptions = true
            };
            return CustomProductStock;
        }
        public object CustomProductPrice(Article article, Gamme gamme = null, ProductSearchCriteria productMagento = null)
        {
            if (gamme != null)
            {
                if (gamme.Reference != null)
                {
                    sku = gamme.Reference;
                }
                else
                {
                    if (article.IsDoubleGamme)
                    {
                        sku = gamme.Value_Intitule + gamme.Value_Intitule2;
                    }
                    else
                    {
                        sku = gamme.Value_Intitule;
                    }
                }
                price = gamme.Price;
                //stock = gamme.Stock;
                if (article.IsDoubleGamme)
                {
                    name = article.Designation + " " + gamme.Value_Intitule + " " + gamme.Value_Intitule2;
                }
                else
                {
                    name = article.Designation + " " + gamme.Value_Intitule;
                }
                if (gamme.Sommeil)
                {
                    //status = 2;
                }
                else
                {
                    if (gamme.Stock > 0)
                    {
                        //status = 1;
                        is_in_stock = true;
                    }
                }

            }
            /*if (productMagento.TotalCount > 0)
            {
                status = 2;
            }*/
            var CustomProductPrice = new
            {
                product = new
                {
                    name = name,
                    sku = sku,
                    status = status,
                    price = price,
                    attribute_set_id = int.Parse(UtilsConfig.Attribute_set_id.ToString()),
                    visibility = 4,
                    type_id = "simple",
                    weight = article.Poid,
                    extension_attributes = new
                    {
                        website_ids = website_ids
                    }
                },
                saveOptions = true
            };
            return CustomProductPrice;
        }
    }
}
