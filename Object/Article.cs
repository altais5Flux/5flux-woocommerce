﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Objets100cLib;
using WebservicesSage.Object;
using WebservicesSage.Object.woocommerce;
using WebservicesSage.Singleton;
using WebservicesSage.Utils;

namespace WebservicesSage.Object
{
    [Serializable()]
    public class Article
    {
        public string Designation { get; set; }
        public string Reference { get; set; }
        public double PrixAchat { get; set; }
        public double PrixVente { get; set; }
        public string Langue1 { get; set; }
        public string Langue2 { get; set; }
        public string CodeBarres { get; set; }
        public double Poid { get; set; }
        public bool Sommeil { get; set; }
        public bool IsPriceTTC { get; set; }
        public bool isGamme { get; set; }
        public bool isMonoGamme { get; set; }
        public List<Gamme> Gammes { get; set; }
        public string IntituleGamme1 { get; set; }
        public string IntituleGamme2 { get; set; }
        public List<String> enumsGammes1 { get; set; }
        public List<String> enumsGammes2 { get; set; }
        public List<PrixGamme> prixGammes { get; set; }
        public List<PrixRemise> prixRemises { get; set; }
        public List<PrixCatTarif> prixCatTarifs { get; set; }
        public List<PrixRemiseClient> prixRemisesClient { get; set; }
        public List<PrixClientTarif> prixClientTarif { get; set; }
        public List<RemiseFamille> remiseFamilles { get; set; }
        public List<InfoLibre> infoLibre { get; set; }
        public bool IsDoubleGamme { get; set; }
        public double Stock { get; set; }
        public bool HaveNomenclature { get; set; }
        public ArticleNomenclature ArticleNomenclature { get; set; }
        public List<Conditionnement> conditionnements { get; set; }
        //editeur article
        public string Resume { get; set; }
        public string LinkRewrite { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public List<Category> Categories { get; set; }
        public string Ecotaxe { get; set; }
        public string Fournisseur { get; set; }
        public string Longueur {get;set;}
        public string Largeur {get;set;}
        public string Taille {get;set;}
        public string Famille { get;set;}
        public string slugFamille { get;set;}
//editeur article
public Article(IBOArticle3 articleFC)
        {
            var compta = SingletonConnection.Instance.Gescom.CptaApplication;
            conditionnements = new List<Conditionnement>();
            infoLibre = new List<InfoLibre>();
            prixRemises = new List<PrixRemise>();
            prixRemisesClient = new List<PrixRemiseClient>();
            prixCatTarifs = new List<PrixCatTarif>();
            prixClientTarif = new List<PrixClientTarif>();
            remiseFamilles = new List<RemiseFamille>();
            
            var gescom = SingletonConnection.Instance.Gescom;
            var infolibreField = gescom.FactoryArticle.InfoLibreFields;
            int compteur = 1;
            foreach (var infoLibreValue in articleFC.InfoLibre)
            {
                if (true)
                {

                }
                infoLibre.Add(new InfoLibre(infolibreField[compteur].Name, infoLibreValue.ToString()));
                compteur++;
            }
            
            Designation = articleFC.AR_Design;
            Reference = articleFC.AR_Ref;
            PrixAchat = articleFC.AR_PrixAchat;
            Famille = articleFC.Famille.FA_Intitule.ToString();
            slugFamille = articleFC.Famille.FA_CodeFamille.ToString();
            PrixVente = articleFC.AR_PrixVen;
            Langue1 = articleFC.AR_Langue1;
            Langue2 = articleFC.AR_Langue2;
            CodeBarres = articleFC.AR_CodeBarre;

            if (UtilsConfig.Depot.ToString().Equals("tous"))
            {
                if (UtilsConfig.DefaultStock.Equals("TRUE"))
                {
                    Stock = articleFC.StockReel();
                }
                else
                {
                    Stock = articleFC.StockATerme();
                }
            }
            else
            {
                foreach (IBOArticleDepot3 art in articleFC.FactoryArticleDepot.List)
                {
                    if (art.Depot.DE_Intitule.ToString().Equals(UtilsConfig.Depot))
                    {
                        if (UtilsConfig.DefaultStock.Equals("TRUE"))
                        {
                            Stock = art.StockReel();
                        }
                        else
                        {
                            Stock = art.StockATerme();
                        }
                    }

                }
            }
            

           //gestion de la conversion du poids en KG pour prestashop
            if (articleFC.AR_UnitePoids == UnitePoidsType.UnitePoidsTypeGramme)
            {
                Poid = articleFC.AR_PoidsBrut / 1000;
            }

            Sommeil = articleFC.AR_Sommeil;
            IsPriceTTC = articleFC.AR_PrixTTC;
            int i = 0;
            string nn = "";
            foreach (IBOArticleTarifCategorie3 item in articleFC.FactoryArticleTarifCategorie.List)
            {
                nn += " " + item.CategorieTarif.CT_Intitule;
                i += 1;
            }
            // gestion des prix par remise
            
            foreach (IBOArticleTarifCategorie3 articleTarif in articleFC.FactoryArticleTarifCategorie.List)
            {
                if (articleTarif.CategorieTarif.CT_Intitule.ToUpper().Equals(UtilsConfig.CategorieTarif.ToString().ToUpper()))
                {
                    string RemiseValue = articleTarif.Remise.ToString();
                    if (!String.IsNullOrEmpty(RemiseValue))
                    {
                        PrixRemise prix = new PrixRemise();
                        prix.reduction_type = "percentage";
                        string[] remise = articleTarif.Remise.ToString().Split('%');
                        prix.RemisePercentage = (double)(Double.Parse(remise[0])) / 100;
                        prix.Born_Sup = 1;
                        prix.CategorieTarifaire = articleTarif.CategorieTarif.CT_Intitule;
                        prixRemises.Add(prix);

                    }
                    //PrixVente = articleTarif.Prix;
                    PrixCatTarif prixC = new PrixCatTarif();
                    prixC.CategorieTarifaire = articleTarif.CategorieTarif.CT_Intitule;
                    prixC.Price = articleTarif.Prix;
                    prixCatTarifs.Add(prixC);
                    double born = 1;
                    foreach (IBOArticleTarifQteCategorie3 tarif in articleTarif.FactoryArticleTarifQte.List)
                    {
                        
                        PrixRemise prix = new PrixRemise();
                        prix.CategorieTarifaire = articleTarif.CategorieTarif.CT_Intitule;
                        prix.Born_Sup = tarif.BorneSup;//tarif.BorneSup;
                        prix.Price = tarif.PrixNet;
                        if (string.IsNullOrEmpty(tarif.Remise.ToString()))
                        {
                            prix.reduction_type = "amount";
                        }
                        else
                        {
                            prix.reduction_type = "percentage";
                            string[] remise = tarif.Remise.ToString().Split('%');
                            prix.RemisePercentage = (double)(Double.Parse(remise[0]));
                        }
                        prix.FixedPrice = articleTarif.Prix;
                        prixRemises.Add(prix);
                    }
                }
            }
            foreach (IBOArticleTarifClient3 articleTarifClient in articleFC.FactoryArticleTarifClient.List)
            {
                double born = 1;
                foreach (IBOArticleTarifQteClient3 tarifClient in articleTarifClient.FactoryArticleTarifQte.List)
                {
                    PrixRemiseClient prix = new PrixRemiseClient();
                    string test0 = articleTarifClient.Article.AR_Ref.ToString();
                    prix.ClientCtNum = articleTarifClient.Client.CT_Num.ToString();
                    prix.Born_Sup = born; // tarifClient.BorneSup;
                    prix.Price = tarifClient.PrixNet;
                    if (string.IsNullOrEmpty(tarifClient.Remise.ToString()))
                    {
                        prix.reduction_type = "amount";
                    }
                    else
                    {
                        prix.reduction_type = "percentage";
                        string[] remise = tarifClient.Remise.ToString().Split('%');
                        prix.RemisePercentage = (double)(Double.Parse(remise[0])) / 100;
                    }
                    prix.FixedPrice = articleTarifClient.Prix;
                    prixRemisesClient.Add(prix);
                    born += tarifClient.BorneSup;
                }
                if (articleTarifClient.FactoryArticleTarifQte.List.Count == 0)
                {
                    PrixClientTarif prix = new PrixClientTarif();
                    prix.ClinetCtNum = articleTarifClient.Client.CT_Num.ToString();
                    prix.Price = articleTarifClient.Prix;
                    string[] remise = articleTarifClient.Remise.ToString().Split('%');
                    if (!String.IsNullOrEmpty(remise[0]))
                    {
                        prix.FixedRemisePercentage = (double)(Double.Parse(remise[0])) / 100;
                    }
                    else
                    {
                        prix.FixedRemisePercentage = 0;
                    }
                    prixClientTarif.Add(prix);
                }

            }
            //gestion remise par famille
            //var gescom = Singleton.SingletonConnection.Instance.Gescom;
            //var compta = SingletonConnection.Instance.Gescom.CptaApplication;
            foreach (IBOFamilleTarifClient item in articleFC.Famille.FactoryFamilleTarifClient.List)
            {
                RemiseFamille remisefamille = new RemiseFamille();
                
                var clientsSageObj = compta.FactoryClient.ReadNumero(item.Client.CT_Num);
                remisefamille.CategorieTarifaire = clientsSageObj.CatTarif.CT_Intitule;
                remisefamille.CtNum = item.Client.CT_Num;
                remisefamille.FixedPrice = 0;
                string[] remise = item.Remise.ToString().Split('%'); //item.Remise.Remise[0].REM_Valeur;
                if (!String.IsNullOrEmpty(remise[0]))
                {
                    remisefamille.RemisePercentage = (double)(Double.Parse(remise[0])) / 100;
                }
                foreach (PrixClientTarif prix in prixClientTarif)
                {
                    if (prix.ClinetCtNum.Equals(remisefamille.CtNum))
                    {
                        remisefamille.FixedPrice = prix.Price;
                        if (prix.FixedRemisePercentage > remisefamille.RemisePercentage)
                        {
                            remisefamille.RemisePercentage = prix.FixedRemisePercentage;
                        }
                        prixClientTarif.Remove(prix);
                        break;
                    }
                }
                if (remisefamille.FixedPrice == 0)
                {
                    foreach (IBOArticleTarifCategorie3 articleTarif in articleFC.FactoryArticleTarifCategorie.List)
                    {
                        if (articleTarif.CategorieTarif.CT_Intitule.Equals(remisefamille.CategorieTarifaire))
                        {
                            remisefamille.FixedPrice = articleTarif.Prix;
                            break;
                        }
                    }
                }


                remiseFamilles.Add(remisefamille);
            }
            /*
            // gestion des prix par remise
            foreach (IBOArticleTarifCategorie3 articleTarif in articleFC.FactoryArticleTarifCategorie.List)
            {
                foreach (IBOArticleTarifQteCategorie3 tarif in articleTarif.FactoryArticleTarifQte.List)
                {
                    PrixRemise prix = new PrixRemise();
                    prix.CategorieTarifaire = articleTarif.CategorieTarif.CT_Intitule;
                    prix.Born_Sup = tarif.BorneSup;
                    prix.Price = tarif.PrixNet;
                    prixRemises.Add(prix);
                }
            }
            //gestion des prix par client
            foreach (IBOArticleTarifClient3 articleTarifClient in articleFC.FactoryArticleTarifClient.List)
            {
                foreach (IBOArticleTarifQteClient3 tarifClient in articleTarifClient.FactoryArticleTarifQte.List)
                {
                    PrixRemiseClient prix = new PrixRemiseClient();
                    string test0 = articleTarifClient.Article.AR_Ref.ToString();
                    prix.ClientCtNum = articleTarifClient.Client.CT_Num.ToString();
                    prix.Born_Sup = tarifClient.BorneSup;
                    prix.Price = tarifClient.PrixNet;
                    if (string.IsNullOrEmpty(tarifClient.Remise.ToString()))
                    {
                        prix.reduction_type = "amount";
                    }
                    else
                    {
                        prix.reduction_type = "percentage";
                        string[] remise = tarifClient.Remise.ToString().Split('%');
                        prix.RemisePercentage = (double)(Double.Parse(remise[0])) / 100;
                    }
                    prixRemisesClient.Add(prix);
                }
                //foreach (IBOArticleTarifQteClient3 tarifClient in articleTarifClient)

            }*/
            if (IsPriceTTC)
            {
                //   PrixVente = articleFC.AR_PrixVen * ;
            }

            // gestion des produits à gammes
            if (articleFC.AR_Type == ArticleType.ArticleTypeGamme)
            {
                isGamme = true;
                isMonoGamme = true;
                Gammes = new List<Gamme>();
                prixGammes = new List<PrixGamme>();
                IsDoubleGamme = false;

                if (articleFC.FactoryArticleGammeEnum2.List.Count > 0) {
                    IsDoubleGamme = true;
                    isMonoGamme = false;
                }
                    

                if (IsDoubleGamme)
                {

                    enumsGammes1 = new List<String>();

                    foreach (IBOArticleGammeEnum3 gamme in articleFC.FactoryArticleGammeEnum1.List)
                    {
                        IntituleGamme1 = articleFC.Gamme1.G_Intitule.ToString();
                        String option = gamme.EG_Enumere.ToString();
                        enumsGammes1.Add(option);
                    }
                    enumsGammes2 = new List<String>();
                    foreach (IBOArticleGammeEnum3 gamme in articleFC.FactoryArticleGammeEnum2.List)
                    {
                        IntituleGamme2 = articleFC.Gamme2.G_Intitule.ToString();
                        String option = gamme.EG_Enumere.ToString();
                        enumsGammes2.Add(option);
                    }
                    foreach (IBOArticleGammeEnum3 gamme in articleFC.FactoryArticleGammeEnum1.List)
                    {
                        
                            int count = 0;
                            foreach (IBOArticleGammeEnumRef3 li in gamme.FactoryArticleGammeEnumRef.List)
                            {
                                foreach (IBPCategorieTarif catTarifaire in Singleton.SingletonConnection.Instance.Gescom.FactoryCategorieTarif.List)
                                {

                                    if (catTarifaire.CT_Intitule.ToString().Equals(UtilsConfig.CategorieTarif.ToString()))
                                    {
                                        ITarifVente2 test = articleFC.TarifVenteCategorieDoubleGamme(catTarifaire, li.ArticleGammeEnum1, li.ArticleGammeEnum2, 1);
                                        PrixGamme prix = new PrixGamme();
                                        if (test.PrixTTC)
                                        {
                                            // calcul TVA
                                            prix.Price = test.Prix / UtilsConfig.TVACalculer;
                                        }
                                        else
                                        {
                                            prix.Price = test.Prix;
                                        }
                                        prix.Gamme1_Intitule = articleFC.Gamme1.G_Intitule;
                                        prix.Gamme1_Value = li.ArticleGammeEnum1.EG_Enumere;
                                        prix.Gamme2_Intitule = articleFC.Gamme2.G_Intitule;
                                        prix.Gamme2_Value = li.ArticleGammeEnum2.EG_Enumere;
                                        prix.Categori_Intitule = catTarifaire.CT_Intitule;

                                        // gestion des arrondi 
                                        prix.Price = Math.Round(prix.Price, UtilsConfig.ArrondiDigits);

                                        prixGammes.Add(prix);
                                    }
                                }




                                Gamme gammeO = new Gamme(articleFC.Gamme1.G_Intitule, li.ArticleGammeEnum1.EG_Enumere, articleFC.Gamme2.G_Intitule, li.ArticleGammeEnum2.EG_Enumere);
                                ITarif2 tarif = articleFC.TarifAchatDoubleGamme(li.ArticleGammeEnum1, li.ArticleGammeEnum2);
                                gammeO.Price = tarif.Prix;
                                gammeO.Reference = tarif.Reference;
                                gammeO.CodeBarre = tarif.CodeBarre;
                                gammeO.Sommeil = li.AE_Sommeil;


                                if (UtilsConfig.Depot.ToString().Equals("tous"))
                                {
                                    if (UtilsConfig.DefaultStock.Equals("TRUE"))
                                    {
                                        gammeO.Stock = articleFC.StockReelDoubleGamme(li.ArticleGammeEnum1, li.ArticleGammeEnum2);
                                    }
                                    else
                                    {
                                        gammeO.Stock = articleFC.StockATermeDoubleGamme(li.ArticleGammeEnum1, li.ArticleGammeEnum2);
                                    }
                                }
                                else
                                {

                                    
                                        foreach (IBOArticleDepot3 art in articleFC.FactoryArticleDepot.List)
                                        {
                                            foreach (IBOArticleDepotGamme3 gam in art.FactoryArticleDepotGamme.List)
                                            {
                                                string enum1 = gam.ArticleGammeEnum1.EG_Enumere.ToString();
                                                string enum2 = gam.ArticleGammeEnum2.EG_Enumere.ToString();
                                                string g1 = li.ArticleGammeEnum1.EG_Enumere.ToString();
                                                string g2 = li.ArticleGammeEnum2.EG_Enumere.ToString();

                                                if (art.Depot.DE_Intitule.ToString().Equals(UtilsConfig.Depot)
                                                    && gam.ArticleGammeEnum1.EG_Enumere.ToString().Equals(li.ArticleGammeEnum1.EG_Enumere.ToString())
                                                    && gam.ArticleGammeEnum2.EG_Enumere.ToString().Equals(li.ArticleGammeEnum2.EG_Enumere.ToString())
                                                    )
                                                {
                                                    
                                                    if (UtilsConfig.DefaultStock.Equals("TRUE"))
                                                    {
                                                        gammeO.Stock = gam.StockReel();
                                                    }
                                                    else
                                                    {
                                                        gammeO.Stock = gam.StockATerme();
                                                    }
                                                }

                                            }


                                        }
                                    


                                }

                                Gammes.Add(gammeO);
                            
                            }
                    }
                }
                else
                {
                    enumsGammes1 = new List<String>();

                    foreach (IBOArticleGammeEnum3 gamme in articleFC.FactoryArticleGammeEnum1.List)
                    {
                        IntituleGamme1 = articleFC.Gamme1.G_Intitule.ToString();
                        String option = gamme.EG_Enumere.ToString();
                        enumsGammes1.Add(option);
                    }
                    foreach (IBOArticleGammeEnum3 gamme in articleFC.FactoryArticleGammeEnum1.List)
                    {
                        ITarif2 tarif = articleFC.TarifAchatMonoGamme(gamme);
                        //IBPCategorieTarif t = Singleton.SingletonConnection.Instance.Gescom.FactoryCategorieTarif.ReadIntitule("Grossistes");

                        foreach (IBPCategorieTarif catTarifaire in Singleton.SingletonConnection.Instance.Gescom.FactoryCategorieTarif.List)
                        {
                            if (catTarifaire.CT_Intitule.ToString().Equals(UtilsConfig.CategorieTarif.ToString()))
                            {
                                if (!String.IsNullOrEmpty(catTarifaire.CT_Intitule))
                                {
                                    ITarifVente2 test = articleFC.TarifVenteCategorieMonoGamme(catTarifaire, gamme, 1);

                                    PrixGamme prix = new PrixGamme();
                                    if (test.PrixTTC)
                                    {
                                        // calcul TVA
                                        prix.Price = test.Prix / UtilsConfig.TVACalculer;
                                    }
                                    else
                                    {
                                        prix.Price = test.Prix;
                                    }
                                    prix.Gamme1_Intitule = articleFC.Gamme1.G_Intitule;
                                    prix.Gamme1_Value = gamme.EG_Enumere;
                                    prix.Categori_Intitule = catTarifaire.CT_Intitule;

                                    // gestion des arrondi 
                                    prix.Price = Math.Round(prix.Price, UtilsConfig.ArrondiDigits);

                                    prixGammes.Add(prix);
                                }
                            }
                        }

                        Gamme gammeO = new Gamme(articleFC.Gamme1.G_Intitule, gamme.EG_Enumere);
                        gammeO.Price = tarif.Prix;
                        gammeO.Reference = tarif.Reference;
                        gammeO.CodeBarre = tarif.CodeBarre;

                        if (UtilsConfig.Depot.ToString().Equals("tous"))
                        {
                            if (UtilsConfig.DefaultStock.Equals("TRUE"))
                            {
                                gammeO.Stock = articleFC.StockReelMonoGamme(gamme);
                            }
                            else
                            {
                                gammeO.Stock = articleFC.StockATermeMonoGamme(gamme);
                            }
                        }
                        else
                        {
                            foreach (IBOArticleDepot3 art in articleFC.FactoryArticleDepot.List)
                            {
                                foreach(IBOArticleDepotGamme3 gam in art.FactoryArticleDepotGamme.List)
                                {
                                    
                                    if (art.Depot.DE_Intitule.ToString().Equals(UtilsConfig.Depot) && gam.ArticleGammeEnum1.EG_Enumere.ToString().Equals(gamme.EG_Enumere.ToString()))
                                    {
                                        if (UtilsConfig.DefaultStock.Equals("TRUE"))
                                        {
                                            gammeO.Stock = gam.StockReel();
                                        }
                                        else
                                        {
                                            gammeO.Stock = gam.StockATerme();
                                        }
                                    }

                                }
                                

                            }
                        }

                        IBOArticleGammeEnumRef3 reff = (IBOArticleGammeEnumRef3)gamme.FactoryArticleGammeEnumRef.List[1];
                        gammeO.Sommeil = reff.AE_Sommeil;
                        Gammes.Add(gammeO);
                    }
                }
            }
            
            if (articleFC.AR_Nomencl != NomenclatureType.NomenclatureTypeAucun && articleFC.AR_Nomencl != NomenclatureType.NomenclatureTypeFabrication)
            {
                HaveNomenclature = true;
                ArticleNomenclature = new ArticleNomenclature();
                ArticleNomenclature.ArticleRef = articleFC.AR_Ref;

                foreach (IBOArticleNomenclature3 nomenclature in articleFC.FactoryArticleNomenclature.List)
                {
                    ArticleNomenclature.NomenclatureRefList.Add(nomenclature.ArticleComposant.AR_Ref);
                }
            }
            if (articleFC.Conditionnement != null)
            {
                //Console.WriteLine("worked");
                foreach (IBOArticleCond3 item in articleFC.FactoryArticleCond.List)
                {
                    Conditionnement cond = new Conditionnement(item.EC_Enumere, item.CO_Ref, item.EC_Quantite.ToString());
                    conditionnements.Add(cond);
                } 
            }

        }

        public Article()
        {

        }
    }
}
