using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UpInBlue.Leonardo.Gallery.Models;

/// <summary>
/// A file in a folder. Invented sample content.
/// </summary>
public sealed record FileItem(string Name, string Kind, string Glyph, DateTimeOffset Modified, string ModifiedBy, long Size, string Location)
{
    /// <summary>The modification date for the list.</summary>
    public string ModifiedText => Formats.Date(Modified);

    /// <summary>The modification date and time for the details pane.</summary>
    public string ModifiedLongText => Formats.DateTime(Modified);

    /// <summary>The size for display.</summary>
    public string SizeText => Formats.Size(Size);

    /// <summary>The text a screen reader reads for the row.</summary>
    public string AccessibleName => $"{Name}, {Kind}, modified {ModifiedText} by {ModifiedBy}, {SizeText}";
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

    /// <summary>Whether the folder is expanded in the tree.</summary>
    [ObservableProperty]
    public partial bool IsExpanded { get; set; }
}
