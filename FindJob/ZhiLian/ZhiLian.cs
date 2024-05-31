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
using FindJob.Model;

namespace FindJob.ZhiLian
{

    public  class ZhiLian
    {

        static string loginUrl = "https://passport.zhaopin.com/login";

        static string homeUrl = "https://sou.zhaopin.com/?";

        static bool isLimit = false;

        static int maxPage = 500;

        static ZhiLianConfig config = ZhiLianConfig.Initialize();
        public static void Run()
        {
            config = ZhiLianConfig.Initialize();
            SeleniumUtil.InitializeDriver();
            login();
            config.Keywords.ForEach(keyword=> {
                SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl(getSearchUrl(keyword, 1));
                submitJobs(keyword);
                isLimit = false;
            });
            SeleniumUtil.CHROME_DRIVER.Quit();
        }

        private static string getSearchUrl(string keyword, int page)
        {
            return homeUrl.appendParam("jl", config.CityCode).appendParam("kw", keyword).appendParam("sl", config.Salary) +"&p=" + page;
        }

        private static void submitJobs(string keyword)
        {
            if (isLimit)
            {
                return;
            }
            SeleniumUtil.WAIT.Until(d=>d.FindElement(By.XPath("//div[Contains(@class, 'joblist-box__item')]")));
            setMaxPages();
            for (int i = 1; i <= maxPage; i++)
            {
                if (i != 1)
                {
                    SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl(getSearchUrl(keyword, i));
                }
                NLogUtil.Info($"开始投递【{keyword}】关键词，第【{i}】页...");
                // 等待岗位出现
                try
                {
                    SeleniumUtil.WAIT.Until(d=>d.FindElement(By.XPath("//div[@class='positionlist']")));
                }
                catch
                {
                    SeleniumUtil.CHROME_DRIVER.Navigate().Refresh();
                    SeleniumUtil.Sleep(1);
                }
                // 全选
                try
                {
                    IWebElement allSelect = SeleniumUtil.WAIT.Until(d=>d.FindElement(By.XPath("//i[@class='betch__checkall__checkbox']")));
                    allSelect.Click();
                }
                catch (Exception e)
                {
                    NLogUtil.Info($"没有全选按钮，程序退出...{e}");
                    continue;
                }
                // 投递
                IWebElement submit = SeleniumUtil.WAIT.Until(d=>d.FindElement(By.XPath("//div[@class='a-job-apply-button']")));
                submit.Click();
                if (checkIsLimit())
                {
                    break;
                }
                SeleniumUtil.Sleep(1);
                // 切换到新的标签页
                List<string> tabs = new List<string>(SeleniumUtil.CHROME_DRIVER.WindowHandles);
                SeleniumUtil.CHROME_DRIVER.SwitchTo().Window(tabs[tabs.Count - 1]);
                //关闭弹框
                try
                {
                    IWebElement result = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//div[@class='deliver-dialog']"));
                    if (result.Text.Contains("申请成功"))
                    {
                        NLogUtil.Info("岗位申请成功！");
                    }
                }
                catch (Exception e)
                {
                    NLogUtil.Error($"关闭投递弹框失败...{e}");
                }
                try
                {
                    IWebElement close = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//img[@title='close-icon']"));
                    close.Click();
                }
                catch
                {
                    if (checkIsLimit())
                    {
                        break;
                    }
                }
                try
                {
                    // 投递相似职位
                    IWebElement checkButton = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//div[Contains(@class, 'applied-select-all')]//input"));
                    if (!checkButton.Selected)
                    {
                        checkButton.Click();
                    }
                    List<IWebElement> jobs = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//div[@class='recommend-job']")).ToList();
                    IWebElement post = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//div[Contains(@class, 'applied-select-all')]//button"));
                    post.Click();
                    printRecommendJobs(jobs);
                    NLogUtil.Info("相似职位投递成功！");
                }
                catch (NoSuchElementException e)
                {
                    NLogUtil.Error($"没有匹配到相似职位...{e}");
                }
                catch (Exception e)
                {
                    NLogUtil.Error($"相似职位投递异常！！！{e}");
                }
                // 投完了关闭当前窗口并切换至第一个窗口
                SeleniumUtil.CHROME_DRIVER.Close();
                SeleniumUtil.CHROME_DRIVER.SwitchTo().Window(tabs[0]);
            }
        }

