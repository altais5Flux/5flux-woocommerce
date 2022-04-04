using LiteDB;
using Newtonsoft.Json;
using Objets100cLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using WebservicesSage.Object;
using WebservicesSage.Object.CustomerSearch;
using WebservicesSage.Object.CustomerSearchByEmail;
using WebservicesSage.Object.DBObject;
using WebservicesSage.Object.Devis;
using WebservicesSage.Object.Order;
using WebservicesSage.Object.woocommerce;
using WebservicesSage.Singleton;
using WebservicesSage.Utils;
using Customer = WebservicesSage.Object.Customer;

namespace WebservicesSage.Cotnroller
{
   

    /// <summary>
    /// Defines the <see cref="ControllerCommande" />.
    /// </summary>
    public static class ControllerCommande
    {
        /// <summary>
        /// Lance le service de check des nouvelles commandes prestashop
        /// Définir le temps de passage de la tâche dans la config.
        /// </summary>
        public static void LaunchService()
        {
            // SingletonUI.Instance.LogBox.Invoke((MethodInvoker)(() => SingletonUI.Instance.LogBox.AppendText("Commande Services Launched " + Environment.NewLine)));

            System.Timers.Timer timer = new System.Timers.Timer();
            //timer.Elapsed += new ElapsedEventHandler(CheckForNewOrderMagento);
            timer.Interval = UtilsConfig.CronTaskCheckForNewOrder;
            timer.Enabled = true;

            System.Timers.Timer timerUpdateStatut = new System.Timers.Timer();
            timerUpdateStatut.Elapsed += new ElapsedEventHandler(UpdateStatuOrder);
            timerUpdateStatut.Interval = UtilsConfig.CronTaskUpdateStatut;
            timerUpdateStatut.Enabled = true;
        }

        public static Task getOrderFromStore(IProgress<ProgressReport> progress)
        {
            return Task.Run(() =>
            {
                if (UtilsConfig.ContactConfig == 1)
                {
                    CheckForNewOrderMagento(progress);
                }
                else
                {
                    CheckForNewOrderMagentoPrefixClient();
                }
            });
        }

        /// <summary>
        /// Event levé par une nouvelle commande dans prestashop.
        /// </summary>
        /// <param name="source">.</param>
        /// <param name="e">.</param>
        public static Task CheckForNewOrderMagento(IProgress<ProgressReport> progress)
        {
            return Task.Run(() =>
            {
                string currentIdOrder = "0";
            string clientCtNum = "";
            string clienttype = "";
            string currentIncrementedId = "";

            try
            {
                //string response = UtilsWebservices.SearchOrder(UtilsConfig.BaseUrl + "/rest/V1/orders", UtilsWebservices.SearchOrderCriteria("order_flag", "1", "eq"));
                List<orderSearchWoocommerce> orderSearch = orderSearchWoocommerce.FromJson(UtilsWebservices.GetWoocommerceData("/wp-json/wc/v3/orders-by-flag/search?order=1"));
                    double percentComplete = 1;
                    int index = 0;
                    int totalProcess = orderSearch.Count;
                    var progressReport = new ProgressReport();
                    if (orderSearch[0].orderItemsWoocommerce.Count > 0)
                    {
                        //todo create BC sage
                        for (int i = 0; i < orderSearch[0].orderItemsWoocommerce.Count; i++)
                        {
                        
                            currentIdOrder = orderSearch[0].orderItemsWoocommerce[i].Id.ToString();
                            string email = "";
                            email = orderSearch[0].orderItemsWoocommerce[i].Billing.Email;

                            try
                            {
                                currentIdOrder = orderSearch[0].orderItemsWoocommerce[i].Id.ToString();

                                if (orderSearch[0].orderItemsWoocommerce[i].Status.Equals("cancelled") || orderSearch[0].orderItemsWoocommerce[i].Status.Equals("refunded") || orderSearch[0].orderItemsWoocommerce[i].Status.Equals("failed ") || orderSearch[0].orderItemsWoocommerce[i].Status.Equals("trash") || orderSearch[0].orderItemsWoocommerce[i].Status.Equals("failed"))
                                {
                                    UtilsWebservices.SendDataJson("", @"/wp-json/wc/v3/orders-by-flag/search?order=2&id=" + currentIdOrder, "PUT");

                                    continue;
                                }
                            
                                List<customerSearchByEmailWoocommerce> ClientSearch = customerSearchByEmailWoocommerce.FromJson(UtilsWebservices.GetWoocommerceData("/wp-json/wc/v3/customers?email="+email));
                                File.AppendAllText("Log\\GetCustomer.txt", ClientSearch[0].ToString() + Environment.NewLine);
                                Customer customer = new Customer();
                                string clientSageObj = ControllerClient.CheckIfClientEmailExist(ClientSearch[0].Email);
                            
                                if (!String.IsNullOrEmpty(clientSageObj))
                                {
                                    IBOClient3 customerSage = SingletonConnection.Instance.Gescom.CptaApplication.FactoryClient.ReadNumero(clientSageObj);
                                    Client ClientData = new Client(customerSage);
                                    var jsonClient = JsonConvert.SerializeObject(customer.updateCustomerWoocommerce(ClientData, customerSage));
                                    //UtilsWebservices.SendDataJson(jsonClient, @"rest/all/V1/customers/" + ClientSearch.Items[0].Id.ToString(), "PUT");
                                    AddNewOrderForCustomer(orderSearch[0].orderItemsWoocommerce[i], clientSageObj, ClientSearch[0]);
                                }
                                else
                                {

                                    string ct_num = ControllerClient.CreateNewClient(ClientSearch[0], orderSearch[0].orderItemsWoocommerce[i]);
                                
                                    if (!String.IsNullOrEmpty(ct_num))
                                    {
                                        //var jsonClient = JsonConvert.SerializeObject(customer.updateCustomerWoocommerce(ct_num, clienttype, client));
                                        //UtilsWebservices.SendDataJson(jsonClient, @"rest/all/V1/customers/" + client.Id.ToString(), "PUT");
                                        // le client à bien été crée on peut intégrer la commande sur son compte sage
                                        AddNewOrderForCustomer(orderSearch[0].orderItemsWoocommerce[i], ct_num, ClientSearch[0]);
                                    }
                                    else
                                    {
                                        File.AppendAllText("Log\\order.txt", DateTime.Now + "erreur creation du client" + Environment.NewLine);
                                        File.AppendAllText("Log\\order.txt", DateTime.Now + "Erreur avec la commande : " + currentIncrementedId + Environment.NewLine);
                                        UtilsWebservices.SendDataJson("", @"/wp-json/wc/v3/orders-by-flag/search?order=2&id=" + orderSearch[0].orderItemsWoocommerce[i].Id.ToString(), "PUT");
                                    }
                                }


                            }
                            catch (Exception s)
                            {
                                UtilsWebservices.SendDataJson("", @"/wp-json/wc/v3/orders-by-flag/search?order=2&id=" + orderSearch[0].orderItemsWoocommerce[i].Id.ToString(), "PUT");

                                StringBuilder sb = new StringBuilder();
                                sb.Append(DateTime.Now + "Erreur avec la commande : " + currentIdOrder + Environment.NewLine);
                                sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                                sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                                File.AppendAllText("Log\\order.txt", sb.ToString());
                                sb.Clear();

                            }
                                progressReport.PercentComplete = index++ * 100 / totalProcess;
                                progress.Report(progressReport);
                                Thread.Sleep(10);
                                MessageBox.Show("Synchronisation terminée", "end", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                        }
                    }
                    else
                    {
                        progressReport.PercentComplete = 100;
                        progress.Report(progressReport);
                        Thread.Sleep(10);
                        MessageBox.Show("Synchronisation terminée", "end", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }


                }
                catch (Exception s)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                File.AppendAllText("Log\\order.txt", sb.ToString());
                sb.Clear();
                UtilsWebservices.SendDataJson("", @"/wp-json/wc/v3/orders-by-flag/search?order=2&id=" + currentIdOrder, "PUT");
                //var jsonFlag = JsonConvert.SerializeObject(UpdateOrderFlag(currentIdOrder, currentIncrementedId, "2"));
                //UtilsWebservices.SendDataJson(jsonFlag, @"rest/all/V1/orders/", "POST");
                //UtilsWebservices.SendDataNoParse(UtilsConfig.BaseUrl + EnumEndPoint.Commande.Value, "errorOrder&orderID=" + currentIdOrder);
            }
            });
        }


        public static void CheckForNewOrderMagentoPrefixClient()
        {

            string currentIdOrder = "0";
            string clientCtNum = "";
            string clienttype = "";
            string currentIncrementedId = "";

            try
            {
                //string response = UtilsWebservices.SearchOrder(UtilsConfig.BaseUrl + "/rest/V1/orders", UtilsWebservices.SearchOrderCriteria("order_flag", "1", "eq"));
                List<orderSearchWoocommerce> orderSearch = orderSearchWoocommerce.FromJson(UtilsWebservices.GetWoocommerceData("/wp-json/wc/v3/orders-by-flag/search?order=1"));
                if (orderSearch[0].orderItemsWoocommerce.Count > 0)
                {
                    //todo create BC sage
                    for (int i = 0; i < orderSearch[0].orderItemsWoocommerce.Count; i++)
                    {

                        currentIdOrder = orderSearch[0].orderItemsWoocommerce[i].Id.ToString();
                        string email = "";
                        email = orderSearch[0].orderItemsWoocommerce[i].Billing.Email;

                        try
                        {
                            currentIdOrder = orderSearch[0].orderItemsWoocommerce[i].Id.ToString();

                            if (orderSearch[0].orderItemsWoocommerce[i].Status.Equals("cancelled") || orderSearch[0].orderItemsWoocommerce[i].Status.Equals("refunded") || orderSearch[0].orderItemsWoocommerce[i].Status.Equals("failed ") || orderSearch[0].orderItemsWoocommerce[i].Status.Equals("trash") || orderSearch[0].orderItemsWoocommerce[i].Status.Equals("failed"))
                            {
                                UtilsWebservices.SendDataJson("", @"/wp-json/wc/v3/orders-by-flag/search?order=2&id=" + currentIdOrder, "PUT");

                                continue;
                            }


                            AddNewOrderForCustomerPrefix(orderSearch[0].orderItemsWoocommerce[i]);

                        }
                        catch (Exception s)
                        {
                            UtilsWebservices.SendDataJson("", @"/wp-json/wc/v3/orders-by-flag/search?order=2&id=" + orderSearch[0].orderItemsWoocommerce[i].Id.ToString(), "PUT");

                            StringBuilder sb = new StringBuilder();
                            sb.Append(DateTime.Now + "Erreur avec la commande : " + currentIdOrder + Environment.NewLine);
                            sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                            sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                            File.AppendAllText("Log\\order.txt", sb.ToString());
                            sb.Clear();

                        }
                    }
                }
            }
            catch (Exception s)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                File.AppendAllText("Log\\order.txt", sb.ToString());
                sb.Clear();
                UtilsWebservices.SendDataJson("", @"/wp-json/wc/v3/orders-by-flag/search?order=2&id=" + currentIdOrder, "PUT");
                //var jsonFlag = JsonConvert.SerializeObject(UpdateOrderFlag(currentIdOrder, currentIncrementedId, "2"));
                //UtilsWebservices.SendDataJson(jsonFlag, @"rest/all/V1/orders/", "POST");
                //UtilsWebservices.SendDataNoParse(UtilsConfig.BaseUrl + EnumEndPoint.Commande.Value, "errorOrder&orderID=" + currentIdOrder);
            }
        }

