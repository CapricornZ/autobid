using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tobid
{
    public interface ISchedulerJob
    {
        void Execute();
    }

    public class KeepAliveJob : ISchedulerJob
    {
        public KeepAliveJob(String endPoint)
        {
            this.EndPoint = endPoint;
        }

        public String EndPoint
        {
            get;
            set;
        }

        public void Execute()
        {
            System.Console.WriteLine(String.Format("{0} KeepAliveJob.Execute()", DateTime.Now));
            string hostName = System.Net.Dns.GetHostName();
            String epKeepAlive = this.EndPoint + "/command/keepAlive.do";
            RestClient restKeepAlive = new RestClient(endpoint: epKeepAlive, method: HttpVerb.POST);
            String rtn = restKeepAlive.MakeRequest(String.Format("?ip={0}", hostName));
        }
    }
}
