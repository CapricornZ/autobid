using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using mshtml;

namespace tobid
{
    public partial class Form1 : Form
    {
        private OrcUtil m_orcPrice;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.webBrowser1.Width = this.Size.Width - 40;
            this.webBrowser1.Height = this.Size.Height - 98;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //this.webBrowser1.Navigate("www.alltobid.com/guopai/contents/56/2050.html");
            this.webBrowser1.Navigate("http://moni.51hupai.org:8081/");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HtmlElement number = this.webBrowser1.Document.Window.Frames[0].Document.All["number"];
            number.SetAttribute("value", "123456");

            HtmlElement idcard = this.webBrowser1.Document.Window.Frames[0].Document.All["idcard"];
            idcard.SetAttribute("value", "1132");

            //处理验证码
            HtmlElement picCaptcha = this.webBrowser1.Document.Window.Frames[0].Document.Images["ImgValiCode"];
            HTMLDocument doc = (HTMLDocument)this.webBrowser1.Document.Window.Frames[0].Document.DomDocument;
            HTMLBody body = (HTMLBody)doc.body;
            Image image = new DomUtil().GetWebImage(body, picCaptcha);
            
            HttpUtil httpUtil = new HttpUtil();
            String txtCaptcha = httpUtil.postByteAsFile("http://192.168.1.5:8080/chapta.ws/upload.do", DomUtil.transferImage2Byte(image));

            HtmlElement captcha = this.webBrowser1.Document.Window.Frames[0].Document.All["picValidCode"];
            captcha.SetAttribute("value", txtCaptcha);
            this.pictureBox1.Image = image;

            HtmlElement query = this.webBrowser1.Document.Window.Frames[0].Document.All["btnquery"];
            query.InvokeMember("click");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.timer.Enabled = true;
            this.timer.Interval = 1000;
            this.timer.Start();
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);

            Hotkey.RegisterHotKey(this.Handle, 100, Hotkey.KeyModifiers.Ctrl, Keys.D3);
            Hotkey.RegisterHotKey(this.Handle, 101, Hotkey.KeyModifiers.Ctrl, Keys.D4);
            Hotkey.RegisterHotKey(this.Handle, 102, Hotkey.KeyModifiers.Ctrl, Keys.D5);
            Hotkey.RegisterHotKey(this.Handle, 103, Hotkey.KeyModifiers.Ctrl, Keys.D6);
            Hotkey.RegisterHotKey(this.Handle, 111, Hotkey.KeyModifiers.Ctrl, Keys.A);
            Hotkey.RegisterHotKey(this.Handle, 110, Hotkey.KeyModifiers.Ctrl, Keys.S);
            Hotkey.RegisterHotKey(this.Handle, 112, Hotkey.KeyModifiers.Ctrl, Keys.D);

            Ini ini = new Ini(Directory.GetCurrentDirectory() + "/config.ini");
            String url = ini.ReadValue("GLOBAL", "URL");
            String poss = ini.ReadValue("GLOBAL", "POSITIONS");

            this.textURL.Text = url;
            this.textPoss.Text = poss;

            this.m_orcPrice = OrcUtil.getInstance(new int[] { 0, 10, 20, 30, 40 }, 0, 8, 13, @"G:\DICT\MONI\PRICE");
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    switch (m.WParam.ToInt32())
                    {
                        case 100://CTRL+3
                            System.Console.WriteLine("HOT KEY 100");
                            this.wholeProcess(300);
                            break;
                        case 101://CTRL+4
                            System.Console.WriteLine("HOT KEY 101");
                            this.wholeProcess(400);
                            break;
                        case 102://CTRL+5
                            System.Console.WriteLine("HOT KEY 102");
                            this.wholeProcess(500);
                            break;
                        case 103://CTRL+6
                            System.Console.WriteLine("HOT KEY 103");
                            this.wholeProcess(600);
                            break;
                        case 110://CTRL+S
                            System.Console.WriteLine("HOT KEY 110");
                            this.process(1);
                            break;
                        case 111://CTRL+A
                            System.Console.WriteLine("HOT KEY 111");
                            this.process(0);
                            break;
                        case 112://CTRL+D
                            System.Console.WriteLine("HOT KEY 112");
                            this.process(2);
                            break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.timer.Close();

            Hotkey.UnregisterHotKey(this.Handle, 100);
            Hotkey.UnregisterHotKey(this.Handle, 101);
            Hotkey.UnregisterHotKey(this.Handle, 102);
            Hotkey.UnregisterHotKey(this.Handle, 103);
            Hotkey.UnregisterHotKey(this.Handle, 110);
            Hotkey.UnregisterHotKey(this.Handle, 111);
            Hotkey.UnregisterHotKey(this.Handle, 112);
        }

        System.Timers.Timer timer = new System.Timers.Timer();
        private delegate void updateMouse(int x, int y);
        private void update(int x, int y)
        {
            this.textBox1.Text = x + "," + y;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //1165,652
            Point screenPoint = Control.MousePosition;
            updateMouse update = new updateMouse(this.update);
            this.Invoke(update, new object[] { screenPoint.X, screenPoint.Y });

        }

