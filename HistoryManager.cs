using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace PaperMasterWin;

public class HistoryManager
{
    private static readonly string HistoryDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PaperMaster");
    private static readonly string HistoryFile = Path.Combine(HistoryDir, "history.json");
    private const int MaxHistory = 50;

    public static List<PaperProject> GetProjects()
    {
        try
        {
            if (File.Exists(HistoryFile))
            {
                var json = File.ReadAllText(HistoryFile);
                return JsonConvert.DeserializeObject<List<PaperProject>>(json) ?? new List<PaperProject>();
            }
        }
        catch { }
        return new List<PaperProject>();
    }

    public static void SaveProject(PaperProject project)
    {
        if (project == null) return;
        project.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var list = GetProjects();
        int found = -1;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Id == project.Id)
            {
                found = i;
                break;
            }
        }

        if (found != -1)
        {
            list[found] = project;
        }
        else
        {
            list.Insert(0, project);
        }

        if (list.Count > MaxHistory)
        {
            list = list.GetRange(0, MaxHistory);
        }

        try
        {
            Directory.CreateDirectory(HistoryDir);
            var json = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(HistoryFile, json);
        }
        catch { }
    }

    public static void DeleteProject(string id)
    {
        var list = GetProjects();
        list.RemoveAll(p => p.Id == id);
        try
        {
            Directory.CreateDirectory(HistoryDir);
            var json = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(HistoryFile, json);
        }
        catch { }
    }
}
