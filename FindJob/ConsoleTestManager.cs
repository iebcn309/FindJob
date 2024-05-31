using FindJob.Boss;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace FindJob
{
    internal class ConsoleTestManager
    {
        private readonly ConsoleMessagePrinter msgPrinter=new ConsoleMessagePrinter();
        private const int exitCode = 0;
        private const string inputClear = "c";
        private const string inputHelp = "h";
        private static readonly string helpMessage =
            $"选择一个平台: {Environment.NewLine}" +
            $"1 BOSS直聘{Environment.NewLine}" +
            $"2 51job{Environment.NewLine}" +
            $"3 拉勾网{Environment.NewLine}" +
            $"4 猎聘{Environment.NewLine}" +
            $"5 智联{Environment.NewLine}";
        public virtual void ShowTestNames()
        {
            msgPrinter.PrintLine();
            msgPrinter.PrintInfo($"1 BOSS直聘");
            msgPrinter.PrintInfo($"2 51job");
            msgPrinter.PrintInfo($"3 拉勾网");
            msgPrinter.PrintInfo($"4 猎聘");
            msgPrinter.PrintInfo($"5 智联");
            msgPrinter.PrintLine();
        }

        private string PrintNamesAndRead()
        {
            msgPrinter.PrintSuccess(
                $"选择一个平台.{Environment.NewLine}输入 {exitCode} 退出, 输入 {inputClear} 清除历史, 输入 {inputHelp} 显示帮助信息.");
            ShowTestNames();
            return System.Console.ReadLine();
        }
        private string PrintErrorAndRead(string message)
        {
            msgPrinter.PrintError(message);
            return System.Console.ReadLine();
        }
        public virtual void ShowTestEntrance()
        {
            var input = PrintNamesAndRead();

            while (true)
            {
                if (input?.ToLower() == inputClear)
                {
                    System.Console.Clear();
                    input = PrintNamesAndRead();
                    continue;
                }
                if (input?.ToLower() == inputHelp)
                {
                    msgPrinter.PrintSuccess(helpMessage);
                    input = PrintNamesAndRead();
                    continue;
                }
                if (int.TryParse(input, out int number))
                {
                    if (number == exitCode)
                        break;

                    if (number < 0 || number > 5)
                    {
                        input = PrintErrorAndRead($"数字超出范围,请重新输入(输入 {exitCode} 退出)");
                        continue;
                    }
 
                    try
                    {
                        switch (number)
                        {
                            case 1:
                                FindJob.Boss.Boss.Run();
                                break;
                            case 2:
                                FindJob.Job51.Job51Automation.Run();
                                break;
                            case 3:
                                FindJob.Lagou.Lagou.Run();
                                break;
                            case 4:
                                FindJob.Liepin.Liepin.Run();
                                break;
                            case 5:
                                FindJob.ZhiLian.ZhiLian.Run();
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        msgPrinter.PrintError(ex.Message);
                        msgPrinter.PrintError(ex.StackTrace ?? "");
                    }

                    input = PrintNamesAndRead();

                }
                else
                {
                    input = PrintErrorAndRead($"输入值({input})无效 请重新输入(输入 {exitCode} 退出)");
                }
            }
        }

    }
}
