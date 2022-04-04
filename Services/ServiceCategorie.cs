using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebservicesSage.Cotnroller;
using WebservicesSage.Object;
using WebservicesSage.Utils;

namespace WebservicesSage.Services
{
    class ServiceCategorie : ServiceAbstract
    {
        public ServiceCategorie()
        {
            setAlive(true);
        }

        public override void ToDoOnFirstCommit()
        {
            throw new NotImplementedException();
        }

        public Task SendCategorie(IProgress<ProgressReport> progress)
        {
            Task taskA = new Task(() => { });
            try
            {
                if (isAlive())
                {

                    taskA = ControllerCategorie.SendAllCategories(progress);
                    return taskA;

                    //ControllerArticle.SendAllArticles();
                }
            }
            catch (Exception e)
            {
                //UtilsMail.SendErrorMail(DateTime.Now + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine, "SERVICES ARTICLE : SendProducts");
            }
            return taskA;

        }

        
    }
}
