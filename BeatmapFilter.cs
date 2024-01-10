﻿/*
using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;

namespace BeatmapExporter
{
    public class BeatmapFilter
    {
        public delegate bool LazerFilter(Beatmap beatmap);

        readonly bool negated;
        readonly LazerFilter lazerFilter;

        public string Description { get; }
        public string[]? Collections { get; }
        public bool Negate
        {
            get => negated;
        }

        public BeatmapFilter(string description, bool negated, LazerFilter lazerFilter)
        {
            this.Description = description;
            this.negated = negated;
            this.lazerFilter = lazerFilter;
            this.Collections = null;
        }
        
        // creates a placeholder filter for collections that will be re-built if collections are available 
        public BeatmapFilter(string description, bool negated, string[] collections) : this(description, negated, b => true)
        {
            this.Collections = collections;
        }


        public bool Includes(Beatmap beatmap) => negated switch
        {
            true => !lazerFilter(beatmap),
            false => lazerFilter(beatmap)
        };
    }

    public class FilterParser
    {
        readonly bool negate;
        readonly string input;
        readonly string[] args;

        public FilterParser(string input)
        {
            bool negate = false;
            // parse filter negation 
            if(input[0] == '!')
            {
                negate = true;
                input = input.Substring(1);
            }

            this.negate = negate;
            this.input = input;
            this.args = input.Split(" ");
        }

        public BeatmapFilter? Parse()
        {
            // input is already lowercased
            if(args.Length < 2)
            {
                Console.WriteLine("Please include both the filter name and filter conditions.");
                return null;
            }
            return args[0] switch
            {
                "stars" => StarsFilter(),
                "length" => LengthFilter(),
                "author" => AuthorFilter(),
                "id" => IDFilter(),
                "bpm" => BPMFilter(),
                "since" => AddedSinceFilter(),
                "artist" => ArtistFilter(),
                "tag" => TagFilter(),
                "mode" => GamemodeFilter(),
                "status" => OnlineStatusFilter(),
                "collection" => CollectionFilter(),
                _ => null
            };
        }

        // get the "remainder" of the input past the command arg and parse as comma-separated arguments
        string[] CommaSeparatedArg(int commandLength) => input.Substring(commandLength + 1 /* include space ).Split(",").Select(s => s.Trim()).ToArray();*/

/*        BeatmapFilter? StarsFilter()
        {
            // stars 6.3
            float starRating;
            if (!float.TryParse(args[1], out starRating))
            {
                Console.WriteLine($"Invalid star rating: {args[1]}");
                return null;
            }
            return new(input, negate,
                b => b.StarRating >= starRating);
        }

        BeatmapFilter? LengthFilter()
        {
            // length 90
            int duration;
            if (!Int32.TryParse(args[1], out duration))
            {
                Console.WriteLine($"Invalid map duration: {args[1]}");
                return null;
            }
            int millis = duration * 1000;
            return new(input, negate,
                b => b.Length >= millis);
        }

        BeatmapFilter? AuthorFilter()
        {
            // author RLC, Nathan
            string[] authors = CommaSeparatedArg(6);
            return new(input, negate,
                b => authors.Contains(b.Metadata.Author.Username.ToLower()));
        }

        BeatmapFilter? IDFilter()
        {
            // id 1, 2, 3
            var beatmapIds = CommaSeparatedArg(2).Select(a => Int32.Parse(a));
            return new(input, negate,
                b =>
                {
                    var beatmapSet = b.BeatmapSet;
                    return beatmapSet is not null && beatmapIds.Contains(beatmapSet.OnlineID);
                });
        }

        BeatmapFilter? BPMFilter()
        {
            // bpm 180
            int bpm;
            if (!Int32.TryParse(args[1], out bpm))
            {
                Console.WriteLine($"Invalid BPM: {args[1]}");
                return null;
            }
            return new(input, negate,
                b => b.BPM >= bpm);
        }

        BeatmapFilter? AddedSinceFilter()
        {
            // since 2:00
            TimeSpan since;
            if(!TimeSpan.TryParse(args[1], out since))
            {
                Console.WriteLine($"Invalid time interval: {args[1]}");
                return null;
            }
            return new(input, negate, b =>
            {
                if (b.BeatmapSet is null) return false; // if map doesn't have a set at all, we can't get the date added
                var lifetime = DateTime.Now - b.BeatmapSet.DateAdded;
                return lifetime < since;
            });
        }

        BeatmapFilter? ArtistFilter()
        {
            // artist Camellia, Nanahira
            string[] artists = CommaSeparatedArg(6);
            return new(input, negate,
                b => artists.Contains(b.Metadata.Artist.ToLower()));
        }

        BeatmapFilter? TagFilter()
        {
            // tag touhou
            string[] tags = CommaSeparatedArg(3);
            return new(input, negate,
                b =>
                {
                    string? beatmapTags = b.Metadata.Tags?.ToLower();
                    return beatmapTags is not null && tags.Any(t => beatmapTags.Contains(t));
                });
        }

        BeatmapFilter? GamemodeFilter()
        {
            // mode osu/mania/ctb/taiko
            int? gamemodeId = args[1] switch
            {
                "osu" => 0,
                "taiko" => 1,
                "ctb" => 2,
                "mania" => 3,
                _ => null
            };
            if(gamemodeId is null)
            {
                Console.WriteLine($"Unknown osu! game mode: {args[1]}. Use osu, mania, ctb, or taiko.");
                return null;
            }
            return new(input, negate,
                b => b.Ruleset.OnlineID == gamemodeId);
        }

        BeatmapFilter? OnlineStatusFilter()
        {
            // status graveyard/leaderboard/ranked/approved/qualified/loved
            int[]? statusId = args[1] switch
            {
                var s when s.StartsWith("graveyard") || s == "unknown" => new[] { -3 },
                var s when s.StartsWith("leaderboard") => new[] { 1, 2, 3, 4 },
                var s when s.StartsWith("rank") => new[] { 1 },
                var s when s.StartsWith("approve") => new[] { 2 },
                var s when s.StartsWith("qualif") => new[] { 3 },
                var s when s.StartsWith("love") => new[] { 4 },
                _ => null
            };
            if(statusId is null)
            {
                Console.WriteLine($"Unknown beatmap status: {args[1]}. Use graveyard, leaderboard, ranked, approved, qualified, or loved.");
                return null;
            }
            return new(input, negate,
                b => statusId.Contains(b.Status));
        }

        BeatmapFilter? CollectionFilter()
        {
            // collection name
            string[] collections = CommaSeparatedArg(10);
            // builds a placeholder filter that will be re-built if/where the user's collections are available
            return new(input, negate, collections);
        }
    }
}
*/



