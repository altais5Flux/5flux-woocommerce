using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebservicesSage.Cotnroller;
using WebservicesSage.Object;
using WebservicesSage.Utils;

namespace WebservicesSage.Services
{
    class ServiceClient : ServiceAbstract
    {
        public ServiceClient()
        {
            setAlive(true);
        }

        public  Task ToDoOnFirstCommit(IProgress<ProgressReport> progress)
        {
            Task taskA = new Task(()=> { });
            try
            {
                if (isAlive())
                {
                    //Task taskA = new Task(() => ControllerClient.SendAllClients());
                    //askA.Start();
                    taskA = ControllerClient.SendAllClients(progress);
                    return taskA;
                }
            }
            catch(Exception e)
            {
                UtilsMail.SendErrorMail(DateTime.Now + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine, "SERVICES CLIENT : ToDoOnFirstCommit");
                
            }
            return taskA;
        }

        public void SendClient(string ct_num)
        {
            try
            {
                if (isAlive())
                {
                    Task taskA = new Task(() => ControllerClient.SendClient(ct_num));
                    taskA.Start();
                }
            }
            catch (Exception e)
            {
                UtilsMail.SendErrorMail(DateTime.Now + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine, "SERVICES CLIENT : ToDoOnFirstCommit");
            }
        }

        public override void ToDoOnFirstCommit()
        {
            throw new NotImplementedException();
        }
    }
}
