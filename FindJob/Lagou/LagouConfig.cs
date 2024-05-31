using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace FindJob.Lagou
{
    public class LagouConfig
    {
        // 搜索关键词列表
        public List<string> Keywords { get; set; }
        //城市编码
        public string CityCode { get; set; }
        //薪资范围
        public string Salary { get; set; }
        //公司规模
        public List<string> Scale { get; set; }

        // 初始化配置方法，转换城市编码和薪资范围
        public static LagouConfig Initialize()
        {
            var data = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Resources", "config.json")));
            var config = data["lagou"].ToObject<LagouConfig>();
            if (config.Salary == "不限")
            {
                config.Salary = "0";
            }
            config.Scale = config.Scale.Select(s => s == "不限" ? "0" : s).ToList();
            return config;
        }
    }
}
