// 程序入口点 —— 启动 WinForms 主窗口，是整个应用的起点
using System;
using System.IO;
using System.Windows.Forms;
namespace ServerUI;
static class Program
{
    // [STAThread] 表示该线程使用单线程单元模型（Single-Threaded Apartment），WinForms 应用必须加上此特性才能正常工作
    [STAThread]
    static void Main()
    {
        // 全局异常兜底：以前构造/加载阶段一旦抛异常，进程会静默退出（用户点了没反应）。
        // 现在统一捕获并弹窗 + 写崩溃日志，确保"打不开"时能看到原因而不是毫无反应。
        AppDomain.CurrentDomain.UnhandledException += (s, e) => Report(e.ExceptionObject as Exception, "UnhandledException");
        Application.ThreadException += (s, e) => Report(e.Exception, "ThreadException");
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        try
        {
            // SetHighDpiMode 必须在创建任何窗口前调用，保证 Win10/Win11 高分屏下不糊、不错位
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Application.Run 启动 Windows 消息循环，显示主窗口并持续监听用户操作，直到窗口关闭程序才退出
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            Report(ex, "StartupFatal");
        }
    }

    // Report() 把异常写入 exe 同目录的崩溃日志并弹窗提示，避免"双击没反应"无从排查
    static void Report(Exception ex, string tag)
    {
        if (ex == null) return;
        var msg = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + tag + "\n" + ex + "\n\n";
        try
        {
            var log = Path.Combine(AppContext.BaseDirectory, "ServerUI-崩溃日志.txt");
            File.AppendAllText(log, msg, System.Text.Encoding.UTF8);
        }
        catch { }
        try
        {
            MessageBox.Show(
                "程序发生错误，已记录到 ServerUI-崩溃日志.txt。\n\n" +
                "常见原因：\n" +
                "  1. 有依赖版需先安装 .NET 10 运行环境（可改用无依赖版）\n" +
                "  2. 被杀毒软件拦截，请加入信任\n\n" +
                "错误信息：\n" + ex.GetBaseException().Message,
                "ServerS4A12 管理器 - 启动失败",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch { }
    }
}