        /// <summary>
        /// The UpdateStatuOrder.
        /// </summary>
        /// <param name="source">The source<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="ElapsedEventArgs"/>.</param>
        public static void UpdateStatuOrder(object source, ElapsedEventArgs e)
        {
            string test = "";
            try
            {
                var gescom = SingletonConnection.Instance.Gescom;
                var compta = SingletonConnection.Instance.Compta;

                //IBICollection AllOrders = gescom.FactoryDocumentVente.List;
                using (var db = new LiteDatabase(@"MyData.db"))
                {
                    // Get OrderMapping from Config

                    string MagentoStatutId, orderStatutId, statut1, statut2, statut3;
                    string[] MagentoID, orderStatut;
                    UtilsConfig.MagentoStatutId.TryGetValue("default", out MagentoStatutId);
                    MagentoID = MagentoStatutId.Split('_');
                    string statutMagento1, statutMagento2, statutMagento3;
                    UtilsConfig.MagentoStatutId.TryGetValue(MagentoID[0], out statutMagento1);
                    UtilsConfig.MagentoStatutId.TryGetValue(MagentoID[1], out statutMagento2);
                    UtilsConfig.MagentoStatutId.TryGetValue(MagentoID[2], out statutMagento3);
                    UtilsConfig.OrderMapping.TryGetValue("default", out orderStatutId);
                    orderStatut = orderStatutId.Split('_');
                    UtilsConfig.OrderMapping.TryGetValue(orderStatut[0], out statut1);
                    UtilsConfig.OrderMapping.TryGetValue(orderStatut[1], out statut2);
                    UtilsConfig.OrderMapping.TryGetValue(orderStatut[2], out statut3);
                    //statut1 =UtilsConfig.OrderMapping. //orderStatut[0];
                    //statut2 = orderStatut[1];
                    //statut3 = orderStatut[2];

                    // Get a collection (or create, if doesn't exist)
                    var col = db.GetCollection<LinkedCommandeDB>("Commande");
                    foreach (LinkedCommandeDB item in col.FindAll())
                    {
                        DocumentType OrderDocumentType = DocumentType.DocumentTypeVenteCommande;
                        string sql = "SELECT DO_Type FROM [" + System.Configuration.ConfigurationManager.AppSettings["DBNAME"].ToString() + "].[dbo].[F_DOCENTETE] WHERE DO_Ref = '" + item.DO_Ref + "'";
                        File.AppendAllText("Log\\SQL.txt", DateTime.Now + sql.ToString() + Environment.NewLine);
                        SqlDataReader orderType = DB.Select(sql);
                        while (orderType.Read())
                        {

                            File.AppendAllText("Log\\SQL.txt", DateTime.Now + orderType.GetValue(0).ToString() + Environment.NewLine);
                            if (orderType.GetValue(0).ToString().Equals("2"))
                            {
                                OrderDocumentType = DocumentType.DocumentTypeVentePrepaLivraison;
                            }
                            else if (orderType.GetValue(0).ToString().Equals("3"))
                            {
                                OrderDocumentType = DocumentType.DocumentTypeVenteLivraison;
                            }
                            else if (orderType.GetValue(0).ToString().Equals("6"))
                            {
                                OrderDocumentType = DocumentType.DocumentTypeVenteFacture;
                            }
                            else if (orderType.GetValue(0).ToString().Equals("7"))
                            {
                                OrderDocumentType = DocumentType.DocumentTypeVenteFactureCpta;
                            }
                        }
                        test = item.OrderID;
                        if (OrderDocumentType.ToString().Equals("DocumentTypeVenteCommande"))
                        {
                            continue;
                        }
                        else
                        {
                            if (OrderDocumentType.ToString().Equals(statut1.Split('_')[0]))
                            {
                                UtilsWebservices.SendDataJson(JsonConvert.SerializeObject(UpdateStatusOnWoocommerce(statutMagento1)), @"wp-json/wc/v3/orders/" + item.OrderID);
                                item.OrderType = statut1.Split('_')[0];
                                col.Update(item);
                                //col.Update()
                                File.AppendAllText("Log\\statut.txt", DateTime.Now + " stat1  " + item.DO_Ref + " " + item.OrderType + Environment.NewLine);
                                continue;
                            }
                            if (OrderDocumentType.ToString().Equals(statut2.Split('_')[0]))
                            {
                                UtilsWebservices.SendDataJson(JsonConvert.SerializeObject(UpdateStatusOnWoocommerce(statutMagento2)), @"wp-json/wc/v3/orders/" + item.OrderID);
                                item.OrderType = statut2.Split('_')[0];
                                col.Update(item);
                                File.AppendAllText("Log\\statut.txt", DateTime.Now + " stat2  " + item.DO_Ref + " " + item.OrderType + Environment.NewLine);
                                //col.Update()
                                continue;
                            }
                            if (OrderDocumentType.ToString().Equals(statut3.Split('_')[0]))
                            {
                                UtilsWebservices.SendDataJson(JsonConvert.SerializeObject(UpdateStatusOnWoocommerce( statutMagento3)), @"wp-json/wc/v3/orders/" + item.OrderID);
                                col.Delete(item.Id);
                                File.AppendAllText("Log\\statut.txt", DateTime.Now + " stat3  " + item.DO_Ref + " " + item.OrderType + Environment.NewLine);
                                continue;
                            }
                        }
                        DB.Disconnect();
                    }
                }
            }
            catch (Exception s)
            {
                UtilsMail.SendErrorMail(DateTime.Now + s.Message + Environment.NewLine + s.StackTrace + Environment.NewLine, "UPDATE STATUT ORDER " + test);
            }
        }

