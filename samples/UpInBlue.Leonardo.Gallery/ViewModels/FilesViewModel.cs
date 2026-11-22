using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UpInBlue.Leonardo.Gallery.Models;

namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// The Files scenario for one open folder: the folder tree, the subfolders and files of the folder, and the details
/// of the selected row. Every folder is its own navigation step, so Back returns to the folder before.
/// </summary>
public sealed partial class FilesViewModel : ObservableObject
{
    private static readonly Lazy<IReadOnlyList<FolderNode>> Library = new(SampleData.Folders);
    private DeletedFile? _lastDeleted;

    /// <summary>
    /// Opens a folder.
    /// </summary>
    /// <param name="key">The <see cref="FolderNode.Key"/> of the folder; null or unknown opens Hofmann Logistics / Contracts.</param>
    public FilesViewModel(string? key = null)
    {
        Folder = Find(key) ?? Folders[0].Children[0];
        for (var parent = Folder.Parent; parent is not null; parent = parent.Parent)
        {
            parent.IsExpanded = true;
        }

        Entries = new ObservableCollection<FileItem>(Folder.Children.Select(child => child.AsEntry()).Concat(Folder.Files));
    }

    /// <summary>The top-level folders. They are shared by all pages, so changes stay while navigating.</summary>
    public IReadOnlyList<FolderNode> Folders => Library.Value;

    /// <summary>The open folder.</summary>
    public FolderNode Folder { get; }

    /// <summary>The subfolders and files of the open folder: folders first, as in File Explorer.</summary>
    public ObservableCollection<FileItem> Entries { get; }

    /// <summary>The selected row, or null.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection), nameof(IsFileSelected), nameof(IsFolderSelected))]
    public partial FileItem? SelectedEntry { get; set; }

    /// <summary>Whether a row is selected. x:Bind function bindings skip null arguments, so views bind to this instead.</summary>
    public bool HasSelection => SelectedEntry is not null;

    /// <summary>Whether a file is selected.</summary>
    public bool IsFileSelected => SelectedEntry is { IsFolder: false };

    /// <summary>Whether a folder is selected.</summary>
    public bool IsFolderSelected => SelectedEntry is { IsFolder: true };

    /// <summary>What just happened, for example "Deleted Price list 2027.xlsx."; empty if nothing.</summary>
    [ObservableProperty]
    public partial string Feedback { get; set; } = string.Empty;

    /// <summary>Whether the last deletion can be undone.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UndoCommand))]
    public partial bool CanUndo { get; set; }

    /// <summary>Whether the open folder contains nothing.</summary>
    public bool IsFolderEmpty => Entries.Count == 0;

    /// <summary>
    /// Finds a folder by its key.
    /// </summary>
    public static FolderNode? Find(string? key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        FolderNode? folder = null;
        IEnumerable<FolderNode> level = Library.Value;
        foreach (var name in key.Split('/'))
        {
            folder = level.FirstOrDefault(candidate => candidate.Name == name);
            if (folder is null)
            {
                return null;
            }

            level = folder.Children;
        }

        return folder;
    }

    /// <summary>
    /// Whether a row can be deleted now. Explains in <see cref="Feedback"/> why not, instead of a disabled command.
    /// </summary>
    public bool CanDelete(FileItem? entry)
    {
        switch (entry)
        {
            case null:
                Feedback = "Select a file first, then delete it.";
                return false;
            case { IsFolder: true }:
                Feedback = "Deleting folders isn't part of this sample.";
                return false;
            default:
                return Entries.Contains(entry);
        }
    }

    /// <summary>
    /// Deletes a file. The deletion can be undone.
    /// </summary>
    public void DeleteFile(FileItem file)
    {
        var index = Entries.IndexOf(file);
        if (index < 0 || file.IsFolder)
        {
            return;
        }

        var folderIndex = Folder.Files.IndexOf(file);
        Entries.RemoveAt(index);
        Folder.Files.Remove(file);
        _lastDeleted = new DeletedFile(file, index, folderIndex);
        CanUndo = true;
        SelectedEntry = null;
        Feedback = $"Deleted {file.Name}.";
        OnPropertyChanged(nameof(IsFolderEmpty));
    }

    /// <summary>
    /// Restores the file deleted last.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        if (_lastDeleted is not { } deleted)
        {
            return;
        }

        Entries.Insert(Math.Min(deleted.Index, Entries.Count), deleted.File);
        Folder.Files.Insert(Math.Clamp(deleted.FolderIndex, 0, Folder.Files.Count), deleted.File);
        _lastDeleted = null;
        CanUndo = false;
        SelectedEntry = deleted.File;
        Feedback = $"Restored {deleted.File.Name}.";
        OnPropertyChanged(nameof(IsFolderEmpty));
    }

    /// <summary>
    /// Explains that a command is not part of the sample, so that every command gives feedback.
    /// </summary>
    [RelayCommand]
    private void SampleOnly(string command) => Feedback = $"{command} isn't part of this sample.";

    private sealed record DeletedFile(FileItem File, int Index, int FolderIndex);
}