//csharp
using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using System;
using System.Linq;

namespace BeatmapExporter
{
    public class BeatmapFilter
    {
        public delegate bool LazerFilter(Beatmap beatmap);

        readonly bool negated;
        readonly LazerFilter lazerFilter;

        public string Description { get; }
        public string[]? Collections { get; }
        public bool Negate => negated;

        public BeatmapFilter(string description, bool negated, LazerFilter lazerFilter)
        {
            this.Description = description;
            this.negated = negated;
            this.lazerFilter = lazerFilter;
            this.Collections = null;
        }

        // creates a placeholder filter for collections that will be re-built if collections are available 
        public BeatmapFilter(string description, bool negated, string[] collections) : this(description, negated, b => true)
        {
            this.Collections = collections;
        }

        public bool Includes(Beatmap beatmap) => negated switch
        {
            true => !lazerFilter(beatmap),
            false => lazerFilter(beatmap)
        };
    }

    public class FilterParser
    {
        readonly bool negate;
        readonly string input;
        readonly string[] args;

        public FilterParser(string input)
        {
            bool negate = false;
            // parse filter negation 
            if (input[0] == '!')
            {
                negate = true;
                input = input.Substring(1);
            }

            this.negate = negate;
            this.input = input;
            this.args = input.Split(" ");
        }

        public BeatmapFilter? Parse()
        {
            // input is already lowercased
            if (args.Length < 2)
            {
                Console.WriteLine("请包括筛选器名称和筛选条件。");
                return null;
            }
            return args[0] switch
            {
                "stars" => StarsFilter(),
                "length" => LengthFilter(),
                "author" => AuthorFilter(),
                "id" => IDFilter(),
                "bpm" => BPMFilter(),
                "since" => AddedSinceFilter(),
               // "artist" => ArtistFilter(),
                "tag" => TagFilter(),
                "mode" => GamemodeFilter(),
                "status" => OnlineStatusFilter(),
                "collection" => CollectionFilter(),
                _ => null
            };
        }

        // get the "remainder" of the input past the command arg and parse as comma-separated arguments
       // string[] CommaSeparatedArg(int commandLength) => input.Substring(commandLength + 1 /* include space */).Split(",").Select(s => s.Trim()).ToArray();
        string[] CommaSeparatedArg(int commandLength) => input.Substring(commandLength).Split(",").Select(s => s.Trim()).ToArray();

