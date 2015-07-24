using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.IO;
using System.Threading;

namespace tobid
{
    public interface ISchedulerJob
    {
        void Execute();
    }

    /// <summary>
    /// KeepAlive : 向服务器发布主机名，获取配置项
    /// </summary>
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
            tobid.rest.Client client = Newtonsoft.Json.JsonConvert.DeserializeObject<tobid.rest.Client>(rtn);
            if (null != client.config)
            {
                SubmitPriceJob.setConfig(client.config.startTime, client.config.expireTime, 300);
            }
        }
    }

    public class SubmitPriceJob : ISchedulerJob
    {
        private static Object lockObj = new Object();

        private static DateTime startTime = new DateTime();
        private static DateTime expireTime = new DateTime();
        private static int deltaPrice;
        private static int executeCount = 1;

        private OrcUtil m_orcPrice;

        public SubmitPriceJob(String endPoint, OrcUtil orcUtil)
        {
            this.EndPoint = endPoint;
            this.m_orcPrice = orcUtil;
        }

        public static void setConfig(DateTime startTime, DateTime expireTime, int deltaPrice)
        {
            lock (lockObj)
            {
                if (startTime > SubmitPriceJob.startTime)
                {
                    SubmitPriceJob.startTime = startTime;
                    SubmitPriceJob.expireTime = expireTime;
                    SubmitPriceJob.deltaPrice = deltaPrice;
                    SubmitPriceJob.executeCount = 0;
                }
            }
        }

        public String EndPoint
        {
            get;
            set;
        }

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

                    rest.GivePrice givePrice = new rest.GivePrice();
                    givePrice.price = new System.Drawing.Point(1156, 352);
                    givePrice.input = new System.Drawing.Point(1189, 496);
                    givePrice.click = new System.Drawing.Point(1312, 500);

                    byte[] content = new ScreenUtil().screenCaptureAsByte(givePrice.price.X, givePrice.price.Y, 52, 18);
                    //this.pictureBox2.Image = Bitmap.FromStream(new System.IO.MemoryStream(content));
                    System.Console.WriteLine("\t\tBEGIN postPrice - " + DateTime.Now.ToString());
                    //String txtPrice = new HttpUtil().postByteAsFile(URL + "/chapta.ws/receive/price.do", content);//远程
                    String txtPrice = this.m_orcPrice.getCharFromPic(new Bitmap(new System.IO.MemoryStream(content)));
                    System.Console.WriteLine("\t\tEND postPrice - " + DateTime.Now.ToString());
                    int price = Int32.Parse(txtPrice);
                    price += deltaPrice;
                    txtPrice = String.Format("{0:D}", price);

                    ScreenUtil.SetCursorPos(givePrice.input.X, givePrice.input.Y);
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
                    ScreenUtil.SetCursorPos(givePrice.click.X, givePrice.click.Y);
                    ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);

                    this.submit(this.EndPoint, new Point[] { new Point(1249,469), new Point(1166,478), new Point(1063,567)}, 1);
                }
                
                Monitor.Exit(SubmitPriceJob.lockObj);
            }
            else
            {
                System.Console.WriteLine("obtain SubmitPriceJob.lockObj timeout");
            }
        }

        private void submit(String URL, Point[] points, int type)
        {
            ScreenUtil.SetCursorPos(points[1].X, points[1].Y);
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

            byte[] content = new ScreenUtil().screenCaptureAsByte(points[0].X, points[0].Y, 108, 28);
            System.Console.WriteLine("\t\tBEGIN postCaptcha - " + DateTime.Now.ToString());
            String txtCaptcha = new HttpUtil().postByteAsFile(URL + "/receive/captcha.do", content);
            System.Console.WriteLine("\t\tEND postCaptcha - " + DateTime.Now.ToString());

            if (type == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    ScreenUtil.keybd_event(ScreenUtil.keycode[txtCaptcha[i].ToString()], 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                }
            }
            else if (type == 1)
            {
                for (int i = 1; i < 5; i++)
                {
                    ScreenUtil.keybd_event(ScreenUtil.keycode[txtCaptcha[i].ToString()], 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                }
            }
            else if (type == 2)
            {
                for (int i = 2; i < 6; i++)
                {
                    ScreenUtil.keybd_event(ScreenUtil.keycode[txtCaptcha[i].ToString()], 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                }
            }

            System.Console.WriteLine("\t\tEND inputCaptcha - " + DateTime.Now.ToString());

            //System.Threading.Thread.Sleep(3000);
            if (points.Length > 2)
            {
                System.Threading.Thread.Sleep(50);
                ScreenUtil.SetCursorPos(points[2].X, points[2].Y);
                ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);

                if (points.Length > 3)
                {
                    System.Threading.Thread.Sleep(50);
                    ScreenUtil.SetCursorPos(points[3].X, points[3].Y);
                    //ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);
                }
            }
        }
    }
}
