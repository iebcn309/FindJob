using FindJob.Model;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace FindJob.Boss
{
    public class Boss
    {
        static int noJobMaxPages = 10; // 无岗位最大页数
        static string homeUrl = "https://www.zhipin.com";
        static string baseUrl = "https://www.zhipin.com/web/geek/job?";
        static HashSet<string> blackCompanies;
        static HashSet<string> blackRecruiters;
        static HashSet<string> blackJobs;
        static List<Job> returnList;
        static string cookiePath = Path.Combine(Environment.CurrentDirectory, "Resources", "bosscookie.json");
        static string dataPath = "";
        static BossConfig config;
        public static string Run()
        {
            config = BossConfig.Initialize();
            returnList = new List<Job>();
            dataPath = Path.Combine(Environment.CurrentDirectory, "Resources", "bossdata.json");
            var data = JsonConvert.DeserializeObject<Dictionary<string, HashSet<string>>>(File.ReadAllText(dataPath));
            blackCompanies = data["blackCompanies"];
            blackRecruiters = data["blackRecruiters"];
            blackJobs = data["blackJobs"];
            SeleniumUtil.InitializeDriver();
            Stopwatch stopwatch = Stopwatch.StartNew();
            PerformLogin();
            string searchUrl = GetSearchUrl();

            foreach (string keyword in config.Keywords)
            {
                int currentPage = 1;
                int consecutiveEmptyPages = 0;
                int previousListSize = -1;

                while (true)
                {
                    NLogUtil.Info($"开始投递关键词【{keyword}】第【{currentPage}】页");
                    string fullUrl = $"{searchUrl}&page={currentPage}";
                    int startSize = returnList.Count;
                    int submissionResult = SubmitResumes(fullUrl, keyword);
                    if (submissionResult == -1)
                    {
                        NLogUtil.Info("今日沟通人数已达上限，请明天再试");
                        break;
                    }
                    if (submissionResult == -2)
                    {
                        NLogUtil.Info("出现异常访问，请手动过验证后再继续投递...");
                        break;
                    }
                    if (submissionResult == startSize)
                    {
                        consecutiveEmptyPages++;
                        if (consecutiveEmptyPages >= noJobMaxPages)
                        {
                            NLogUtil.Info($"【{keyword}】关键词已经连续【{consecutiveEmptyPages}】页无岗位，结束该关键词的投递...");
                            break;
                        }
                        else
                        {
                            NLogUtil.Info($"【{keyword}】关键词第【{currentPage}】页无岗位,目前已连续【{consecutiveEmptyPages}】页无新岗位...");
                        }
                    }
                    else
                    {
                        consecutiveEmptyPages = 0;
                        previousListSize = submissionResult;
                    }
                    currentPage++;
                }
            }
            
            NLogUtil.Info(returnList.Any() ? $"新发起聊天公司如下:\n{string.Join("\n", returnList)}" : "未发起新的聊天...");
            NLogUtil.Info($"共发起 {returnList.Count} 个聊天,用时{stopwatch.Elapsed.Minutes}分{stopwatch.Elapsed.Seconds}秒");
            stopwatch.Stop();
            SaveData(dataPath);
            SeleniumUtil.CHROME_DRIVER.Close();
            SeleniumUtil.CHROME_DRIVER.Quit();
            return "";
        }
        public static string GetSearchUrl()
        {
            return baseUrl.appendParam("city", config.CityCode).appendParam("jobType", config.JobType).appendParam("salary", config.Salary).appendListParam("experience", config.Experience).appendListParam("degree", config.Degree).appendListParam("scale", config.Scale).appendListParam("stage", config.Stage);
        }

        private static void SaveData(string filePath)
        {
            // 实现保存数据到文件逻辑
            Dictionary<string, HashSet<string>> data = new Dictionary<string, HashSet<string>>
             {
                 {"blackCompanies", blackCompanies},
                 {"blackRecruiters", blackRecruiters},
                 { "blackJobs", blackJobs}
             };
            string jsonData = JsonConvert.SerializeObject(data);
            File.WriteAllText(filePath, jsonData);
        }
        private static void UpdateListData()
        {
            SeleniumUtil.CHROME_DRIVER.CommandExecutor.Execute(new Command("get", JsonConvert.SerializeObject(new Dictionary<string, object> { { "url", "https://www.zhipin.com/web/geek/chat" } })));
            // = "https://www.zhipin.com/web/geek/chat";
            SeleniumUtil.GetWait(3);

            var jsExecutor = SeleniumUtil.CHROME_DRIVER;
            bool shouldBreak = false;
            while (!shouldBreak)
            {
                try
                {
                    var bottomElement = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//div[@class='finished']"));
                    if ("没有更多了".Equals(bottomElement.Text, StringComparison.Ordinal))
                    {
                        shouldBreak = true;
                    }
                }
                catch (NoSuchElementException) // 更具体的异常处理
                {
                    // Ignore exception as before, but it's clearer what's being ignored.
                }

                var itemList = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//li[@role='listitem']"));
                for (var i = 0; i < itemList.Count; i++)
                {
                    try
                    {
                        var companyElement = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//span[@class='name-box']//span[2]"))[i];
                        var companyName = companyElement.Text.Trim();
                        var messageElement = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//span[@class='last-msg-text']")).ElementAt(i);
                        var message = messageElement.Text.Trim();

                        if (ContainsRejectionKeywords(message) && !ContainsNegativeKeyword(message))
                        {
                            NLogUtil.Info($"黑名单公司：【{companyName}】，信息：【{message}】");
                            if (!blackCompanies.Any(c => companyName.Contains(c, StringComparison.OrdinalIgnoreCase)))
                            {
                                companyName = companyName.Replace("...", "");
                                // 正则表达式，匹配至少两个汉字或至少四个字母字符
                                string pattern = @"[\u4e00-\u9fa5]{2,}|[a-zA-Z]{4,}";
                                if (Regex.IsMatch(companyName, pattern))
                                {
                                    blackCompanies.Add(companyName);
                                }
                            }
                        }
                    }
                    catch (NoSuchElementException) // Specific exception handling
                    {
                        NLogUtil.Error("查找黑名单公司时发生异常...");
                    }
                }

                var loadMoreElement = FindLoadMoreElement();
                if (loadMoreElement != null)
                {
                    ScrollToElement(jsExecutor, loadMoreElement);
                }
                else
                {
                    ScrollToBottom(jsExecutor);
                }
            }

            NLogUtil.Info($"黑名单公司数量：{blackCompanies.Count}");
        }

        private static bool ContainsRejectionKeywords(string message)
        {
            var rejectionKeywords = new[] { "不", "感谢", "但", "遗憾", "需要本", "对不" };
            return rejectionKeywords.Any(message.Contains);
        }

        private static bool ContainsNegativeKeyword(string message)
        {
            var negativeKeywords = new[] { "不是", "不生" };
            return negativeKeywords.Any(message.Contains);
        }

        private static IWebElement FindLoadMoreElement()
        {
            try
            {
                SeleniumUtil.WAIT.Until(d => d.FindElement(By.XPath("//div[contains(text(), '滚动加载更多')]")));
                return SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//div[contains(text(), '滚动加载更多')]"));
            }
            catch (WebDriverTimeoutException)
            {
                NLogUtil.Info("没找到滚动条...");
                return null;
            }
        }

        private static void ScrollToElement(IJavaScriptExecutor executor, IWebElement element)
        {
            try
            {
                executor.ExecuteScript("arguments[0].scrollIntoView();", element);
            }
            catch (Exception ex)
            {
                NLogUtil.Error("滚动到元素出错" + ex.ToString());
            }
        }

        private static void ScrollToBottom(IJavaScriptExecutor executor)
        {
            try
            {
                executor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
            }
            catch (Exception ex)
            {
                NLogUtil.Error("滚动到页面底部出错" + ex.ToString());
            }
        }
        private static int SubmitResumes(string url, string keyword)
        {
            SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl($"{url}&query={keyword}");
            SeleniumUtil.WAIT.Until(d => d.FindElement(By.CssSelector("[class*='job-title clearfix']")));

            var jobCards = SeleniumUtil.CHROME_DRIVER.FindElements(By.CssSelector("li.job-card-wrapper"));
            var jobs = new List<Job>();

            foreach (var jobCard in jobCards)
            {
                var infoPublic = jobCard.FindElement(By.CssSelector("div.info-public"));
                var recruiterText = infoPublic.Text;
                var recruiterName = infoPublic.FindElement(By.CssSelector("em")).Text;

                if (blackRecruiters.Contains(recruiterName))
                    continue; // 跳过黑名单招聘人员

                var jobNameElement = jobCard.FindElement(By.CssSelector("div.job-title span.job-name"));
                var jobName = jobNameElement.Text;

                if (blackJobs.Contains(jobName) || !IsTargetJob(keyword, jobName))
                    continue; // 跳过黑名单岗位

                var companyName = jobCard.FindElement(By.CssSelector("div.company-info h3.company-name")).Text;

                if (blackCompanies.Contains(companyName))
                    continue; // 跳过黑名单公司

                var job = new Job
                {
                    Recruiter = $"{recruiterText.Replace(recruiterName, "")}:{recruiterName}",
                    Href = jobCard.FindElement(By.TagName("a")).GetAttribute("href"),
                    JobName = jobName,
                    JobArea = jobCard.FindElement(By.CssSelector("div.job-title span.job-area")).Text,
                    Salary = jobCard.FindElement(By.CssSelector("div.job-info span.salary")).Text,
                    CompanyTag = string.Join("·", jobCard.FindElements(By.CssSelector("div.job-info ul.tag-list li")).Select(e => e.Text)).TrimEnd('·')
                };

                jobs.Add(job);
            }

            foreach (var job in jobs)
            {
                var jse = SeleniumUtil.CHROME_DRIVER;
                jse.ExecuteScript($"window.open('{job.Href}', '_blank');");

                var tabs = new List<string>(SeleniumUtil.CHROME_DRIVER.WindowHandles);
                SeleniumUtil.CHROME_DRIVER.SwitchTo().Window(tabs.Last());

                try
                {
                    SeleniumUtil.WAIT.Until(d => d.FindElement(By.CssSelector("[class*='btn btn-startchat']")));
                }
                catch (Exception)
                {
                    var errorElement = SeleniumUtil.FindElement(By.XPath("//div[@class='error-content']"));
                    if (errorElement != null && errorElement.Text.Contains("异常访问"))
                        return -2;
                }

                SeleniumUtil.Sleep(1);
                var btnStartChat = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("[class*='btn btn-startchat']"));

                if (btnStartChat.Text == "立即沟通")
                {
                    btnStartChat.Click();

                    if (IsLimit())
                    {
                        SeleniumUtil.Sleep(1);
                        return -1;
                    }

                    try
                    {
                        SeleniumUtil.Sleep(1);
                        try
                        {
                            // 尝试关闭可能存在的输入区域弹窗
                            var closeButton = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//i[@class='icon-close']"));
                            closeButton.Click();
                            btnStartChat.Click();
                        }
                        catch (NoSuchElementException)
                        {
                            // 如果元素不存在，则忽略异常继续执行
                        }

                        // 等待并点击聊天输入框
                        var inputField = SeleniumUtil.WAIT.Until(d => d.FindElement(By.Id("chat-input")));
                        inputField.Click();
                        SeleniumUtil.SleepByMilliseconds(500);

                        // 检查是否有不匹配提示并处理
                        try
                        {
                            var dialogElement = SeleniumUtil.CHROME_DRIVER.FindElement(By.ClassName("dialog-container"));
                            if (dialogElement.Text == "不匹配")
                            {
                                SeleniumUtil.CHROME_DRIVER.Close();
                                SeleniumUtil.CHROME_DRIVER.SwitchTo().Window(tabs.First());
                                continue;
                            }
                        }
                        catch (NoSuchElementException)
                        {
                            // 如果没有找到对话框，说明岗位匹配，继续发送消息流程
                            NLogUtil.Info("岗位匹配，下一步发送消息...");
                        }

                        // 发送预设问候语
                        inputField.SendKeys(config.SayHi);
                        var sendMessageButton = SeleniumUtil.WAIT.Until(d => d.FindElement(By.XPath("//button[@type='send']")));
                        sendMessageButton.Click();

                        IWebElement recruiterNameElement = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//p[@class='base-info fl']/span[@class='name']"));
                        IWebElement recruiterTitleElement = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//p[@class='base-info fl']/span[@class='base-title']"));
                        string recruiter = recruiterNameElement.Text + " " + recruiterTitleElement.Text;

                        IWebElement companyElement;
                        try
                        {
                            companyElement = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//p[@class='base-info fl']/span[not(@class)]"));
                        }
                        catch
                        {
                            SeleniumUtil.WAIT.Until(d => d.FindElement(By.XPath("//p[@class='base-info fl']/span[not(@class)]")));
                            companyElement = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//p[@class='base-info fl']/span[not(@class)]"));

                        }
                        string company = null;
                        if (companyElement != null)
                        {
                            company = companyElement.Text;
                            job.CompanyName = company;
                        }

                        IWebElement positionNameElement = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//a[@class='position-content']/span[@class='position-name']"));
                        IWebElement salaryElement = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//a[@class='position-content']/span[@class='salary']"));
                        IWebElement cityElement = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//a[@class='position-content']/span[@class='city']"));
                        string position = positionNameElement.Text + " " + salaryElement.Text + " " + cityElement.Text;
                        // 记录日志并保存任务
                        NLogUtil.Info($"投递【{company ?? $"未知公司: {job.Href}"}】公司，【{position}】职位，招聘官:【{recruiter}】");
                        returnList.Add(job);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"发送消息失败: {ex.Message}", ex);
                    }

                }

                SeleniumUtil.CHROME_DRIVER.Close();
                SeleniumUtil.CHROME_DRIVER.SwitchTo().Window(tabs.First());
            }

            return returnList.Count;
        }
        private static bool IsTargetJob(string keyword, string jobName)
        {
            var keywordIsAI = new[] { "大模型", "AI" }.Contains(keyword);

            var jobIsDesign = new[] { "设计", "视觉", "产品", "运营" }.Contains(jobName);

            var jobIsAI = new[] { "AI", "人工智能", "大模型", "生成" }.Contains(jobName);

            if (keywordIsAI)
            {
                if (jobIsDesign)
                {
                    return false; // 关键词为AI但工作名称含设计，则排除
                }
                else if (!jobIsAI)
                {
                    return true; // 关键词为AI且工作名称不含AI关键字，则保留
                }
            }
            return true; // 默认情况下认为是目标工作
        }

        private static bool IsLimit()
        {
            try
            {
                SeleniumUtil.Sleep(1);
                string text = SeleniumUtil.CHROME_DRIVER.FindElement(By.ClassName("dialog-con")).Text;
                return text.Contains("已达上限");
            }
            catch
            {
                return false;
            }
        }
        private static void PerformLogin()
        {
            NLogUtil.Info("正在打开Boss直聘网站...");
            SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl(homeUrl);

            if (SeleniumUtil.IsCookieValid(cookiePath))
            {
                SeleniumUtil.LoadCookie(cookiePath);
                SeleniumUtil.CHROME_DRIVER.Navigate().Refresh();
                SeleniumUtil.Sleep(2);
            }

            if (IsRequiredToLogin())
            {
                NLogUtil.Info("Cookie失效，尝试扫码登录...");
                ScanAndLogin();
            }
        }

        private static bool IsRequiredToLogin()
        {
            try
            {
                string text = SeleniumUtil.CHROME_DRIVER.FindElement(By.ClassName("btns")).Text;
                return !string.IsNullOrEmpty(text) && text.Contains("登录");
            }
            catch (NoSuchElementException)
            {
                NLogUtil.Info("Cookie有效，已登录...");
                return false;
            }
        }

        private static void ScanAndLogin()
        {
            SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl(homeUrl + "/web/user/?ka=header-login");
            NLogUtil.Info("等待登录...");
            SeleniumUtil.WAIT.Until(d => d.FindElement(By.XPath("//a[@ka='header-home-logo']")));
            bool isLoggedIn = false;
            while (!isLoggedIn)
            {
                try
                {
                    SeleniumUtil.WAIT.Until(d => d.FindElement(By.XPath("//*[@id=\"header\"]/div[1]/div[1]/a")));
                    SeleniumUtil.WAIT.Until(d => d.FindElement(By.XPath("//*[@id=\"wrap\"]/div[2]/div[1]/div/div[1]/a[2]")));

                    isLoggedIn = true;
                    NLogUtil.Info("登录成功！保存Cookie...");
                }
                catch (WebDriverTimeoutException)
                {
                    NLogUtil.Info("登录失败，两秒后重试...");
                }
                finally
                {
                    SeleniumUtil.Sleep(2);
                }
            }
            SeleniumUtil.SaveCookie(cookiePath);
        }
    }
}
