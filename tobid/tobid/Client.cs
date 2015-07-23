using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tobid.rest
{
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
        public System.Drawing.Point price
        {
            get;
            set;
        }

        public System.Drawing.Point input
        {
            get;
            set;
        }

        public System.Drawing.Point click
        {
            get;
            set;
        }
    }
}
