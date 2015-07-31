using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tobid.rest
{
    /// <summary>
    /// 客户机信息
    /// </summary>
    public class Client
    {
        public String ip { get;set; }
        public DateTime updateTime { get; set; }
        public Config config { get; set; }
        public Operation[] operation { get; set; }
    }

    /// <summary>
    /// 配置信息
    /// </summary>
    public class Config
    {
        public String no { get; set; }
        public String passwd { get; set; }
        public String pid { get; set; }
        public String pname { get; set; }
        public DateTime startTime { get; set; }
        public DateTime expireTime { get; set;}
        public DateTime updateTime { get; set; }
    }

    public abstract class Operation
    {
        public int id { get; set; }
        public String type { get; set; }
        public String content { get; set; }
        public DateTime startTime { get; set; }
        public DateTime expireTime { get; set; }
        public DateTime updateTime { get; set; }
    }

    public class BidOperation : Operation
    {
        public int price { get; set; }
    }

    public class LoginOperation : Operation
    {
        public String no { get; set; }
        public String password { get; set; }
    }

    public class Position
    {
        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public int x { get; set; }
        public int y { get; set; }
    }
    public class GivePrice
    {
        public Position price { get; set; }
        public Position inputBox { get; set; }
        public Position button { get; set; }
    }

    public class SubmitPrice
    {
        public Position[] captcha { get; set; }
        public Position inputBox { get; set; }
        public Position[] buttons { get; set; }
    }

    /// <summary>
    /// 竞价
    /// </summary>
    public class Bid
    {
        public GivePrice give { get; set; }
        public SubmitPrice submit { get; set; }
    }
}
