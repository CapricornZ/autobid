using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace tobid.rest
{
    /// <summary>
    /// 客户机信息
    /// </summary>
    public class Client
    {
        public String ip
        {
            get;
            set;
        }

        public DateTime updateTime
        {
            get;
            set;
        }

        public Config config
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 配置信息
    /// </summary>
    public class Config
    {
        public String no
        {
            get;
            set;
        }

        public String passwd
        {
            get;
            set;
        }

        public String pid
        {
            get;
            set;
        }

        public String pname
        {
            get;
            set;
        }

        public DateTime startTime
        {
            get;
            set;
        }

        public DateTime expireTime
        {
            get;
            set;
        }

        public DateTime updateTime
        {
            get;
            set;
        }
    }

    public class GivePrice
    {
        public Point price { get; set; }

        public Point inputBox { get; set; }

        public Point button { get; set; }
    }

    public class SubmitPrice
    {
        public Point[] captcha { get; set; }

        public Point inputBox { get; set; }

        public Point[] buttons { get; set; }
    }
}
