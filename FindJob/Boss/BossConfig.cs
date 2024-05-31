using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
 

namespace FindJob.Boss
{
    public class BossConfig
    {
        // 用于打招呼的语句
        public string SayHi { get; set; }

        // 搜索关键词列表
        public List<string> Keywords { get; set; }

        // 城市编码
        public string CityCode { get; set; }

        // 行业列表
        public List<string> Industry { get; set; }

        // 工作经验要求
        public List<string> Experience { get; set; }

        // 工作类型
        public string JobType { get; set; }

        // 薪资范围
        public string Salary { get; set; }

        // 学历要求列表
        public List<string> Degree { get; set; }

        // 公司规模列表
        public List<string> Scale { get; set; }

        // 公司融资阶段列表
        public List<string> Stage { get; set; }

        public static BossConfig Initialize()
        {
            var data = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Resources", "config.json")));
            var config = data["boss"].ToObject<BossConfig>();
            // 转换城市编码
            config.CityCode = typeof(FindJob.Boss.CityCode).EnumToList().Find(e => e.Describe == config.CityCode)?.Value.ToString();
            // 转换工作类型
            config.JobType = typeof(FindJob.Boss.JobType).EnumToList().Find(e => e.Describe == config.JobType)?.Value.ToString();
            // 转换薪资范围
            config.Salary = typeof(FindJob.Boss.Salary).EnumToList().Find(e => e.Describe == config.Salary)?.Value.ToString();
            // 转换工作经验要求
            var experienceList =typeof(FindJob.Boss.Experience).EnumToList();
            config.Experience = config.Experience?.Select(exp => experienceList.Find(e => e.Describe == exp)?.Value.ToString()).ToList();
            // 转换学历要求
            var degreeList = typeof(FindJob.Boss.Degree).EnumToList();
            config.Degree = config.Degree?.Select(deg => degreeList.Find(e => e.Describe == deg)?.Value.ToString()).ToList();
            // 转换公司规模
            var scaleList = typeof(FindJob.Boss.Scale).EnumToList();
            config.Scale = config.Scale?.Select(scl => scaleList.Find(e => e.Describe == scl)?.Value.ToString()).ToList();
            // 转换公司融资阶段
            var financingList = typeof(FindJob.Boss.Financing).EnumToList();
            config.Stage = config.Stage?.Select(stg => financingList.Find(e => e.Describe == stg)?.Value.ToString()).ToList();
            // 转换行业
            var industryList = typeof(FindJob.Boss.Industry).EnumToList();
            config.Industry = config.Industry?.Select(ind => industryList.Find(e => e.Describe == ind)?.Value.ToString()).ToList();

            return config;
        }
    }
}
