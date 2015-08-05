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

    /// <summary>
    /// 操作类
    /// </summary>
    public abstract class Operation
    {
        public int id { get; set; }
        public String type { get; set; }
        public String content { get; set; }
        public DateTime startTime { get; set; }
        public DateTime expireTime { get; set; }
        public DateTime updateTime { get; set; }
    }

    /// <summary>
    /// 第二阶段出价
    /// </summary>
    public class BidOperation : Operation
    {
        /// <summary>
        /// 原价基础上Delta价格
        /// </summary>
        public int price { get; set; }
    }

    /// <summary>
    /// 登录
    /// </summary>
    public class LoginOperation : Operation
    {
    }

    /// <summary>
    /// 第一阶段出价
    /// </summary>
    public class Step1Operation : Operation
    {
        public int price { get; set; }
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

    /// <summary>
    /// 出价的坐标
    /// </summary>
    public class GivePrice
    {
        public Position price { get; set; }
        public Position inputBox { get; set; }
        public Position button { get; set; }
    }

    /// <summary>
    /// 出价验证码坐标
    /// </summary>
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

    public class OrcConfig
    {
        public int[] offsetX { get; set; }
        public int offsetY { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int minNearSpots { get; set; }
    }

    public class GlobalConfig
    {
        public IList<OrcConfig> orcConfigs { get; set; }
        public String repository { get; set; }

        public OrcConfig price{ get { return this.orcConfigs[0]; } }
        public OrcConfig tips { get { return this.orcConfigs[1]; } }
        public OrcConfig tipsNo { get { return this.orcConfigs[2]; } }
        public OrcConfig loading { get { return this.orcConfigs[3]; } }
    }
}
