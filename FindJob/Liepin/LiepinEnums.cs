using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindJob.Liepin
{
    // 城市代码枚举
    public enum CityCode
    {
        [Description("不限")]
        None = 0,
        [Description("全国")]
        All = 410,
        [Description("北京")]
        Beijing = 010,
        [Description("上海")]
        Shanghai = 020,
        [Description("广州")]
        Guangzhou = 050020,
        [Description("深圳")]
        Shenzhen = 050090,
        [Description("成都")]
        Chengdu = 280020,
    }
}
