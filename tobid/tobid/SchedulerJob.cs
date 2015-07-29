using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.IO;
using System.Threading;

namespace tobid
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
        private ReceiveOperation receiveOperation;
        public String EndPoint { get; set; }

        public KeepAliveJob(String endPoint, ReceiveOperation receiveOperation)
        {
            this.EndPoint = endPoint;
            this.receiveOperation = receiveOperation;
        }

        public void Execute()
        {
            System.Console.WriteLine(String.Format("{0} KeepAliveJob.Execute()", DateTime.Now));
            string hostName = System.Net.Dns.GetHostName();
            String epKeepAlive = this.EndPoint + "/command/keepAlive.do";
            RestClient restKeepAlive = new RestClient(endpoint: epKeepAlive, method: HttpVerb.POST);
            String rtn = restKeepAlive.MakeRequest(String.Format("?ip={0}", hostName));
            tobid.rest.Client client = Newtonsoft.Json.JsonConvert.DeserializeObject<tobid.rest.Client>(rtn);
            if (null != client.config && client.operation != null)
            {
                SubmitPriceJob.setConfig(client.config.startTime, client.config.expireTime, 300, client.operation[0]);
                this.receiveOperation(client.operation[0]);
            }
        }
    }

    /// <summary>
    /// SubmitPrice : 每秒检查，符合条件执行出价Action
    /// </summary>
    public class SubmitPriceJob : ISchedulerJob
    {
        private static Object lockObj = new Object();

        private static rest.Bid operation;
        private static DateTime startTime = new DateTime();
        private static DateTime expireTime = new DateTime();
        private static int deltaPrice;
        private static int executeCount = 1;

        private OrcUtil m_orcPrice;
        private OrcUtil m_orcCaptchaTip;

        public SubmitPriceJob(String endPoint, OrcUtil orcUtil)
        {
            this.EndPoint = endPoint;
            this.m_orcPrice = orcUtil;
        }

        public static void setConfig(DateTime startTime, DateTime expireTime, int deltaPrice, rest.Operation operation)
        {
            lock (lockObj)
            {
                if (startTime > SubmitPriceJob.startTime)
                {
                    SubmitPriceJob.startTime = startTime;
                    SubmitPriceJob.expireTime = expireTime;
                    SubmitPriceJob.deltaPrice = deltaPrice;
                    SubmitPriceJob.executeCount = 0;

                    rest.Bid bid = Newtonsoft.Json.JsonConvert.DeserializeObject<rest.Bid>(operation.content);
                    SubmitPriceJob.operation = bid;
                }
            }
        }

        public String EndPoint { get; set; }

        public void Execute(){

            DateTime now = DateTime.Now;
            System.Console.WriteLine("NOW : " + now);
            System.Console.WriteLine("ExpireTime : " + SubmitPriceJob.expireTime);
            System.Console.WriteLine("ExecuteCount : " + SubmitPriceJob.executeCount);
            if (Monitor.TryEnter(SubmitPriceJob.lockObj, 500))
            {
                if (now >= SubmitPriceJob.startTime && now <= SubmitPriceJob.expireTime && SubmitPriceJob.executeCount==0)
                {
                    SubmitPriceJob.executeCount++;

                    System.Console.WriteLine("trigger Fired");
                    String epPostCaptcha = this.EndPoint + "/receive/captcha.do";

                    rest.GivePrice givePrice = SubmitPriceJob.operation.give;

                    byte[] content = new ScreenUtil().screenCaptureAsByte(givePrice.price.x, givePrice.price.y, 52, 18);
                    System.Console.WriteLine("\t\tBEGIN postPrice - " + DateTime.Now.ToString());
                    String txtPrice = this.m_orcPrice.getCharFromPic(new Bitmap(new System.IO.MemoryStream(content)));
                    System.Console.WriteLine("\t\tEND postPrice - " + DateTime.Now.ToString());
                    int price = Int32.Parse(txtPrice);
                    price += deltaPrice;
                    txtPrice = String.Format("{0:D}", price);

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

                    this.submit(this.EndPoint, SubmitPriceJob.operation.submit);
                }
                
                Monitor.Exit(SubmitPriceJob.lockObj);
            }
            else
            {
                System.Console.WriteLine("obtain SubmitPriceJob.lockObj timeout");
            }
        }

        private void submit(String URL, rest.SubmitPrice points)
        {
            ScreenUtil.SetCursorPos(points.inputBox.x, points.inputBox.y);
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

            byte[] content = new ScreenUtil().screenCaptureAsByte(points.captcha[0].x, points.captcha[0].y, 108, 28);
            System.Console.WriteLine("\t\tBEGIN postCaptcha - " + DateTime.Now.ToString());
            String txtCaptcha = new HttpUtil().postByteAsFile(URL + "/receive/captcha.do", content);
            System.Console.WriteLine("\t\tEND postCaptcha - " + DateTime.Now.ToString());

            
            {
                for (int i = 1; i < 5; i++)
                {
                    ScreenUtil.keybd_event(ScreenUtil.keycode[txtCaptcha[i].ToString()], 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                }
            }

            System.Console.WriteLine("\t\tEND inputCaptcha - " + DateTime.Now.ToString());
            ScreenUtil.SetCursorPos(points.buttons[0].x, points.buttons[0].y);
            //ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);

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
