using BeatmapExporter.Exporters.Lazer.LazerDB;
using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace BeatmapExporter.Exporters.Lazer
{
    public class LazerExporter : IBeatmapExporter
    {
        readonly LazerDatabase lazerDb;
        readonly ExporterConfiguration config;

        readonly List<BeatmapSet> beatmapSets;
        readonly int beatmapCount;

        int selectedBeatmapCount; // 内部维护的已选择谱面数量
        List<BeatmapSet> selectedBeatmapSets;

        readonly Transcoder transcoder;

        readonly Dictionary<string, MapCollection>? collections;
        List<string> selectedFromCollections;

        public LazerExporter(LazerDatabase lazerDb, List<BeatmapSet> beatmapSets, List<BeatmapCollection>? lazerCollections)
        {
            this.lazerDb = lazerDb;

            var nonEmpty = beatmapSets.Where(set => set.Beatmaps.Count > 0).OrderBy(set => set.OnlineID).ToList();
            this.beatmapSets = nonEmpty;
            this.selectedBeatmapSets = nonEmpty;

            var allBeatmaps = nonEmpty.SelectMany(s => s.Beatmaps).ToList();

            int count = allBeatmaps.Count;
            this.beatmapCount = count;
            this.selectedBeatmapCount = count;

            this.selectedFromCollections = new();

            this.config = new ExporterConfiguration("lazerexport");

            if (lazerCollections != null)
            {
                collections = new();
                var colIndex = 1;
                foreach (var coll in lazerCollections)
                {
                    var colMaps = allBeatmaps
                        .Where(b => coll.BeatmapMD5Hashes.Contains(b.MD5Hash))
                        .ToList();
                    collections[coll.Name] = new MapCollection(colIndex, colMaps);
                    colIndex++;
                }
            }
            else
            {
                Console.WriteLine("集合过滤和信息将不可用。");
                collections = null;
            }

            this.transcoder = new Transcoder();
        }

        public int BeatmapSetCount
        {
            get => beatmapSets.Count;
        }

        public int BeatmapCount
        {
            get => beatmapCount;
        }

        public int SelectedBeatmapSetCount
        {
            get => selectedBeatmapSets.Count;
        }

        public int SelectedBeatmapCount
        {
            get => selectedBeatmapCount;
        }

        public int CollectionCount
        {
            get => collections?.Count ?? 0;
        }

        public ExporterConfiguration Configuration
        {
            get => config;
        }

        public List<string> SelectedFromCollections
        {
            get => selectedFromCollections;
        }

        IEnumerable<String> FilterInfo() => config.Filters.Select((f, i) =>
        {
            int includedCount = beatmapSets.SelectMany(set => set.Beatmaps).Count(b => f.Includes(b));
            return $"{i + 1}. {f.Description} ({includedCount} 谱面)";
        });

        public string FilterDetail() => string.Join("\n", FilterInfo());

        public void DisplaySelectedBeatmaps()
        {
            // 显示当前已选择的所有谱面
            foreach (var map in selectedBeatmapSets)
            {
                Console.WriteLine(map.Display());
            }
        }

        public void ExportBeatmaps()
        {
            // 对当前已选择的谱面执行导出操作
            // 生成应跳过的难度文件的排除文件哈希集合
            // 这些是原始谱面中的难度，但不是'已选择'的难度
            // 在进行文件导出时，我们不关心导出的是哪个难度文件、音频文件等
            var excludedHashes =
                from set in selectedBeatmapSets // 获取至少包含一个已选择难度的所有谱面集
                from map in set.Beatmaps // 获取来自这些谱面集的每个难度（不考虑过滤）
                where !set.SelectedBeatmaps.Contains(map) // 获取将被过滤导出的难度
                select map.Hash;
            var excluded = excludedHashes.ToList();

            string exportDir = config.ExportPath;
            Directory.CreateDirectory(exportDir);
            Console.WriteLine($"已选择 {SelectedBeatmapSetCount} 谱面集进行导出。");

            BeatmapExporter.OpenExportDirectory(exportDir);

            // 将所有命名文件导出到 .osz 压缩文件，排除过滤的难度
            int attempted = 0;
            int exported = 0;
            foreach (var mapset in selectedBeatmapSets)
            {
                attempted++;
                string filename = mapset.ArchiveFilename();
                Console.WriteLine($"无法导出 ({attempted}/{SelectedBeatmapSetCount}): {filename}");

                Stream? export = null;
                try
                {
                    string exportPath = Path.Combine(exportDir, filename);
                    export = File.Open(exportPath, FileMode.CreateNew);

                    using ZipArchive osz = new(export, ZipArchiveMode.Create, true);
                    foreach (var namedFile in mapset.Files)
                    {
                        string hash = namedFile.File.Hash;
                        if (excluded.Contains(hash))
                            continue;
                        var entry = osz.CreateEntry(namedFile.Filename, config.CompressionLevel);
                        using var entryStream = entry.Open();
                        // 从 lazer 文件存储中打开实际的难度/音频/图像文件
                        using var file = lazerDb.OpenHashedFile(hash);
                        file?.CopyTo(entryStream);
                    }
                    exported++;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"不能导出 {filename} :: {e.Message}");
                }
                finally
                {
                    export?.Dispose();
                }
            }

            string location = Path.GetFullPath(exportDir);
            Console.WriteLine($"已从 {location} 导出 {exported}/{SelectedBeatmapSetCount} 谱面。");
        }

        public void ExportAudioFiles()
        {
            // 执行将歌曲导出为 .mp3 文件的操作
            string exportDir = config.ExportPath;
            Directory.CreateDirectory(exportDir);

            Console.WriteLine($"正在将 {selectedBeatmapSets.Count} 谱面集中的音频导出为 .mp3 文件。");
            if (transcoder.Available)
                Console.WriteLine("如果许多已选择的谱面不是 .mp3 格式，此操作将花费更长时间。");
            else
                Console.WriteLine("未找到 FFmpeg 运行时。将跳过使用其他音频格式而不是 .mp3 的谱面。\n确保 ffmpeg.exe 位于系统 PATH 中或放置在此 BeatmapExporter.exe 目录中以启用转码。");

            BeatmapExporter.OpenExportDirectory(exportDir);

            int exportedAudioFiles = 0;
            int attempted = 0;
            foreach (var mapset in selectedBeatmapSets)
            {
                // 从此集合中获取具有不同音频文件的任何谱面难度
                // 通常只有一个 'audio.mp3' 由约定命名。也可能跨不同难度有多个
                var uniqueMetadata = mapset
                    .SelectedBeatmaps
                    .Select(b => b.Metadata)
                    .GroupBy(m => m.AudioFile)
                    .Select(g => g.First())
                    .ToList();

                foreach (var metadata in uniqueMetadata)
                {
                    try
                    {
                        // 如果音频不是 .mp3 格式，则进行转码
                        string extension = Path.GetExtension(metadata.AudioFile);
                        bool transcode = extension.ToLower() != ".mp3";
                        string transcodeNotice = transcode ? $"（需要从 {extension} 转码）" : "";

                        // 生成比 'audio.mp3' 更有意义的文件名
                        string outputFilename = metadata.OutputAudioFilename(mapset.OnlineID);
                        string outputFile = Path.Combine(exportDir, outputFilename);

                        attempted++;
                        Console.WriteLine($"({attempted}/?) 导出 {outputFilename}{transcodeNotice}");

                        using FileStream? audio = lazerDb.OpenNamedFile(mapset, metadata.AudioFile);
                        if (audio is null)
                            continue;

                        if (transcode)
                        {
                            // 转码器（FFmpeg）不可用，跳过。
                            if (!transcoder.Available)
                            {
                                Console.WriteLine($"谱面具有非 mp3 音频：{metadata.AudioFile}。FFmpeg 未加载，跳过。");
                                continue;
                            }
                            try
                            {
                                transcoder.TranscodeMP3(audio, outputFile);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"无法转码音频：{metadata.AudioFile}。发生错误 :: {e.Message}");
                                continue;
                            }
                        }
                        else
                        {
                            using FileStream output = File.Open(outputFile, FileMode.CreateNew);
                            audio.CopyTo(output);
                        }

                        // 设置 mp3 标签
                        try
                        {
                            var mp3 = TagLib.File.Create(outputFile);
                            if (string.IsNullOrEmpty(mp3.Tag.Title))
                                mp3.Tag.Title = metadata.TitleUnicode;
                            if (mp3.Tag.Performers.Count() == 0)
                                mp3.Tag.Performers = new[] { metadata.ArtistUnicode };
                            if (string.IsNullOrEmpty(mp3.Tag.Description))
                                mp3.Tag.Description = metadata.Tags;
                            mp3.Tag.Comment = $"{mapset.OnlineID} {metadata.Tags}";

                            // 将谱面背景设置为专辑封面
                            if (mp3.Tag.Pictures.Count() == 0 && metadata.BackgroundFile is not null)
                            {
                                using FileStream? bg = lazerDb.OpenNamedFile(mapset, metadata.BackgroundFile);
                                if (bg is not null)
                                {
                                    using MemoryStream ms = new();
                                    bg.CopyTo(ms);
                                    byte[] image = ms.ToArray();

                                    var cover = new TagLib.Id3v2.AttachmentFrame
                                    {
                                        Type = TagLib.PictureType.FrontCover,
                                        Description = "Background",
                                        MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
                                        Data = image,
                                    };
                                    mp3.Tag.Pictures = new[] { cover };
                                }
                            }

                            mp3.Save();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"无法为 {outputFilename} 设置元数据 :: {e.Message}\n导出将继续。");
                        }
                        exportedAudioFiles++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"无法导出谱面 :: {e.Message}");
                    }
                }
            }

            string location = Path.GetFullPath(exportDir);
            Console.WriteLine($"已从 {location} 导出 {exportedAudioFiles}/{attempted} 音频文件，从 {SelectedBeatmapCount} 谱面。");
        }

        public void ExportBackgroundFiles()
        {
            // perform export of beatmap backgrounds
            string exportDir = config.ExportPath;
            Directory.CreateDirectory(exportDir);

            BeatmapExporter.OpenExportDirectory(exportDir);

            int exportedBackgroundFiles = 0;
            int attempted = 0;
            foreach (var mapset in selectedBeatmapSets)
            {
                // 从此集合中获取具有不同背景图像文件名的谱面难度
                var uniqueMetadata = mapset
                    .SelectedBeatmaps
                    .Select(b => b.Metadata)
                    .Where(m => m.BackgroundFile != null)
                    .GroupBy(m => m.BackgroundFile)
                    .Select(g => g.First())
                    .ToList();

                foreach (var metadata in uniqueMetadata)
                {
                    try
                    {
                        // 获取包含原始背景名称的背景的输出文件名
                        string outputFilename = metadata.OutputBackgroundFilename(mapset.OnlineID);
                        string outputFile = Path.Combine(exportDir, outputFilename);

                        attempted++;
                        Console.WriteLine($"({attempted}/?) 导出 {outputFilename}");

                        using FileStream? background = lazerDb.OpenNamedFile(mapset, metadata.BackgroundFile);
                        if (background is null)
                            continue;

                        using FileStream output = File.Open(outputFile, FileMode.CreateNew);
                        background.CopyTo(output);

                        exportedBackgroundFiles++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"无法导出背景图像 :: {e.Message}");
                    }
                }
            }

            string location = Path.GetFullPath(exportDir);
            Console.WriteLine($"已从 {location} 导出 {exportedBackgroundFiles}/{attempted} 背景文件，从 {SelectedBeatmapCount} 谱面。");
        }
        public void DisplayCollections()
        {
            if (collections is not null)
            {
                Console.Write("osu! 集合:\n\n");
                foreach (var (name, (index, maps)) in collections)
                {
                    Console.WriteLine($"#{index}: {name} ({maps.Count} 谱面)");
                }
            }
            else
            {
                Console.WriteLine("无法加载 osu!lazer 集合数据库。集合信息和过滤不可用。");
            }
            Console.Write("\n这里显示的集合名称可用于\"collection\" 谱面过滤。\n");
        }

        private readonly Regex idCollection = new("#([0-9]+)", RegexOptions.Compiled);
        public void UpdateSelectedBeatmaps()
        {
            List<string> collFilters = new();
            List<BeatmapFilter> beatmapFilters = new();
            bool negateColl = false;
            foreach (var filter in config.Filters)
            {
                // 验证集合过滤请求
                if (filter.Collections is not null)
                {
                    if (collections is not null)
                    {
                        List<string> filteredCollections = new();
                        foreach (var requestedFilter in filter.Collections)
                        {
                            string? targetCollection = null;
                            var match = idCollection.Match(requestedFilter);
                            if (match.Success)
                            {
                                // 查找通过索引请求的任何集合过滤器
                                var collectionId = int.Parse(match.Groups[1].Value);
                                targetCollection = collections.FirstOrDefault(c => c.Value.CollectionID == collectionId).Key;
                            }
                            else
                            {
                                var exists = collections.ContainsKey(requestedFilter);
                                if (exists)
                                {
                                    targetCollection = requestedFilter;
                                }
                            }
                            if (targetCollection != null)
                            {
                                filteredCollections.Add(targetCollection);
                            }
                            else
                            {
                                Console.WriteLine($"无法找到集合：{requestedFilter}。");
                            }
                        }
                        collFilters.AddRange(filteredCollections);
                        negateColl = filter.Negate;
                    }
                    else
                    {
                        Console.WriteLine("无法过滤集合，集合在启动时不可用！");
                    }
                }
                else // 此过滤器不是集合过滤器
                    beatmapFilters.Add(filter);
            }

            // 重新构建 'collection' 过滤器以优化/缓存这些 beatmaps/collections 的迭代
            if (collections is not null && collFilters.Count > 0)
            {
                // 从选择的过滤器中构建包含的 beatmap ids 列表
                var includedHashes = collections
                    .Where(c => collFilters.Any(c => c == "-all") switch
                    {
                        true => true,
                        false => collFilters.Any(filter => string.Equals(filter, c.Key, StringComparison.OrdinalIgnoreCase))
                    })
                    .SelectMany(c => c.Value.Beatmaps.Select(b => b.ID))
                    .ToList();

                string desc = string.Join(", ", collFilters);
                BeatmapFilter collFilter = new($"集合过滤器: {(negateColl ? "不在 " : "")}{desc}", negateColl,
                    b => includedHashes.Contains(b.ID));

                // 删除占位符集合过滤器后，添加重新构建的过滤器
                beatmapFilters.Add(collFilter);
            }

            // 集合过滤器将在上述被重新构建或删除（如果集合不可用的话）
            config.Filters = beatmapFilters;

            // 根据当前过滤器计算和缓存 '已选择' 的 beatmaps
            int selectedCount = 0;
            List<BeatmapSet> selectedSets = new();
            foreach (var set in beatmapSets)
            {
                var filteredMaps =
                    from map in set.Beatmaps
                    where config.Filters.All(f => f.Includes(map))
                    select map;
                var selected = filteredMaps.ToList();

                set.SelectedBeatmaps = selected;
                selectedCount += selected.Count;

                // 在过滤谱面后，“已选择的谱面集”将只包含仍然至少有 1 个谱面的集合
                if (selected.Count > 0)
                {
                    selectedSets.Add(set);
                }
            }

            this.selectedBeatmapSets = selectedSets;
            this.selectedBeatmapCount = selectedCount;
        }
    }
}

