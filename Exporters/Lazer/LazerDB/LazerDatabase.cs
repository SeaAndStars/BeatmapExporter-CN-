using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using Realms;
using Realms.Exceptions;

/*namespace BeatmapExporter.Exporters.Lazer.LazerDB
{
    public class LazerDatabase
    {
        const int LazerSchemaVersion = 40;
        readonly string database;
        readonly string filesDirectory;

        private LazerDatabase(string database)
        {
            this.database = database;
            filesDirectory = Path.Combine(Path.GetDirectoryName(database)!, "files");
        }

        static string? GetDatabaseFile(string directory)
        {
            string path = Path.Combine(directory, "client.realm");
            return File.Exists(path) ? path : null;
        }

        public static LazerDatabase? Locate(string directory)
        {
            string? dbFile = GetDatabaseFile(directory);
            if (dbFile is null)
            {
                Console.Write("osu! song database not found. Please find and provide your osu! data folder.\nThe folder should contain a \"client.realm\" file and can be opened from in-game.\n\nFolder path: ");
                string? input = Console.ReadLine();
                if (input is not null)
                {
                    dbFile = GetDatabaseFile(input);
                }
            }
            return dbFile is not null ? new LazerDatabase(dbFile) : null;
        }

        public Realm? Open()
        {
            RealmConfiguration config = new(database)
            {
                IsReadOnly = true,
                SchemaVersion = LazerSchemaVersion
            };
            config.Schema = new[] {
                typeof(Beatmap),
                typeof(BeatmapCollection),
                typeof(BeatmapDifficulty),
                typeof(BeatmapMetadata),
                typeof(BeatmapSet),
                typeof(BeatmapUserSettings),
                typeof(RealmFile),
                typeof(RealmNamedFileUsage),
                typeof(RealmUser),
                typeof(Ruleset),
                typeof(ModPreset)
            };

            try
            {
                return Realm.GetInstance(config);
            }
            catch (RealmException re)
            {
                Console.WriteLine($"\nError opening database: {re.Message}");
                if(re.Message.Contains("does not equal last set version"))
                {
                    Console.WriteLine("The osu!lazer database structure has updated since the last BeatmapExporter update.");
                    Console.WriteLine("\nYou can check https://github.com/kabiiQ/BeatmapExporter/releases for a new release, or file an issue there to let me know it needs updating if it's been a few days.");
                }
                return null;
            }
        }

        string HashedFilePath(string hash) => Path.Combine(filesDirectory, hash[..1], hash[..2], hash);

        public FileStream? OpenHashedFile(string hash)
        {
            try
            {
                string path = HashedFilePath(hash);
                return File.Open(path, FileMode.Open);
            }
            catch (IOException ioe)
            {
                Console.WriteLine($"Unable to open file: {hash} :: {ioe.Message}");
                return null;
            }
        }

        public FileStream? OpenNamedFile(BeatmapSet set, string filename)
        {
            // get named file from specific beatmap - check if it exists in this beatmap
            string? fileHash = set.Files.FirstOrDefault(f => f.Filename == filename)?.File?.Hash;
            if(fileHash is null)
            {
                Console.WriteLine($"File {filename} not found in beatmap {set.ArchiveFilename()}");
                return null;
            }
            try
            {
                string path = HashedFilePath(fileHash);
                return File.Open(path, FileMode.Open);
            }
            catch (IOException ioe)
            {
                Console.WriteLine($"Unable to open file: {filename} from beatmap {set.ArchiveFilename()} :: {ioe.Message}");
                return null;
            }
        }
    }
}*/

namespace BeatmapExporter.Exporters.Lazer.LazerDB
{
    // osu! 游戏数据库操作类
    public class LazerDatabase
    {
        // osu! Lazer 数据库的架构版本
        private const int LazerSchemaVersion = 40;

        // 数据库文件路径
        private readonly string database;

        // 文件目录路径
        private readonly string filesDirectory;

