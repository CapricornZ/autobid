using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Quartz;
using tobid.util.http;

namespace tobid.scheduler.quartz
{
    public delegate void ReceiveOperation(rest.Operation operation);

    public class CommonJobParam
    {   
    }

    public class QuartzJob:Quartz.IJob
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(QuartzJob));

        public virtual void Execute(IJobExecutionContext context)
        {
            logger.Debug("KeepAliveJob.Execute()");
        }
    }
}
