# Folder Bookmarks

A small Windows desktop app that works like a bookmark bar for folders. Add a project folder as a card, put cards into groups, and click a card to open that folder in File Explorer.

![Windows](https://img.shields.io/badge/platform-Windows%2010%2B-blue) ![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4)

## What it does

- **Cards for folders.** Each card has a title and a folder path. Clicking the card opens the folder in Explorer.
- **Groups.** Give a card a group name such as `Healthcare` or `AI ML` and cards are shown in sections. Type a new name to create a group. Hover a group heading to rename it in place.
- **Drag and drop.** Drag cards to reorder them, or drop a card into another group to move it.
- **Edit and remove.** Hover a card for edit and remove buttons. Edit lets you fix a wrong folder or title.
- **Search.** Filters by title, path, or group.
- **Works offline.** No network access. The UI is plain HTML and CSS embedded in the exe.

Bookmarks are stored as JSON at:

```
%APPDATA%\FolderBookmarks\bookmarks.json
```

## Install

Download `FolderBookmarks.exe` from the [latest release](../../releases/latest) and run it. Nothing else to install: the exe is self-contained and includes the .NET runtime.

The UI renders through Microsoft Edge WebView2, which ships with Windows 10 and 11.

## Build from source

```bash
dotnet run
```

To produce the single-file exe used in releases:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish
```

## How it is built

- `Program.cs` is a WinForms window hosting a WebView2 control. It handles four messages from the page: load bookmarks, save bookmarks, pick a folder, open a folder.
- `index.html` is the whole UI, with no frameworks or dependencies.
