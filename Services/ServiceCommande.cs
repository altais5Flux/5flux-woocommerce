using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebservicesSage.Cotnroller;
using WebservicesSage.Utils;
using WebservicesSage.Utils.Enums;
using Objets100cLib;
using WebservicesSage.Object;

namespace WebservicesSage.Services
{
    class ServiceCommande : ServiceAbstract
    {

        public ServiceCommande()
        {
            setAlive(true);
        }

        public Task ToDoOnLaunch(IProgress<ProgressReport> progress)
        {
            Task taskA = new Task(() => { });
            try
            {
                if (isAlive())
                {
                    // check if configuration is here
                    //AddStatusConfiguration();
                    //Task taskA = new Task(() => ControllerCommande.LaunchService());
                    //taskA.Start();

                    taskA = ControllerCommande.getOrderFromStore(progress);
                    return taskA;
                }
            }
            catch(Exception e)
            {
                UtilsMail.SendErrorMail(DateTime.Now + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine, "SERVICES COMMANDE : ToDoOnFirstCommit");
            }
            return taskA;
        }

        public override void ToDoOnFirstCommit()
        {
            if (isAlive())
            {
                
            }
        }

        private void checkForConfiguration()
        {

        }

        private void AddStatusConfiguration()
        {
            try
            {
                string response = UtilsWebservices.SendData(UtilsConfig.BaseUrl + EnumEndPoint.Commande.Value, "getStatus");
                int i = 0;
                //UtilsConfig.AddNodeInCustomSection("OrderSetting/OrderMapping", "2", DocumentType.DocumentTypeVenteCommande.ToString());
            }
            catch (Exception e)
            {

            }
        }
    }
}
