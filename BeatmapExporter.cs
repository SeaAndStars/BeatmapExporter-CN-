/*
using BeatmapExporter.Exporters;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace BeatmapExporter
{
    public class BeatmapExporter
    {
        readonly IBeatmapExporter exporter;
        readonly ExporterConfiguration config;

        public BeatmapExporter(IBeatmapExporter exporter)
        {
            this.exporter = exporter;
            this.config = exporter.Configuration;
        }

        public void StartApplicationLoop()
        {
            while (true)
            {
                ApplicationLoop();
            }
        }

        void ApplicationLoop()
        {
            // output main application menu
            Console.Write($"\n1. Export selected {config.ExportFormatUnitName} ({exporter.SelectedBeatmapSetCount} beatmap sets, {exporter.SelectedBeatmapCount} beatmaps)\n2. Display selected beatmap sets ({exporter.SelectedBeatmapSetCount}/{exporter.BeatmapSetCount} beatmap sets)\n3. Display {exporter.CollectionCount} beatmap collections\n4. Advanced export settings (.mp3/image export, compression, export location)\n5. Edit beatmap selection/filters\n\n0. Exit\nSelect operation: ");

            string? input = Console.ReadLine();
            if (input is null)
            {
                ExporterLoader.Exit();
            }

            if (!int.TryParse(input, out int op) || op is < 0 or > 5)
            {
                Console.WriteLine("\nInvalid operation selected.");
                return;
            }

            switch (op)
            {
                case 0:
                    Environment.Exit(0);
                    break;
                case 1:
                    switch(config.ExportFormat)
                    {
                        case ExporterConfiguration.Format.Beatmap:
                            exporter.ExportBeatmaps();
                            break;
                        case ExporterConfiguration.Format.Audio:
                            exporter.ExportAudioFiles();
                            break;
                        case ExporterConfiguration.Format.Background:
                            exporter.ExportBackgroundFiles();
                            break;
                    }
                    break;
                case 2:
                    exporter.DisplaySelectedBeatmaps();
                    break;
                case 3:
                    exporter.DisplayCollections();
                    break;
                case 4:
                    ExportConfiguration();
                    break;
                case 5:
                    BeatmapFilterSelection();
                    break;
            }
        }

        void ExportConfiguration()
        {
            while(true)
            {
                StringBuilder settings = new();
                settings
                    .Append("\n--- Advanced export settings ---\n* indicates a setting that has been changed.\n")
                    .Append("\n1. ");

                bool exportBeatmaps = config.ExportFormat == ExporterConfiguration.Format.Beatmap;
                switch(config.ExportFormat)
                {
                    case ExporterConfiguration.Format.Beatmap:
                        settings.Append("Type 1: Beatmaps will be exported in osu! archive format (.osz)");
                        break;
                    case ExporterConfiguration.Format.Audio:
                        settings.Append("Type 2: Beatmap audio files will be renamed, tagged and exported (.mp3 format)*");
                        break;
                    case ExporterConfiguration.Format.Background:
                        settings.Append("Type 3: Only beatmap background images will be exported (original format)*");
                        break;
                }

                settings
                    .Append("\n2. Export path: ")
                    .Append(Path.GetFullPath(config.ExportPath));
                if (config.ExportPath != config.DefaultExportPath)
                    settings.Append('*');

                if(exportBeatmaps)
                {
                    settings.Append("\n3. ");
                    if (config.CompressionEnabled)
                        settings.Append(".osz compression is enabled (slow export, smaller file sizes)*");
                    else
                        settings.Append(".osz compression is disabled (fastest export)");
                }

                settings.Append("\n\nEdit setting # (Blank to save settings): ");

                Console.Write(settings.ToString());
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input, out int op) || op < 1 || op > (exportBeatmaps ? 4 : 2))
                {
                    Console.Write("\nInvalid operation selected.\n");
                    return;
                }

                switch(op)
                {
                    case 1:
                        switch (config.ExportFormat)
                        {
                            case ExporterConfiguration.Format.Beatmap:
                                config.ExportFormat = ExporterConfiguration.Format.Audio;
                                break;
                            case ExporterConfiguration.Format.Audio:
                                config.ExportFormat = ExporterConfiguration.Format.Background;
                                break;
                            case ExporterConfiguration.Format.Background:
                                config.ExportFormat = ExporterConfiguration.Format.Beatmap;
                                break;
                        }
                        break;
                    case 2:
                        Console.Write($"\nPath selected must be valid for your platform or export will fail! Be careful of invalid filename characters on Windows.\nAudio exports will automatically export to a '{ExporterConfiguration.DefaultAudioPath}' folder at this location.\nDefault export path: {config.DefaultExportPath}\nCurrent export path: {config.ExportPath}\nNew export path: ");
                        string? pathInput = Console.ReadLine();
                        if (string.IsNullOrEmpty(pathInput))
                            continue;
                        config.ExportPath = pathInput;
                        Console.WriteLine($"- CHANGED: Export location set to {Path.GetFullPath(config.ExportPath)}");
                        break;
                    case 3:
                        if(config.CompressionEnabled)
                        {
                            Console.WriteLine("- CHANGED: .osz output compression has been disabled.");
                            config.CompressionEnabled = false;
                        }
                        else
                        {
                            Console.WriteLine("- CHANGED: .osz output compression has been enabled.");
                            config.CompressionEnabled = true;
                        }
                        break;
                }
            }
        }

        void BeatmapFilterSelection()
        {
            Console.Write("\n--- Beatmap Selection ---\n");

            Console.Write(
@"只有符合所有激活的筛选条件的谱面才会被导出。
在筛选条件前加上""!""将对筛选条件取反，如果你想使用""小于""筛选条件。""!""可以与所有筛选条件一起使用，尽管示例仅显示了星级的用法。

示例：
- 仅导出6.3星及以上的谱面：stars 6.3
- 低于6.3星的谱面（使用否定）：!stars 6.3
- 长度超过1分30秒（90秒）：length 90
- 180BPM及以上：bpm 180
- 在过去7天内添加的谱面：since 7
- 在过去5小时内添加的谱面：since 5:00
- 特定谱面ID（逗号分隔）：id 1
- 由RLC或Nathan制作的谱面（逗号分隔）：author RLC, Nathan
- 特定艺术家（逗号分隔）：artist Camellia, nanahira
- 标签包含""touhou""：tag touhou
- 特定游戏模式：mode osu/mania/ctb/taiko
- 谱面状态：graveyard/leaderboard/ranked/approved/qualified/loved
- 包含在名为""songs""的特定收藏中：collection songs
- 包含在收藏列表中标有#1的特定收藏中：collection #1
- 包含在任何收藏中：collection -all
- 移除特定筛选条件（使用上面列表中的行号）：remove 1
- 移除所有筛选条件：reset
返回导出菜单：exit"
"
);

            while (true)
            {
                var filters = config.Filters;
                if (filters.Count > 0)
                {
                    Console.Write("----------------------\nCurrent beatmap filters:\n\n");
                    Console.Write(exporter.FilterDetail());
                    Console.Write($"\nMatched beatmap sets: {exporter.SelectedBeatmapSetCount}/{exporter.BeatmapSetCount}\n\n");
                }
                else
                {
                    Console.Write("\n\nThere are no active beatmap filters. ALL beatmaps currently selected for export.\n\n");
                }

                // start filter selection ui mode
                Console.Write("Select filter (Blank to save selection): ");

                string? input = Console.ReadLine()?.ToLower();
                if (string.IsNullOrEmpty(input))
                {
                    return;
                }

                string[] command = input.Split(" ");
                // check for filter "remove" operations, otherwise pass to parse filter 
                switch (command[0])
                {
                    case "remove":
                        string? idArg = command.ElementAtOrDefault(1);
                        TryRemoveBeatmapFilter(idArg);
                        break;
                    case "reset":
                        ResetBeatmapFilters();
                        break;
                    case "exit":
                        return;
                    default:
                        // parse as new filter
                        BeatmapFilter? filter = new FilterParser(input).Parse();
                        if (filter is not null)
                        {
                            filters.Add(filter);
                            exporter.UpdateSelectedBeatmaps();
                            Console.Write("\nFilter added.\n\n");
                        }
                        else
                        {
                            Console.WriteLine($"Invalid filter '{command[0]}'.");
                        }
                        break;
                }
            }
        }

        void TryRemoveBeatmapFilter(string? idArg)
        {
            var filters = config.Filters;
            if (idArg is null || !int.TryParse(idArg, out int id) || id < 1 || id > filters.Count)
            {
                Console.WriteLine($"Not an existing rule ID: {idArg}");
                return;
            }

            filters.RemoveAt(id - 1);
            Console.WriteLine("Filter removed.");
            exporter.UpdateSelectedBeatmaps();
            return;
        }

        void ResetBeatmapFilters()
        {
            config.Filters.Clear();
            exporter.UpdateSelectedBeatmaps();
        }

        public static void OpenExportDirectory(string directory)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", directory);
            }
        }
    }
}
*/

