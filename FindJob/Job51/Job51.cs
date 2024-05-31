using FindJob.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FindJob.Job51
{
    public static class Job51Automation
    {
        static string cookiePath = Path.Combine(Environment.CurrentDirectory, "Resources", "job51cookie.json");
        private static string homeUrl = "https://www.51job.com";
        private static string loginUrl = "https://login.51job.com/login.php?lang=c&url=https://www.51job.com/&qrlogin=2";
        private static string baseUrl = "https://we.51job.com/pc/search?";
        private static Job51Config config;
        static List<string> returnList;

        public static void Run()
        {
            config = Job51Config.Initialize();
            returnList = new List<string>();
            SeleniumUtil.InitializeDriver();
            Stopwatch stopwatch = Stopwatch.StartNew();
            string searchUrl = GetSearchUrl();
            PerformLogin();
            foreach (var keyword in config.Keywords)
            {
                SubmitResumes(searchUrl + $"&keyword={keyword}");
            }

            NLogUtil.Info($"共投递{returnList.Count}个简历,用时{stopwatch.Elapsed.TotalMinutes}分");
            SeleniumUtil.Sleep(30);
            SeleniumUtil.CHROME_DRIVER.Close();
            SeleniumUtil.CHROME_DRIVER.Quit();
        }

        private static void SubmitResumes(string url)
        {
            SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl(url);
            SeleniumUtil.Sleep(1);
            try
            {
                var elements = SeleniumUtil.CHROME_DRIVER.FindElements(By.ClassName("ss"));
                if (elements.Any())
                {
                    elements.First().Click();
                }
            }
            catch (Exception)
            {
                FindAnomaly();
            }
            int maxPage = 50;
            for (int j = 1; j <= maxPage; j++)
            {
                bool jumpPageSuccess = false;
                while (!jumpPageSuccess)
                {
                    try
                    {
                        var jumpPageElement = SeleniumUtil.WAIT.Until(d => d.FindElement(By.Id("jump_page")));
                        SeleniumUtil.Sleep(5);

                        jumpPageElement.Click();
                        jumpPageElement.Clear();
                        jumpPageElement.SendKeys(j.ToString());
                        SeleniumUtil.WAIT.Until(d => d.FindElement(By.CssSelector("#app > div > div.post > div > div > div.j_result > div > div:nth-child(2) > div > div.bottom-page > div > div > span.jumpPage"))).Click();

                        SeleniumUtil.ACTIONS.KeyDown(Keys.Control).SendKeys(Keys.Home).KeyUp(Keys.Control).Perform();
                        NLogUtil.Info($"切换到第{j}页");
                        jumpPageSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        NLogUtil.Error($"跳转页面操作异常，尝试重新刷新页面...{ex}");
                        SeleniumUtil.CHROME_DRIVER.Navigate().Refresh();
                        SeleniumUtil.Sleep(1);
                        FindAnomaly();
                    }
                }
                PostCurrentJob();
            }
        }
        // 简化postCurrentJob方法
        private static void PostCurrentJob()
        {
            SeleniumUtil.Sleep(1);
            // 选择所有岗位，批量投递
            var checkboxes = SeleniumUtil.CHROME_DRIVER.FindElements(By.CssSelector("div.ick"));
            if (!checkboxes.Any())
            {
                return;
            }

            var titles = SeleniumUtil.CHROME_DRIVER.FindElements(By.CssSelector("[class*='jname text-cut']"));
            var companies = SeleniumUtil.CHROME_DRIVER.FindElements(By.CssSelector("[class*='cname text-cut']"));

            foreach (var (checkbox, title, company) in checkboxes.Zip(titles, companies))
            {
                ((IJavaScriptExecutor)SeleniumUtil.CHROME_DRIVER).ExecuteScript("arguments[0].click();", checkbox);
                returnList.Add($"{company.Text} | {title.Text}");
                NLogUtil.Info($"选中职位：{company.Text} | {title.Text}");
            }
            SeleniumUtil.Sleep(1);
            SeleniumUtil.ACTIONS.KeyDown(Keys.Control).SendKeys(Keys.Home).KeyUp(Keys.Control).Perform();
            while (true)
            {
                try
                {
                    // 查询按钮是否存在
                    IWebElement parent = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("div.tabs_in"));
                    var button = parent.FindElements(By.CssSelector("button.p_but"));
                    // 如果按钮存在，则点击
                    if (button != null && button.Any())
                    {
                        SeleniumUtil.Sleep(1);
                        button[1].Click();
                        break;
                    }
                }
                catch (ElementClickInterceptedException e)
                {
                    NLogUtil.Error($"失败，1s后重试..{e}");
                    SeleniumUtil.Sleep(1);
                }
            }
            try
            {
                SeleniumUtil.Sleep(3);
                String text = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//div[@class='successContent']")).Text;
                if (text.Contains("快来扫码下载~"))
                {
                    //关闭弹窗
                    SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("[class*='van-icon van-icon-cross van-popup__close-icon van-popup__close-icon--top-right']")).Click();
                }
            }
            catch 
            {
                NLogUtil.Info("未找到投递成功弹窗！可能为单独投递申请弹窗！");
            }
            String particularly = null;
            try
            {
                particularly = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("//div[@class='el-dialog__body']/span")).Text;
            }
            catch
            {
            }
            if (particularly != null && particularly.Contains("需要到企业招聘平台单独申请"))
            {
                //关闭弹窗
                SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("#app > div > div.post > div > div > div.j_result > div > div:nth-child(2) > div > div:nth-child(2) > div:nth-child(2) > div > div.el-dialog__header > button > i")).Click();
                NLogUtil.Info("关闭单独投递申请弹窗成功！");
            }
        }
        // findAnomaly方法保持不变，但建议添加更具体的异常类型捕获
        private static void FindAnomaly()
        {
            try
            {
                var verifyText = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("#WAF_NC_WRAPPER > p.waf-nc-title")).Text;
                if (verifyText.Contains("访问验证"))
                {
                    NLogUtil.Info("遇到访问验证，程序即将退出...");
                    SeleniumUtil.CHROME_DRIVER.Close();

                    SeleniumUtil.CHROME_DRIVER.Quit();
                    Environment.Exit(-2);
                }
            }
            catch (NoSuchElementException)
            {
                NLogUtil.Info("未检测到访问验证提示，程序继续执行...");
            }
            // 处理其他异常情况
        }

        public static string GetSearchUrl()
        {
            return baseUrl.appendListParam("jobArea", config.JobArea).appendListParam("salary", config.Salary);
        }
        private static void PerformLogin()
        {
            SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl(homeUrl);
            if (SeleniumUtil.IsCookieValid(cookiePath))
            {
                SeleniumUtil.LoadCookie(cookiePath);
                SeleniumUtil.CHROME_DRIVER.Navigate().Refresh();
                SeleniumUtil.Sleep(2);
            }
            if (IsRequiredToLogin())
            {
                NLogUtil.Error("cookie失效，尝试扫码登录...");
                ScanAndLogin();
            }
        }
        private static bool IsRequiredToLogin()
        {
            try
            {
                string text = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//p[@class=\"tit\"]")).Text;
                return text != null && text.Contains("登录");
            }
            catch (NoSuchElementException)
            {
                NLogUtil.Info("Cookie有效，已登录...");
                return false;
            }
        }
        private static void ScanAndLogin()
        {
            SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl(loginUrl);
            NLogUtil.Info("等待扫码登陆...");
            SeleniumUtil.WAIT.Until(d => d.FindElement(By.Id("hasresume")));
            SeleniumUtil.SaveCookie(cookiePath);

        }
    }

}
