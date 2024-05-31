using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindJob.Model
{
    [Serializable]
    public class Job
    {
        public string Title { get; set; }
        public bool JobStatus { get; set; }
        /// <summary>  
        /// 岗位链接  
        /// </summary>  
        public string Href { get; set; }

        /// <summary>  
        /// 岗位名称  
        /// </summary>  
        public string JobName { get; set; }

        /// <summary>  
        /// 岗位地区  
        /// </summary>  
        public string JobArea { get; set; }

        /// <summary>  
        /// 岗位信息
        /// </summary>  
        public string JobInfo { get; set; }

        /// <summary>  
        /// 岗位薪水  
        /// </summary>  
        public string Salary { get; set; }

        /// <summary>  
        /// 公司标签  
        /// </summary>  
        public string CompanyTag { get; set; }

        /// <summary>  
        /// HR名称  
        /// </summary>  
        public string Recruiter { get; set; }

        /// <summary>  
        /// 公司名字  
        /// </summary>  
        public string CompanyName { get; set; }

        /// <summary>  
        /// 公司信息
        /// </summary>  
        public string CompanyInfo { get; set; }

        /// <summary>  
        /// 重写ToString方法，默认输出不包含链接的信息  
        /// </summary>  
        /// <returns>返回岗位信息的字符串表示</returns>  
        public override string ToString()
        {
            return ToStringForPlatform(Platform.BOSS); // 默认使用BOSS平台的格式  
        }

        /// <summary>  
        /// 根据不同的平台返回不同的岗位信息字符串表示  
        /// </summary>  
        /// <param name="platform">平台枚举</param>  
        /// <returns>返回岗位信息的字符串表示</returns>  
        public string ToStringForPlatform(Platform platform)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("【{0}, {1}, {2}, {3}, {4}, {5}", CompanyName, JobName, JobArea, Salary, CompanyTag, Recruiter);

            // 根据平台添加链接信息  
            if (platform == Platform.ZHILIAN)
            {
                sb.AppendFormat(", {0}", Href);
            }

            sb.Append("】");
            return sb.ToString();
        }
    }
    public enum Platform
    {
        ZHILIAN,
        BOSS,
        // 可以添加其他平台  
    }
}
