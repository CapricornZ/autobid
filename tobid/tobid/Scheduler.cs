using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

namespace tobid
{
    public class SchedulerConfiguration
    {
        //时间间隔
        private int sleepInterval;
        //任务列表
        private ISchedulerJob job;

        public int SleepInterval { get { return sleepInterval; } }
        public ISchedulerJob Job { 
            get { return job; }
            set { this.job = value; }
        }

        //调度配置类的构造函数
        public SchedulerConfiguration(int newSleepInterval)
        {
            sleepInterval = newSleepInterval;
        }
    }

    public class Scheduler
    {
        private int no = 0;
        private SchedulerConfiguration configuration = null;

        public Scheduler(SchedulerConfiguration config)
        {
            configuration = config;
        }

        public void Start()
        {   
            while (true)
            {
                //执行每一个任务
                //foreach (ISchedulerJob job in configuration.Jobs)
                try
                {
                    ThreadStart myThreadDelegate = new ThreadStart(this.configuration.Job.Execute);
                    Thread myThread = new Thread(myThreadDelegate);
                    myThread.Name = String.Format("{0}-{1}", Thread.CurrentThread.Name, ++no);
                    myThread.Start();
                    Thread.Sleep(this.configuration.SleepInterval);
                }
                catch (ThreadAbortException abortException)
                {
                    System.Console.WriteLine(abortException);
                    return;
                }
            }
        }
    }
}