        private void button3_Click(object sender, EventArgs e)
        {
            String[] pos = this.textBox2.Text.Split(new char[] { ',' });
            byte[] content = new ScreenUtil().screenCaptureAsByte(Int32.Parse(pos[0]), Int32.Parse(pos[1]), 120, 24);
            this.pictureBox3.Image = Bitmap.FromStream(new System.IO.MemoryStream(content));
            String txtCaptcha = new HttpUtil().postByteAsFile(this.textURL.Text + "/chapta.ws/receive/captcha.do", content);
            this.label1.Text = txtCaptcha;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            System.Console.WriteLine(String.Format("{0} -- start TEST PRICE --", DateTime.Now.ToString("HH:mm:ss.ffff")));
            String[] pos = this.textBox2.Text.Split(new char[] { ',' });
            byte[] content = new ScreenUtil().screenCaptureAsByte(Int32.Parse(pos[0]), Int32.Parse(pos[1]), 100, 24);
            this.pictureBox3.Image = Bitmap.FromStream(new System.IO.MemoryStream(content));
            //String txtCaptcha = new HttpUtil().postByteAsFile(this.textURL.Text + "/chapta.ws/receive/price.do", content);
            //OrcUtil orcPrice = OrcUtil.getInstance(new int[] { 0, 10, 20, 30, 40 }, 0, 8, 13, @"G:\DICT\MONI\PRICE");
            String txtPrice = this.m_orcPrice.getCharFromPic(new Bitmap(this.pictureBox3.Image));
            this.label1.Text = txtPrice;
            System.Console.WriteLine(String.Format("{0} -- end TEST PRICE --", DateTime.Now.ToString("HH:mm:ss.ffff")));
        }

        private class Util
        {
            public static Point[] getPoint(TextBox txtBox)
            {
                String[] array = txtBox.Text.Split(new char[] { ';' });
                Point[] rtn = new Point[array.Length];
                int i = 0;
                foreach (String item in array){
                    String[] pos = item.Split(new char[]{','});
                    rtn[i++] = new Point(Int32.Parse(pos[0]), Int32.Parse(pos[1]));
                }
                return rtn;
            }
        }

        private void wholeProcess(int deltaPrice)
        {
            System.Console.WriteLine("BEGIN - " + DateTime.Now.ToString());
            Point[] points = Util.getPoint(this.textPoss);
            Point[] pointPrice = new Point[3];
            Point[] pointSubmit = new Point[points.Length - 3];
            for(int i=0; i<3; i++)
                pointPrice[i] = points[i];
            for (int i = 3; i < points.Length; i++)
                pointSubmit[i - 3] = points[i];
            
            System.Console.WriteLine("\tBEGIN givePrice - " + DateTime.Now.ToString());
            this.givePrice(this.textURL.Text, pointPrice, deltaPrice);
            System.Console.WriteLine("\tEND givePrice - " + DateTime.Now.ToString());
            //System.Threading.Thread.Sleep(500);
            //System.Console.WriteLine("\tBEGIN submit - " + DateTime.Now.ToString());
            //this.subimt(this.textURL.Text, pointSubmit);
            //System.Console.WriteLine("\tEND submit - " + DateTime.Now.ToString());
        }

        private void process(int type)
        {
            System.Console.WriteLine("BEGIN - " + DateTime.Now.ToString());
            Point[] points = Util.getPoint(this.textPoss);
            Point[] pointPrice = new Point[3];
            Point[] pointSubmit = new Point[points.Length - 3];
            for (int i = 0; i < 3; i++)
                pointPrice[i] = points[i];
            for (int i = 3; i < points.Length; i++)
                pointSubmit[i - 3] = points[i];

            //System.Threading.Thread.Sleep(500);
            System.Console.WriteLine("\tBEGIN submit - " + DateTime.Now.ToString());
            this.subimt(this.textURL.Text, pointSubmit, type);
            System.Console.WriteLine("\tEND submit - " + DateTime.Now.ToString());
        }

        private void givePrice(String URL, Point[] points, int deltaPrice)
        {
            byte[] content = new ScreenUtil().screenCaptureAsByte(points[0].X, points[0].Y, 52, 18);
            this.pictureBox2.Image = Bitmap.FromStream(new System.IO.MemoryStream(content));
            System.Console.WriteLine("\t\tBEGIN postPrice - " + DateTime.Now.ToString());
            //String txtPrice = new HttpUtil().postByteAsFile(URL + "/chapta.ws/receive/price.do", content);//远程
            String txtPrice = this.m_orcPrice.getCharFromPic(new Bitmap(this.pictureBox2.Image));
            System.Console.WriteLine("\t\tEND postPrice - " + DateTime.Now.ToString());
            int price = Int32.Parse(txtPrice);
            price += deltaPrice;
            txtPrice = String.Format("{0:D5}", price);

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

            for (int i = 0; i < txtPrice.Length; i++)
            {
                System.Threading.Thread.Sleep(50);
                ScreenUtil.keybd_event(ScreenUtil.keycode[txtPrice[i].ToString()], 0, 0, 0);
            }

            //点击出价
            System.Threading.Thread.Sleep(50);
            ScreenUtil.SetCursorPos(points[2].X, points[2].Y);
            ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);
        }

        private void subimt(String URL, Point[] points, int type)
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
            this.pictureBox1.Image = Bitmap.FromStream(new System.IO.MemoryStream(content));

            System.Console.WriteLine("\t\tBEGIN postCaptcha - " + DateTime.Now.ToString());
            String txtCaptcha = new HttpUtil().postByteAsFile(URL + "/chapta.ws/receive/captcha.do", content);
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
                //ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);

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