        /// <summary>
        /// Crée une nouvelle commande pour un utilisateur.
        /// </summary>
        /// <param name="orderMagento">The orderMagento<see cref="orderSearch[0]Item"/>.</param>
        /// <param name="CT_Num">Client.</param>
        /// <param name="customerMagento">The customerMagento<see cref="CustomerSearch"/>.</param>
        public static void AddNewOrderForCustomer(OrderItemsWoocommerce orderWoocommerce, string CT_Num, customerSearchByEmailWoocommerce customerMagento)
        {
            File.AppendAllText("Log\\Commande.txt", DateTime.Now + "Begin Creation de commande : " + orderWoocommerce.Id.ToString() + Environment.NewLine);
            var gescom = SingletonConnection.Instance.Gescom;

            // création de l'entête de la commande 

            IBOClient3 customer = gescom.CptaApplication.FactoryClient.ReadNumero(CT_Num);
            IBODocumentVente3 order = gescom.FactoryDocumentVente.CreateType(DocumentType.DocumentTypeVenteLivraison);
            
            order.SetDefault();
            order.SetDefaultClient(customer);
            order.DO_Date = DateTime.Now;

            // TODO Manage Order Carrier
            /*string code_relais = "";
            try
            {
                code_relais = orderWoocommerce.ExtensionAttributes.DpdPickupId.ToString() + " ";
            }
            catch (Exception exception)
            {
                code_relais = "";
            }*/

            /*try
            {
                string carrier_id = "default";

                if (orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Method.Equals("dpd_predict"))
                {
                    carrier_id = "1";

                }
                if (orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Method.Equals("colissimo_homecl") || orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Method.Equals("colissimo_homesi"))
                {
                    carrier_id = "2";
                }
                if (orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Method.Equals("flatrate_flatrate"))
                {
                    carrier_id = "3";
                }
                if (orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Method.Equals("dpd_pickup"))
                {
                    carrier_id = "4";
                }
                order.Expedition = gescom.FactoryExpedition.ReadIntitule(UtilsConfig.OrderCarrierMapping[carrier_id]);
            }
            catch (Exception s)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now + "problème order expidition" + Environment.NewLine);
                sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                UtilsMail.SendErrorMail(DateTime.Now + s.Message + Environment.NewLine + s.StackTrace + Environment.NewLine, "TRANSPORTEUR");
                File.AppendAllText("Log\\order.txt", sb.ToString());
                sb.Clear();
            }*/
            order.Souche = gescom.FactorySoucheVente.ReadIntitule(UtilsConfig.Souche);
            order.DO_Ref = orderWoocommerce.Id.ToString();
            order.SetDefaultDO_Piece();
            // on définis l'adresse de livraison de la commande
            /*if (orderWoocommerce.PaymentMethod.Equals("checkmo") || orderWoocommerce.PaymentMethod.Equals("banktransfer"))
            {
                order.DO_Statut = DocumentStatutType.DocumentStatutTypeConfirme;// statut commande bloqué dans sage
            }
            else
            {
                //order.DO_Statut = DocumentStatutType.DocumentStatutTypeSaisie; // statut commande SAISIE dans sage
            }*/

            //order.Collaborateur = gescom.CptaApplication.FactoryCollaborateur.ReadNomPrenom("Saisie Web", "");
            order.Write();
            /*try
            {
                if (orderWoocommerce.Payment.Method.Equals("sogecommerce_standard"))
                {
                    IBODocumentAcompte3 acompte = (IBODocumentAcompte3)order.FactoryDocumentAcompte.Create();
                    acompte.DR_Montant = orderWoocommerce.BaseTotalDue;
                    acompte.DR_Date = DateTime.Now;
                    foreach (IBPReglement3 reglement in gescom.CptaApplication.FactoryReglement.List)
                    {
                        if (reglement.R_Intitule.Equals("Carte Bancaire"))
                        {
                            acompte.Reglement = reglement;
                            break;
                        }
                    }

                    acompte.DR_Libelle = "Acompte";
                    acompte.Write();
                    order.Write();
                }
            }
            catch (Exception exc)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now + "probleme Acompte : " + orderWoocommerce.IncrementId.ToString() + Environment.NewLine);
                sb.Append(DateTime.Now + exc.Message + Environment.NewLine);
                sb.Append(DateTime.Now + exc.StackTrace + Environment.NewLine);
                File.AppendAllText("Log\\acompte.txt", sb.ToString());
                sb.Clear();
            }*/
             string intitule1 = "";
            intitule1 = orderWoocommerce.Shipping.FirstName + " " + orderWoocommerce.Shipping.LastName;
            if (intitule1.Length > 35)
            {
                intitule1 = intitule1.Substring(0, 35);
            }

            bool asAdressMatch = false;
            IBOClientLivraison3 currentAdress = null;
            if (!String.IsNullOrEmpty(orderWoocommerce.Shipping.Company)
                && !String.IsNullOrEmpty(intitule1)
                )
            {
                foreach (IBOClientLivraison3 tmpAdress in customer.FactoryClientLivraison.List)
                {

                    if (tmpAdress.LI_Intitule.Equals(orderWoocommerce.Shipping.Company.ToUpper())
                        || tmpAdress.LI_Intitule.Equals(intitule1.ToUpper())
                        )
                    {
                        currentAdress = tmpAdress;
                        asAdressMatch = true;
                        break;
                    }
                }
                /*string intitule = "";
                intitule =  orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.FirstName + " " + orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.LastName;
                if (intitule.Length > 35)
                {
                    intitule.Substring(0, 35);
                }
                if (tmpAdress.LI_Intitule.Equals(intitule.ToUpper()))
                {
                    currentAdress = tmpAdress;
                    asAdressMatch = true;
                    break;
                }*/

            }

            if (asAdressMatch)
            {
                try
                {
                    IBOClientLivraison3 adress;
                    adress = currentAdress;

                }
                catch(Exception e)
                {

                }

            }
            else{

            

            try
            {
                // si on a trouver aucune adresse coresspondante sur le client alors on la crée
                IBOClientLivraison3 adress;
                adress = (IBOClientLivraison3)customer.FactoryClientLivraison.Create();
                adress.SetDefault();



                /*if (!asAdressMatch)
                {
                    adress = (IBOClientLivraison3)customer.FactoryClientLivraison.Create();
                    adress.SetDefault();
                }
                else
                {
                    adress = currentAdress;
                }*/

                adress.Telecom.EMail = customer.Telecom.EMail;
                /*
                try
                {
                    string carrier_id = jsonOrder["order_carriere"].ToString();
                    adress.Expedition = gescom.FactoryExpedition.ReadIntitule(UtilsConfig.OrderCarrierMapping[carrier_id]);
                }

                */
                if (!String.IsNullOrEmpty(orderWoocommerce.Shipping.Company))
                {
                    if (orderWoocommerce.Shipping.Company.Length > 35)
                    {
                        adress.LI_Intitule = orderWoocommerce.Shipping.Company.ToUpper().Substring(0, 35);
                    }
                    else
                    {
                        adress.LI_Intitule = orderWoocommerce.Shipping.Company.ToUpper();
                    }
                }
                else
                {
                    string intitule = "";
                    intitule = orderWoocommerce.Shipping.FirstName + " " + orderWoocommerce.Shipping.LastName;
                    if (intitule.Length > 35)
                    {
                        intitule = intitule.Substring(0, 35);
                    }
                    adress.LI_Intitule = intitule.ToUpper();
                }

                // Setup champ contact dans adress
                if ((orderWoocommerce.Shipping.FirstName + " " + orderWoocommerce.Shipping.LastName).Length > 35)
                {
                    adress.LI_Contact = (orderWoocommerce.Shipping.FirstName + " " + orderWoocommerce.Shipping.LastName).Substring(0, 35);
                }
                else
                {
                    adress.LI_Contact = orderWoocommerce.Shipping.FirstName + " " + orderWoocommerce.Shipping.LastName;
                }

                if (orderWoocommerce.Shipping.Address1.ToString().Length > 35)
                {
                    adress.Adresse.Adresse = orderWoocommerce.Shipping.Address1.ToString().Substring(0, 35);
                }
                else
                {
                    adress.Adresse.Adresse = orderWoocommerce.Shipping.Address1.ToString();
                }

                if (orderWoocommerce.Shipping.Address2.ToString().Length > 35)
                {
                    adress.Adresse.Complement = orderWoocommerce.Shipping.Address2.ToString().Substring(0, 35);
                }
                else
                {
                    adress.Adresse.Complement = orderWoocommerce.Shipping.Address2.ToString();
                }



                adress.Adresse.CodePostal = orderWoocommerce.Shipping.Postcode.ToString();
                adress.Adresse.Ville = orderWoocommerce.Shipping.City.ToString();
                /*var region = CultureInfo
                                        .GetCultures(CultureTypes.SpecificCultures)
                                        .Select(ci => new RegionInfo(ci.LCID))
                                        .FirstOrDefault(rg => rg.TwoLetterISORegionName == orderWoocommerce.Shipping.Country);*/
                    //CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("fr-FR");
                RegionInfo myRI1 = new RegionInfo(orderWoocommerce.Shipping.Country);
                adress.Adresse.Pays = myRI1.DisplayName.ToString();
                adress.Telecom.Telephone = orderWoocommerce.Shipping.Phone;
                //adress.Telecom.Telecopie = jsonOrder["shipping_phone_mobile"].ToString();

                if (String.IsNullOrEmpty(UtilsConfig.CondLivraison))
                {
                    // pas de configuration renseigner pour CondLivraison par defaut
                    // todo log
                }
                else
                {
                    adress.ConditionLivraison = gescom.FactoryConditionLivraison.ReadIntitule(UtilsConfig.CondLivraison);
                }
                /*try
                {
                    string carrier_id = "default";
                    if (orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Method.Equals("dpd_predict"))
                    {

                        carrier_id = "1";

                    }
                    if (orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Method.Equals("colissimo_homecl") || orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Method.Equals("colissimo_homesi"))
                    {
                        carrier_id = "2";
                    }
                    if (orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Method.Equals("flatrate_flatrate"))
                    {
                        carrier_id = "3";
                    }
                    if (orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Method.Equals("dpd_pickup"))
                    {
                        carrier_id = "4";
                    }
                    adress.Expedition = gescom.FactoryExpedition.ReadIntitule(UtilsConfig.OrderCarrierMapping[carrier_id]);
                }
                catch (Exception s)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DateTime.Now + "problème order expidition" + Environment.NewLine);
                    sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                    sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                    UtilsMail.SendErrorMail(DateTime.Now + s.Message + Environment.NewLine + s.StackTrace + Environment.NewLine, "TRANSPORTEUR");
                    File.AppendAllText("Log\\order.txt", sb.ToString());
                    sb.Clear();
                }*/
                adress.Write();

                // on ajoute une adresse par defaut sur la fiche client si il y en a pas

                // On met à jour l'adresse de facturation du client

                /*if (customerMagento.HasMethod("DefaultBilling"))
                {
                    Object.CustomerSearch.Address defaultAddress = new Object.CustomerSearch.Address();
                    foreach (Object.CustomerSearch.Address addressCustomer in customerMagento.Addresses)
                    {
                        if (addressCustomer.Id == customerMagento.DefaultBilling)
                        {
                            defaultAddress = addressCustomer;
                            break;
                        }
                    }
                    if (defaultAddress.Address1.Length > 35)
                    {
                        customer.Adresse.Adresse = defaultAddress.Address1.ToString().Substring(0, 35);
                    }
                    else
                    {
                        customer.Adresse.Adresse = defaultAddress.Address1.ToString();
                    }
                    if (defaultAddress.Street.Count > 1)
                    {
                        customer.Adresse.Complement = defaultAddress.Address2.ToString();
                    }

                    customer.Adresse.CodePostal = defaultAddress.Postcode.ToString();
                    customer.Adresse.Ville = defaultAddress.City.ToString();
                    var region1 = CultureInfo
                                    .GetCultures(CultureTypes.SpecificCultures)
                                    .Select(ci => new RegionInfo(ci.LCID))
                                    .FirstOrDefault(rg => rg.TwoLetterISORegionName == defaultAddress.CountryId);
                    customer.Adresse.Pays = region1.DisplayName.ToString();
                    customer.Telecom.Telephone = defaultAddress.Telephone.ToString();
                    customer.Write();
                }
                else
                {
                    customer.Adresse.Adresse = adress.Adresse.Adresse.ToString();
                    if (!String.IsNullOrEmpty(adress.Adresse.Complement.ToString()))
                    {
                        customer.Adresse.Complement = adress.Adresse.Complement.ToString();
                    }
                    customer.Adresse.CodePostal = adress.Adresse.CodePostal.ToString();
                    customer.Adresse.Ville = adress.Adresse.Ville.ToString();
                    customer.Adresse.Pays = adress.Adresse.Pays.ToString();
                    customer.Telecom.Telephone = adress.Telecom.Telephone.ToString();
                    customer.Write();
                }*/

                //order.FraisExpedition = orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Total.ShippingAmount;
                order.LieuLivraison = adress;
            }
            catch (Exception s)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now + "problem in customer adress" + Environment.NewLine);
                sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                File.AppendAllText("Log\\order.txt", sb.ToString());
                sb.Clear();
            }
        }
            order.Write();
            /*try
            {
                //take care of infolibre commande
                if (orderWoocommerce.StatusHistories.Count > 0)
                {
                    foreach (StatusHistory statusHistory in orderWoocommerce.StatusHistories)
                    {
                        if (String.IsNullOrEmpty(statusHistory.Status) && statusHistory.IsVisibleOnFront.ToString().Equals("1") && !String.IsNullOrEmpty(statusHistory.Comment) && statusHistory.CreatedAt.Equals(orderWoocommerce.CreatedAt))
                        {
                            order.InfoLibre[3] = statusHistory.Comment;
                        }
                    }
                    //order.InfoLibre[3] = orderWoocommerce.StatusHistories[0]. Payment.Po_number.ToString();
                    order.Write();
                }
            }
            catch (Exception exception)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now + "probleme status histories" + Environment.NewLine);
                sb.Append(DateTime.Now + exception.Message + Environment.NewLine);
                sb.Append(DateTime.Now + exception.StackTrace + Environment.NewLine);
                File.AppendAllText("Log\\order.txt", sb.ToString());
                sb.Clear();
            }*/

