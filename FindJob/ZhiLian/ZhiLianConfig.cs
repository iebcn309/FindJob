using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindJob.ZhiLian
{
    public class ZhiLianConfig
    {
        // 搜索关键词列表
        public List<string> Keywords { get; set; }
        //城市编码
        public string CityCode { get; set; }
        //薪资范围
        public string Salary { get; set; }
        // 初始化配置方法，转换城市编码和薪资范围
        public static ZhiLianConfig Initialize()
        {
            var data = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Resources", "config.json")));
            var config = data["zhilian"].ToObject<ZhiLianConfig>();
            config.CityCode = typeof(FindJob.Liepin.CityCode).EnumToList().Find(e => e.Describe == config.CityCode)?.Value.ToString();
            if (config.Salary == "不限")
            {
                config.Salary = "0";
            }
            return config;
        }
    }
}
