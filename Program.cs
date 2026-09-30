using System.Diagnostics;
using System.Text.Json;
using Microsoft.Web.WebView2.WinForms;

namespace FolderBookmarks;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

class MainForm : Form
{
    static readonly string StorePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FolderBookmarks", "bookmarks.json");
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    readonly WebView2 _web = new() { Dock = DockStyle.Fill };

    public MainForm()
    {
        Text = "Folder Bookmarks";
        Width = 1100; Height = 700;
        StartPosition = FormStartPosition.CenterScreen;
        Controls.Add(_web);
        Load += async (_, _) => await InitAsync();
    }

    async Task InitAsync()
    {
        await _web.EnsureCoreWebView2Async();
        _web.CoreWebView2.WebMessageReceived += (_, e) => Handle(JsonDocument.Parse(e.WebMessageAsJson).RootElement);
        using var html = typeof(MainForm).Assembly.GetManifestResourceStream("index.html")!;
        _web.NavigateToString(new StreamReader(html).ReadToEnd());
    }

    // Messages from JS: {type:"load"} | {type:"save", bookmarks:[...]} | {type:"pick"} | {type:"open", path}
    void Handle(JsonElement msg)
    {
        switch (msg.GetProperty("type").GetString())
        {
            case "load":
                Send(new { type = "bookmarks", bookmarks = LoadBookmarks() });
                break;
            case "save":
                Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
                File.WriteAllText(StorePath, msg.GetProperty("bookmarks").GetRawText());
                break;
            case "pick":
                using (var dlg = new FolderBrowserDialog { Description = "Select project folder", UseDescriptionForTitle = true })
                    if (dlg.ShowDialog(this) == DialogResult.OK) Send(new { type = "picked", path = dlg.SelectedPath });
                break;
            case "open":
                var path = msg.GetProperty("path").GetString()!;
                if (Directory.Exists(path)) Process.Start("explorer.exe", $"\"{path}\"");
                else Send(new { type = "error", message = $"Folder not found:\n{path}" });
                break;
        }
    }

    void Send(object o) => _web.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(o, Json));

    // Pass the file through untouched so the page owns the schema.
    static JsonElement LoadBookmarks()
    {
        try { return JsonDocument.Parse(File.ReadAllText(StorePath)).RootElement.Clone(); }
        catch { return JsonDocument.Parse("[]").RootElement; }
    }
}
