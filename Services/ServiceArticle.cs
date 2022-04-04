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
    class ServiceArticle : ServiceAbstract
    {
        public ServiceArticle()
        {
            setAlive(true);
        }

        public void ToDoOnLaunch()
        {
            try
            {
                if (isAlive())
                {
                    ControllerArticle.LaunchService();
                }
            }
            catch (Exception e)
            {
            }
        }

        public override void ToDoOnFirstCommit()
        {
            if (isAlive())
            {
                //Task taskA = new Task(() => ControllerArticle.SendAllArticles());
                //taskA.Start();
                //ControllerArticle.SendAllArticles();
            }
        }

        public Task SendProducts(IProgress<ProgressReport> progress)
        {
            Task taskA = new Task(() => { });
            try
            {
                if (isAlive())
                {

                    taskA = ControllerArticle.SendAllArticles(progress);
                    return taskA;

                    //ControllerArticle.SendAllArticles();
                }
            }
            catch(Exception e)
            {
                UtilsMail.SendErrorMail(DateTime.Now + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine, "SERVICES ARTICLE : SendProducts");
            }
            return taskA;

        }

        public void SendCustomProduct(string reference)
        {
            try
            {
                if (isAlive())
                {
                    Task taskA = new Task(() => ControllerArticle.SendCustomArticles(reference));
                    taskA.Start();
                    //ControllerArticle.SendAllArticles();
                    //ControllerArticle.SendCustomArticles(reference);
                }
            }
            catch(Exception e)
            {
                UtilsMail.SendErrorMail(DateTime.Now + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine, "SERVICES ARTICLE : SendCustomProduct");
            }
        }

        public Task SendPriceProduct(IProgress<ProgressReport> progress)
        {
            Task taskA = new Task(() => { });
            try
            {
                if (isAlive())
                {

                    taskA = ControllerArticle.SendPrice(progress);
                    return taskA;

                    //ControllerArticle.SendAllArticles();
                }
            }
            catch (Exception e)
            {
                UtilsMail.SendErrorMail(DateTime.Now + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine, "SERVICES ARTICLE : SendProducts");
            }
            return taskA;
        }

        public void SendCustomPrice(string reference)
        {
            try
            {
                if (isAlive())
                {
                    Task taskA = new Task(() => ControllerArticle.SendCustomPrice(reference));
                    taskA.Start();
                    //ControllerArticle.SendAllArticles();
                    //ControllerArticle.SendCustomArticles(reference);
                }
            }
            catch (Exception e)
            {
                UtilsMail.SendErrorMail(DateTime.Now + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine, "SERVICES ARTICLE : SendCustomProduct");
            }
        }

        public Task SendStock(IProgress<ProgressReport> progress)
        {
            Task taskA = new Task(() => { });
            try
            {
                if (isAlive())
                {

                    taskA = ControllerArticle.SendAllArticles(progress);
                    return taskA;

                    //ControllerArticle.SendAllArticles();
                }
            }
            catch (Exception e)
            {
                UtilsMail.SendErrorMail(DateTime.Now + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine, "SERVICES ARTICLE : SendProducts");
            }
            return taskA;

        }

        public Task SendCustomStock(IProgress<ProgressReport> progress)
        {
            Task taskA = new Task(() => { });
            try
            {
                if (isAlive())
                {

                    taskA = ControllerArticle.SendAllArticles(progress);
                    return taskA;

                    //ControllerArticle.SendAllArticles();
                }
            }
            catch (Exception e)
            {
                UtilsMail.SendErrorMail(DateTime.Now + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine, "SERVICES ARTICLE : SendProducts");
            }
            return taskA;

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
