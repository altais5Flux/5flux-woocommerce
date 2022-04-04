using Newtonsoft.Json;
using Objets100cLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Unidecode.NET;
using WebservicesSage.Object;
using WebservicesSage.Object.Categories;
using WebservicesSage.Singleton;
using WebservicesSage.Utils;

namespace WebservicesSage.Cotnroller
{
    class ControllerCategorie
    {

        public static int SendCustumCategories(string categorie, string slug)
        {
            
            Product product = new Product();
            int idCat = int.Parse(UtilsConfig.Category);
            String id = "";
            try
            {
                List<CategorieSearchWoocommerce> orderSearch = CategorieSearchWoocommerce.FromJson(UtilsWebservices.GetWoocommerceCategorie("/wp-json/wc/v3/products/categories"));
                string fam = "";
                
                fam = slug;

                if (!String.IsNullOrEmpty(fam))
                {
                    
                    //String slug = "";
                    Boolean foundParentFamille = false;
                    for (int i = 0; i < orderSearch.Count; i++)
                    {
                        string name = orderSearch[i].Slug.ToString().Unidecode();
                        if (fam.ToUpper().Equals(orderSearch[i].Slug.ToUpper().ToString()))
                        {
                            id = orderSearch[i].Id.ToString();
                            slug = orderSearch[i].Slug.ToString();
                            foundParentFamille = true;
                            break;
                        }
                    }

                    if (!foundParentFamille)
                    {
                        var json = JsonConvert.SerializeObject(product.CategorieArticle(fam, slug));
                        CreatedCategorie cats = CreatedCategorie.FromJson(UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products/categories"));
                        id = cats.Id.ToString();
                    }

                    Boolean found = false;
                    for (int i = 0; i < orderSearch.Count; i++)
                    {
                        if (slug.ToUpper().ToString().Equals(orderSearch[i].Slug.ToUpper().ToString()))
                        {
                            found = true;
                        }
                    }

                    if (!found && !string.IsNullOrEmpty(id))
                    {
                        var json = JsonConvert.SerializeObject(product.CategorieArticleParent(categorie, id));
                        File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                        CreatedCategorie cats = CreatedCategorie.FromJson(UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products/categories"));
                        id = cats.Id.ToString();
                    }

                }
                else
                {
                    Boolean found = false;
                    for (int i = 0; i < orderSearch.Count; i++)
                    {
                        if (slug.ToUpper().ToString().Equals(orderSearch[i].Slug.ToUpper().ToString()))
                        {

                            found = true;
                        }
                    }

                    if (!found)
                    {
                        var json = JsonConvert.SerializeObject(product.CategorieArticle(categorie.ToString(), slug.ToString()));
                        File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                        CreatedCategorie cats = CreatedCategorie.FromJson(UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products/categories"));
                        id = cats.Id.ToString();
                    }
                }
                idCat = int.Parse(id);
            }
            catch (Exception e)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now + Environment.NewLine);
                sb.Append(DateTime.Now + e.Message + Environment.NewLine);
                sb.Append(DateTime.Now + e.StackTrace + Environment.NewLine);
                File.AppendAllText("Log\\categorie.txt", sb.ToString());
                sb.Clear();
            }


            return idCat;

        }


        public static Task SendAllCategories(IProgress<ProgressReport> progress)
       {
            return Task.Run(() =>
            {

                var gescom = SingletonConnection.Instance.Gescom;
                Product product = new Product();
                int index = 1;
                int totalProcess = gescom.FactoryFamille.List.Count;
                var progressReport = new ProgressReport();

                foreach (IBOFamille3 famille in gescom.FactoryFamille.List)
                {
                    try
                    {
                        List<CategorieSearchWoocommerce> orderSearch = CategorieSearchWoocommerce.FromJson(UtilsWebservices.GetWoocommerceCategorie("/wp-json/wc/v3/products/categories"));
                        string fam = "";
                        try
                        {
                            fam = famille.FamilleCentral.FA_CodeFamille.ToString();
                        }
                        catch (Exception e)
                        {

                        }


                        if (!String.IsNullOrEmpty(fam))
                        {
                            String id = "";
                            String slug = "";
                            Boolean foundParentFamille = false;
                            for (int i = 0; i < orderSearch.Count; i++)
                            {
                                string name = orderSearch[i].Slug.ToString().Unidecode();
                                if (fam.ToUpper().Equals(orderSearch[i].Slug.ToUpper().ToString()))
                                {
                                    id = orderSearch[i].Id.ToString();
                                    slug = orderSearch[i].Slug.ToString();
                                    foundParentFamille = true;
                                }
                            }

                            if (!foundParentFamille)
                            {
                                var json = JsonConvert.SerializeObject(product.CategorieArticle(fam, famille.FA_CodeFamille.ToString()));
                                File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                CreatedCategorie cats = CreatedCategorie.FromJson(UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products/categories"));
                                id = cats.Id.ToString();
                            }

                            Boolean found = false;
                            for (int i = 0; i < orderSearch.Count; i++)
                            {
                                if (famille.FA_CodeFamille.ToUpper().ToString().Equals(orderSearch[i].Slug.ToUpper().ToString()))
                                {

                                    found = true;
                                }
                            }

                            if (!found && !string.IsNullOrEmpty(id))
                            {
                                var json = JsonConvert.SerializeObject(product.CategorieArticleParent(famille.FA_Intitule.ToString(), id));
                                File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products/categories");
                            }

                        }
                        else
                        {
                            Boolean found = false;
                            for (int i = 0; i < orderSearch.Count; i++)
                            {
                                if (famille.FA_CodeFamille.ToUpper().ToString().Equals(orderSearch[i].Slug.ToUpper().ToString()))
                                {
                                    found = true;
                                }
                            }

                            if (!found)
                            {
                                var json = JsonConvert.SerializeObject(product.CategorieArticle(famille.FA_Intitule.ToString(), famille.FA_CodeFamille.ToString()));
                                File.AppendAllText("Log\\data.txt", json.ToString() + Environment.NewLine);
                                UtilsWebservices.SendDataJson(json, "wp-json/wc/v3/products/categories");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(DateTime.Now + Environment.NewLine);
                        sb.Append(DateTime.Now + e.Message + Environment.NewLine);
                        sb.Append(DateTime.Now + e.StackTrace + Environment.NewLine);
                        File.AppendAllText("Log\\categorie.txt", sb.ToString());
                        sb.Clear();
                    }

                    progressReport.PercentComplete = index++ * 100 / totalProcess;
                    progress.Report(progressReport);
                    Thread.Sleep(10);

                }

            });
        }
    }
}
