using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindJob.Boss
{

    /// <summary>  
    /// 工作经验枚举  
    /// </summary> 
    public enum Experience
    {
        [Description("不限")]
        None = 0,
        [Description("在校生")]
        Student = 108,
        [Description("应届生")]
        Graduate = 102,
        [Description("经验不限")]
        Unlimited = 101,
        [Description("一年以内")]
        LessThanOneYear = 103,
        [Description("1-3年")]
        OneToThreeYears = 104,
        [Description("3-5年")]
        ThreeToFiveYears = 105,
        [Description("5-10年")]
        FiveToTenYears = 106,
        [Description("10年以上")]
        MoreThanTenYears = 107
    }

    // 城市代码枚举
    public enum CityCode
    {
        [Description("不限")]
        None = 0, 
        [Description("全国")]
        All = 100010000, 
        [Description("北京")]
        Beijing = 101010100, 
        [Description("上海")]
        Shanghai = 101020100, 
        [Description("广州")]
        Guangzhou = 101280100, 
        [Description("深圳")]
        Shenzhen = 101280600, 
        [Description("成都")]
        Chengdu = 101270100,
    }

    // 职位类型枚举
    public enum JobType
    {
        [Description("不限")]
        None = 0, 
        [Description("全职")]
        FullTime = 1901, 
        [Description("兼职")]
        PartTime = 1903,
    }

    // 薪资范围枚举
    public enum Salary
    {
        [Description("不限")]
        None = 0,
        [Description("3K以下")]
        Below3k = 402,
        [Description("3-5K")]
        From3kTo5k = 403,
         [Description("5-10K")]
         From5KTo10K = 404,
         [Description("10-20K")]
         From10KTo20K = 405,
         [Description("20-50K")]
         From20KTo50K = 406,
         [Description("50K以上")]
         Above50K = 407
    }

    // 学历要求枚举
    public enum Degree
    {
        [Description("不限")]
        None = 0,
        [Description("初中及以下")]
        BelowJuniorHighSchool = 209,
        [Description("中专/中技")]
        SecondaryVocational = 208,
        [Description("高中")]
        HighSchool = 206,
        [Description("大专")]
        JuniorCollege = 202,
        [Description("本科")]
        Bachelor = 203,
        [Description("硕士")]
        Master = 204,
        [Description("博士")]
        Doctor = 205,
    }

    // 公司规模枚举
    public enum Scale
    {
        [Description("不限")]
        None = 0,
        [Description("0-20人")]
        ZeroToTwenty = 301,
        [Description("20-99人")]
        TwentyToNinetyNine = 302,
        [Description("100-499人")]
        HundredToFourNineNine = 303,
        [Description("500-999人")]
        FiveHundredToNineNineNine = 304,
        [Description("1000-9999人")]
        ThousandToNineNineNineNine = 305,
        [Description("10000人以上")]
        TenThousandAbove = 306,
    }

    // 融资阶段枚举
    public enum Financing
    {
        [Description("不限")]
        None = 0,
        [Description("未融资")]
        Unfunded = 801,
        [Description("天使轮")]
        AngelRound = 802,
        [Description("A轮")]
        ARound = 803,
        [Description("B轮")]
        BRound = 804,
        [Description("C轮")]
        CRound = 805,
        [Description("D轮及以上")]
        DAndAbove = 806,
        [Description("已上市")]
        Listed = 807,
        [Description("不需要融资")]
        NoNeed = 808,
    }

    // 行业分类枚举
    public enum Industry
    {
        [Description("不限")]

        None = 0, 
        [Description("互联网")]
        Internet = 100020, 
        [Description("计算机软件")]
        ComputerSoftware = 100021, 
        [Description("云计算")]
        CloudComputing = 100029,
    }
}
