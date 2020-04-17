namespace FLUT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using MadMilkman.Ini;

    using NUnit.Framework;

    public class Tests
    {
        [Test]
        public void CheckRooms()
        {
            if (Errors.RoomErrors.Count is 0)
                Assert.Pass("No room errors found");

            Assert.Fail(string.Join('\n', Errors.RoomErrors));
        }

        [Test]
        public void CheckEquipment()
        {
            if (Errors.EquipmentErrors.Count is 0)
                Assert.Pass("No equipment errors found");

            Assert.Fail(string.Join('\n', Errors.EquipmentErrors));
        }

        [Test]
        public void CheckShipArch()
        {
            if (Errors.ShipArchErrors.Count is 0)
                Assert.Pass("No ShipArch errors found");

            Assert.Fail(string.Join('\n', Errors.ShipArchErrors));
        }

        [Test]
        public void CheckZones()
        {
            if (Errors.ZoneErrors.Count is 0)
                Assert.Pass("No Zone errors found");

            Assert.Fail(string.Join('\n', Errors.ZoneErrors));
        }

        private List<string> systems = new List<string>();

        [Test, Order(1)]
        public void CheckUniverse()
        {
            IniFile ini = new IniFile(
                new IniOptions { KeyDuplicate = IniDuplication.Allowed, SectionDuplicate = IniDuplication.Allowed });
            string path = Environment.GetEnvironmentVariable("FOLDER");
            ini.Load(path + "/EXE/Freelancer.ini");
            ini.Load(
                path + "/DATA/" + ini.Sections.First(x => x.Name == "Data").Keys.First(x => x.Name == "universe")
                    .Value);

            foreach (var section in ini.Sections)
            {
                if (section.Name.ToLower() != "system")
                    continue;

                this.systems.Add(section.Keys.First(x => x.Name.ToLower() == "file").Value);
            }

            Assert.True(Errors.SystemErrors.Count is 0, string.Join('\n', Errors.UniverseErrors));
        }

        [Test, Order(2)]
        public void CheckSystems()
        {
            foreach (var system in this.systems)
            {
                IniFile ini = new IniFile(
                    new IniOptions
                        {
                            KeyDuplicate = IniDuplication.Allowed, SectionDuplicate = IniDuplication.Allowed
                        });
                string path = Environment.GetEnvironmentVariable("FOLDER");
                ini.Load(path + "/DATA/UNIVERSE/" + system);

                List<string> nicknames = new List<string>();
                Dictionary<string, List<string>> paths = new Dictionary<string, List<string>>();
                var reg = new Regex(@".*?_path_(\D+)(\d+)_(\d+)");

                foreach (var section in ini.Sections)
                {

                    if (section.Name.ToLower() == "zone")
                    {
                        // Zone_Rh04_path_hessians1_5
                        var zone = section.Keys.FirstOrDefault(x => x.Name.ToLower() == "nickname")?.Value;
                        Match match = reg.Match(zone);
                        if (match.Success)
                        {
                            if (paths.ContainsKey(match.Groups[1].Value))
                                paths[match.Groups[1].Value].Add(zone);
                            else
                                paths[match.Groups[1].Value] = new List<string>() { zone };
                        }

                        continue;
                    }

                    var nick = section.Keys.FirstOrDefault(x => x.Name.ToLower() == "nickname")?.Value;
                    if (nick is null)
                        continue;

                    if (nicknames.Contains(nick))
                        Errors.AddError(
                            $"[Warning] nickname {nick} already exists in file: {system}",
                            Errors.SystemErrors);
                    else
                        nicknames.Add(nick);
                }

                foreach (var pPath in paths)
                {
                    int i = 0, ii = 1;
                    foreach (var p in pPath.Value)
                    {
                        Match match = reg.Match(p);
                        int one = Convert.ToInt32(match.Groups[2].Value);
                        int two = Convert.ToInt32(match.Groups[3].Value);

                        if (two is 1)
                        {
                            ii = 1;
                            i++;
                        }

                        if (one != i || two != ii)
                            Errors.AddError(
                                $"[Error] Path: {p} is in the wrong order or is missing a number in file: {system}.",
                                Errors.SystemErrors);

                        ii++;
                    }

                    i++;
                }

                foreach (var nick in nicknames)
                {
                    if (nicknames.Count(x => x == "nick") > 0)
                        Errors.AddError($"[Error] Duplicate nickname ({nick}) in file: {system}", Errors.SystemErrors);
                }
            }

            Assert.True(Errors.SystemErrors.Count is 0, string.Join('\n', Errors.SystemErrors));
        }
    }
}