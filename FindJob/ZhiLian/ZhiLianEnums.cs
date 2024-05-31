using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindJob.ZhiLian
{
    // 城市代码枚举
    public enum CityCode
    {
        [Description("不限")]
        None = 0,
        [Description("北京")]
        Beijing = 530,
        [Description("上海")]
        Shanghai = 538,
        [Description("广州")]
        Guangzhou = 763,
        [Description("深圳")]
        Shenzhen = 765,
        [Description("成都")]
        Chengdu = 801,
    }
}
