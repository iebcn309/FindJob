using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FindJob.Liepin
{

    public static class Liepin
    {
        static string homeUrl = "https://www.liepin.com/";
        static string cookiePath = "./src/main/java/liepin/cookie.json";
        static int maxPage = 50;
        static List<string> resultList = new List<string>();
        static string baseUrl = "https://www.liepin.com/zhaopin/?";
        static LiepinConfig config;
        public static void Run()
        {
            config = LiepinConfig.Initialize();
            SeleniumUtil.InitializeDriver();
            login();
            foreach (string keyword in config.Keywords)
            {
                submit(keyword);
            }
            printResult();
            SeleniumUtil.CHROME_DRIVER.Close();
            SeleniumUtil.CHROME_DRIVER.Quit();
        }

        private static void printResult()
        {
            NLogUtil.Info($"投递完成,共投递 {resultList.Count} 个岗位！");
            NLogUtil.Info($"今日投递岗位:{string.Join("\n", resultList)}");
        }
        private static void submit(string keyword)
        {
            SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl(getSearchUrl() + "&key=" + keyword);
            SeleniumUtil.WAIT.Until(d => d.FindElement(By.ClassName("list-pagination-box")));
            IWebElement div = SeleniumUtil.CHROME_DRIVER.FindElement(By.ClassName("list-pagination-box"));
            List<IWebElement> lis = div.FindElements(By.TagName("li")).ToList();
            setMaxPage(lis);
            for (int i = 0; i < maxPage; i++)
            {
                SeleniumUtil.WAIT.Until(d => d.FindElement(By.ClassName("subscribe-card-box")));
                NLogUtil.Info($"正在投递【{keyword}】第【{i + 1}】页...");
                submitJob();
                NLogUtil.Info($"已投递第【{i + 1}】页所有的岗位...\n");
                div = SeleniumUtil.CHROME_DRIVER.FindElement(By.ClassName("list-pagination-box"));
                IWebElement nextPage = div.FindElement(By.XPath(".//li[@title='Next Page']"));
                if (nextPage.GetAttribute("disabled") == null)
                {
                    nextPage.Click();
                }
                else
                {
                    break;
                }
            }
            NLogUtil.Info("【{}】关键词投递完成！", keyword);
        }

        private static string getSearchUrl()
        {
            return baseUrl.appendParam("city", config.CityCode).appendParam("salary", config.Salary) +
                    "&currentPage=" + 0 + "&dq=" + config.CityCode;
        }


        private static void setMaxPage(List<IWebElement> lis)
        {
            try
            {
                if (int.TryParse(lis[lis.Count() - 2].Text, out int page))
                {
                    maxPage = page;
                }
            }
            catch
            {
            }
        }

        private static void submitJob()
        {
            int count = SeleniumUtil.CHROME_DRIVER.FindElements(By.CssSelector("div.job-list-box div[style*='margin-bottom']")).Count;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                string jobName = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//*[Contains(@class, 'job-title-box')]"))[i].Text.Replace("\n", " ").Replace("【 ", "[").Replace(" 】", "]");
                string companyName = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//*[Contains(@class, 'company-name')]"))[i].Text.Replace("\n", " ");
                string salary = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//*[Contains(@class, 'job-salary')]"))[i].Text.Replace("\n", " ");
                string recruiterName = null;
                IWebElement name;
                try
                {
                    name = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//*[Contains(@class, 'recruiter-name')]"))[i];
                    recruiterName = name.Text;
                }
                catch (Exception e)
                {
                    NLogUtil.Error(e.Message);
                }
                IWebElement title;
                string recruiterTitle = null;
                try
                {
                    title = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//*[Contains(@class, 'recruiter-title')]"))[i];
                    recruiterTitle = title.Text;
                }
                catch (Exception e)
                {
                    NLogUtil.Info($"【{companyName}】招聘人员:【{recruiterName}】没有职位描述{e}");
                }
                try
                {
                    name = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//div[@class='jsx-1313209507 recruiter-name ellipsis-1']"))[i];
                    SeleniumUtil.ACTIONS.MoveToElement(name).Perform();
                }
                catch
                {
                }
                IWebElement button;
                try
                {
                    button = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//button[@class='ant-btn ant-btn-primary ant-btn-round']"));
                }
                catch
                {
                    continue;
                }
                string text = button.Text;
                if (text.Contains("聊一聊"))
                {
                    try
                    {
                        button.Click();
                    }
                    catch
                    {
                    }
                    SeleniumUtil.WAIT.Until(d => d.FindElement(By.ClassName("__im_basic__header-wrap")));
                    SeleniumUtil.WAIT.Until(d => d.FindElement(By.XPath("//textarea[Contains(@class, '__im_basic__textarea')]")));
                    IWebElement input = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//textarea[Contains(@class, '__im_basic__textarea')]"));
                    input.Click();
                    SeleniumUtil.Sleep(1);
                    IWebElement close = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("div.__im_basic__contacts-title svg"));
                    close.Click();
                    SeleniumUtil.WAIT.Until(d => d.FindElement(By.XPath("//div[Contains(@class, 'recruiter-info-box')]")));

                    resultList.Add(sb.Append("【").Append(companyName).Append(" ").Append(jobName).Append(" ").Append(salary).Append(" ").Append(recruiterName).Append(" ").Append(recruiterTitle).Append("】").ToString());
                    sb = sb.Clear();
                    NLogUtil.Info($"发起新聊天:【{companyName}】的【{jobName}·{salary}】岗位, 【{recruiterName}:{recruiterTitle}】");
                }
                SeleniumUtil.ACTIONS.MoveByOffset(125, 0).Perform();
            }
        }

        private static void login()
        {
            NLogUtil.Info("正在打开猎聘网站...");
            SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl(homeUrl);
            NLogUtil.Info("猎聘正在登录...");
            if (SeleniumUtil.IsCookieValid(cookiePath))
            {
                SeleniumUtil.LoadCookie(cookiePath);
                SeleniumUtil.CHROME_DRIVER.Navigate().Refresh();
            }
            SeleniumUtil.WAIT.Until(d => d.FindElement(By.Id("header-logo-box")));
            if (isLoginRequired())
            {
                NLogUtil.Info("cookie失效，尝试扫码登录...");
                scanLogin();
                SeleniumUtil.SaveCookie(cookiePath);
            }
            else
            {
                NLogUtil.Info("cookie有效，准备投递...");
            }
        }

        private static bool isLoginRequired()
        {
            string currentUrl = SeleniumUtil.CHROME_DRIVER.Url;
            return !currentUrl.Contains("c.liepin.com");
        }

        private static void scanLogin()
        {
            try
            {
                SeleniumUtil.Click(By.ClassName("btn-sign-switch"));
                NLogUtil.Info("等待扫码..");
                SeleniumUtil.WAIT.Until(d => d.FindElement(By.XPath("//*[@id=\"main-container\"]/div/div[3]/div[2]/div[3]/div[1]/div[1]")));
            }
            catch (Exception e)
            {
                NLogUtil.Error($"scanLogin() 失败: {e.Message}");
            }
        }

    }

}