        // 构造函数，私有以确保只能通过 Locate 方法获取实例
        private LazerDatabase(string database)
        {
            this.database = database;
            filesDirectory = Path.Combine(Path.GetDirectoryName(database)!, "files");
        }

        // 获取数据库文件路径
        private static string? GetDatabaseFile(string directory)
        {
            string path = Path.Combine(directory, "client.realm");
            return File.Exists(path) ? path : null;
        }

        // 定位 osu! Lazer 数据库实例
        public static LazerDatabase? Locate(string directory)
        {
            string? dbFile = GetDatabaseFile(directory);

            // 如果数据库文件不存在，提示用户提供 osu! 数据文件夹路径
            if (dbFile is null)
            {
                Console.Write("osu! 歌曲数据库未找到。请查找并提供您的 osu! 数据文件夹。\n该文件夹应包含一个名为 \"client.realm\" 的文件，并可从游戏中打开。\n\n文件夹路径: ");
                string? input = Console.ReadLine();
                if (input is not null)
                {
                    dbFile = GetDatabaseFile(input);
                }
            }

            return dbFile is not null ? new LazerDatabase(dbFile) : null;
        }

        // 打开数据库连接
        public Realm? Open()
        {
            RealmConfiguration config = new(database)
            {
                IsReadOnly = true,
                SchemaVersion = LazerSchemaVersion
            };

            // 指定数据库的模型类
            config.Schema = new[] {
                typeof(Beatmap),
                typeof(BeatmapCollection),
                typeof(BeatmapDifficulty),
                typeof(BeatmapMetadata),
                typeof(BeatmapSet),
                typeof(BeatmapUserSettings),
                typeof(RealmFile),
                typeof(RealmNamedFileUsage),
                typeof(RealmUser),
                typeof(Ruleset),
                typeof(ModPreset)
            };

            try
            {
                // 获取数据库实例
                return Realm.GetInstance(config);
            }
            catch (RealmException re)
            {
                // 处理数据库打开异常
                Console.WriteLine($"\n打开数据库时出错: {re.Message}");

                if (re.Message.Contains("与上一个数据库版本不匹配"))
                {
                    Console.WriteLine("osu!lazer 数据库结构已更新，与上一次 BeatmapExporter 更新不一致。");
                    Console.WriteLine("\n您可以在 https://github.com/kabiiQ/BeatmapExporter/releases 上查看新版本，如果已经过去几天还未更新，请在该页面提出问题通知我需要更新。");
                }

                return null;
            }
        }

        // 生成哈希文件路径
        private string HashedFilePath(string hash) => Path.Combine(filesDirectory, hash[..1], hash[..2], hash);

        // 打开哈希文件
        public FileStream? OpenHashedFile(string hash)
        {
            try
            {
                string path = HashedFilePath(hash);
                return File.Open(path, FileMode.Open);
            }
            catch (IOException ioe)
            {
                // 处理文件打开异常
                Console.WriteLine($"无法打开文件: {hash} :: {ioe.Message}");
                return null;
            }
        }

        // 打开指定 BeatmapSet 的命名文件
        public FileStream? OpenNamedFile(BeatmapSet set, string filename)
        {
            // 从特定 beatmap 中获取命名文件，检查是否存在
            string? fileHash = set.Files.FirstOrDefault(f => f.Filename == filename)?.File?.Hash;
            if (fileHash is null)
            {
                Console.WriteLine($"在 beatmap {set.ArchiveFilename()} 中未找到文件 {filename}");
                return null;
            }

            try
            {
                string path = HashedFilePath(fileHash);
                return File.Open(path, FileMode.Open);
            }
            catch (IOException ioe)
            {
                // 处理文件打开异常
                Console.WriteLine($"无法打开文件: {filename}，来自 beatmap {set.ArchiveFilename()} :: {ioe.Message}");
                return null;
            }
        }
    }
}
