namespace FLUT
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    using LibreLancer;
    using LibreLancer.Data;

    using NUnit.Framework;

    public static class Errors
    {
        public static void AddError(string line, List<string> list)
        {
            if (!list.Contains(line))
                list.Add(line);
        }

        public static List<string> BaseErrors = new List<string>();
        public static List<string> EquipmentErrors = new List<string>();
        public static List<string> FuseErrors = new List<string>();
        public static List<string> RoomErrors = new List<string>();
        public static List<string> ShipArchErrors = new List<string>();
        public static List<string> SystemErrors = new List<string>();
        public static List<string> UniverseErrors = new List<string>();
        public static List<string> ZoneErrors = new List<string>();
    }

    public class Program
    {
        public FileSystem VFS;

        public FreelancerData Data;

        class NoSyncThread : IUIThread
        {
            public void EnsureUIThread(Action work) => work();

            public void QueueUIThread(Action work) => work();
        }

        public static StringBuilder sb = new StringBuilder();
        public static void LogLine(string line, LogSeverity severity)
        {
            sb.AppendLine(line);
            line = line.Replace("\\", "/");
            // Ignore custom config sections
            if (new Regex("\\[Error\\].*? Invalid Section in .*?: Config").IsMatch(line) || new Regex("\\[Error\\].*? Invalid Section in .*?: Prefixes").IsMatch(line))
                return;

            // Ignore multi ammo weapon errors
            if (line.Contains("WEAPONS") && line.Contains("Duplicate of ids"))
                return;

            if (line.Contains("Room file not found"))
                Errors.AddError(line, Errors.RoomErrors);

            else if (line.Contains("universe/systems") && !line.Contains("rooms"))
                Errors.AddError(line, Errors.SystemErrors);

            else if (line.Contains("Exclusion Zone"))
                Errors.AddError(line, Errors.ZoneErrors);

            else if (line.Contains("fuse"))
                Errors.AddError(line, Errors.FuseErrors);

            else if (line.Contains("shiparch.ini"))
                Errors.AddError(line, Errors.ShipArchErrors);

            else
                Console.WriteLine();
        }

        [SetUp]
        public void Setup()
        {
            FLLog.UIThread = new NoSyncThread();
            FLLog.AppendLine = LogLine;

            Environment.SetEnvironmentVariable("FOLDER", Directory.GetCurrentDirectory());
            string path = Environment.GetEnvironmentVariable("FOLDER");
            this.VFS = FileSystem.FromFolder(path);
            FreelancerIni flIni = new FreelancerIni(this.VFS);
            this.Data = new FreelancerData(flIni, this.VFS);
            this.Data.LoadData();
        }

        [Test]
        public void DataLoaded() => Assert.True(this.Data.Loaded);
    }
}