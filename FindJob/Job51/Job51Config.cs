using FindJob.Boss;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindJob.Job51
{
    public class Job51Config
    {
        // 搜索关键词列表
        public List<string> Keywords { get; set; }

        // 城市编码列表
        public List<string> JobArea { get; set; }

        // 薪资范围列表
        public List<string> Salary { get; set; }

        // 初始化配置方法，转换城市编码和薪资范围
        public static Job51Config Initialize()
        {
            var data = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Resources", "config.json")));
            var config = data["job51"].ToObject<Job51Config>();
            // 将城市编码转换为实际代码
            var jobAreaList = typeof(FindJob.Job51.JobArea).EnumToList();
            config.JobArea = config.JobArea.Select(value => value == "不限" ? "0" : jobAreaList.Find(e => e.Describe == value)?.Value.ToString("000000")).ToList();
            // 将薪资范围转换为代码
            var salaryList = typeof(FindJob.Job51.Salary).EnumToList();
            config.Salary = config.Salary.Select(value => value == "不限" ? "0" : salaryList.Find(e => e.Describe == value)?.Value.ToString("00")).ToList();
            return config;
        }
    }
}
