using BeatmapExporter.Exporters.Lazer.LazerDB;
using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using Realms;
using System.Runtime.InteropServices;

/*namespace BeatmapExporter.Exporters.Lazer
{
    public static class LazerLoader
    {
        public static BeatmapExporter Load(string? directory)
        {

            // osu!lazer has been selected at this point. 
            // load the osu!lazer database here, can operate on lazer-specific objects
            // assume default lazer directory, prompting user if not found (or specified as arg)
            if(directory is null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // default install location: %appdata%/osu
                    directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Application Support/osu");
                }
                else
                {
                    directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/osu");
                }
            }

            Console.Write($" --- kabii's Lazer Exporter ---\n\nChecking directory: {directory}\nRun this application with your osu!lazer storage directory as an argument if this is not your osu! data location.\n");

            // load beatmap information into memory
            LazerDatabase? database = LazerDatabase.Locate(directory);
            if (database is null)
            {
                Console.WriteLine("osu! database not found in default location or selected.");
                ExporterLoader.Exit();
            }

            Realm? realm = database!.Open();
            if (realm is null)
            {
                Console.WriteLine("\nUnable to open osu! database.");
                ExporterLoader.Exit();
            }

            Console.Write("\nosu! database opened successfully.\nLoading beatmaps...\n");

            // load beatmaps into memory for filtering/export later
            List<BeatmapSet> beatmaps = realm!.All<BeatmapSet>().ToList();

            Console.WriteLine("Loading osu!lazer collections...");
            List<BeatmapCollection> collections = realm.All<BeatmapCollection>().ToList();

            // start console i/o loop
            LazerExporter lazerExporter = new(database, beatmaps, collections);
            return new BeatmapExporter(lazerExporter);
        }
    }
}*/

namespace BeatmapExporter.Exporters.Lazer
{
    public static class LazerLoader
    {
        public static BeatmapExporter Load(string? directory)
        {
            // 在此时已选择osu!lazer。
            // 在这里加载osu!lazer数据库，可以操作特定于lazer的对象。
            // 假设默认的lazer目录，如果未找到（或指定为参数），则提示用户
            if (directory is null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // 默认安装位置：%appdata%/osu
                    directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Application Support/osu");
                }
                else
                {
                    directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/osu");
                }
            }

            Console.Write($" --- kabii's Lazer Exporter(CN) ---\n\n检查目录：{directory}\n如果这不是您的osu!数据位置，请使用osu!lazer存储目录作为参数运行此应用程序。\n");

            // 将谱面信息加载到内存
            LazerDatabase? database = LazerDatabase.Locate(directory);
            if (database is null)
            {
                Console.WriteLine("未在默认位置或选定位置找到osu!数据库。");
                ExporterLoader.Exit();
            }

            Realm? realm = database!.Open();
            if (realm is null)
            {
                Console.WriteLine("\n无法打开osu!数据库。");
                ExporterLoader.Exit();
            }

            Console.Write("\nosu!数据库成功打开。\n正在加载谱面...\n");

            // 将谱面加载到内存以供以后过滤/导出
            List<BeatmapSet> beatmaps = realm!.All<BeatmapSet>().ToList();

            Console.WriteLine("正在加载osu!lazer集合...");
            List<BeatmapCollection> collections = realm.All<BeatmapCollection>().ToList();

            // 开始控制台输入/输出循环
            LazerExporter lazerExporter = new(database, beatmaps, collections);
            return new BeatmapExporter(lazerExporter);
        }
    }
}
