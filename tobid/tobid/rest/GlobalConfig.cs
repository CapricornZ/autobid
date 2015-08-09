using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tobid.rest
{
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
        public String tag { get; set; }

        public OrcConfig price { get { return this.orcConfigs[0]; } }
        public OrcConfig tips { get { return this.orcConfigs[1]; } }
        public OrcConfig tipsNo { get { return this.orcConfigs[2]; } }
        public OrcConfig loading { get { return this.orcConfigs[3]; } }
    }
}
