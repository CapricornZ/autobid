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

using tobid.rest;
using tobid.scheduler;
using tobid.scheduler.jobs;
using tobid.util;
using tobid.util.orc;
using tobid.util.http;

namespace tobid
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.SetVisibleCore(true); 
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.webBrowser1.Width = this.Size.Width - 40;
            this.webBrowser1.Height = this.Size.Height - 127;
        }

        private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Form1));
        private OrcUtil m_orcPrice;
        private OrcUtil m_orcCaptchaLoading;
        private OrcUtil m_orcCaptchaTip;
        private OrcUtil m_orcCaptchaTipNo;
        private CaptchaUtil m_orcCaptchaTipsUtil;

        private System.Threading.Thread keepAliveThread;
        private System.Threading.Thread submitPriceThread;
        private System.Timers.Timer timer = new System.Timers.Timer();

        private Scheduler m_schedulerKeepAlive;
        private Scheduler m_schedulerSubmit;
        private delegate void updateMouse(int x, int y);

        private void update(int x, int y)
        {
            this.textBox1.Text = x + "," + y;
        }

        private void receiveOperation(rest.Operation operation)
        {
            try
            {   
                //ShowInfoJob showInfo = new ShowInfoJob("MESSAGE!");
                //System.Threading.ThreadStart myThreadDelegate = new System.Threading.ThreadStart(showInfo.Execute);
                //System.Threading.Thread myThread = new System.Threading.Thread(myThreadDelegate);
                //myThread.Start();
            }
            catch
            {
            }

            if (null != operation)
            {
                rest.BidOperation bidOps = (rest.BidOperation)operation;
                rest.Bid bid = Newtonsoft.Json.JsonConvert.DeserializeObject<rest.Bid>(operation.content);
                this.positionDialog.bid = bid;
                this.label3.Text = String.Format("配置：+{5} @[{4}], 价格[{0},{1}], 校验码[{2},{3}]", bid.give.price.x, bid.give.price.y, bid.submit.captcha[0].x, bid.submit.captcha[0].y, operation.startTime, bidOps.price);
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //1165,652
            Point screenPoint = Control.MousePosition;
            updateMouse update = new updateMouse(this.update);
            this.Invoke(update, new object[] { screenPoint.X, screenPoint.Y });
        }

        private Quartz.ISchedulerFactory schedulerFactory;
        private Quartz.IScheduler scheduler;
        private void Form1_Load(object sender, EventArgs e)
        {
            Quartz.Xml.XMLSchedulingDataProcessor processor = new Quartz.Xml.XMLSchedulingDataProcessor(new Quartz.Simpl.SimpleTypeLoadHelper());
            schedulerFactory = new Quartz.Impl.StdSchedulerFactory();
            scheduler = schedulerFactory.GetScheduler();
            processor.ProcessFileAndScheduleJobs("~/quartz_jobs.xml", scheduler);
            scheduler.Start();

            Form.CheckForIllegalCrossThreadCalls = false;
            this.timer.Enabled = true;
            this.timer.Interval = 500;
            this.timer.Start();
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);

            Hotkey.RegisterHotKey(this.Handle, 100, Hotkey.KeyModifiers.Ctrl, Keys.D3);
            Hotkey.RegisterHotKey(this.Handle, 101, Hotkey.KeyModifiers.Ctrl, Keys.D4);
            Hotkey.RegisterHotKey(this.Handle, 102, Hotkey.KeyModifiers.Ctrl, Keys.D5);
            Hotkey.RegisterHotKey(this.Handle, 103, Hotkey.KeyModifiers.Ctrl, Keys.D6);
            Hotkey.RegisterHotKey(this.Handle, 111, Hotkey.KeyModifiers.Ctrl, Keys.Left);
            Hotkey.RegisterHotKey(this.Handle, 110, Hotkey.KeyModifiers.Ctrl, Keys.Up);
            Hotkey.RegisterHotKey(this.Handle, 112, Hotkey.KeyModifiers.Ctrl, Keys.Right);

            Ini ini = new Ini(Directory.GetCurrentDirectory() + "/config.ini");
            String url = ini.ReadValue("GLOBAL", "URL");
            String debug = ini.ReadValue("GLOBAL", "DEBUG");

            this.textURL.Text = url;
            if("true".Equals(debug.ToLower())){

                AllocConsole();
                SetConsoleTitle("千万不要关掉我!");
                IntPtr windowHandle = FindWindow(null, System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                IntPtr closeMenu = GetSystemMenu(windowHandle, IntPtr.Zero);
                uint SC_CLOSE = 0xF060;
                RemoveMenu(closeMenu, SC_CLOSE, 0x0);
            }

            //加载配置项1
            IGlobalConfig configResource = Resource.getInstance(url);//加载配置

            this.Text = String.Format("虎牌帮帮忙 - {0}", configResource.tag);
            this.m_orcPrice = configResource.Price;//价格识别
            this.m_orcCaptchaLoading = configResource.Loading;//LOADING识别
            this.m_orcCaptchaTip = configResource.Tips;//验证码提示（文字）
            this.m_orcCaptchaTipNo = configResource.TipsNo;//验证码提示（数字）
            this.m_orcCaptchaTipsUtil = new CaptchaUtil(m_orcCaptchaTip, m_orcCaptchaTipNo);

            //加载配置项2
            KeepAliveJob keepAliveJob = new KeepAliveJob(url, new ReceiveOperation(this.receiveOperation));
            keepAliveJob.Execute();

            //this.m_orcPrice = OrcUtil.getInstance(new int[] { 0, 10, 20, 30, 40 }, 0, 8, 13, new FileStream("price.resx", FileMode.Open));
            //this.m_orcCaptchaLoading = OrcUtil.getInstance(new int[] { 0, 16, 32, 48, 64, 80, 96 }, 7, 15, 14, new FileStream("loading.resx", FileMode.Open));
            //this.m_orcCaptchaTip = OrcUtil.getInstance(new int[] { 0, 16, 32, 48 }, 0, 15, 16, new FileStream("captcha.tips.resx", FileMode.Open));
            //this.m_orcCaptchaTipNo = OrcUtil.getInstance(new int[] { 64, 88 }, 0, 7, 16, new FileStream("captcha.tips.no.resx", FileMode.Open));

            //this.m_orcPrice = OrcUtil.getInstance(new int[] { 0, 10, 20, 30, 40 }, 0, 8, 13, priceDict);
            //this.m_orcCaptchaLoading = OrcUtil.getInstance(new int[] { 0, 16, 32, 48, 64, 80, 96 }, 7, 15, 14, loadingDict);
            //this.m_orcCaptchaTip = OrcUtil.getInstance(new int[] { 0, 16, 32, 48 }, 0, 15, 16, tipDict);
            //this.m_orcCaptchaTipNo = OrcUtil.getInstance(new int[] { 64, 104 }, 0, 7, 16, tipDict + "/no");
            //this.m_orcCaptchaUtil = new CaptchaUtil(m_orcCaptchaTip, m_orcCaptchaTipNo);

            //keepAlive任务配置
            SchedulerConfiguration config5M = new SchedulerConfiguration(1000 * 60 * 1);
            config5M.Job = new KeepAliveJob(url, new ReceiveOperation(this.receiveOperation));
            m_schedulerKeepAlive = new Scheduler(config5M);

            //Action任务配置
            SchedulerConfiguration config1S = new SchedulerConfiguration(1000);
            config1S.Job = new SubmitPriceJob(url, this.m_orcPrice, this.m_orcCaptchaLoading, this.m_orcCaptchaTipsUtil);
            m_schedulerSubmit = new Scheduler(config1S);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(null != this.scheduler)
                this.scheduler.Shutdown();

            if(null != this.timer)
                this.timer.Close();

            Hotkey.UnregisterHotKey(this.Handle, 100);
            Hotkey.UnregisterHotKey(this.Handle, 101);
            Hotkey.UnregisterHotKey(this.Handle, 102);
            Hotkey.UnregisterHotKey(this.Handle, 103);
            Hotkey.UnregisterHotKey(this.Handle, 110);
            Hotkey.UnregisterHotKey(this.Handle, 111);
            Hotkey.UnregisterHotKey(this.Handle, 112);

            if (null != this.keepAliveThread)
                this.keepAliveThread.Abort();
            if (null != this.submitPriceThread)
                this.submitPriceThread.Abort();
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
                        case 110://CTRL+UP
                            System.Console.WriteLine("HOT KEY 110");
                            this.process(1);//出验证码中间4位
                            break;
                        case 111://CTRL+LEFT
                            System.Console.WriteLine("HOT KEY 111");
                            this.process(0);//出验证码前4位
                            break;
                        case 112://CTRL+RIGHT
                            System.Console.WriteLine("HOT KEY 112");
                            this.process(2);//出验证码后4位
                            break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint="ShowWindow")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint="FindWindowA")]
        public static extern IntPtr FindWindowA(String lp1, String lp2);

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        private static extern bool AllocConsole(); //启动窗口
        [System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "FreeConsole")]
        private static extern bool FreeConsole();      //释放窗口，即关闭
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "FindWindow")]
        extern static IntPtr FindWindow(string lpClassName, string lpWindowName);//找出运行的窗口

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        extern static IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert); //取出窗口运行的菜单

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "RemoveMenu")]
        extern static IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags); //灰掉按钮

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        public static extern bool SetConsoleTitle(string strMessage);

        private void button1_Click(object sender, EventArgs e)
        {
            //AllocConsole();
            //SetConsoleTitle("千万不要关掉我");
            //IntPtr windowHandle = FindWindow(null, System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            //IntPtr closeMenu = GetSystemMenu(windowHandle, IntPtr.Zero);
            //uint SC_CLOSE = 0xF060;
            //RemoveMenu(closeMenu, SC_CLOSE, 0x0);

            //System.Diagnostics.Process process = System.Diagnostics.Process.Start("iexplore.exe", "http://moni.51hupai.org:8081");
            //System.Threading.Thread.Sleep(500);

            //IntPtr hTray = FindWindowA("IEFrame", null);
            //ShowWindow(hTray, 3);

            //this.webBrowser1.Navigate("http://moni.51hupai.org:8081");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /*
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
            */
        }

        /// <summary>
        /// 测试验证码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_captcha_Click(object sender, EventArgs e)
        {
            String[] pos = this.textBox2.Text.Split(new char[] { ',' });
            byte[] content = new ScreenUtil().screenCaptureAsByte(Int32.Parse(pos[0]), Int32.Parse(pos[1]), 120, 28);
            this.pictureBox3.Image = Bitmap.FromStream(new System.IO.MemoryStream(content));

            if (this.checkBox1.Checked)//如果选中“校验码”
            {
                String txtCaptcha = new HttpUtil().postByteAsFile(this.textURL.Text + "/receive/captcha/detail.do", content);
                String[] array = Newtonsoft.Json.JsonConvert.DeserializeObject<String[]>(txtCaptcha);

                this.pictureBox4.Image = new Bitmap(new MemoryStream(Convert.FromBase64String(array[0])));
                this.pictureBox5.Image = new Bitmap(new MemoryStream(Convert.FromBase64String(array[1])));
                this.pictureBox6.Image = new Bitmap(new MemoryStream(Convert.FromBase64String(array[2])));
                this.pictureBox7.Image = new Bitmap(new MemoryStream(Convert.FromBase64String(array[3])));
                this.pictureBox8.Image = new Bitmap(new MemoryStream(Convert.FromBase64String(array[4])));
                this.pictureBox9.Image = new Bitmap(new MemoryStream(Convert.FromBase64String(array[5])));
                this.label1.Text = array[6];
            }
            else//测试“正在加载校验码”
            {
                String strLoading = this.m_orcCaptchaLoading.getCharFromPic(new Bitmap(new System.IO.MemoryStream(content)));

                this.pictureBox4.Image = this.m_orcCaptchaLoading.SubImgs[0];
                this.pictureBox5.Image = this.m_orcCaptchaLoading.SubImgs[1];
                this.pictureBox6.Image = this.m_orcCaptchaLoading.SubImgs[2];
                this.pictureBox7.Image = this.m_orcCaptchaLoading.SubImgs[3];
                this.pictureBox7.Image = this.m_orcCaptchaLoading.SubImgs[4];
                this.pictureBox7.Image = this.m_orcCaptchaLoading.SubImgs[5];
                this.label2.Text = strLoading;
            }
        }

        /// <summary>
        /// 测试价格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_price_Click(object sender, EventArgs e)
        {
            System.Console.WriteLine(String.Format("{0} -- start TEST PRICE --", DateTime.Now.ToString("HH:mm:ss.ffff")));
            String[] pos = this.textBox2.Text.Split(new char[] { ',' });
            byte[] content = new ScreenUtil().screenCaptureAsByte(Int32.Parse(pos[0]), Int32.Parse(pos[1]), 100, 24);
            this.pictureBox3.Image = Bitmap.FromStream(new System.IO.MemoryStream(content));
            String txtPrice = this.m_orcPrice.getCharFromPic(new Bitmap(this.pictureBox3.Image));
            PictureBox[] controlls = new PictureBox[]{
                this.pictureBox4, this.pictureBox5, this.pictureBox6, 
                this.pictureBox7, this.pictureBox8, this.pictureBox9
            };
            for (int i = 0; i < this.m_orcPrice.SubImgs.Count; i++)
                controlls[i].Image = this.m_orcPrice.SubImgs[i];
            this.label1.Text = txtPrice;
            System.Console.WriteLine(String.Format("{0} -- end TEST PRICE --", DateTime.Now.ToString("HH:mm:ss.ffff")));
        }

        /// <summary>
        /// 测试验证码提示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_tips_Click(object sender, EventArgs e)
        {
            //m_orcCaptchaUtil
            System.Console.WriteLine(String.Format("{0} -- start TEST TIPs --", DateTime.Now.ToString("HH:mm:ss.ffff")));
            String[] pos = this.textBox2.Text.Split(new char[] { ',' });
            byte[] content = new ScreenUtil().screenCaptureAsByte(Int32.Parse(pos[0]), Int32.Parse(pos[1]), 140, 24);
            this.pictureBox3.Image = Bitmap.FromStream(new System.IO.MemoryStream(content));
            //String txtTips = this.m_orcCaptchaTip.getCharFromPic(new Bitmap(this.pictureBox3.Image));
            this.label2.Text = this.m_orcCaptchaTipsUtil.getActive("123456", new Bitmap(new MemoryStream(content)));
            PictureBox[] controlls = new PictureBox[]{
                this.pictureBox4, this.pictureBox5, this.pictureBox6, 
                this.pictureBox7, this.pictureBox8, this.pictureBox9
            };
            for (int i = 0; i < this.m_orcCaptchaTipsUtil.SubImgs.Count; i++)
                controlls[i].Image = this.m_orcCaptchaTipsUtil.SubImgs[i];

            System.Console.WriteLine(String.Format("{0} -- end TEST TIPs --", DateTime.Now.ToString("HH:mm:ss.ffff")));
        }
        
        /// <summary>
        /// 出价
        /// </summary>
        /// <param name="deltaPrice"></param>
        private void wholeProcess(int deltaPrice)
        {   
            this.givePrice(this.textURL.Text, this.positionDialog.bid.give, deltaPrice);
        }

        /// <summary>
        /// 出校验码
        /// </summary>
        /// <param name="type"></param>
        private void process(int type)
        {
            this.subimt(this.textURL.Text, this.positionDialog.bid.submit, type);
        }

        private void givePrice(String URL, rest.GivePrice points, int deltaPrice)
        {
            logger.Info("BEGIN 出价格");
            byte[] content = new ScreenUtil().screenCaptureAsByte(points.price.x, points.price.y, 52, 18);
            this.pictureBox2.Image = Bitmap.FromStream(new System.IO.MemoryStream(content));
            logger.Info("\tBEGIN postPrice");
            String txtPrice = this.m_orcPrice.getCharFromPic(new Bitmap(this.pictureBox2.Image));
            logger.Info("\tEND   postPrice");
            int price = Int32.Parse(txtPrice);
            price += deltaPrice;
            txtPrice = String.Format("{0:D5}", price);

            logger.Info("\tBEGIN input PRICE");
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

            for (int i = 0; i < txtPrice.Length; i++)
            {
                System.Threading.Thread.Sleep(50);
                ScreenUtil.keybd_event(ScreenUtil.keycode[txtPrice[i].ToString()], 0, 0, 0);
            }
            logger.Info("\tEND   input PRICE");

            //点击出价
            System.Threading.Thread.Sleep(50);
            ScreenUtil.SetCursorPos(points.button.x, points.button.y);
            ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);
            logger.Info("END   出价格");
        }

        private void subimt(String URL, rest.SubmitPrice points, int type)
        {
            logger.Info("BEGIN 验证码");
            ScreenUtil.SetCursorPos(points.inputBox.x, points.inputBox.y);
            ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);
            //ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);

            logger.Info("\tBEGIN make INPUTBOX blank");
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
            logger.Info("\tEND   make INPUTBOX blank");

            byte[] content = new ScreenUtil().screenCaptureAsByte(points.captcha[0].x, points.captcha[0].y, 128, 28);
            this.pictureBox1.Image = Bitmap.FromStream(new System.IO.MemoryStream(content));
            String strLoading = this.m_orcCaptchaLoading.getCharFromPic(new Bitmap(new MemoryStream(content)));
            logger.InfoFormat("LOADING : {0}", strLoading);
            if ("正在获取校验码".Equals(strLoading))
            {
                logger.InfoFormat("正在获取校验码，关闭&打开窗口重新获取");
                ScreenUtil.SetCursorPos(points.buttons[0].x+188, points.buttons[0].y);//取消按钮
                ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);
                return;
            }

            //byte[] content = null;
            //Boolean isLoading = true;
            //int retry = 0;
            //while (isLoading)
            //{
            //    content = new ScreenUtil().screenCaptureAsByte(points.captcha[0].x, points.captcha[0].y, 128, 28);
            //    String strLoading = this.m_orcCaptchaLoading.getCharFromPic(new Bitmap(new MemoryStream(content)));
            //    logger.InfoFormat("\t try to LOADING = {0}", strLoading);
            //    if ("正在获取校验码".Equals(strLoading))
            //    {
            //        if (retry > 3)//4次都在获取
            //            return;//放弃本次出价
            //        logger.InfoFormat("\t re-try {0}", ++retry);
            //        System.Threading.Thread.Sleep(250);
            //    }
            //    else
            //        isLoading = false;
            //}

            logger.Info("\tBEGIN postCaptcha");
            String txtCaptcha = new HttpUtil().postByteAsFile(URL + "/receive/captcha.do", content);
            logger.Info("\tEND   postCaptcha");

            logger.Info("\tBEGIN input ACTIVE CAPTCHA [" + type + "]");
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
            logger.Info("\tEND   input ACTIVE CAPTCHA");

            {
                System.Threading.Thread.Sleep(50);
                MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
                DialogResult dr = MessageBox.Show("确定要提交出价吗?", "提交出价", messButton, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (dr == DialogResult.OK)
                {
                    logger.InfoFormat("用户选择确定出价");
                    ScreenUtil.SetCursorPos(points.buttons[0].x, points.buttons[0].y);//确定按钮
                    ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);

                    System.Threading.Thread.Sleep(1000);
                    ScreenUtil.SetCursorPos(points.buttons[0].x + 188 / 2, points.buttons[0].y - 10);//确定按钮
                    //ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);
                }
                else
                {
                    logger.InfoFormat("用户选择取消出价");
                    ScreenUtil.SetCursorPos(points.buttons[0].x + 188, points.buttons[0].y);//取消按钮
                    ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);
                }
                

                //if (points.Length > 3)
                //{
                //    System.Threading.Thread.Sleep(50);
                //    ScreenUtil.SetCursorPos(points[3].X, points[3].Y);
                    //ScreenUtil.mouse_event((int)(MouseEventFlags.Absolute | MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), 0, 0, 0, IntPtr.Zero);
                //}
            }
            logger.Info("END   验证码");
        }

        /// <summary>
        /// 打开配置对话框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_openDialog(object sender, EventArgs e)
        {
            this.positionDialog.url = this.textURL.Text;
            this.positionDialog.ShowDialog(this);
            this.positionDialog.BringToFront();
        }

        /// <summary>
        /// 提交配置坐标到服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_sync2Server(object sender, EventArgs e)
        {
            //if (positionDialog.bid != null)
            //{
                //string hostName = System.Net.Dns.GetHostName();
                //string endpoint = this.textURL.Text + "/command/operation/screenconfig/BID/accept.do";
                //RestClient rest = new RestClient(endpoint: endpoint, method: HttpVerb.POST, postObj: this.positionDialog.bid);
                //String response = rest.MakeRequest("?fromHost=" + hostName);
            //}
        }

        /// <summary>
        /// 手动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton1.Checked)
            {
                if(null != this.keepAliveThread)
                    this.keepAliveThread.Abort();
                if(null != this.submitPriceThread)
                    this.submitPriceThread.Abort();
            }
        }

        /// <summary>
        /// 自动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton2.Checked)
            {
                System.Threading.ThreadStart keepAliveThread = new System.Threading.ThreadStart(this.m_schedulerKeepAlive.Start);
                this.keepAliveThread = new System.Threading.Thread(keepAliveThread);
                this.keepAliveThread.Name = "keepAliveThread";
                this.keepAliveThread.Start();

                System.Threading.ThreadStart submitPriceThreadStart = new System.Threading.ThreadStart(this.m_schedulerSubmit.Start);
                this.submitPriceThread = new System.Threading.Thread(submitPriceThreadStart);
                this.submitPriceThread.Name = "submitPriceThread";
                this.submitPriceThread.Start();
            }
        }
    }
}
