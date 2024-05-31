using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Runtime.InteropServices;


namespace FindJob
{

    public static class SeleniumUtil
    {
        public static ChromeDriver CHROME_DRIVER;
        public static Actions ACTIONS;
        public static WebDriverWait WAIT;

        public static void InitializeDriver()
        {
            GetChromeDriver();
            GetActions();
            GetWait(60);
        }

        private static void GetChromeDriver()
        {           
            string osName = RuntimeInformation.OSDescription;
            NLogUtil.Info($"当前操作系统为【{osName}】");
            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //{
            //    options.BinaryLocation = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            //    ChromeDriverService.CreateDefaultService(@"src\main\resources\chromedriver.exe");
            //}
            //else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            //{
            //    options.BinaryLocation = @"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";
            //    ChromeDriverService.CreateDefaultService(@"src\main\resources\chromedriver-mac-arm64\chromedriver");

            //}
            //else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //{
            //    options.BinaryLocation = @"/usr/bin/google-chrome-stable";
            //    ChromeDriverService.CreateDefaultService(@"src\main\resources\chromedriver-linux64\chromedriver");
            //}
            ChromeDriverService service = ChromeDriverService.CreateDefaultService(System.Environment.CurrentDirectory);
            //  service.HideCommandPromptWindow = true;


            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--test-type", "--ignore-certificate-errors");
            options.AddArgument("enable-automation");
            //   options.AddArgument("headless");
            //  options.AddArguments("--proxy-server=http://user:password@yourProxyServer.com:8080");

            options.AddExtension(Path.Combine(Environment.CurrentDirectory, "Resources", "xpathHelper.crx"));
            //if (Screen.AllScreens.Length > 1)
            //{
            //    options.AddArgument("--window-position=2800,1000");
            //}
            //options.AddArgument("--headless"); // 无头模式
            CHROME_DRIVER = new ChromeDriver(options);
            CHROME_DRIVER.Manage().Window.Maximize();
        }
        public static void SaveCookie(string path)
        {
            var cookies = CHROME_DRIVER.Manage().Cookies.AllCookies.ToList();
            var jsonArray = new JArray(cookies.Select(cookie => new JObject
            {
                ["name"] = cookie.Name,
                ["value"] = cookie.Value,
                ["domain"] = cookie.Domain,
                ["path"] = cookie.Path,
                ["expiry"] = cookie.Expiry.HasValue ? new DateTimeOffset(cookie.Expiry.Value).ToUnixTimeMilliseconds() : null,
                ["isSecure"] = cookie.Secure,
                ["isHttpOnly"] = cookie.IsHttpOnly
            }));
            SaveCookieToFile(jsonArray, path);
        }

        private static void SaveCookieToFile(JArray jsonArray, string path)
        {
            try
            {
                File.WriteAllText(path, jsonArray.ToString(Formatting.Indented));
                NLogUtil.Info($"Cookie已保存到文件：{path}");
            }
            catch (IOException e)
            {
                NLogUtil.Error($"保存cookie异常！保存路径:{path}{e}");
            }
        }
        public static void GetActions()
        {
            ACTIONS = new Actions(CHROME_DRIVER);
        }
        public static void GetWait(long time)
        {
            WAIT = new WebDriverWait(CHROME_DRIVER, TimeSpan.FromSeconds(time));
        }
        public static void Sleep(int seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }
        public static void SleepByMilliseconds(int milliseconds)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(milliseconds));
        }
        public static IWebElement FindElement(By by)
        {
            try
            {
                return CHROME_DRIVER.FindElement(by);
            }
            catch
            {
                NLogUtil.Error("无法找到元素：" + by.GetDescription());
                return null;
            }
        }
        public static void Click(By by)
        {
            try
            {
                FindElement(by).Click();
            }
            catch
            {
                NLogUtil.Error("点击元素时出错：" + by.GetDescription());
            }
        }

        public static bool IsCookieValid(string cookiePath)
        {
            return File.Exists(cookiePath);
        }

        public static void LoadCookie(string cookiePath)
        {   
            // 首先清除由于浏览器打开已有的cookies
            CHROME_DRIVER.Manage().Cookies.DeleteAllCookies();
            // 从文件中读取JSON
            JArray jsonArray = null;
            try
            {
                string jsonText = File.ReadAllText(cookiePath);
                if (!string.IsNullOrWhiteSpace(jsonText))
                {
                    jsonArray = JArray.Parse(jsonText);
                }
            }
            catch (IOException e)
            {
                NLogUtil.Error("读取cookie时发生异常。"+ e.ToString());
            }

            // 遍历JSON数组中的每个对象，并从中获取cookie的信息
            if (jsonArray != null)
            {
                foreach (JObject jsonObject in jsonArray)
                {
                    string name = jsonObject["name"]?.ToString();
                    string value = jsonObject["value"]?.ToString();
                    string domain = jsonObject["domain"]?.ToString();
                    string path = jsonObject["path"]?.ToString();
                    long expiryTimestamp = jsonObject["expiry"]?.Value<long>() ?? 0;
                    DateTime? expiry = expiryTimestamp > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(expiryTimestamp).UtcDateTime : (DateTime?)null;
                    bool isSecure = jsonObject["isSecure"].Value<bool>();
                    bool isHttpOnly = jsonObject["isHttpOnly"].Value<bool>();

                    // 使用这些信息来创建新的Cookie对象，并将它们添加到WebDriver中
                    
                    Cookie cookie = new Cookie(name, value, domain, path, expiry, isSecure, isHttpOnly,"");
                    try
                    {
                        CHROME_DRIVER.Manage().Cookies.AddCookie(cookie);
                    }
                    catch (Exception)
                    {
                        // 空异常处理，如果添加cookie失败则忽略
                    }
                }

                // 将修改后的jsonArray写回文件
                SaveCookieToFile(jsonArray, cookiePath);
            }
        }
    }
}
