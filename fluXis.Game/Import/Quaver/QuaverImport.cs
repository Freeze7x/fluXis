using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using fluXis.Game.Database;
using fluXis.Game.Import.FluXis;
using fluXis.Game.Import.Quaver.Map;
using fluXis.Game.Map;
using Newtonsoft.Json;
using osu.Framework.Logging;
using osu.Framework.Platform;
using YamlDotNet.Serialization;

namespace fluXis.Game.Import.Quaver;

public class QuaverImport : MapImporter
{
    public QuaverImport(FluXisRealm realm, MapStore mapStore, Storage storage)
        : base(realm, mapStore, storage)
    {
    }

    public override Task Import(string path)
    {
        return new Task(() =>
        {
            Logger.Log("Importing Quaver map: " + path);
            string fileName = Path.GetFileNameWithoutExtension(path);

            ZipArchive qp = ZipFile.OpenRead(path);

            List<MapInfo> mapInfos = new();

            foreach (var entry in qp.Entries)
            {
                if (entry.FullName.EndsWith(".qua"))
                {
                    QuaverMap quaverMap = parseQuaverMap(entry);
                    MapInfo map = quaverMap.ToMapInfo();
                    mapInfos.Add(map);

                    string json = JsonConvert.SerializeObject(map);
                    string destPath = Path.Combine(Storage.GetFullPath("import"), fileName, entry.FullName + ".fsc");
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.WriteAllText(destPath, json);
                }
                else
                {
                    string destPath = Path.Combine(Storage.GetFullPath("import"), fileName, entry.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    entry.ExtractToFile(destPath, true);
                }
            }

            qp.Dispose();

            ZipArchive fms = ZipFile.Open(Path.Combine(Storage.GetFullPath("import"), fileName + ".fms"), ZipArchiveMode.Create);

            // add all files from the import folder
            foreach (var file in Directory.GetFiles(Path.Combine(Storage.GetFullPath("import"), fileName)))
                fms.CreateEntryFromFile(file, Path.GetFileName(file));

            fms.Dispose();
            Directory.Delete(Path.Combine(Storage.GetFullPath("import"), fileName), true);
            new FluXisImport(Realm, MapStore, Storage).Import(Path.Combine(Storage.GetFullPath("import"), fileName + ".fms")).Start();
        });
    }

    private QuaverMap parseQuaverMap(ZipArchiveEntry entry)
    {
        string yaml = new StreamReader(entry.Open()).ReadToEnd();

        var builder = new DeserializerBuilder();
        builder.IgnoreUnmatchedProperties();
        var deserializer = builder.Build();

        QuaverMap map = deserializer.Deserialize<QuaverMap>(yaml);
        return map;
    }
}