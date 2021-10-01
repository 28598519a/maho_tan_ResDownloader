using System;
using System.Collections.Generic;
using System.Windows;

namespace maho_tan資源下載器
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        public static string Root = Environment.CurrentDirectory;
        public static string Outpath = Root;
        public static string Hotpath = String.Empty;
        public static string Respath = String.Empty;
        public static string Project = String.Empty;
        public static string Cfg1 = String.Empty;
        public static string Cfg3 = String.Empty;
        public static string Cfg5 = String.Empty;
        public static int glocount = 0;
        public static string MAIN_SERVER_URL = "https://jprd-main-sakura.fusion-studio.co.jp:7001/";
        public static string resourceUrl = "https://cdn-static-jprd-sakura.fusion-studio.co.jp/t_cn/1.15.0/resources/";
        public static string webClientUrl = "https://cdn-static-jprd-sakura.fusion-studio.co.jp/t_cn/1.15.0/client/html5/";
        public static List<string> log = new List<string>();
    }
}
