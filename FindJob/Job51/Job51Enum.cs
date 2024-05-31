using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindJob.Job51
{
    public enum JobArea
    {
        [Description("不限")]
        None = 0,
        [Description("北京")]
        Beijing = 010000,
        [Description("上海")]
        Shanghai = 020000,
        [Description("广州")]
        Guangzhou = 030200,
        [Description("深圳")]
        Shenzhen = 040000,
        [Description("成都")]
        Chengdu = 090200,
    }
    // 薪资范围枚举
    public enum Salary
    {
        [Description("不限")]
        None = 0,
        [Description("2千以下")]
        Below2k = 01,
        [Description("2-3千")]
        From2kTo3k = 02,
        [Description("3-4.5千")]
        From3kTo4_5k = 03,
        [Description("4.5-6千")]
        From4_5kTo6k = 04,
        [Description("6-8千")]
        From6KTo8K = 05,
        [Description("0.8-1万")]
        From8KTo10K = 06,
        [Description("1-1.5万")]
        From10KTo15K = 07,
        [Description("1.5-2万")]
        From15KTo20K = 08,
        [Description("2-3万")]
        From20KTo30K = 09,

        [Description("3-4万")]
        From30KTo40K = 10,
        [Description("4-5万")]
        From40KTo50K = 11,
        [Description("5万以上")]
        Above50K = 12
    }
}