            // création des lignes de la commandes
            try
            {

                foreach (LineItem product in orderWoocommerce.LineItems)
                {
                    
                    IBODocumentVenteLigne3 docLigne = (IBODocumentVenteLigne3)order.FactoryDocumentLigne.Create();
                    var ArticleExist = gescom.FactoryArticle.ExistReference(product.Sku);
                    if (ArticleExist)
                    {
                        IBOArticle3 article1 = gescom.FactoryArticle.ReadReference(product.Sku.ToString());
                        Article article = new Article(article1);

                        if(!article.HaveNomenclature && !article.isGamme && !article.IsDoubleGamme)
                        {
                            docLigne.SetDefaultArticle(article1, Int32.Parse(product.Quantity.ToString()));
                            docLigne.DL_PrixUnitaire = double.Parse(product.Price.Replace('.', ','));
                            //docLigne.DL_PUTTC = 10.9;
                            //docLigne.Remise.Remise[1].REM_Type = RemiseType.RemiseTypePourcent;
                            //docLigne.Remise.Remise[1].REM_Valeur = 5;
                        }

                        // insertion des article à nomenclature
                        if (article.HaveNomenclature)
                        {
                            docLigne.SetDefaultArticle(article1, Int32.Parse(product.Quantity.ToString()));
                            //docLigne.Remise.Remise[1].REM_Type = RemiseType.RemiseTypePourcent;
                            //docLigne.Remise.Remise[1].REM_Valeur = 5;
                            docLigne.Write();
                            foreach (IBOArticleNomenclature3 item in article1.FactoryArticleNomenclature.List)
                            {
                                IBODocumentVenteLigne3 docligneComposant = (IBODocumentVenteLigne3)order.FactoryDocumentLigne.Create();
                                double qte = item.NO_Qte * Int32.Parse(product.Quantity.ToString());
                                docligneComposant.SetDefaultArticle(item.ArticleComposant, qte);
                                docligneComposant.ArticleCompose = item.Article;
                                //docLigne.Remise.Remise[1].REM_Type = RemiseType.RemiseTypePourcent;
                                //docLigne.Remise.Remise[1].REM_Valeur = 5;
                                docligneComposant.Write();
                            }
                        }
                        
                        if (article.isMonoGamme)
                        {

                            // produit à simple gamme
                            IBOArticleGammeEnum3 articleEnum = ControllerArticle.GetArticleGammeEnum1(article1, product);
                            docLigne.SetDefaultArticleMonoGamme(articleEnum, Int32.Parse(product.Quantity.ToString()));
                            docLigne.DL_PrixUnitaire = double.Parse(product.Price.Replace('.', ','));
                            //docLigne.Remise.Remise[1].REM_Type = RemiseType.RemiseTypePourcent;
                            //docLigne.Remise.Remise[1].REM_Valeur = 5;
                        }
                        else if (article.IsDoubleGamme)
                        {

                            if (product.MetaData[0].Key.ToUpper().Equals(article.IntituleGamme1.ToUpper()))
                            {
                                IBOArticleGammeEnum3 articleEnum = ControllerArticle.GetArticleGammeEnum1(article1, product);
                                IBOArticleGammeEnum3 articleEnum2 = ControllerArticle.GetArticleGammeEnum2(article1, product);
                                docLigne.SetDefaultArticleDoubleGamme(articleEnum, articleEnum2, Int32.Parse(product.Quantity.ToString()));
                                docLigne.DL_PrixUnitaire = double.Parse(product.Price.Replace('.',','));
                                //docLigne.Remise.Remise[1].REM_Type = RemiseType.RemiseTypePourcent;
                                //docLigne.Remise.Remise[1].REM_Valeur = 5;

                            }
                            else
                            {
                                IBOArticleGammeEnum3 articleEnum = ControllerArticle.GetArticleGammeEnum1(article1, product);
                                IBOArticleGammeEnum3 articleEnum2 = ControllerArticle.GetArticleGammeEnum2(article1, product);
                                docLigne.SetDefaultArticleDoubleGamme(articleEnum2, articleEnum, Int32.Parse(product.Quantity.ToString()));
                                docLigne.DL_PrixUnitaire = double.Parse(product.Price.Replace('.', ','));
                                //docLigne.Remise.Remise[1].REM_Type = RemiseType.RemiseTypePourcent;
                                //docLigne.Remise.Remise[1].REM_Valeur = 5;
                            }

                            // produit à double gamme

                        }
                       
                        

                    }
                    else
                    {
                        //article à conditionnement
                        String[] SKU = product.Sku.Split('|');
                        IBOArticle3 article1 = gescom.FactoryArticle.ReadReference(SKU[0].ToString());
                        Article CondArticle = new Article(article1);

                        if (CondArticle.conditionnements.Count > 0)
                        {
                            
                            IBOArticleCond3 articleCond3 = ControllerArticle.GetArticleConditionnementEnum(article1);
                            docLigne.SetDefaultArticleConditionnement(articleCond3, Int32.Parse(product.Quantity.ToString()));
                            docLigne.DL_PrixUnitaire = double.Parse(product.Price.Replace('.', ','));
                            //docLigne.Remise.Remise[1].REM_Type = RemiseType.RemiseTypePourcent;
                            //docLigne.Remise.Remise[1].REM_Valeur = 5;
                        }

                    }
                    docLigne.Write();
                }
                //IBODocumentLigne3 docLignePort = (IBODocumentLigne3)order.FactoryDocumentLigne.Create();
                //string frais = orderWoocommerce.ShippingTotal;
                // reffrais = "PORTEMB1";

                //IBOArticle3 articlePort = gescom.FactoryArticle.ReadReference("PORTEMB1");

                //docLignePort.SetDefaultArticle(articlePort, Int32.Parse("1"));
                //docLignePort.DL_PrixUnitaire = double.Parse(frais, System.Globalization.CultureInfo.InvariantCulture);
                //docLignePort.Write();
                /*IBODocumentLigne3 docLigne1 = (IBODocumentLigne3)order.FactoryDocumentLigne.Create();
                docLigne1.SetDefaultArticle(gescom.FactoryArticle.ReadReference(UtilsConfig.DefaultTransportReference), 1);
                docLigne1.DL_PrixUnitaire = Convert.ToDouble(orderWoocommerce.ShippingAmount.ToString().Replace('.', ','));
                docLigne1.Write();*/
            }
            catch (Exception e)
            {
                UtilsWebservices.sendNewFlagWoocommerce("", @"/wp-json/wc/v3/orders-by-flag/search?order=2&id=" + orderWoocommerce.Id.ToString(), "PUT");
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now + "problem Document line" + Environment.NewLine);
                sb.Append(DateTime.Now + e.Message + Environment.NewLine);
                sb.Append(DateTime.Now + e.StackTrace + Environment.NewLine);
                UtilsMail.SendErrorMail(DateTime.Now + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine, "COMMANDE LIGNE");
                File.AppendAllText("Log\\order.txt", sb.ToString());
                sb.Clear();
                // UtilsWebservices.SendDataNoParse(UtilsConfig.BaseUrl + EnumEndPoint.Commande.Value, "errorOrder&orderID=" + jsonOrder["id_order"]);
                order.Remove();
                return;
            }
            //order.Expedition = orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Total.ShippingAmount;
            //order.Write();
            File.AppendAllText("Log\\Commande.txt", DateTime.Now + "End Creation de commande : " + orderWoocommerce.Id.ToString() + " ,Client : " + order.Client.CT_Num + " ,Do_piece : " + order.DO_Piece + Environment.NewLine);
            addOrderToLocalDB(orderWoocommerce.Id.ToString(), order.Client.CT_Num, order.DO_Piece, order.DO_Ref, orderWoocommerce.Id.ToString());
            // TODO updateOrderFlag using custom PHP script
            //var jsonFlag = JsonConvert.SerializeObject(UpdateOrderFlag(orderWoocommerce.Id.ToString(), orderWoocommerce.Id.ToString(), "0"));
            UtilsWebservices.sendNewFlagWoocommerce("", @"/wp-json/wc/v3/orders-by-flag/search?order=0&id="+orderWoocommerce.Id.ToString(), "PUT");
        }

        public static Boolean ExistAdress(string adress, string email)
        {
            string sql = "SELECT [LI_No] FROM [" + System.Configuration.ConfigurationManager.AppSettings["DBNAME"].ToString() + "].[dbo].[F_LIVRAISON] where CT_Num = '"+UtilsConfig.PrefixClient.ToString()+"' and LI_Adresse ='" + adress.Replace("'", "''") + "' and LI_EMail = '" + email + "'";
            File.AppendAllText("Log\\test.txt", DateTime.Now + " requete SQL exist adress: " + sql + Environment.NewLine);
            SqlDataReader AddressLiNo = DB.Select(sql);
            while (AddressLiNo.Read())
            {
                return true;
            }
            DB.Disconnect();
            return false;
        }

        public static string GetIntitulAdress(string adress, string email)
        {
            string sql = "SELECT [LI_Intitule] FROM [" + System.Configuration.ConfigurationManager.AppSettings["DBNAME"].ToString() + "].[dbo].[F_LIVRAISON] where CT_Num = '" + UtilsConfig.PrefixClient.ToString() + "' and LI_Adresse ='" + adress.Replace("'", "''") + "' and LI_EMail = '" + email + "'";
            File.AppendAllText("Log\\test.txt", DateTime.Now + " requete SQL get intitule : " + sql + Environment.NewLine);
            SqlDataReader AddressLiNo = DB.Select(sql);
            while (AddressLiNo.Read())
            {
                return AddressLiNo.GetValue(0).ToString();
            }
            DB.Disconnect();
            return "";
        }
        public static void createAdress(OrderItemsWoocommerce orderWoocommerce, IBOClient3 customer, string AdressType)
        {
            int AdressNumber = 0;
            SqlDataReader DbAdressNumber = DB.Select("select count(*) from [" + System.Configuration.ConfigurationManager.AppSettings["DBNAME"].ToString() + "].[dbo].[F_LIVRAISON] where LI_EMail like '" + orderWoocommerce.Billing.Email.ToString() + "'");
            while (DbAdressNumber.Read())
            {
                AdressNumber = Int32.Parse(DbAdressNumber.GetValue(0).ToString());
            }
            DB.Disconnect();
            // requete SQL
            if (AdressNumber == 0)
            {
                AdressNumber = 1;
            }
            else
            {
                AdressNumber++;
            }
            IBOClientLivraison3 adress = (IBOClientLivraison3)customer.FactoryClientLivraison.Create();
            adress.SetDefault();
            string intitule = "";
            if (AdressNumber > 9)
            {
                intitule = orderWoocommerce.Shipping.LastName + " " + orderWoocommerce.Shipping.FirstName.ToUpper() + " " + AdressNumber.ToString();
            }
            else
            {
                intitule = orderWoocommerce.Shipping.LastName + " " + orderWoocommerce.Shipping.FirstName.ToUpper() + " 0" + AdressNumber.ToString();
            }
            adress.LI_Intitule = intitule;
            adress.LI_Contact = AdressType;
            adress.Adresse.Adresse = orderWoocommerce.Shipping.Address1.ToString();
            adress.Telecom.EMail = orderWoocommerce.Billing.Email.ToString();
            adress.Telecom.Telephone = orderWoocommerce.Billing.Phone.ToString();
            adress.Adresse.CodePostal = orderWoocommerce.Shipping.Postcode.ToString();
            adress.Adresse.Ville = orderWoocommerce.Shipping.City.ToString();
            RegionInfo myRI1 = new RegionInfo(orderWoocommerce.Shipping.Country);
            adress.Adresse.Pays = myRI1.DisplayName.ToString();
            adress.Write();
            
        }

        public static void createBillingAdress(OrderItemsWoocommerce orderWoocommerce, IBOClient3 customer, string AdressType)
        {
            int AdressNumber = 0;
            SqlDataReader DbAdressNumber = DB.Select("select count(*) from [" + System.Configuration.ConfigurationManager.AppSettings["DBNAME"].ToString() + "].[dbo].[F_LIVRAISON] where LI_EMail like '" + orderWoocommerce.Billing.Email.ToString() + "'");
            while (DbAdressNumber.Read())
            {
                AdressNumber = Int32.Parse(DbAdressNumber.GetValue(0).ToString());
            }
            DB.Disconnect();
            // requete SQL
            if (AdressNumber == 0)
            {
                AdressNumber = 1;
            }
            else
            {
                AdressNumber++;
            }
            IBOClientLivraison3 adress = (IBOClientLivraison3)customer.FactoryClientLivraison.Create();
            adress.SetDefault();
            string intitule = "";
            if (AdressNumber > 9)
            {
                intitule = orderWoocommerce.Billing.LastName + " " + orderWoocommerce.Billing.FirstName.ToUpper() + " " + AdressNumber.ToString();
            }
            else
            {
                intitule = orderWoocommerce.Billing.LastName + " " + orderWoocommerce.Billing.FirstName.ToUpper() + " 0" + AdressNumber.ToString();
            }
            adress.LI_Intitule = intitule;
            adress.LI_Contact = AdressType;
            adress.Adresse.Adresse = orderWoocommerce.Billing.Address1.ToString();
            adress.Telecom.EMail = orderWoocommerce.Billing.Email.ToString();
            adress.Telecom.Telephone = orderWoocommerce.Billing.Phone.ToString();
            adress.Adresse.CodePostal = orderWoocommerce.Billing.Postcode.ToString();
            adress.Adresse.Ville = orderWoocommerce.Billing.City.ToString();
            RegionInfo myRI1 = new RegionInfo(orderWoocommerce.Billing.Country);
            adress.Adresse.Pays = myRI1.DisplayName.ToString();
            adress.Write();

        }

        public static void createShippingAdress(OrderItemsWoocommerce orderWoocommerce, IBOClient3 customer, string AdressType)
        {
            int AdressNumber = 0;
            SqlDataReader DbAdressNumber = DB.Select("select count(*) from [" + System.Configuration.ConfigurationManager.AppSettings["DBNAME"].ToString() + "].[dbo].[F_LIVRAISON] where LI_EMail like '" + orderWoocommerce.Billing.Email.ToString() + "'");
            while (DbAdressNumber.Read())
            {
                AdressNumber = Int32.Parse(DbAdressNumber.GetValue(0).ToString());
            }
            DB.Disconnect();
            // requete SQL
            if (AdressNumber == 0)
            {
                AdressNumber = 1;
            }
            else
            {
                AdressNumber++;
            }
            IBOClientLivraison3 adress = (IBOClientLivraison3)customer.FactoryClientLivraison.Create();
            adress.SetDefault();
            string intitule = "";
            if (AdressNumber > 9)
            {
                intitule = orderWoocommerce.Shipping.LastName + " " + orderWoocommerce.Shipping.FirstName.ToUpper() + " " + AdressNumber.ToString();
            }
            else
            {
                intitule = orderWoocommerce.Shipping.LastName + " " + orderWoocommerce.Shipping.FirstName.ToUpper() + " 0" + AdressNumber.ToString();
            }
            adress.LI_Intitule = intitule;
            adress.LI_Contact = AdressType;
            adress.Adresse.Adresse = orderWoocommerce.Shipping.Address1.ToString();
            adress.Telecom.EMail = orderWoocommerce.Billing.Email.ToString();
            adress.Telecom.Telephone = orderWoocommerce.Billing.Phone.ToString();
            adress.Adresse.CodePostal = orderWoocommerce.Shipping.Postcode.ToString();
            adress.Adresse.Ville = orderWoocommerce.Shipping.City.ToString();
            RegionInfo myRI1 = new RegionInfo(orderWoocommerce.Shipping.Country);
            adress.Adresse.Pays = myRI1.DisplayName.ToString();
            adress.Write();

        }



        public static void AddNewOrderForCustomerPrefix(OrderItemsWoocommerce orderWoocommerce)
        {
            File.AppendAllText("Log\\Commande.txt", DateTime.Now + "Begin Creation de commande : " + orderWoocommerce.Id.ToString() + Environment.NewLine);
            var gescom = SingletonConnection.Instance.Gescom;

            // création de l'entête de la commande 
            int BillingLI_NO = 0;
            int ShippingLi_No = 0;
            string intitulAdressLivraison = "";
            string intitulAdressFacturation = "";

            IBOClient3 customer = gescom.CptaApplication.FactoryClient.ReadNumero(UtilsConfig.PrefixClient.ToString());

            if (ExistAdress(orderWoocommerce.Billing.Address1.ToString(),orderWoocommerce.Billing.Email.ToString() ))
            {
                intitulAdressFacturation = GetIntitulAdress(orderWoocommerce.Billing.Address1.ToString(), orderWoocommerce.Billing.Email.ToString());
            }
            if (ExistAdress(orderWoocommerce.Shipping.Address1.ToString(), orderWoocommerce.Billing.Email.ToString()))
            {
                intitulAdressLivraison = GetIntitulAdress(orderWoocommerce.Shipping.Address1.ToString(), orderWoocommerce.Billing.Email.ToString());
            }
            if (String.IsNullOrEmpty(intitulAdressFacturation) && String.IsNullOrEmpty(intitulAdressLivraison))
            {
                if (orderWoocommerce.Shipping.Address1.Equals(orderWoocommerce.Billing.Address1))
                {
                    File.AppendAllText("Log\\test.txt", DateTime.Now + " Création addresse livraison/facturation : " + Environment.NewLine);
                    createAdress(orderWoocommerce, customer, "Livraison/Facturation");
                }
                else
                {
                    if (String.IsNullOrEmpty(intitulAdressFacturation))
                    {
                        createBillingAdress(orderWoocommerce, customer, "Facturation");
                        File.AppendAllText("Log\\test.txt", DateTime.Now + " Création addresse facturation : " + Environment.NewLine);
                    }
                    if (String.IsNullOrEmpty(intitulAdressLivraison))
                    {
                        createShippingAdress(orderWoocommerce, customer, "Livraison");
                        File.AppendAllText("Log\\test.txt", DateTime.Now + " Création addresse livraison : " + Environment.NewLine);
                    }
                }
            }


            IBODocumentVente3 order = gescom.FactoryDocumentVente.CreateType(DocumentType.DocumentTypeVenteLivraison);

            order.SetDefault();
            order.SetDefaultClient(customer);
            order.DO_Date = DateTime.Now;

            order.Souche = gescom.FactorySoucheVente.ReadIntitule(UtilsConfig.Souche);
            order.DO_Ref = orderWoocommerce.Id.ToString();
            order.SetDefaultDO_Piece();
            order.Write();
           
            string intitule1 = "";
            intitule1 = orderWoocommerce.Shipping.FirstName + " " + orderWoocommerce.Shipping.LastName;
            if (intitule1.Length > 35)
            {
                intitule1 = intitule1.Substring(0, 35);
            }

            bool asAdressMatch = false;
            IBOClientLivraison3 currentAdress = null;
            if (!String.IsNullOrEmpty(orderWoocommerce.Shipping.Company)
                && !String.IsNullOrEmpty(intitule1)
                )
            {
                foreach (IBOClientLivraison3 tmpAdress in customer.FactoryClientLivraison.List)
                {

                    if (tmpAdress.LI_Intitule.Equals(orderWoocommerce.Shipping.Company.ToUpper())
                        || tmpAdress.LI_Intitule.Equals(intitule1.ToUpper())
                        )
                    {
                        currentAdress = tmpAdress;
                        asAdressMatch = true;
                        break;
                    }
                }
          

            }

            if (asAdressMatch)
            {
                try
                {
                    IBOClientLivraison3 adress;
                    adress = currentAdress;

                }
                catch (Exception e)
                {

                }

            }
            else
            {

                try
                {
                    // si on a trouver aucune adresse coresspondante sur le client alors on la crée
                    IBOClientLivraison3 adress;
                    adress = (IBOClientLivraison3)customer.FactoryClientLivraison.Create();
                    adress.SetDefault();

                    adress.Telecom.EMail = customer.Telecom.EMail;
                    
                    if (!String.IsNullOrEmpty(orderWoocommerce.Shipping.Company))
                    {
                        if (orderWoocommerce.Shipping.Company.Length > 35)
                        {
                            adress.LI_Intitule = orderWoocommerce.Shipping.Company.ToUpper().Substring(0, 35);
                        }
                        else
                        {
                            adress.LI_Intitule = orderWoocommerce.Shipping.Company.ToUpper();
                        }
                    }
                    else
                    {
                        string intitule = "";
                        intitule = orderWoocommerce.Shipping.FirstName + " " + orderWoocommerce.Shipping.LastName;
                        if (intitule.Length > 35)
                        {
                            intitule = intitule.Substring(0, 35);
                        }
                        adress.LI_Intitule = intitule.ToUpper();
                    }

                    // Setup champ contact dans adress
                    if ((orderWoocommerce.Shipping.FirstName + " " + orderWoocommerce.Shipping.LastName).Length > 35)
                    {
                        adress.LI_Contact = (orderWoocommerce.Shipping.FirstName + " " + orderWoocommerce.Shipping.LastName).Substring(0, 35);
                    }
                    else
                    {
                        adress.LI_Contact = orderWoocommerce.Shipping.FirstName + " " + orderWoocommerce.Shipping.LastName;
                    }

                    if (orderWoocommerce.Shipping.Address1.ToString().Length > 35)
                    {
                        adress.Adresse.Adresse = orderWoocommerce.Shipping.Address1.ToString().Substring(0, 35);
                    }
                    else
                    {
                        adress.Adresse.Adresse = orderWoocommerce.Shipping.Address1.ToString();
                    }

                    if (orderWoocommerce.Shipping.Address2.ToString().Length > 35)
                    {
                        adress.Adresse.Complement = orderWoocommerce.Shipping.Address2.ToString().Substring(0, 35);
                    }
                    else
                    {
                        adress.Adresse.Complement = orderWoocommerce.Shipping.Address2.ToString();
                    }



                    adress.Adresse.CodePostal = orderWoocommerce.Shipping.Postcode.ToString();
                    adress.Adresse.Ville = orderWoocommerce.Shipping.City.ToString();
                    /*var region = CultureInfo
                                            .GetCultures(CultureTypes.SpecificCultures)
                                            .Select(ci => new RegionInfo(ci.LCID))
                                            .FirstOrDefault(rg => rg.TwoLetterISORegionName == orderWoocommerce.Shipping.Country);*/
                    //CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("fr-FR");
                    RegionInfo myRI1 = new RegionInfo(orderWoocommerce.Shipping.Country);
                    adress.Adresse.Pays = myRI1.DisplayName.ToString();
                    adress.Telecom.Telephone = orderWoocommerce.Shipping.Phone;
                    //adress.Telecom.Telecopie = jsonOrder["shipping_phone_mobile"].ToString();

                    if (String.IsNullOrEmpty(UtilsConfig.CondLivraison))
                    {
                        // pas de configuration renseigner pour CondLivraison par defaut
                        // todo log
                    }
                    else
                    {
                        adress.ConditionLivraison = gescom.FactoryConditionLivraison.ReadIntitule(UtilsConfig.CondLivraison);
                    }
               
                    adress.Write();

                    
                    order.LieuLivraison = adress;
                }
                catch (Exception s)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DateTime.Now + "problem in customer adress" + Environment.NewLine);
                    sb.Append(DateTime.Now + s.Message + Environment.NewLine);
                    sb.Append(DateTime.Now + s.StackTrace + Environment.NewLine);
                    File.AppendAllText("Log\\order.txt", sb.ToString());
                    sb.Clear();
                }
            }
            order.Write();
            

            // création des lignes de la commandes
            try
            {

                foreach (LineItem product in orderWoocommerce.LineItems)
                {

                    IBODocumentVenteLigne3 docLigne = (IBODocumentVenteLigne3)order.FactoryDocumentLigne.Create();
                    var ArticleExist = gescom.FactoryArticle.ExistReference(product.Sku);
                    if (ArticleExist)
                    {
                        IBOArticle3 article1 = gescom.FactoryArticle.ReadReference(product.Sku.ToString());
                        
                        Article article = new Article(article1);

                        if (!article.HaveNomenclature && !article.isGamme && !article.IsDoubleGamme)
                        {
                            docLigne.SetDefaultArticle(article1, Int32.Parse(product.Quantity.ToString()));
                            docLigne.DL_PrixUnitaire = Int32.Parse(product.Price);
                            //docLigne.Remise.Remise[1].REM_Type = RemiseType.RemiseTypePourcent;
                            //docLigne.Remise.Remise[1].REM_Valeur = 5;
                        }

                        // insertion des article à nomenclature
                        if (article.HaveNomenclature)
                        {
                            docLigne.SetDefaultArticle(article1, Int32.Parse(product.Quantity.ToString()));
                            //docLigne.Remise.Remise[1].REM_Type = RemiseType.RemiseTypePourcent;
                            //docLigne.Remise.Remise[1].REM_Valeur = 5;
                            docLigne.Write();
                            foreach (IBOArticleNomenclature3 item in article1.FactoryArticleNomenclature.List)
                            {
                                IBODocumentVenteLigne3 docligneComposant = (IBODocumentVenteLigne3)order.FactoryDocumentLigne.Create();
                                double qte = item.NO_Qte * Int32.Parse(product.Quantity.ToString());
                                docligneComposant.SetDefaultArticle(item.ArticleComposant, qte);
                                docligneComposant.ArticleCompose = item.Article;
                                //docLigne.Remise.Remise[1].REM_Type = RemiseType.RemiseTypePourcent;
                                //docLigne.Remise.Remise[1].REM_Valeur = 5;
                                docligneComposant.Write();
                            }
                        }

                        if (article.isMonoGamme)
                        {

                            // produit à simple gamme
                            IBOArticleGammeEnum3 articleEnum = ControllerArticle.GetArticleGammeEnum1(article1, product);
                            docLigne.SetDefaultArticleMonoGamme(articleEnum, Int32.Parse(product.Quantity.ToString()));
                            docLigne.DL_PrixUnitaire = Int32.Parse(product.Price);
                            //docLigne.Remise.Remise[1].REM_Type = RemiseType.RemiseTypePourcent;
                            //docLigne.Remise.Remise[1].REM_Valeur = 5;
                        }
                        else if (article.IsDoubleGamme)
                        {

                            if (product.MetaData[0].Key.ToUpper().Equals(article.IntituleGamme1.ToUpper()))
                            {
                                IBOArticleGammeEnum3 articleEnum = ControllerArticle.GetArticleGammeEnum1(article1, product);
                                IBOArticleGammeEnum3 articleEnum2 = ControllerArticle.GetArticleGammeEnum2(article1, product);
                                docLigne.SetDefaultArticleDoubleGamme(articleEnum, articleEnum2, Int32.Parse(product.Quantity.ToString()));
                                docLigne.DL_PrixUnitaire = Int32.Parse(product.Price);

                                //docLigne.Remise.Remise[1].REM_Type = RemiseType.RemiseTypePourcent;
                                //docLigne.Remise.Remise[1].REM_Valeur = 5;

   
                            }
                            else
                            {
                                IBOArticleGammeEnum3 articleEnum = ControllerArticle.GetArticleGammeEnum1(article1, product);
                                IBOArticleGammeEnum3 articleEnum2 = ControllerArticle.GetArticleGammeEnum2(article1, product);
                                docLigne.SetDefaultArticleDoubleGamme(articleEnum2, articleEnum, Int32.Parse(product.Quantity.ToString()));
                                docLigne.DL_PrixUnitaire = Int32.Parse(product.Price);

                                //docLigne.Remise.Remise[1].REM_Type = RemiseType.RemiseTypePourcent;
                                //docLigne.Remise.Remise[1].REM_Valeur = 5;


                            }

                            // produit à double gamme

                        }



                    }
                    else
                    {
                        //article à conditionnement
                        String[] SKU = product.Sku.Split('|');
                        IBOArticle3 article1 = gescom.FactoryArticle.ReadReference(SKU[0].ToString());
                        Article CondArticle = new Article(article1);

                        if (CondArticle.conditionnements.Count > 0)
                        {

                            IBOArticleCond3 articleCond3 = ControllerArticle.GetArticleConditionnementEnum(article1);
                            docLigne.SetDefaultArticleConditionnement(articleCond3, Int32.Parse(product.Quantity.ToString()));
                            docLigne.DL_PrixUnitaire = Int32.Parse(product.Price);
                            //docLigne.Remise.Remise[1].REM_Type = RemiseType.RemiseTypePourcent;
                            //docLigne.Remise.Remise[1].REM_Valeur = 5;
                        }

                    }
                    docLigne.Write();
                }
                //IBODocumentLigne3 docLignePort = (IBODocumentLigne3)order.FactoryDocumentLigne.Create();
                //string frais = orderWoocommerce.ShippingTotal;
                // reffrais = "PORTEMB1";

                //IBOArticle3 articlePort = gescom.FactoryArticle.ReadReference("PORTEMB1");

                //docLignePort.SetDefaultArticle(articlePort, Int32.Parse("1"));
                //docLignePort.DL_PrixUnitaire = double.Parse(frais, System.Globalization.CultureInfo.InvariantCulture);
                //docLignePort.Write();
                /*IBODocumentLigne3 docLigne1 = (IBODocumentLigne3)order.FactoryDocumentLigne.Create();
                docLigne1.SetDefaultArticle(gescom.FactoryArticle.ReadReference(UtilsConfig.DefaultTransportReference), 1);
                docLigne1.DL_PrixUnitaire = Convert.ToDouble(orderWoocommerce.ShippingAmount.ToString().Replace('.', ','));
                docLigne1.Write();*/
            }
            catch (Exception e)
            {
                UtilsWebservices.sendNewFlagWoocommerce("", @"/wp-json/wc/v3/orders-by-flag/search?order=2&id=" + orderWoocommerce.Id.ToString(), "PUT");
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now + "problem Document line" + Environment.NewLine);
                sb.Append(DateTime.Now + e.Message + Environment.NewLine);
                sb.Append(DateTime.Now + e.StackTrace + Environment.NewLine);
                UtilsMail.SendErrorMail(DateTime.Now + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine, "COMMANDE LIGNE");
                File.AppendAllText("Log\\order.txt", sb.ToString());
                sb.Clear();
                // UtilsWebservices.SendDataNoParse(UtilsConfig.BaseUrl + EnumEndPoint.Commande.Value, "errorOrder&orderID=" + jsonOrder["id_order"]);
                order.Remove();
                return;
            }
            //order.Expedition = orderWoocommerce.ExtensionAttributes.ShippingAssignments[0].Shipping.Total.ShippingAmount;
            //order.Write();
            File.AppendAllText("Log\\Commande.txt", DateTime.Now + "End Creation de commande : " + orderWoocommerce.Id.ToString() + " ,Client : " + order.Client.CT_Num + " ,Do_piece : " + order.DO_Piece + Environment.NewLine);
            addOrderToLocalDB(orderWoocommerce.Id.ToString(), order.Client.CT_Num, order.DO_Piece, order.DO_Ref, orderWoocommerce.Id.ToString());
            // TODO updateOrderFlag using custom PHP script
            //var jsonFlag = JsonConvert.SerializeObject(UpdateOrderFlag(orderWoocommerce.Id.ToString(), orderWoocommerce.Id.ToString(), "0"));
            UtilsWebservices.sendNewFlagWoocommerce("", @"/wp-json/wc/v3/orders-by-flag/search?order=0&id=" + orderWoocommerce.Id.ToString(), "PUT");
        }

        /// <summary>
        /// The CreateDevis.
        /// </summary>
        /// <param name="devisList">The devisList<see cref="DataGridView"/>.</param>
        public static void CreateDevis(DataGridView devisList)
        {
            string clientCtNum = "";
            string clienttype = "";
            foreach (DataGridViewRow item in devisList.Rows)
            {
                if (item.Cells[0].Value.ToString().Equals("True"))
                {

                    string currentIdDevis = item.Cells[1].Value.ToString(); //orderSearch[0].Items[i].EntityId.ToString();
                    var DevisSearch = Object.Devis.Devis.FromJson(UtilsWebservices.GetMagentoData("rest/V1/amasty_quote/search" + UtilsWebservices.SearchOrderCriteria("quote_id", currentIdDevis, "eq")));
                    if (DevisSearch.TotalCount > 0 & !String.IsNullOrEmpty(DevisSearch.Items[0].Customer.Id.ToString()))
                    {
                        CustomerSearch client = UtilsWebservices.GetClientCtNum(DevisSearch.Items[0].Customer.Id.ToString());
                        try
                        {
                            for (int j = 0; j < client.CustomAttributes.Count; j++)
                            {
                                if (client.CustomAttributes[j].AttributeCode.Equals("sage_number"))
                                {
                                    clientCtNum = client.CustomAttributes[j].Value.ToString();
                                }
                                if (client.CustomAttributes[j].AttributeCode.Equals("customer_type"))
                                {
                                    clienttype = client.CustomAttributes[j].Value.ToString();
                                }
                            }
                        }
                        catch (Exception e)
                        {

                            clientCtNum = "";
                        }
                        if (ControllerClient.CheckIfClientExist(clientCtNum))
                        {

                            // si le client existe on associé la devis à son compte
                            AddNewDevisForCustomer(DevisSearch.Items[0], clientCtNum, client);

                        }
                        else
                        {/*
                            // si le client n'existe pas on récupère les info de magento et on le crée dans la base sage 
                            //string client = UtilsWebservices.SendDataNoParse(UtilsConfig.BaseUrl + EnumEndPoint.Client.Value, "getClient&clientID=" + order["id_customer"]);
                            string ct_num = ControllerClient.CreateNewClientDevis(client, DevisSearch);
                            Object.Customer customerMagento = new Object.Customer();
                            var jsonClient = JsonConvert.SerializeObject(customerMagento.UpdateCustomer(ct_num, clienttype, client.Id.ToString()));
                            UtilsWebservices.SendDataJson(jsonClient, @"rest/all/V1/customers/" + client.Id.ToString(), "PUT");
                            if (!String.IsNullOrEmpty(ct_num))
                            {
                                // le client à bien été crée on peut intégrer la commande sur son compte sage
                                AddNewDevisForCustomer(DevisSearch.Items[0], ct_num, client);
                            }*/


                            CustomerSearchByEmail ClientSearch = CustomerSearchByEmail.FromJson(UtilsWebservices.GetMagentoData("rest/V1/customers/search" + UtilsWebservices.SearchOrderCriteria("email", client.Email, "eq")));
                            Customer customerMagento = new Customer();
                            string clientSageObj = ControllerClient.CheckIfClientEmailExist(client.Email);
                            if (!String.IsNullOrEmpty(clientSageObj))
                            {
                                IBOClient3 customerSage = SingletonConnection.Instance.Gescom.CptaApplication.FactoryClient.ReadNumero(clientSageObj);
                                Client ClientData = new Client(customerSage);
                                var jsonClient = JsonConvert.SerializeObject(customerMagento.UpdateCustomer(clientSageObj, clienttype, client));
                                UtilsWebservices.SendDataJson(jsonClient, @"rest/all/V1/customers/" + ClientSearch.Items[0].Id.ToString(), "PUT");
                                AddNewDevisForCustomer(DevisSearch.Items[0], clientSageObj, client);
                            }
                            else
                            {
                                string ct_num = ControllerClient.CreateNewClientDevis(client, DevisSearch);//.Items[0]);

                                if (!String.IsNullOrEmpty(ct_num))
                                {
                                    var jsonClient = JsonConvert.SerializeObject(customerMagento.UpdateCustomer(ct_num, clienttype, client));
                                    UtilsWebservices.SendDataJson(jsonClient, @"rest/all/V1/customers/" + client.Id.ToString(), "PUT");
                                    // le client à bien été crée on peut intégrer la commande sur son compte sage
                                    AddNewDevisForCustomer(DevisSearch.Items[0], ct_num, client);
                                }
                            }
                        }
                    }

                }

            }
        }

        /// <summary>
        /// The AddNewDevisForCustomer.
        /// </summary>
        /// <param name="devisItem">The devisItem<see cref="DevisItem"/>.</param>
        /// <param name="ct_num">The ct_num<see cref="string"/>.</param>
        /// <param name="client">The client<see cref="CustomerSearch"/>.</param>
        private static void AddNewDevisForCustomer(DevisItem devisItem, string ct_num, CustomerSearch client)
        {
            /*var gescom = SingletonConnection.Instance.Gescom;

            // création de l'entête de la commande 

            IBOClient3 customer = gescom.CptaApplication.FactoryClient.ReadNumero(ct_num);
            IBODocumentVente3 order = gescom.FactoryDocumentVente.CreateType(DocumentType.DocumentTypeVenteDevis);
            order.SetDefault();
            order.SetDefaultClient(customer);
            order.DO_Date = DateTime.Now;
            order.Souche = gescom.FactorySoucheVente.ReadIntitule(UtilsConfig.Souche);
            order.DO_Ref = "WEB " + devisItem.Id.ToString();//orderMagento.EntityId.ToString();
            order.SetDefaultDO_Piece();

            order.Write();
            // création des lignes de la commandes
            try
            {
                foreach (Object.Devis.ItemItem product in devisItem.Items)
                {
                    if (product.ProductType.Equals("configurable"))
                    {
                        continue;
                    }
                    IBODocumentLigne3 docLigne = (IBODocumentLigne3)order.FactoryDocumentLigne.Create();
                    var ArticleExist = gescom.FactoryArticle.ExistReference(product.Sku);
                    if (ArticleExist)
                    {
                        IBOArticle3 article1 = gescom.FactoryArticle.ReadReference(product.Sku.ToString());
                        Article CondArticle = new Article(article1);
                        if (CondArticle.conditionnements.Count > 0)
                        {
                            //String[] SKU = product.Sku.Split('|');
                            //IBOArticle3 article1 = gescom.FactoryArticle.ReadReference(SKU[0].ToString());

                            IBOArticleCond3 articleCond3 = ControllerArticle.GetArticleConditionnementEnum(article1);
                            docLigne.SetDefaultArticleConditionnement(articleCond3, Int32.Parse(product.Qty.ToString()));
                        }
                        else
                        {
                            docLigne.DL_PrixUnitaire = double.Parse(product.Price.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                            // produit simple
                            docLigne.SetDefaultArticle(gescom.FactoryArticle.ReadReference(product.Sku), Int32.Parse(product.Qty.ToString()));
                        }
                        //SHipping price

                        /*if (product["product_ref"].ToString().Equals("TRANSPORT"))
                        {
                            docLigne.DL_PrixUnitaire = Convert.ToDouble(orderMagento.ShippingAmount.ToString().Replace('.', ','));
                        }
                        else if (product["product_ref"].ToString().Equals("REMISE"))
                        {
                            docLigne.DL_PrixUnitaire = Convert.ToDouble(product.Price.ToString().Replace('.', ','));
                        }
                    }
                    else
                    {
                        // on récupère la chaine de gammages d'un produit
                        string product_attribut_string = GetParentProductDetails(product.Sku).ToString();
                        String[] subgamme = product_attribut_string.Split('|');
                        IBOArticle3 article = gescom.FactoryArticle.ReadReference(subgamme[0].ToString());
                        if (subgamme.Length == 3)
                        {
                            // produit à simple gamme
                            IBOArticleGammeEnum3 articleEnum = ControllerArticle.GetArticleGammeEnum1(article, new Gamme(subgamme[1], subgamme[2]));
                            docLigne.SetDefaultArticleMonoGamme(articleEnum, Int32.Parse(product.Qty.ToString()));
                        }
                        else if (subgamme.Length == 5)
                        {
                            // produit à double gamme
                            IBOArticleGammeEnum3 articleEnum = ControllerArticle.GetArticleGammeEnum1(article, new Gamme(subgamme[1], subgamme[2], subgamme[3], subgamme[4]));
                            IBOArticleGammeEnum3 articleEnum2 = ControllerArticle.GetArticleGammeEnum2(article, new Gamme(subgamme[1], subgamme[2], subgamme[3], subgamme[4]));
                            docLigne.SetDefaultArticleDoubleGamme(articleEnum, articleEnum2, Int32.Parse(product.Qty.ToString()));
                        }
                    }
                    docLigne.Write();
                }

            }
            catch (Exception e)
            {
                //UtilsWebservices.UpdateOrderFlag(order.EntityId.ToString(), "2");
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now + e.Message + Environment.NewLine);
                sb.Append(DateTime.Now + e.StackTrace + Environment.NewLine);
                //UtilsMail.SendErrorMail(DateTime.Now + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine, "COMMANDE LIGNE");
                File.AppendAllText("Log\\Devis.txt", sb.ToString());
                sb.Clear();
                // UtilsWebservices.SendDataNoParse(UtilsConfig.BaseUrl + EnumEndPoint.Commande.Value, "errorOrder&orderID=" + jsonOrder["id_order"]);
                order.Remove();
                return;
            }*/
        }

        /// <summary>
        /// The addOrderToLocalDB.
        /// </summary>
        /// <param name="orderID">The orderID<see cref="string"/>.</param>
        /// <param name="CT_Num">The CT_Num<see cref="string"/>.</param>
        /// <param name="DO_piece">The DO_piece<see cref="string"/>.</param>
        /// <param name="DO_Ref">The DO_Ref<see cref="string"/>.</param>
        /// <param name="incremented_id">The incremented_id<see cref="string"/>.</param>
        private static void addOrderToLocalDB(string orderID, string CT_Num, string DO_piece, string DO_Ref, string incremented_id)
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"MyData.db"))
            {
                // Get a collection (or create, if doesn't exist)
                var col = db.GetCollection<LinkedCommandeDB>("Commande");

                // Create your new customer instance
                var commande = new LinkedCommandeDB
                {
                    OrderID = orderID,
                    OrderType = "DocumentTypeVentePrepaLivraison",
                    CT_Num = CT_Num,
                    DO_piece = DO_piece,
                    DO_Ref = DO_Ref,
                    incremented_id = incremented_id

                };
                col.Insert(commande);
            }
        }

        /// <summary>
        /// The GetPrestaOrderStatutFromMapping.
        /// </summary>
        /// <param name="orderSageType">The orderSageType<see cref="DocumentType"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetPrestaOrderStatutFromMapping(DocumentType orderSageType)
        {
            string prestaType;
            if (UtilsConfig.OrderMapping.TryGetValue(orderSageType.ToString(), out prestaType))
            {
                return prestaType;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// The UpdateStatusOnMagento.
        /// </summary>
        /// <param name="orderID">The orderID<see cref="string"/>.</param>
        /// <param name="incremented_id">The incremented_id<see cref="string"/>.</param>
        /// <param name="status">The status<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static object UpdateStatusOnWoocommerce( string status)
        {
           

            return new
            {
                status = status
            };
            //return JsonConvert.SerializeObject(UpdateOrder);
        }

        /// <summary>
        /// The UpdateOrderFlag.
        /// </summary>
        /// <param name="orderID">The orderID<see cref="string"/>.</param>
        /// <param name="incremented_id">The incremented_id<see cref="string"/>.</param>
        /// <param name="flag">The flag<see cref="string"/>.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public static object UpdateOrderFlag(string orderID, string incremented_id, string flag)
        {
            var updateFlag = new
            {
                entity = new
                {
                    entity_id = orderID,
                    increment_id = incremented_id,
                    extension_attributes = new
                    {
                        order_flag = flag
                    }
                }
            };
            return updateFlag;
        }

        /// <summary>
        /// The GetParentProductDetails.
        /// </summary>
        /// <param name="sku">The sku<see cref="string"/>.</param>
        /// <returns>The <see cref="StringBuilder"/>.</returns>
        public static StringBuilder GetParentProductDetails(string sku)
        {
            var gescom = SingletonConnection.Instance.Gescom;
            var articlesSageObj = gescom.FactoryArticle.List;
            StringBuilder results = new StringBuilder();
            results.Append("");
            foreach (IBOArticle3 articleSage in articlesSageObj)
            {
                // on check si l'article est cocher en publier sur le site marchand
                if (!articleSage.AR_Publie)
                    continue;
                Article article = new Article(articleSage);
                if (article.isGamme)
                {
                    foreach (Gamme doubleGamme in article.Gammes)
                    {
                        if (article.IsDoubleGamme)
                        {
                            if (doubleGamme.Reference.Equals(sku))
                            {
                                results.Append(article.Reference);
                                results.Append("|");
                                results.Append(doubleGamme.Intitule);
                                results.Append("|");
                                results.Append(doubleGamme.Value_Intitule);
                                results.Append("|");
                                results.Append(doubleGamme.Intitule2);
                                results.Append("|");
                                results.Append(doubleGamme.Value_Intitule2);
                                return results;
                            }
                        }
                        else
                        {
                            if (doubleGamme.Reference.Equals(sku))
                            {
                                results.Append(article.Reference);
                                results.Append("|");
                                results.Append(doubleGamme.Intitule);
                                results.Append("|");
                                results.Append(doubleGamme.Value_Intitule);
                                return results;
                            }
                        }
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// The HasMethod.
        /// </summary>
        /// <param name="objectToCheck">The objectToCheck<see cref="object"/>.</param>
        /// <param name="methodName">The methodName<see cref="string"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool HasMethod(this object objectToCheck, string methodName)
        {
            var type = objectToCheck.GetType();
            return type.GetMethod(methodName) != null;
        }
    }
}