        BeatmapFilter? StarsFilter()
        {
            // stars 6.3
            float starRating;
            if (!float.TryParse(args[1], out starRating))
            {
                Console.WriteLine($"无效的星级: {args[1]}");
                return null;
            }
            return new(input, negate,
                b => b.StarRating >= starRating);
        }

        BeatmapFilter? LengthFilter()
        {
            // length 90
            int duration;
            if (!Int32.TryParse(args[1], out duration))
            {
                Console.WriteLine($"无效的谱面时长: {args[1]}");
                return null;
            }
            int millis = duration * 1000;
            return new(input, negate,
                b => b.Length >= millis);
        }

        BeatmapFilter? AuthorFilter()
        {
            // author RLC, Nathan
            string[] authors = CommaSeparatedArg(6);
            return new(input, negate,
                b => authors.Contains(b.Metadata.Author.Username.ToLower()));
        }

        BeatmapFilter? IDFilter()
        {
            // id 1, 2, 3
            var beatmapIds = CommaSeparatedArg(2).Select(a => Int32.Parse(a));
            return new(input, negate,
                b =>
                {
                    var beatmapSet = b.BeatmapSet;
                    return beatmapSet is not null && beatmapIds.Contains(beatmapSet.OnlineID);
                });
        }

        BeatmapFilter? BPMFilter()
        {
            // bpm 180
            int bpm;
            if (!Int32.TryParse(args[1], out bpm))
            {
                Console.WriteLine($"无效的BPM: {args[1]}");
                return null;
            }
            return new(input, negate,
                b => b.BPM >= bpm);
        }

        BeatmapFilter? AddedSinceFilter()
        {
            // since 2:00
            TimeSpan since;
            if (!TimeSpan.TryParse(args[1], out since))
            {
                Console.WriteLine($"无效的时间间隔: {args[1]}");
                return null;
            }
            return new(input, negate, b =>
            {
                if (b.BeatmapSet is null) return false; // if map doesn't have a set at all, we can't get the date added
                var lifetime = DateTime.Now - b.BeatmapSet.DateAdded;
                return lifetime < since;
            });
        }

        /*BeatmapFilter? ArtistFilter()
        {
            // artist Camellia, Nanahira
            string[] artists = Comma;
            //string[] artists = CommaSeparatedArg(6);

            SeparatedArg(6);
            return new(input, negate,
                b => artists.Contains(b.Metadata.Artist.ToLower()));
        }*/

        BeatmapFilter? TagFilter()
        {
            // tag touhou
            string[] tags = CommaSeparatedArg(3);
            return new(input, negate,
                b =>
                {
                    string? beatmapTags = b.Metadata.Tags?.ToLower();
                    return beatmapTags is not null && tags.Any(t => beatmapTags.Contains(t));
                });
        }

        BeatmapFilter? GamemodeFilter()
        {
            // mode osu/mania/ctb/taiko
            int? gamemodeId = args[1] switch
            {
                "osu" => 0,
                "taiko" => 1,
                "ctb" => 2,
                "mania" => 3,
                _ => null
            };
            if (gamemodeId is null)
            {
                Console.WriteLine($"未知的osu!游戏模式: {args[1]}。请使用 osu、mania、ctb 或 taiko。");
                return null;
            }
            return new(input, negate,
                b => b.Ruleset.OnlineID == gamemodeId);
        }

        BeatmapFilter? OnlineStatusFilter()
        {
            // status graveyard/leaderboard/ranked/approved/qualified/loved
            int[]? statusId = args[1] switch
            {
                var s when s.StartsWith("graveyard") || s == "unknown" => new[] { -3 },
                var s when s.StartsWith("leaderboard") => new[] { 1, 2, 3, 4 },
                var s when s.StartsWith("rank") => new[] { 1 },
                var s when s.StartsWith("approve") => new[] { 2 },
                var s when s.StartsWith("qualif") => new[] { 3 },
                var s when s.StartsWith("love") => new[] { 4 },
                _ => null
            };
            if (statusId is null)
            {
                Console.WriteLine($"未知的谱面状态: {args[1]}。请使用 graveyard、leaderboard、ranked、approved、qualified 或 loved。");
                return null;
            }
            return new(input, negate,
                b => statusId.Contains(b.Status));
        }

        BeatmapFilter? CollectionFilter()
        {
            // collection name
            string[] collections = CommaSeparatedArg(10);
            // builds a placeholder filter that will be re-built if/where the user's collections are available
            return new(input, negate, collections);
        }
    }
}
