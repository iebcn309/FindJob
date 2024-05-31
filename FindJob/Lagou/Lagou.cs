using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using NLog;
using OpenQA.Selenium.Interactions;
using Microsoft.Extensions.Options;
using System.Xml.Linq;


namespace FindJob.Lagou
{
    public static class Lagou
    {
        private static int currentPage = 1;
        private static int maxPage = 500;
        private static string baseUrl = "https://www.lagou.com";
        private static string wechatUrl = "https://open.weixin.qq.com/connect/qrconnect?appid=wx9d8d3686b76baff8&redirect_uri=https%3A%2F%2Fpassport.lagou.com%2Foauth20%2Fcallback_weixinProvider.html&response_type=code&scope=snsapi_login#wechat_redirect";
        private static int jobCount = 0;
        static string cookiePath = Path.Combine(Environment.CurrentDirectory, "Resources", "lagoucookie.json");
        private static LagouConfig config;
        public static void Run()
        {
            config = LagouConfig.Initialize();
            SeleniumUtil.InitializeDriver();
            PerformLogin();

            SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl(baseUrl);
            baseUrl = "https://www.lagou.com/wn/zhaopin?fromSearch=true";
            foreach (var keyword in config.Keywords)
            {
                string searchUrl = GetSearchUrl(keyword);
                SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl(searchUrl);
                SetMaxPage();
                for (int i = currentPage; i <= maxPage; i++)
                {
                    Submit();
                }
            }
            NLogUtil.Info($"投递完成,共投递 {jobCount} 个岗位！");
        }
        public static string GetSearchUrl(string keyword)
        {
            return baseUrl.appendParam("city", config.CityCode).appendParam("kd", keyword).appendParam("yx", config.Salary).appendListParam("gm", config.Scale);
        }
        private static void SetMaxPage()
        {
            // 模拟 Ctrl + End
            SeleniumUtil.ACTIONS.KeyDown(Keys.Control).SendKeys(Keys.End).KeyUp(Keys.Control).Perform();
            IWebElement secondLastLi = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("(//ul[@class='lg-pagination']/li)[last()-1]"));
            if (secondLastLi != null && int.TryParse(secondLastLi.Text, out int parsedMaxPage))
            {
                maxPage = parsedMaxPage;
            }
            // 模拟 Ctrl + Home
            SeleniumUtil.ACTIONS.KeyDown(Keys.Control).SendKeys(Keys.Home).KeyUp(Keys.Control).Perform();
        }

