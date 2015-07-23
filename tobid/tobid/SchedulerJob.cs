using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.IO;

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

        private static DateTime startTime = DateTime.Now;
        private static DateTime expireTime=DateTime.Now;
        private static int deltaPrice;

        private OrcUtil m_orcPrice;

        public static void setConfig(DateTime startTime, DateTime expireTime, int deltaPrice)
        {
            lock (lockObj)
            {
                SubmitPriceJob.startTime = startTime;
                SubmitPriceJob.expireTime = expireTime;
                SubmitPriceJob.deltaPrice = deltaPrice;
            }
        }

        public SubmitPriceJob(String endPoint, OrcUtil orcUtil)
        {
            this.EndPoint = endPoint;
            this.m_orcPrice = orcUtil;
        }

        public String EndPoint
        {
            get;
            set;
        }

        public void Execute()
        {
            DateTime now = DateTime.Now;
            System.Console.WriteLine(String.Format("{0} SubmitPriceJob.Execute()", now));
            if (now >= SubmitPriceJob.startTime && now <= SubmitPriceJob.expireTime)
            {
                System.Console.WriteLine("trigger Fired");
                String epPostCaptcha = this.EndPoint + "/receive/captcha.do";

                rest.GivePrice givePrice = new rest.GivePrice();
                givePrice.price = new System.Drawing.Point(825, 368);
                givePrice.input = new System.Drawing.Point(874, 518);
                givePrice.click = new System.Drawing.Point(983, 515);

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
            }
            else
            {
            }
        }
    }
}
