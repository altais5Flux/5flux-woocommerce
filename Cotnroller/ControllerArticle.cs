using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebservicesSage.Singleton;
using WebservicesSage.Object;
using WebservicesSage.Object.Search;
using Objets100cLib;
using System.Windows.Forms;
using WebservicesSage.Utils;
using WebservicesSage.Utils.Enums;
using System.Timers;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Attribute = WebservicesSage.Object.Attribute.Attribute;
using WebservicesSage.Object.woocommerce;
using System.Threading;

namespace WebservicesSage.Cotnroller
{
    class ControllerArticle
    {

        public static void LaunchService()
        {/*
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(SendStockCrone);
            timer.Interval = UtilsConfig.CronTaskStock;
            timer.Enabled = true;*/
            System.Timers.Timer timerUpdateStatut = new System.Timers.Timer();
            timerUpdateStatut.Elapsed += new ElapsedEventHandler(SendAllProductsCron);
            timerUpdateStatut.Interval = 200000;
            timerUpdateStatut.Enabled = true;
        }
        public static void SendAllProductsCron(object source, ElapsedEventArgs e)
        {
            
               
                var gescom = SingletonConnection.Instance.Gescom;
                string RefErr = "";
                string JsonErr = "";
                try
                {
                    foreach (IBOArticle3 articleSageObj in gescom.FactoryArticle.List)
                    {
                        if (!articleSageObj.AR_Publie)
                        {
                            continue;
                        }
                        else
                        {
                            RefErr = articleSageObj.AR_Ref;
                            try
                            {

                                Product product = new Product();
                                Article article;
                                article = new Article(articleSageObj);
                                ProductSearchWoocommerce productWoocommerce = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference);

                                string articleXML = UtilsSerialize.SerializeObject<Article>(article);
                                if (article.conditionnements.Count > 0)
                                {
                                    foreach (Conditionnement item in article.conditionnements)
                                    {
                                        ProductSearchWoocommerce productWoocommerceConditionnement = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference + "|" + item.Enumere);
                                        var jsonC = JsonConvert.SerializeObject(product.SimpleConditionnementProductjson(article, item, productWoocommerce));
                                    File.AppendAllText("Log\\data.txt", jsonC.ToString() + Environment.NewLine);

                                    if (productWoocommerceConditionnement == null)
                                        {
                                            string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wc/v3/products");
                                        }
                                        else
                                        {
                                            string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wc/v3/products" + productWoocommerceConditionnement.Id.ToString(), "PUT");
                                        }
                                    }

                                }
                                else
                                {
                                    if (article.isGamme)
                                    {
                                        var json = JsonConvert.SerializeObject(product.ConfigurableProductjson(article, productWoocommerce));
                                        File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                        if (productWoocommerce == null)
                                        {
                                            string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products");
                                            ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                            foreach (PrixGamme gamme in article.prixGammes)
                                            {
                                                string jsonVariation = JsonConvert.SerializeObject(product.ProductVariation(article, gamme));
                                                File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                                string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "wp-json/wc/v3/products/" + productCreated.Id.ToString() + "/variations");
                                            }
                                        }
                                        else
                                        {
                                            string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products/" + productWoocommerce.Id.ToString(), "PUT");
                                            ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                            foreach (long id in productCreated.Variations)
                                            {
                                                string deleteVariations = UtilsWebservices.GetWoocommerceData("wp-json/wc/v3/products/" + productWoocommerce.Id.ToString() + "/variations/" + id + "?force=true", "DELETE");
                                            }

                                            foreach (PrixGamme gamme in article.prixGammes)
                                            {
                                                string jsonVariation = JsonConvert.SerializeObject(product.ProductVariation(article, gamme));
                                                File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                                string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "wp-json/wc/v3/products/" + productCreated.Id.ToString() + "/variations");
                                            }
                                        }


                                    }
                                    else
                                    {
                                        var json = JsonConvert.SerializeObject(product.SimpleProductjson(article, null, null, null, productWoocommerce));
                                        File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                        if (productWoocommerce == null)
                                        {
                                            string response = UtilsWebservices.SendDataJson(json, "/wp-json/wc/v3/products");
                                        }
                                        else
                                        {
                                            string responseC = UtilsWebservices.SendDataJson(json, "/wp-json/wc/v3/products/" + productWoocommerce.Id.ToString(), "PUT");
                                        }



                                    }

                                }
                            }
                            catch (Exception s)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append(DateTime.Now + RefErr + Environment.NewLine);
                                sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                                sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                                File.AppendAllText("Log\\SyncAll.txt", sb.ToString());
                                sb.Clear();
                            }
                        }
                        // SingletonUI.Instance.ProgressBar.Invoke((MethodInvoker)(() => SingletonUI.Instance.ProgressBar.Value = 100));

                    }
                    MessageBox.Show("Synchronisation terminée", "end",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }
            
        }
        /// <summary>
        /// Permets de remonter toute la base articles de SAGE vers Prestashop
        /// Ne remonte que les articles coché en publier sur le site marchand !
        /// </summary>
        public static Task SendAllArticles(IProgress<ProgressReport> progress)
        {
            return Task.Run(() =>
            {
                
                string RefErr = "";
                string JsonErr = "";
                double percentComplete = 0;
                try
                {
                        var gescom = SingletonConnection.Instance.Gescom;
                        int index = 1;
                        int totalProcess = gescom.FactoryArticle.List.Count;
                        var progressReport = new ProgressReport();
                        foreach (IBOArticle3 articleSageObj in gescom.FactoryArticle.List)
                    {
                        Product product = new Product();
                        Article article;
                        if (!articleSageObj.AR_Publie)
                        {
                                progressReport.PercentComplete = index++ * 100 / totalProcess;
                                progress.Report(progressReport);
                                Thread.Sleep(10);
                                continue;
                        }
                        else
                        {
                            try
                            {


                                article = new Article(articleSageObj);
                                ProductSearchWoocommerce productWoocommerce = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference);

                                string articleXML = UtilsSerialize.SerializeObject<Article>(article);
                                if (article.conditionnements.Count > 0)
                                {
                                    foreach (Conditionnement item in article.conditionnements)
                                    {
                                        ProductSearchWoocommerce productWoocommerceConditionnement = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference + "|" + item.Enumere);
                                        var jsonC = JsonConvert.SerializeObject(product.SimpleConditionnementProductjson(article, item, productWoocommerce));
                                            File.AppendAllText("Log\\data.txt", jsonC.ToString() + Environment.NewLine);
                                            if (productWoocommerceConditionnement == null)
                                        {
                                            string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wc/v3/products");
                                        }
                                        else
                                        {
                                            string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wc/v3/products" + productWoocommerceConditionnement.Id.ToString(), "PUT");
                                        }
                                    }

                                }
                                else
                                {
                                    if (article.isGamme)
                                    {
                                       var json = JsonConvert.SerializeObject(product.ConfigurableProductjson(article, productWoocommerce));
                                    File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                    if (productWoocommerce == null)
                                    {
                                        string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products");
                                        ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                        foreach (PrixGamme gamme in article.prixGammes)
                                        {
                                            string jsonVariation = JsonConvert.SerializeObject(product.ProductVariation(article, gamme));
                                            File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                            string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "wp-json/wc/v3/products/" + productCreated.Id.ToString() + "/variations");
                                        }
                                    }
                                    else
                                    {
                                        string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products/" + productWoocommerce.Id.ToString(), "PUT");
                                        ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                        foreach(long id in productCreated.Variations)
                                        {
                                           string deleteVariations  = UtilsWebservices.GetWoocommerceData("wp-json/wc/v3/products/" + productWoocommerce.Id.ToString() + "/variations/"+id+ "?force=true", "DELETE");
                                        }

                                        foreach (PrixGamme gamme in article.prixGammes)
                                        {
                                            string jsonVariation = JsonConvert.SerializeObject(product.ProductVariation(article, gamme));
                                            File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                            string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "wp-json/wc/v3/products/" + productCreated.Id.ToString() + "/variations");
                                        }
                                    }


                                    }
                                    else
                                    {
                                        var json = JsonConvert.SerializeObject(product.SimpleProductjson(article, null, null, null, productWoocommerce));
                                        var fileName = @"Log\\data.txt";
                                        FileInfo fi = new FileInfo(fileName);
                                        var size = fi.Length;
                                        File.Create("Log\\data.txt").Close();
                                        File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                        if (productWoocommerce == null)
                                        {
                                            string response = UtilsWebservices.SendDataJson(json, "/wp-json/wc/v3/products");
                                        }
                                        else
                                        {
                                            string responseC = UtilsWebservices.SendDataJson(json, "/wp-json/wc/v3/products/" + productWoocommerce.Id.ToString(), "PUT");
                                        }



                                    }

                                }
                            }
                            catch (Exception s)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append(DateTime.Now + RefErr + Environment.NewLine);
                                sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                                sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                                File.AppendAllText("Log\\SyncAll.txt", sb.ToString());
                                sb.Clear();
                            }
                        }
                            // SingletonUI.Instance.ProgressBar.Invoke((MethodInvoker)(() => SingletonUI.Instance.ProgressBar.Value = 100));
                            progressReport.PercentComplete = index++ * 100 / totalProcess;
                            progress.Report(progressReport);
                            Thread.Sleep(10);
                        }
                    MessageBox.Show("Synchronisation terminée", "end",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                }
                
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }
            });
        }



        public static void SendCustomArticles(string reference)
        {
            try
            {
                List<ArticleNomenclature> ArticleNomenclature = new List<ArticleNomenclature>();
                var gescom = SingletonConnection.Instance.Gescom;
                Product product = new Product();
                Article article;
                string RefErr = "";

                var articleSageObj = gescom.FactoryArticle.ReadReference(reference);
                //var articletest = articleSageObj;

                if (!articleSageObj.AR_Publie)
                {
                    MessageBox.Show("Article non publie sur le site", "end",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                }
                else
                {
                    RefErr = articleSageObj.AR_Ref;
                    try
                    {


                        article = new Article(articleSageObj);
                        ProductSearchWoocommerce productWoocommerce = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference);

                        string articleXML = UtilsSerialize.SerializeObject<Article>(article);
                        if (article.conditionnements.Count > 0)
                        {
                            foreach (Conditionnement item in article.conditionnements)
                            {
                                ProductSearchWoocommerce productWoocommerceConditionnement = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference + "|" + item.Enumere);
                                var jsonC = JsonConvert.SerializeObject(product.SimpleConditionnementProductjson(article, item, productWoocommerce));
                                File.AppendAllText("Log\\data.txt", jsonC.ToString() + Environment.NewLine);
                                if (productWoocommerceConditionnement == null)
                                {
                                    string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wc/v3/products");
                                }
                                else
                                {
                                    string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wc/v3/products" + productWoocommerceConditionnement.Id.ToString(), "PUT");
                                }
                            }

                        }
                        else
                        {
                            if (article.isGamme)
                            {
                                var json = JsonConvert.SerializeObject(product.ConfigurableProductjson(article, productWoocommerce));
                                File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                if (productWoocommerce == null)
                                {
                                    string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products");
                                    ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                    foreach (PrixGamme gamme in article.prixGammes)
                                    {
                                        string jsonVariation = JsonConvert.SerializeObject(product.ProductVariation(article, gamme));
                                        File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                        string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "wp-json/wc/v3/products/" + productCreated.Id.ToString() + "/variations");
                                    }
                                }
                                else
                                {
                                    string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products/" + productWoocommerce.Id.ToString(), "PUT");
                                    ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                    foreach(long id in productCreated.Variations)
                                    {
                                       string deleteVariations  = UtilsWebservices.GetWoocommerceData("wp-json/wc/v3/products/" + productWoocommerce.Id.ToString() + "/variations/"+id+ "?force=true", "DELETE");
                                    }

                                    foreach (PrixGamme gamme in article.prixGammes)
                                    {
                                        string jsonVariation = JsonConvert.SerializeObject(product.ProductVariation(article, gamme));
                                        File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                        string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "wp-json/wc/v3/products/" + productCreated.Id.ToString() + "/variations");
                                    }
                                }

                            }
                            else
                            {
                                var json = JsonConvert.SerializeObject(product.SimpleProductjson(article, null, null, null, productWoocommerce));
                                File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                if (productWoocommerce == null)
                                {
                                    string response = UtilsWebservices.SendDataJson(json, "/wp-json/wc/v3/products");
                                }
                                else
                                {
                                    string responseC = UtilsWebservices.SendDataJson(json, "/wp-json/wc/v3/products/" + productWoocommerce.Id.ToString(), "PUT");
                                }
                            }

                        }
                        MessageBox.Show("Synchronisation terminée", "end",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                    }
                    catch (Exception s)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(DateTime.Now + RefErr + Environment.NewLine);
                        sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                        sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                        File.AppendAllText("Log\\SyncAll.txt", sb.ToString());
                        sb.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "error",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Error);
            }

        }

        /// <summary>
        /// Permets de récupérer une liste d'articles propre depuis une liste d'artivcle SAGE
        /// Permets de gérer la configuration des produits
        /// </summary>
        /// <param name="articleSageObj">Liste d'article SAGE</param>
        /// <returns></returns>
        public static List<Article> GetListOfProductToProcess(IBICollection articleSageObj)
        {
            List<Article> articleToProcess = new List<Article>();
            string CurrentRefArticle = "";

            int incre = 0;
            foreach (IBOArticle3 articleSage in articleSageObj)
            {
                CurrentRefArticle = articleSage.AR_Ref;
                try
                {
                    //SingletonUI.Instance.ArticleNumber.Invoke((MethodInvoker)(() => SingletonUI.Instance.ArticleNumber.Text = "Fetching Data : " + incre));

                    // on check si l'article est cocher en publier sur le site marchand
                    if (!articleSage.AR_Publie)
                        continue;

                    Article article = new Article(articleSage);

                    if (!HandleArticleError(article))
                    {
                        articleToProcess.Add(article);
                    }
                }
                catch (Exception e)
                {
                    UtilsMail.SendErrorMail(DateTime.Now + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine, "ARTICLE " + CurrentRefArticle);
                }
                incre++;
            }
            return articleToProcess;
        }

        /// <summary>
        /// Permet de vérifier si un article comporte des erreur ou non
        /// </summary>
        /// <param name="article">Article à tester</param>
        /// <returns></returns>
        private static bool HandleArticleError(Article article)
        {

            return false;
        }

        /// <summary>
        /// Permet de récupérer l'énuméré SAGE 1 d'un article 
        /// </summary>
        /// <param name="article"></param>
        /// <param name="gamme">Gamme sur laquelle nous devont chercher l'énuméré</param>
        /// <returns></returns>
        public static IBOArticleGammeEnum3 GetArticleGammeEnum1(IBOArticle3 article, LineItem product)
        {
            foreach (IBOArticleGammeEnum3 articleEnum in article.FactoryArticleGammeEnum1.List)
            {
                if (articleEnum.EG_Enumere.Equals(product.MetaData[0].Value))
                {
                    return articleEnum;
                }
            }

            return null;
        }
        public static IBOArticleCond3 GetArticleConditionnementEnum(IBOArticle3 article)
        {
            foreach (IBOArticleCond3 articleEnum in article.FactoryArticleCond.List)
            {
                if (!String.IsNullOrEmpty(articleEnum.EC_Enumere))
                {
                    return articleEnum;
                }
            }

            return null;
        }

        /// <summary>
        /// Permet de récupérer l'énuméré SAGE 2 d'un article 
        /// </summary>
        /// <param name="article"></param>
        /// <param name="gamme">Gamme sur laquelle nous devont chercher l'énuméré</param>
        /// <returns></returns>
        public static IBOArticleGammeEnum3 GetArticleGammeEnum2(IBOArticle3 article, LineItem product)
        {
            foreach (IBOArticleGammeEnum3 articleEnum in article.FactoryArticleGammeEnum2.List)
            {

                if (articleEnum.EG_Enumere.Equals(product.MetaData[1].Value))
                {
                    return articleEnum;
                }


            }

            return null;
        }

        public static void SendStockCrone(object source, ElapsedEventArgs e)
        {
            var gescom = SingletonConnection.Instance.Gescom;
            string RefErr = "";
            string JsonErr = "";
            try
            {
                foreach (IBOArticle3 articleSageObj in gescom.FactoryArticle.List)
                {
                    Product product = new Product();
                    Article article;
                    if (!articleSageObj.AR_Publie)
                    {
                        continue;
                    }
                    else
                    {
                        try
                        {


                            article = new Article(articleSageObj);
                            ProductSearchWoocommerce productWoocommerce = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference);

                            string articleXML = UtilsSerialize.SerializeObject<Article>(article);
                            if (article.conditionnements.Count > 0)
                            {
                                foreach (Conditionnement item in article.conditionnements)
                                {
                                    ProductSearchWoocommerce productWoocommerceConditionnement = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference + "|" + item.Enumere);
                                    var jsonC = JsonConvert.SerializeObject(product.SimpleConditionnementProductjson(article, item, productWoocommerce));
                                    if (productWoocommerceConditionnement == null)
                                    {
                                        string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wc/v3/products");
                                    }
                                    else
                                    {
                                        string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wc/v3/products" + productWoocommerceConditionnement.Id.ToString(), "PUT");
                                    }
                                }

                            }
                            else
                            {
                                if (article.isGamme)
                                {
                                    var json = JsonConvert.SerializeObject(product.ConfigurableProductjson(article, productWoocommerce));
                                    File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                    if (productWoocommerce == null)
                                    {
                                        string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products");
                                        ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                        foreach (PrixGamme gamme in article.prixGammes)
                                        {
                                            string jsonVariation = JsonConvert.SerializeObject(product.ProductVariation(article, gamme));
                                            File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                            string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "wp-json/wc/v3/products/" + productCreated.Id.ToString() + "/variations");
                                        }
                                    }
                                    else
                                    {
                                        string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products/" + productWoocommerce.Id.ToString(), "PUT");
                                        ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                        foreach (long id in productCreated.Variations)
                                        {
                                            string deleteVariations = UtilsWebservices.GetWoocommerceData("wp-json/wc/v3/products/" + productWoocommerce.Id.ToString() + "/variations/" + id + "?force=true", "DELETE");
                                        }

                                        foreach (PrixGamme gamme in article.prixGammes)
                                        {
                                            string jsonVariation = JsonConvert.SerializeObject(product.ProductVariation(article, gamme));
                                            File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                            string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "wp-json/wc/v3/products/" + productCreated.Id.ToString() + "/variations");
                                        }
                                    }

                                }
                                else
                                {
                                    var json = JsonConvert.SerializeObject(product.SimpleProductjson(article, null, null, null, productWoocommerce));
                                    File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                    if (productWoocommerce == null)
                                    {
                                        string response = UtilsWebservices.SendDataJson(json, "/wp-json/wc/v3/products");
                                    }
                                    else
                                    {
                                        string responseC = UtilsWebservices.SendDataJson(json, "/wp-json/wc/v3/products/" + productWoocommerce.Id.ToString(), "PUT");
                                    }



                                }

                            }
                        }
                        catch (Exception s)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append(DateTime.Now + RefErr + Environment.NewLine);
                            sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                            sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                            File.AppendAllText("Log\\SyncAll.txt", sb.ToString());
                            sb.Clear();
                        }
                    }
                    // SingletonUI.Instance.ProgressBar.Invoke((MethodInvoker)(() => SingletonUI.Instance.ProgressBar.Value = 100));

                }
            
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        public static void SendStock()
        {
            var gescom = SingletonConnection.Instance.Gescom;
            string RefErr = "";
            string JsonErr = "";
            try
            {
                foreach (IBOArticle3 articleSageObj in gescom.FactoryArticle.List)
                {
                    Product product = new Product();
                    Article article;
                    if (!articleSageObj.AR_Publie)
                    {
                        continue;
                    }
                    else
                    {
                        try
                        {


                            article = new Article(articleSageObj);
                            ProductSearchWoocommerce productWoocommerce = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference);

                            string articleXML = UtilsSerialize.SerializeObject<Article>(article);
                            if (article.conditionnements.Count > 0)
                            {
                                foreach (Conditionnement item in article.conditionnements)
                                {
                                    ProductSearchWoocommerce productWoocommerceConditionnement = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference + "|" + item.Enumere);
                                    var jsonC = JsonConvert.SerializeObject(product.SimpleConditionnementProductjson(article, item, productWoocommerce));
                                    File.AppendAllText("Log\\data.txt", jsonC.ToString() + Environment.NewLine);
                                    if (productWoocommerceConditionnement == null)
                                    {
                                        string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wc/v3/products");
                                    }
                                    else
                                    {
                                        string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wc/v3/products" + productWoocommerceConditionnement.Id.ToString(), "PUT");
                                    }
                                }

                            }
                            else
                            {
                                if (article.isGamme)
                                {
                                    var json = JsonConvert.SerializeObject(product.ConfigurableProductjson(article, productWoocommerce));
                                    File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                    if (productWoocommerce == null)
                                    {
                                        string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products");
                                        ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                        foreach (PrixGamme gamme in article.prixGammes)
                                        {
                                            string jsonVariation = JsonConvert.SerializeObject(product.ProductVariation(article, gamme));
                                            File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                            string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "wp-json/wc/v3/products/" + productCreated.Id.ToString() + "/variations");
                                        }
                                    }
                                    else
                                    {
                                        string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products/" + productWoocommerce.Id.ToString(), "PUT");
                                        ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                        foreach (long id in productCreated.Variations)
                                        {
                                            string deleteVariations = UtilsWebservices.GetWoocommerceData("wp-json/wc/v3/products/" + productWoocommerce.Id.ToString() + "/variations/" + id + "?force=true", "DELETE");
                                        }

                                        foreach (PrixGamme gamme in article.prixGammes)
                                        {
                                            string jsonVariation = JsonConvert.SerializeObject(product.ProductVariation(article, gamme));
                                            File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                            string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "wp-json/wc/v3/products/" + productCreated.Id.ToString() + "/variations");
                                        }
                                    }

                                }
                                else
                                {
                                    var json = JsonConvert.SerializeObject(product.SimpleProductjson(article, null, null, null, productWoocommerce));
                                    File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                    if (productWoocommerce == null)
                                    {
                                        string response = UtilsWebservices.SendDataJson(json, "/wp-json/wc/v3/products");
                                    }
                                    else
                                    {
                                        string responseC = UtilsWebservices.SendDataJson(json, "/wp-json/wc/v3/products/" + productWoocommerce.Id.ToString(), "PUT");
                                    }



                                }

                            }
                        }
                        catch (Exception s)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append(DateTime.Now + RefErr + Environment.NewLine);
                            sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                            sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                            File.AppendAllText("Log\\SyncAll.txt", sb.ToString());
                            sb.Clear();
                        }
                    }
                    // SingletonUI.Instance.ProgressBar.Invoke((MethodInvoker)(() => SingletonUI.Instance.ProgressBar.Value = 100));

                }
                MessageBox.Show("Synchronisation terminée", "end",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        public static void SendCustomStock(string reference)
        {
            try
            {
                List<ArticleNomenclature> ArticleNomenclature = new List<ArticleNomenclature>();
                var gescom = SingletonConnection.Instance.Gescom;
                Product product = new Product();
                Article article;
                string RefErr = "";

                var articleSageObj = gescom.FactoryArticle.ReadReference(reference);
                //var articletest = articleSageObj;

                if (!articleSageObj.AR_Publie)
                {
                    MessageBox.Show("Article non publie sur le site", "end",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                }
                else
                {
                    RefErr = articleSageObj.AR_Ref;
                    try
                    {


                        article = new Article(articleSageObj);
                        ProductSearchWoocommerce productWoocommerce = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference);

                        string articleXML = UtilsSerialize.SerializeObject<Article>(article);
                        if (article.conditionnements.Count > 0)
                        {
                            foreach (Conditionnement item in article.conditionnements)
                            {
                                ProductSearchWoocommerce productWoocommerceConditionnement = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference + "|" + item.Enumere);
                                var jsonC = JsonConvert.SerializeObject(product.SimpleConditionnementProductjson(article, item, productWoocommerce));
                                File.AppendAllText("Log\\data.txt", jsonC.ToString() + Environment.NewLine);
                                if (productWoocommerceConditionnement == null)
                                {
                                    string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wc/v3/products");
                                }
                                else
                                {
                                    string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wc/v3/products" + productWoocommerceConditionnement.Id.ToString(), "PUT");
                                }
                            }
                        }
                        else
                        {
                            if (article.isGamme)
                            {
                                var json = JsonConvert.SerializeObject(product.ConfigurableProductjson(article, productWoocommerce));
                                File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                if (productWoocommerce == null)
                                {
                                    string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products");
                                    ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                    foreach (PrixGamme gamme in article.prixGammes)
                                    {
                                        string jsonVariation = JsonConvert.SerializeObject(product.ProductVariation(article, gamme));
                                        File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                        string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "wp-json/wc/v3/products/" + productCreated.Id.ToString() + "/variations");
                                    }
                                }
                                else
                                {
                                    string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products/" + productWoocommerce.Id.ToString(), "PUT");
                                    ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                    foreach (long id in productCreated.Variations)
                                    {
                                        string deleteVariations = UtilsWebservices.GetWoocommerceData("wp-json/wc/v3/products/" + productWoocommerce.Id.ToString() + "/variations/" + id + "?force=true", "DELETE");
                                    }

                                    foreach (PrixGamme gamme in article.prixGammes)
                                    {
                                        string jsonVariation = JsonConvert.SerializeObject(product.ProductVariation(article, gamme));
                                        File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                        string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "wp-json/wc/v3/products/" + productCreated.Id.ToString() + "/variations");
                                    }
                                }

                            }
                            else
                            {
                                var json = JsonConvert.SerializeObject(product.SimpleProductjson(article, null, null, null, productWoocommerce));
                                File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                if (productWoocommerce == null)
                                {
                                    string response = UtilsWebservices.SendDataJson(json, "/wp-json/wc/v3/products");
                                }
                                else
                                {
                                    string responseC = UtilsWebservices.SendDataJson(json, "/wp-json/wc/v3/products/" + productWoocommerce.Id.ToString(), "PUT");
                                }

                            }

                        }
                        MessageBox.Show("Synchronisation terminée", "end",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                    }
                    catch (Exception s)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(DateTime.Now + RefErr + Environment.NewLine);
                        sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                        sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                        File.AppendAllText("Log\\SyncAll.txt", sb.ToString());
                        sb.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "error",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Error);
            }

        }

        public static Task SendPrice(IProgress<ProgressReport> progress)
        {
            return Task.Run(() =>
            {
                try
            {
                var gescom = SingletonConnection.Instance.Gescom;
                var articleSageObj = gescom.FactoryArticle.List;
                var articles = GetListOfProductToProcess(articleSageObj);

                int increm = 1;
                int tmpiter = articles.Count % 9;
                int iter = (articles.Count - tmpiter) / 9;
                    int index = 1;
                    int totalProcess = articles.Count;
                    var progressReport = new ProgressReport();

                    foreach (Article article in articles)
                {
                        string RefErr = article.Reference;
                    try { 
                    ProductSearchWoocommerce productWoocommerce = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference);
                    Product product = new Product();
                    if (article.conditionnements.Count > 0)
                    {
                        foreach (Conditionnement item in article.conditionnements)
                        {
                            ProductSearchWoocommerce productWoocommerceConditionnement = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference + "|" + item.Enumere);
                            var jsonC = JsonConvert.SerializeObject(product.SimpleConditionnementProductPricejson(article, item, productWoocommerce));
                            File.AppendAllText("Log\\data.txt", jsonC.ToString() + Environment.NewLine);
                            if (productWoocommerceConditionnement == null)
                            {
                                string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wholesale/v1/products");
                            }
                            else
                            {
                                string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wholesale/v1/products/" + productWoocommerceConditionnement.Id.ToString(), "PUT");
                            }
                        }

                    }
                    else
                    {
                        if (article.isGamme)
                        {

                            var json = JsonConvert.SerializeObject(product.ConfigurableProductjsonPrice(article, productWoocommerce));
                            File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                            if (productWoocommerce == null)
                            {
                                string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products");
                                ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                foreach (PrixGamme gamme in article.prixGammes)
                                {
                                    string jsonVariation = JsonConvert.SerializeObject(product.ProductVariationPrice(article, gamme));
                                    File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                    string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "/wp-json/wholesale/v1/products/" + productCreated.Id.ToString() + "/variations");
                                }
                            }
                            else
                            {

                                string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products/" + productWoocommerce.Id.ToString(), "PUT");
                                ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                foreach (long id in productCreated.Variations)
                                {
                                    string deleteVariations = UtilsWebservices.GetWoocommerceData("wp-json/wc/v3/products/" + productWoocommerce.Id.ToString() + "/variations/" + id + "?force=true", "DELETE");
                                }
                                foreach (PrixGamme gamme in article.prixGammes)
                                {
                                    string jsonVariation = JsonConvert.SerializeObject(product.ProductVariationPrice(article, gamme));
                                    File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                    string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "/wp-json/wholesale/v1/products/" + productCreated.Id.ToString() + "/variations");
                                }
                            }


                        }
                        else
                        {
                            var json = JsonConvert.SerializeObject(product.SimpleProductPricejson(article, null, null, null, productWoocommerce));
                            File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                            if (productWoocommerce == null)
                            {
                                string response = UtilsWebservices.SendDataJson(json, "/wp-json/wholesale/v1/products");
                            }
                            else
                            {
                                string responseC = UtilsWebservices.SendDataJson(json, "/wp-json/wholesale/v1/products/" + productWoocommerce.Id.ToString(), "PUT");
                            }

                        }
                    }


                    increm++;
                    }
                    catch (Exception s)
                    {
                            StringBuilder sb = new StringBuilder();
                            sb.Append(DateTime.Now + RefErr + Environment.NewLine);
                            sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                            sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                            File.AppendAllText("Log\\SyncAll.txt", sb.ToString());
                            sb.Clear();
                    }
                        progressReport.PercentComplete = index++ * 100 / totalProcess;
                        progress.Report(progressReport);
                        Thread.Sleep(10);
                    }

                    


                    // SingletonUI.Instance.ProgressBar.Invoke((MethodInvoker)(() => SingletonUI.Instance.ProgressBar.Value = 100));
                    MessageBox.Show("Synchronisation terminée", "end",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            });
        }

        public static void SendCustomPrice(string reference)
        {
            try
            {
                var gescom = SingletonConnection.Instance.Gescom;
                var articleSageObj = gescom.FactoryArticle.ReadReference(reference);
                Article article = new Article(articleSageObj);
                string RefErr = article.Reference;
                int increm = 1;
                try {

                    ProductSearchWoocommerce productWoocommerce = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference);
                    Product product = new Product();
                    if (article.conditionnements.Count > 0)
                    {
                        foreach (Conditionnement item in article.conditionnements)
                        {
                            ProductSearchWoocommerce productWoocommerceConditionnement = UtilsWebservices.getWoocommerceProduct("/wp-json/wc/v3/product-by-sku/search?sku=" + article.Reference + "|" + item.Enumere);
                            var jsonC = JsonConvert.SerializeObject(product.SimpleConditionnementProductPricejson(article, item, productWoocommerce));
                            File.AppendAllText("Log\\data.txt", jsonC.ToString() + Environment.NewLine);
                            if (productWoocommerceConditionnement == null)
                            {
                                string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wholesale/v1/products");
                            }
                            else
                            {
                                string responseC = UtilsWebservices.SendDataJson(jsonC, "wp-json/wholesale/v1/products/" + productWoocommerceConditionnement.Id.ToString(), "PUT");
                            }
                        }

                    }
                    else
                    {
                        if (article.isGamme)
                        {

                            var json = JsonConvert.SerializeObject(product.ConfigurableProductjsonPrice(article, productWoocommerce));
                            File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                            if (productWoocommerce == null)
                            {
                                string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products");
                                ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                foreach (PrixGamme gamme in article.prixGammes)
                                {
                                    string jsonVariation = JsonConvert.SerializeObject(product.ProductVariationPrice(article, gamme));
                                    File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                    string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "/wp-json/wholesale/v1/products/" + productCreated.Id.ToString() + "/variations");
                                }
                            }
                            else
                            {

                                string response = UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products/" + productWoocommerce.Id.ToString(), "PUT");
                                ProductCreatedGamme productCreated = ProductCreatedGamme.FromJson(response);
                                foreach (long id in productCreated.Variations)
                                {
                                    string deleteVariations = UtilsWebservices.GetWoocommerceData("wp-json/wc/v3/products/" + productWoocommerce.Id.ToString() + "/variations/" + id + "?force=true", "DELETE");
                                }
                                foreach (PrixGamme gamme in article.prixGammes)
                                {
                                    string jsonVariation = JsonConvert.SerializeObject(product.ProductVariationPrice(article, gamme));
                                    File.AppendAllText("Log\\data.txt", jsonVariation.ToString() + Environment.NewLine);
                                    string responseVariation = UtilsWebservices.SendDataJson(jsonVariation, "/wp-json/wholesale/v1/products/" + productCreated.Id.ToString() + "/variations");
                                }
                            }
                        }
                        else
                        {
                            var json = JsonConvert.SerializeObject(product.SimpleProductPricejson(article, null, null, null, productWoocommerce));
                            File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                            if (productWoocommerce == null)
                            {
                                string response = UtilsWebservices.SendDataJson(json, "/wp-json/wholesale/v1/products");
                            }
                            else
                            {
                                string responseC = UtilsWebservices.SendDataJson(json, "/wp-json/wholesale/v1/products/" + productWoocommerce.Id.ToString(), "PUT");
                            }

                        }
                    }

                }
                catch(Exception s)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DateTime.Now + RefErr + Environment.NewLine);
                    sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                    sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                    File.AppendAllText("Log\\SyncAll.txt", sb.ToString());
                    sb.Clear();
                }
                

                MessageBox.Show("Synchronisation terminée", "end",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
    }
}







