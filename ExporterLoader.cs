/*
using BeatmapExporter.Exporters.Lazer;

namespace BeatmapExporter
{
    internal class ExporterLoader
    {
        const string Version = "1.3.11";
        static async Task Main(string[] args) 
        {
            // check application version 
            try
            {
                var client = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(2),
                };
                var latest = await client.GetStringAsync("https://raw.githubusercontent.com/kabiiQ/BeatmapExporter/main/VERSION");
                if(latest != Version)
                {
                    Console.WriteLine($"UPDATE AVAILABLE for BeatmapExporter: ({Version} -> {latest})\nhttps://github.com/kabiiQ/BeatmapExporter/releases/latest\n");
                }
            }
            catch (Exception) { } // unable to load version from github. not critical error, dont bother user

            // currently only load lazer, can add interface for selecting osu stable here later
            BeatmapExporter exporter = LazerLoader.Load(args.FirstOrDefault());

            exporter.StartApplicationLoop();

            Exit();
        }

        public static void Exit()
        {
            // keep console open
            Console.Write("\nPress any key to exit.\n");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
*/
//csharp
using BeatmapExporter.Exporters.Lazer;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BeatmapExporter
{
    internal class ExporterLoader
    {
        const string Version = "1.3.11";

        static async Task Main(string[] args)
        {
            // 检查应用程序版本
            try
            {
                using var client = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(2),
                };

                var latest = await client.GetStringAsync("https://raw.githubusercontent.com/kabiiQ/BeatmapExporter/main/VERSION");
                if (latest != Version)
                {
                    Console.WriteLine($"BeatmapExporter 有可用的更新: ({Version} -> {latest})\nhttps://github.com/kabiiQ/BeatmapExporter/releases/latest\n");
                }
            }
            catch (Exception)
            {
                // 无法从 GitHub 加载版本。这不是关键错误，不要打扰用户
            }

            // 目前只加载 Lazer，以后可以在这里添加选择 osu stable 的接口
            BeatmapExporter exporter = LazerLoader.Load(args.FirstOrDefault());

            exporter.StartApplicationLoop();

            Exit();
        }

        public static void Exit()
        {
            // 保持控制台打开
            Console.Write("\n按任意键退出。\n");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
