using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.IO;
using System.Threading;

using tobid.rest.json;
using tobid.util.http;
using tobid.util.orc;
using tobid.util;

namespace tobid.scheduler.jobs
{
    public delegate void ReceiveOperation(rest.Operation operation);

    public interface ISchedulerJob
    {
        void Execute();
    }

    /// <summary>
    /// KeepAlive : 向服务器发布主机名，获取配置项
    /// </summary>
    public class KeepAliveJob : ISchedulerJob
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("KeepAliveJob");

        private ReceiveOperation receiveOperation;
        public String EndPoint { get; set; }

        public KeepAliveJob(String endPoint, ReceiveOperation receiveOperation)
        {
            this.EndPoint = endPoint;
            this.receiveOperation = receiveOperation;
        }

        public void Execute()
        {
            logger.Debug(String.Format("{0} - {1} KeepAliveJob.Execute()", Thread.CurrentThread.Name, DateTime.Now));
            string hostName = System.Net.Dns.GetHostName();
            String epKeepAlive = this.EndPoint + "/command/keepAlive.do";
            RestClient restKeepAlive = new RestClient(endpoint: epKeepAlive, method: HttpVerb.POST);
            String rtn = restKeepAlive.MakeRequest(String.Format("?ip={0}", hostName));
            tobid.rest.Client client = Newtonsoft.Json.JsonConvert.DeserializeObject<tobid.rest.Client>(rtn, new OperationConvert());
            if (client.operation != null && client.operation.Length > 0)
            {
                SubmitPriceJob.setConfig(client.config, client.operation[0]);
                this.receiveOperation(client.operation[0]);
            }
        }
    }
    
    /// <summary>
    /// SubmitPrice : 每秒检查，符合条件执行出价Action
    /// </summary>
    public class SubmitPriceJob : ISchedulerJob
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(SubmitPriceJob));
        private static Object lockObj = new Object();

        private static rest.Bid operation;
        private static DateTime startTime = new DateTime();
        private static DateTime expireTime = new DateTime();
        private static int deltaPrice;
        private static int executeCount = 1;

        private OrcUtil m_orcPrice;
        private CaptchaUtil m_captchaUtil;
        public SubmitPriceJob(String endPoint, OrcUtil orcUtil, CaptchaUtil captchaUtil)
        {
            this.EndPoint = endPoint;
            this.m_orcPrice = orcUtil;
            this.m_captchaUtil = captchaUtil;
        }

        public static void setConfig(rest.Config config, rest.Operation operation)
        {
            logger.Debug("setConfig {...}");
            lock (lockObj)
            {
                if (operation.startTime > SubmitPriceJob.startTime)
                {
                    SubmitPriceJob.executeCount = 0;

                    SubmitPriceJob.deltaPrice = ((rest.BidOperation)operation).price;
                    SubmitPriceJob.startTime = operation.startTime;
                    SubmitPriceJob.expireTime = operation.expireTime;

                    rest.Bid bid = Newtonsoft.Json.JsonConvert.DeserializeObject<rest.Bid>(operation.content);
                    SubmitPriceJob.operation = bid;
                }
            }
        }

        public String EndPoint { get; set; }

        public void Execute(){

            DateTime now = DateTime.Now;
            logger.Debug(String.Format("{0} - NOW:{1}, {{Expire:{2}, Count:{3}}}", Thread.CurrentThread.Name, now, SubmitPriceJob.expireTime, SubmitPriceJob.executeCount));
            if (Monitor.TryEnter(SubmitPriceJob.lockObj, 500))
            {
                if (now >= SubmitPriceJob.startTime && now <= SubmitPriceJob.expireTime && SubmitPriceJob.executeCount==0)
                {
                    SubmitPriceJob.executeCount++;
                    logger.Debug("trigger Fired");

                    //出价
                    this.givePrice(SubmitPriceJob.operation.give, deltaPrice);
                    //提交
                    this.submit(this.EndPoint, SubmitPriceJob.operation.submit);
                }
                
                Monitor.Exit(SubmitPriceJob.lockObj);
            }
            else
            {
                logger.Error("obtain SubmitPriceJob.lockObj timeout");
            }
        }

        /// <summary>
        /// 获取当前价格，+delta，出价
        /// </summary>
        /// <param name="givePrice">坐标</param>
        /// <param name="delta">差价</param>
        private void givePrice(rest.GivePrice givePrice, int delta)
        {
            byte[] content = new ScreenUtil().screenCaptureAsByte(givePrice.price.x, givePrice.price.y, 52, 18);
            String txtPrice = this.m_orcPrice.getCharFromPic(new Bitmap(new System.IO.MemoryStream(content)));
            int price = Int32.Parse(txtPrice);
            price += delta;
            txtPrice = String.Format("{0:D}", price);

            //INPUT BOX
            ScreenUtil.SetCursorPos(givePrice.inputBox.x, givePrice.inputBox.y);
            ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);

            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["BACKSPACE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["BACKSPACE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["BACKSPACE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["BACKSPACE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["BACKSPACE"], 0, 0, 0);

            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["DELETE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["DELETE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["DELETE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["DELETE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["DELETE"], 0, 0, 0);

            for (int i = 0; i < txtPrice.Length; i++)
            {
                System.Threading.Thread.Sleep(50);
                ScreenUtil.keybd_event(ScreenUtil.keycode[txtPrice[i].ToString()], 0, 0, 0);
            }

            //点击出价
            System.Threading.Thread.Sleep(50);
            ScreenUtil.SetCursorPos(givePrice.button.x, givePrice.button.y);
            ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);
        }

        private void submit(String URL, rest.SubmitPrice submitPoints)
        {
            ScreenUtil.SetCursorPos(submitPoints.inputBox.x, submitPoints.inputBox.y);
            ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);

            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["BACKSPACE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["BACKSPACE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["BACKSPACE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["BACKSPACE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["BACKSPACE"], 0, 0, 0);

            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["DELETE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["DELETE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["DELETE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["DELETE"], 0, 0, 0);
            System.Threading.Thread.Sleep(50); ScreenUtil.keybd_event(ScreenUtil.keycode["DELETE"], 0, 0, 0);

            byte[] binaryCaptcha = new ScreenUtil().screenCaptureAsByte(submitPoints.captcha[0].x, submitPoints.captcha[0].y, 108, 28);
            logger.Debug("\t\tBEGIN postCaptcha - " + DateTime.Now.ToString());
            String txtCaptcha = new HttpUtil().postByteAsFile(URL + "/receive/captcha.do", binaryCaptcha);
            logger.Debug("\t\tEND postCaptcha - " + DateTime.Now.ToString());

            byte[] binaryTips = new ScreenUtil().screenCaptureAsByte(submitPoints.captcha[1].x, submitPoints.captcha[1].y, 112, 16);
            String strActive = this.m_captchaUtil.getActive(txtCaptcha, new Bitmap(new System.IO.MemoryStream(binaryTips)));
            
            {
                for (int i = 0; i < strActive.Length; i++)
                {
                    ScreenUtil.keybd_event(ScreenUtil.keycode[strActive[i].ToString()], 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                }
            }

            ScreenUtil.SetCursorPos(submitPoints.buttons[0].x, submitPoints.buttons[0].y);
            ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);

            //System.Threading.Thread.Sleep(3000);
            //if (points.Length > 2)
            //{
            //    System.Threading.Thread.Sleep(50);
            //    ScreenUtil.SetCursorPos(points[2].X, points[2].Y);
            //    ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);

            //    if (points.Length > 3)
            //    {
            //        System.Threading.Thread.Sleep(50);
            //        ScreenUtil.SetCursorPos(points[3].X, points[3].Y);
                    //ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);
            //    }
            //}
        }
    }
}