        private static bool checkIsLimit()
        {
            try
            {
                SeleniumUtil.SleepByMilliseconds(500);
                IWebElement result = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//div[@class='a-job-apply-workflow']"));
                if (result.Text.Contains("达到上限"))
                {
                    NLogUtil.Info("今日投递已达上限！");
                    isLimit = true;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static void setMaxPages()
        {
            // 模拟 Ctrl + End
            SeleniumUtil.ACTIONS.KeyDown(Keys.Control).SendKeys(Keys.End).KeyUp(Keys.Control).Perform();
            while (true)
            {
                try
                {
                    IWebElement button = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//button[Contains(@class, 'btn') and Contains(@class, 'soupager__btn')][last()]"));
                    if (button.GetAttribute("disabled") != null)
                    {
                        // 按钮被禁用，退出循环
                        break;
                    }
                    button.Click();
                }
                catch
                {
                }
            }
            IWebElement lastPage = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//span[@class='soupager__index soupager__index--active']"));
            if (lastPage != null && int.TryParse(lastPage.Text, out int pp))
            {
                maxPage = pp;
            }
            // 模拟 Ctrl + Home
            SeleniumUtil.ACTIONS.KeyDown(Keys.Control).SendKeys(Keys.Home).KeyUp(Keys.Control).Perform();
        }

        private static void printRecommendJobs(List<IWebElement> jobs)
        {
            jobs.ForEach(j=> {
                string jobName = j.FindElement(By.XPath(".//*[Contains(@class, 'recommend-job__position')]")).Text;
                string salary = j.FindElement(By.XPath(".//span[@class='recommend-job__demand__salary']")).Text;
                string years = j.FindElement(By.XPath(".//span[@class='recommend-job__demand__experience']")).Text.Replace("\n", " ");
                string education = j.FindElement(By.XPath(".//span[@class='recommend-job__demand__educational']")).Text.Replace("\n", " ");
                string companyName = j.FindElement(By.XPath(".//*[Contains(@class, 'recommend-job__cname')]")).Text;
                string companyTag = j.FindElement(By.XPath(".//*[Contains(@class, 'recommend-job__demand__cinfo')]")).Text.Replace("\n", " ");
                Job job = new Job();
                job.JobName=jobName;
                job.Salary=salary;
                job.CompanyTag=companyTag;
                job.CompanyName=companyName;
                job.JobInfo=years + "·" + education;
                NLogUtil.Info($"投递【{companyName}】公司【{jobName}】岗位，薪资【{salary}】，要求【{years}·{education}】，规模【{companyTag}】");
            });
        }

        private static List<Job> getPositionList()
        {
            int jobSize = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//div[@class='joblist-box__item clearfix']")).Count;
            List<IWebElement> jobNameList = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//div[@class='jobinfo__top']//a")).ToList();
            List<IWebElement> salaryList = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//div[@class='jobinfo__top']//p")).ToList();
            List<IWebElement> jobAreaList = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//div[@class='jobinfo__other-info-item']//span")).ToList();
            List<IWebElement> companyNameList = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//div[Contains(@class, 'companyinfo__top')]//a")).ToList();
            List<IWebElement> companyTagList = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//div[Contains(@class, 'companyinfo__tag')]")).ToList();
            List<IWebElement> recruiterList = SeleniumUtil.CHROME_DRIVER.FindElements(By.XPath("//div[Contains(@class, 'companyinfo__staff-name')]")).ToList();
            if (jobNameList.Count != jobSize)
            {
                NLogUtil.Info("jobNameList size does not match jobSize");
            }
            if (salaryList.Count != jobSize)
            {
                NLogUtil.Info("salaryList size does not match jobSize");
            }
            if (jobAreaList.Count != jobSize)
            {
                NLogUtil.Info("jobAreaList size does not match jobSize");
            }
            if (companyNameList.Count != jobSize)
            {
                NLogUtil.Info("companyNameList size does not match jobSize");
            }
            if (companyTagList.Count != jobSize)
            {
                NLogUtil.Info("companyTagList size does not match jobSize");
            }
            if (recruiterList.Count != jobSize)
            {
                NLogUtil.Info("recruiterList size does not match jobSize");
            }
            List<Job> result = new List<Job>();
            for (int i = 0; i < jobSize; i++)
            {
                Job job = new Job();
                job.JobName = jobNameList[i].Text;
                job.Salary = salaryList[i].Text;
                job.JobArea = jobAreaList[i].Text.Replace("\n", " ");
                job.Recruiter = recruiterList[i].Text.Trim();
                job.CompanyTag = companyTagList[i].Text.Replace("\n", " ");
                job.Href = companyNameList[i].GetAttribute("href");
                job.CompanyName = companyNameList[i].Text;
                result.Add(job);
                NLogUtil.Info($"选中【{job.CompanyName}】公司【{job.JobName}】岗位，【{job.JobArea}】地区，薪资【{job.Salary}】，标签【{job.CompanyTag}】，HR【{job.Recruiter}】");
            }
            return result;
        }

        private static void login()
        {
            SeleniumUtil.CHROME_DRIVER.Navigate().GoToUrl(loginUrl);
            if (SeleniumUtil.IsCookieValid("./src/main/java/zhilian/cookie.json"))
            {
                SeleniumUtil.LoadCookie("./src/main/java/zhilian/cookie.json");
                SeleniumUtil.CHROME_DRIVER.Navigate().Refresh();
                SeleniumUtil.Sleep(1);
            }
            if (isLoginRequired())
            {
                scanLogin();
            }
        }

        private static void scanLogin()
        {
            try
            {
                IWebElement button = SeleniumUtil.CHROME_DRIVER.FindElement(By.XPath("//div[@class='zppp-panel-normal-bar__img']"));
                button.Click();
                NLogUtil.Info("等待扫码登录中...");
                SeleniumUtil.WAIT.Until(d=>d.FindElement(By.XPath("//div[@class='zp-main__personal']")));
                NLogUtil.Info("扫码登录成功！");
                SeleniumUtil.SaveCookie("./src/main/java/zhilian/cookie.json");
            }
            catch (Exception e)
            {
                NLogUtil.Error($"扫码登录异常！{e}");
                System.Environment.Exit(-1);
            }
        }

        private static bool isLoginRequired()
        {
            return !SeleniumUtil.CHROME_DRIVER.Url.Contains("i.zhaopin.com");
        }
    }

}