//csharp
using BeatmapExporter.Exporters;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BeatmapExporter
{
    public class BeatmapExporter
    {
        readonly IBeatmapExporter exporter;
        readonly ExporterConfiguration config;

        public BeatmapExporter(IBeatmapExporter exporter)
        {
            this.exporter = exporter;
            this.config = exporter.Configuration;
        }

        public void StartApplicationLoop()
        {
            while (true)
            {
                ApplicationLoop();
            }
        }

        void ApplicationLoop()
        {
            // 输出主应用菜单
            Console.Write($"\n1. 导出已选择的 {config.ExportFormatUnitName}（{exporter.SelectedBeatmapSetCount} 谱面集，{exporter.SelectedBeatmapCount} 谱面）\n2. 显示已选择的谱面集（{exporter.SelectedBeatmapSetCount}/{exporter.BeatmapSetCount} 谱面集）\n3. 显示 {exporter.CollectionCount} 谱面收藏\n4. 高级导出设置（.mp3/图像导出，压缩，导出位置）\n5. 编辑谱面选择/筛选\n\n0. 退出\n选择操作: ");

            string? input = Console.ReadLine();
            if (input is null)
            {
                ExporterLoader.Exit();
            }

            if (!int.TryParse(input, out int op) || op is < 0 or > 5)
            {
                Console.WriteLine("\n选择的操作无效。");
                return;
            }

            switch (op)
            {
                case 0:
                    Environment.Exit(0);
                    break;
                case 1:
                    switch (config.ExportFormat)
                    {
                        case ExporterConfiguration.Format.Beatmap:
                            exporter.ExportBeatmaps();
                            break;
                        case ExporterConfiguration.Format.Audio:
                            exporter.ExportAudioFiles();
                            break;
                        case ExporterConfiguration.Format.Background:
                            exporter.ExportBackgroundFiles();
                            break;
                    }
                    break;
                case 2:
                    exporter.DisplaySelectedBeatmaps();
                    break;
                case 3:
                    exporter.DisplayCollections();
                    break;
                case 4:
                    ExportConfiguration();
                    break;
                case 5:
                    BeatmapFilterSelection();
                    break;
            }
        }

        void ExportConfiguration()
        {
            while (true)
            {
                StringBuilder settings = new();
                settings
                    .Append("\n--- 高级导出设置 ---\n* 表示已更改的设置。\n")
                    .Append("\n1. ");

                bool exportBeatmaps = config.ExportFormat == ExporterConfiguration.Format.Beatmap;
                switch (config.ExportFormat)
                {
                    case ExporterConfiguration.Format.Beatmap:
                        settings.Append("类型 1: 谱面将以 osu! 归档格式导出 (.osz)");
                        break;
                    case ExporterConfiguration.Format.Audio:
                        settings.Append("类型 2: 谱面音频文件将被重命名，标记并导出（.mp3 格式）*");
                        break;
                    case ExporterConfiguration.Format.Background:
                        settings.Append("类型 3: 仅导出谱面背景图像（原始格式）*");
                        break;
                }

                settings
                    .Append("\n2. 导出路径: ")
                    .Append(Path.GetFullPath(config.ExportPath));
                if (config.ExportPath != config.DefaultExportPath)
                    settings.Append('*');

                if (exportBeatmaps)
                {
                    settings.Append("\n3. ");
                    if (config.CompressionEnabled)
                        settings.Append(".osz 压缩已启用（导出较慢，文件较小）*");
                    else
                        settings.Append(".osz 压缩已禁用（最快导出）");
                }

                settings.Append("\n\n编辑设置编号（留空保存设置）: ");

                Console.Write(settings.ToString());
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input, out int op) || op < 1 || op > (exportBeatmaps ? 4 : 2))
                {
                    Console.Write("\n选择的操作无效。\n");
                    return;
                }

                switch (op)
                {
                    case 1:
                        switch (config.ExportFormat)
                        {
                            case ExporterConfiguration.Format.Beatmap:
                                config.ExportFormat = ExporterConfiguration.Format.Audio;
                                break;
                            case ExporterConfiguration.Format.Audio:
                                config.ExportFormat = ExporterConfiguration.Format.Background;
                                break;
                            case ExporterConfiguration.Format.Background:
                                config.ExportFormat = ExporterConfiguration.Format.Beatmap;
                                break;
                        }
                        break;
                    case 2:
                        Console.Write($"\n所选路径必须对您的平台有效，否则导出将失败！在 Windows 上小心无效的文件名字符。\n音频导出将自动导出到此位置的 '{ExporterConfiguration.DefaultAudioPath}' 文件夹。\n默认导出路径: {config.DefaultExportPath}\n当前导出路径: {config.ExportPath}\n新导出路径: ");
                        string? pathInput = Console.ReadLine();
                        if (string.IsNullOrEmpty(pathInput))
                            continue;
                        config.ExportPath = pathInput;
                        Console.WriteLine($"- 已更改: 导出位置设置为 {Path.GetFullPath(config.ExportPath)}");
                        break;
                    case 3:
                        if (config.CompressionEnabled)
                        {
                            Console.WriteLine("- 已更改: .osz 输出压缩已禁用。");
                            config.CompressionEnabled = false;
                        }
                        else
                        {
                            Console.WriteLine("- 已更改: .osz 输出压缩已启用。");
                            config.CompressionEnabled = true;
                        }
                        break;
                }
            }
        }

        void BeatmapFilterSelection()
        {
            Console.Write("\n--- 谱面选择 ---\n");

            Console.Write(
                @"只有符合所有激活的筛选条件的谱面才会被导出。
在筛选条件前加上""!""将对筛选条件取反，如果你想使用""小于""筛选条件。""!""可以与所有筛选条件一起使用，尽管示例仅显示了星级的用法。

示例：
- 仅导出6.3星及以上的谱面：stars 6.3
- 低于6.3星的谱面（使用否定）：!stars 6.3
- 长度超过1分30秒（90秒）：length 90
- 180BPM及以上：bpm 180
- 在过去7天内添加的谱面：since 7
- 在过去5小时内添加的谱面：since 5:00
- 特定谱面ID（逗号

分隔）：id 1
- 由RLC或Nathan制作的谱面（逗号分隔）：author RLC, Nathan
- 特定艺术家（逗号分隔）：artist Camellia, nanahira
- 标签包含""touhou""：tag touhou
- 特定游戏模式：mode osu/mania/ctb/taiko
- 谱面状态：graveyard/leaderboard/ranked/approved/qualified/loved
- 包含在名为""songs""的特定收藏中：collection songs
- 包含在收藏列表中标有#1的特定收藏中：collection #1
- 包含在任何收藏中：collection -all
- 移除特定筛选条件（使用上面列表中的行号）：remove 1
- 移除所有筛选条件：reset
返回导出菜单：exit"
            );

            while (true)
            {
                var filters = config.Filters;
                if (filters.Count > 0)
                {
                    Console.Write("----------------------\n当前谱面筛选条件:\n\n");
                    Console.Write(exporter.FilterDetail());
                    Console.Write($"\n匹配的谱面集数量: {exporter.SelectedBeatmapSetCount}/{exporter.BeatmapSetCount}\n\n");
                }
                else
                {
                    Console.Write("\n\n没有激活的谱面筛选条件。当前选择导出的是所有谱面。\n\n");
                }

                // 启动筛选条件选择 UI 模式
                Console.Write("选择筛选条件（留空保存选择）: ");

                string? input = Console.ReadLine()?.ToLower();
                if (string.IsNullOrEmpty(input))
                {
                    return;
                }

                string[] command = input.Split(" ");
                // 检查是否有 "remove" 操作，否则传递到解析筛选条件
                switch (command[0])
                {
                    case "remove":
                        string? idArg = command.ElementAtOrDefault(1);
                        TryRemoveBeatmapFilter(idArg);
                        break;
                    case "reset":
                        ResetBeatmapFilters();
                        break;
                    case "exit":
                        return;
                    default:
                        // 解析为新的筛选条件
                        BeatmapFilter? filter = new FilterParser(input).Parse();
                        if (filter is not null)
                        {
                            filters.Add(filter);
                            exporter.UpdateSelectedBeatmaps();
                            Console.Write("\n已添加筛选条件。\n\n");
                        }
                        else
                        {
                            Console.WriteLine($"无效的筛选条件 '{command[0]}'。");
                        }
                        break;
                }
            }
        }

        void TryRemoveBeatmapFilter(string? idArg)
        {
            var filters = config.Filters;
            if (idArg is null || !int.TryParse(idArg, out int id) || id < 1 || id > filters.Count)
            {
                Console.WriteLine($"不是现有的规则编号: {idArg}");
                return;
            }

            filters.RemoveAt(id - 1);
            Console.WriteLine("筛选条件已移除。");
            exporter.UpdateSelectedBeatmaps();
            return;
        }

        void ResetBeatmapFilters()
        {
            config.Filters.Clear();
            exporter.UpdateSelectedBeatmaps();
        }

        public static void OpenExportDirectory(string directory)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", directory);
            }
        }
    }
}