        private static void Submit()
        {
            // 尝试获取所有岗位元素
            List<IWebElement> elements = null;
            try
            {
                SeleniumUtil.ACTIONS.SendKeys(Keys.Home).Perform();
                SeleniumUtil.Sleep(1);
                SeleniumUtil.WAIT.Until(d=>d.FindElement(By.Id("openWinPostion")));
                elements = SeleniumUtil.CHROME_DRIVER.FindElements(By.Id("openWinPostion")).ToList();
            }
            catch (Exception)
            {
                // 忽略异常
            }

            // 遍历岗位元素进行投递
            for (int i = 0; i < elements.Count; i++)
            {
                IWebElement element = null;
                try
                {
                    element = elements[i];
                }
                catch
                {
                    NLogUtil.Error($"获取岗位列表中某个岗位失败，岗位列表数量：{i + 1},获取第【{elements.Count}】个元素失败");
                }
                try
                {
                    SeleniumUtil.ACTIONS.MoveToElement(element).Perform();
                }
                catch
                {
                    GetWindow();
                }
                if (-1 == TryClick(element, i))
                {
                    continue;
                }
                SeleniumUtil.Sleep(1);
                GetWindow();
                IWebElement submit;
                try
                {
                    submit = SeleniumUtil.CHROME_DRIVER.FindElement(By.ClassName("resume-deliver"));
                }
                catch
                {
                    SeleniumUtil.Sleep(10);
                    continue;
                }
                if ("投简历".Equals(submit.Text))
                {
                    string jobTitle = null;
                    string companyName = null;
                    string jobInfo = null;
                    string companyInfo = null;
                    string salary = null;
                    string weal = null;
                    try
                    {
                        jobTitle = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("span.name__36WTQ")).Text;
                        companyName = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("span.company")).Text;
                        jobInfo = string.Join("/", SeleniumUtil.CHROME_DRIVER.FindElements(By.CssSelector("h3.position-tags span")).Select(e => e.Text));
                        companyInfo = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("div.header__HY1Cm")).Text;
                        salary = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("span.salary__22Kt_")).Text;
                        weal = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("li.labels")).Text;
                    }
                    catch (Exception e)
                    {
                        NLogUtil.Error($"获取职位信息失败{e}");
                        try
                        {
                            jobTitle = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("span.position-head-wrap-position-name")).Text;
                            companyName = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("span.company")).Text;
                            jobInfo = string.Join("/", SeleniumUtil.CHROME_DRIVER.FindElements(By.CssSelector("h3.position-tags span:not(.tag-point)")).Select(e => e.Text));
                            companyInfo = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("span.company")).Text;
                            salary = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("span.salary")).Text;
                            weal = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("dd.job-advantage p")).Text;
                        }
                        catch (Exception ex)
                        {
                            NLogUtil.Error($"第二次获取职位信息失败，放弃了{ex}");
                        }
                    }
                    NLogUtil.Info($"投递: {jobTitle},职位: {jobTitle},公司: {companyName},职位信息: {jobInfo},公司信息: {companyInfo},薪资: {salary},福利: {weal}");
                    jobCount++;
                    SeleniumUtil.Sleep(2);
                    submit.Click();
                    SeleniumUtil.Sleep(2);
                    try
                    {
                        IWebElement send = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("body > div:nth-child(45) > div > div.lg-design-modal-wrap.position-modal > div > div.lg-design-modal-content > div.lg-design-modal-footer > button.lg-design-btn.lg-design-btn-default"));
                        if ("确认投递".Equals(send.Text))
                        {
                            send.Click();
                        }
                    }
                    catch (Exception e)
                    {
                        NLogUtil.Error($"没有【确认投递】的弹窗，继续！{e}");
                    }
                    try
                    {
                        IWebElement confirm = SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("button.lg-design-btn.lg-design-btn-primary span"));
                        string buttonText = confirm.Text;
                        if ("我知道了".Equals(buttonText))
                        {
                            confirm.Click();
                        }
                        else
                        {
                            SeleniumUtil.Sleep(1);
                        }
                    }
                    catch (Exception e)
                    {
                        NLogUtil.Error($"第一次点击【我知道了】按钮失败...重试xpath点击...{e}");
                        SeleniumUtil.Sleep(1);
                        try
                        {
                            SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("/html/body/div[7]/div/div[2]/div/div[2]/div[2]/button[2]")).Click();
                        }
                        catch (Exception ex)
                        {
                            NLogUtil.Error($"第二次点击【我知道了】按钮失败...放弃了！{ex}" );
                            SeleniumUtil.Sleep(10);
                            SeleniumUtil.CHROME_DRIVER.Navigate().Refresh();
                        }
                    }
                    try
                    {
                        SeleniumUtil.Sleep(2);
                        SeleniumUtil.CHROME_DRIVER.FindElement(By.CssSelector("#__next > div:nth-child(3) > div > div > div.feedback_job__3EnWp > div.feedback_job_title__2y8Bj > div.feedback_job_deliver__3UIB5.feedback_job_active__3bbLa")).Click();
                    }
                    catch (Exception e)
                    {
                        NLogUtil.Error($"这个岗位没有推荐职位...{e}");
                        SeleniumUtil.Sleep(1);
                    }
                }
                else if ("立即沟通".Equals(submit.Text))
                {
                    submit.Click();
                    SeleniumUtil.WAIT.Until(d=>d.FindElement(By.XPath("//*[@id=\"modalConIm\"]"))).Click();
                }
                else
                {
                    NLogUtil.Info("这个岗位没有投简历按钮...一秒后关闭标签页面！");
                    SeleniumUtil.Sleep(1);
                }
                SeleniumUtil.CHROME_DRIVER.Close();
                GetWindow();
            }
        }

        private static void GetWindow()
        {
            try
            {
                var windowHandles = SeleniumUtil.CHROME_DRIVER.WindowHandles;
                if (windowHandles.Count > 1)
                {
                    SeleniumUtil.CHROME_DRIVER.SwitchTo().Window(windowHandles[1]);
                }
                else
                {
                    SeleniumUtil.CHROME_DRIVER.SwitchTo().Window(windowHandles[0]);
                }
            }
            catch (Exception)
            {
                // 忽略异常
            }
        }
        private static int TryClick(IWebElement element, int i)
        {
            bool isClicked = false;
            int maxRetryCount = 10;
            int retryCount = 0;

            while (!isClicked && retryCount < maxRetryCount)
            {
                try
                {
                    element.Click();
                    isClicked = true;
                }
                catch (Exception e)
                {
                    retryCount++;
                    NLogUtil.Error($"element.click() 点击失败，正在尝试重新点击...(正在尝试：第 {retryCount} 次){e}");
                    SeleniumUtil.Sleep(5);
                    try
                    {
                        SeleniumUtil.CHROME_DRIVER.FindElements(By.Id("openWinPostion"))[i].Click();
                        isClicked = true;
                    }
                    catch (Exception ex)
                    {
                        NLogUtil.Error($"get(i).click() 重试失败，尝试使用Actions点击...(正在尝试：第 {retryCount} 次){ex}");
                        SeleniumUtil.Sleep(5);
                        try
                        {
                            SeleniumUtil.ACTIONS.KeyDown(Keys.Control).Click(element).KeyUp(Keys.Control).Build().Perform();
                            isClicked = true;
                        }
                        catch (Exception exc)
                        {
                            NLogUtil.Error($"使用Actions点击也失败，等待10秒后再次尝试...(正在尝试：第 {retryCount} 次){exc}");
                            SeleniumUtil.Sleep(10);
                        }
                    }
                }
            }
            if (!isClicked)
            {
                NLogUtil.Error($"已尝试 {maxRetryCount} 次，已达最大重试次数，少侠请重新来过！");
                NLogUtil.Info($"已投递 {jobCount} 次，正在退出...");
                SeleniumUtil.CHROME_DRIVER.Quit();
                return -1;
            }
            else
            {
                return 0;
            }
        }
        private static void PerformLogin()
        {
            NLogUtil.Info("正在打开拉勾...");
            SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl(baseUrl);
            NLogUtil.Info("拉勾正在登录...");

            if (SeleniumUtil.IsCookieValid(cookiePath))
            {
                SeleniumUtil.LoadCookie(cookiePath);
                SeleniumUtil.CHROME_DRIVER.Navigate().Refresh();
            }
            SeleniumUtil.WAIT.Until(d=>d.FindElement(By.Id("search_button")));

            if (IsRequiredToLogin())
            {
                NLogUtil.Info("Cookie失效，尝试扫码登录...");
                ScanAndLogin();
                SeleniumUtil.SaveCookie(cookiePath);
            }
            else
            {
                NLogUtil.Info("cookie有效，准备投递...");
            }
        }

        private static bool IsRequiredToLogin()
        {
            try
            {
                string text = SeleniumUtil.CHROME_DRIVER.FindElement(By.Id("lg_tbar")).Text;
                return !string.IsNullOrEmpty(text) && text.Contains("登录");
            }
            catch
            {
                return true;
            }
        }
        private static void ScanAndLogin()
        {
            try
            {
                SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl(wechatUrl);
                NLogUtil.Info("等待扫码...");
                SeleniumUtil.WAIT.Until(d => d.FindElement(By.Id("search_button")));
            }
            catch
            {
                SeleniumUtil.CHROME_DRIVER.Navigate().Refresh();
            }
        }
    }
}
