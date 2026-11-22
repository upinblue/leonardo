using System.Collections.ObjectModel;
using UpInBlue.Leonardo.WinUI.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UpInBlue.Leonardo.Gallery.Models;

/// <summary>
/// A row of the file list: a file, or a subfolder that can be opened. Invented sample content.
/// </summary>
public sealed record FileItem(string Name, string Kind, string Glyph, DateTimeOffset Modified, string ModifiedBy, long Size, string Location)
{
    /// <summary>The glyph of a folder.</summary>
    public const string FolderGlyph = "";

    /// <summary>The subfolder this row stands for, or null for a file.</summary>
    public FolderNode? Folder { get; init; }

    /// <summary>The status of the file, or null. Shown as a status dot left of the icon.</summary>
    public StatusKind? Status { get; init; }

    /// <summary>What the status means for this file, for example "Checked out by Anna Berg".</summary>
    public string StatusDescription { get; init; } = string.Empty;

    /// <summary>Whether the file has a status.</summary>
    public bool HasStatus => Status is not null;

    /// <summary>The status for the dot; only read when <see cref="HasStatus"/> is true.</summary>
    public StatusKind StatusValue => Status ?? StatusKind.New;

    /// <summary>Whether the row is a folder that can be opened.</summary>
    public bool IsFolder => Folder is not null;

    /// <summary>The modification date for the list, or a dash if nothing was modified yet.</summary>
    public string ModifiedText => Modified == default ? Formats.Size(null) : Formats.Date(Modified);

    /// <summary>The modification date and time for the details pane.</summary>
    public string ModifiedLongText => Modified == default ? Formats.Size(null) : Formats.DateTime(Modified);

    /// <summary>The size for display; a dash for folders.</summary>
    public string SizeText => Formats.Size(IsFolder ? null : Size);

    /// <summary>The text a screen reader reads for the row.</summary>
    public string AccessibleName => IsFolder
        ? $"{Name}, folder, {Kind}"
        : $"{Name}, {Kind}{(HasStatus ? $", {StatusDescription}" : string.Empty)}, modified {ModifiedText} by {ModifiedBy}, {SizeText}";
}

/// <summary>
/// A folder in the folder tree.
/// </summary>
public sealed partial class FolderNode : ObservableObject
{
    /// <summary>
    /// Creates a folder.
    /// </summary>
    public FolderNode(string name, string path, IEnumerable<FolderNode>? children = null, IEnumerable<FileItem>? files = null)
    {
        Name = name;
        Path = path;
        Children = new ObservableCollection<FolderNode>(children ?? []);
        Files = new ObservableCollection<FileItem>(files ?? []);
        foreach (var child in Children)
        {
            child.Parent = this;
        }
    }

    /// <summary>The parent folder, or null for a top-level folder.</summary>
    public FolderNode? Parent { get; private set; }

    /// <summary>This folder and its parents, from the top-level folder down.</summary>
    public IReadOnlyList<FolderNode> Trail
    {
        get
        {
            var trail = new List<FolderNode>();
            for (var folder = this; folder is not null; folder = folder.Parent)
            {
                trail.Insert(0, folder);
            }

            return trail;
        }
    }

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <summary>The folder name.</summary>
    public string Name { get; }

    /// <summary>The path of the parent folders, for example "Customers / Hofmann Logistics".</summary>
    public string Path { get; }

    /// <summary>The subfolders.</summary>
    public ObservableCollection<FolderNode> Children { get; }

    /// <summary>The files directly in this folder.</summary>
    public ObservableCollection<FileItem> Files { get; }

    /// <summary>
    /// The path of this folder from the top-level folder, for example "Hofmann Logistics/Contracts".
    /// Used as the navigation parameter of the Files page.
    /// </summary>
    public string Key => string.Join('/', Trail.Select(folder => folder.Name));

    /// <summary>
    /// The row that stands for this folder in the file list of its parent. Its date is the newest change inside.
    /// </summary>
    public FileItem AsEntry()
    {
        var newest = AllFiles().MaxBy(file => file.Modified);
        var count = Children.Count + Files.Count;
        var kind = count == 1 ? "1 item" : $"{count} items";
        return new FileItem(Name, kind, FileItem.FolderGlyph, newest?.Modified ?? default, newest?.ModifiedBy ?? string.Empty, 0, Path)
        {
            Folder = this,
        };
    }

    /// <summary>The files in this folder and all its subfolders.</summary>
    public IEnumerable<FileItem> AllFiles() => Files.Concat(Children.SelectMany(child => child.AllFiles()));

    /// <summary>Whether the folder is expanded in the tree.</summary>
    [ObservableProperty]
    public partial bool IsExpanded { get; set; }
}
