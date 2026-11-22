using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UpInBlue.Leonardo.Gallery.Models;
using UpInBlue.Leonardo.WinUI.Controls;

namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// The Files scenario for one open folder: the folder tree, the subfolders and files of the folder, a summary of what
/// changed since the last visit, and the details of the selected row. Every folder is its own navigation step, so Back
/// returns to the folder before.
/// </summary>
public sealed partial class FilesViewModel : ObservableObject
{
    private static readonly Lazy<IReadOnlyList<FolderNode>> Library = new(SampleData.Folders);
    private readonly List<FileItem> _all;
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

        _all = [.. Folder.Children.Select(child => child.AsEntry()), .. Folder.Files];
        Entries = new ObservableCollection<FileItem>(_all);
        Summary =
        [
            new SummaryLineItem { Kind = SummaryKind.New, Command = FilterCommand },
            new SummaryLineItem { Kind = SummaryKind.CheckedOut, Command = FilterCommand },
            new SummaryLineItem { Kind = SummaryKind.Conflict, Command = FilterCommand },
        ];
        UpdateSummary();
    }

    /// <summary>
    /// Raised when a filter of the summary line was turned on or off. The view calls <see cref="ApplyFilter"/>, so that it can animate the change.
    /// </summary>
    public event EventHandler? FilterRequested;

    /// <summary>The top-level folders. They are shared by all pages, so changes stay while navigating.</summary>
    public IReadOnlyList<FolderNode> Folders => Library.Value;

    /// <summary>The open folder.</summary>
    public FolderNode Folder { get; }

    /// <summary>The rows shown: subfolders first, as in File Explorer, then files; only files that match the active filters.</summary>
    public ObservableCollection<FileItem> Entries { get; }

    /// <summary>"Since your last visit: 2 new · 1 checked out · 1 conflict". Each phrase filters the list.</summary>
    public ObservableCollection<SummaryLineItem> Summary { get; }

    /// <summary>The selected row, or null.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection), nameof(IsFileSelected), nameof(IsFolderSelected), nameof(SelectedHasStatus))]
    public partial FileItem? SelectedEntry { get; set; }

    /// <summary>Whether a row is selected. x:Bind function bindings skip null arguments, so views bind to this instead.</summary>
    public bool HasSelection => SelectedEntry is not null;

    /// <summary>Whether a file is selected.</summary>
    public bool IsFileSelected => SelectedEntry is { IsFolder: false };

    /// <summary>Whether a folder is selected.</summary>
    public bool IsFolderSelected => SelectedEntry is { IsFolder: true };

    /// <summary>Whether the selected file has a status.</summary>
    public bool SelectedHasStatus => SelectedEntry is { HasStatus: true };

    /// <summary>What just happened, for example "Deleted Price list 2027.xlsx."; empty if nothing.</summary>
    [ObservableProperty]
    public partial string Feedback { get; set; } = string.Empty;

    /// <summary>Whether the last deletion can be undone.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UndoCommand))]
    public partial bool CanUndo { get; set; }

    /// <summary>Whether the open folder contains nothing.</summary>
    public bool IsFolderEmpty => _all.Count == 0;

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
    /// Shows the rows that match the active filters of the summary line, keeping their order. Without an active filter, all rows show.
    /// </summary>
    /// <param name="explain">Whether to say in <see cref="Feedback"/> that a filter is on, after the user changed one.</param>
    public void ApplyFilter(bool explain = false)
    {
        var active = Summary.Where(item => item.IsActive).Select(item => KindOf(item.Kind)).ToHashSet();
        var wanted = _all.Where(entry => active.Count == 0 || (entry.Status is { } status && active.Contains(status))).ToList();

        foreach (var entry in Entries.Except(wanted).ToList())
        {
            Entries.Remove(entry);
        }

        for (var index = 0; index < wanted.Count; index++)
        {
            if (index >= Entries.Count || !ReferenceEquals(Entries[index], wanted[index]))
            {
                Entries.Insert(index, wanted[index]);
            }
        }

        if (explain)
        {
            Feedback = active.Count == 0 ? string.Empty : "Showing only the files of the selected phrases. Turn them off to see everything.";
        }
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
        var index = _all.IndexOf(file);
        if (index < 0 || file.IsFolder)
        {
            return;
        }

        var folderIndex = Folder.Files.IndexOf(file);
        _all.RemoveAt(index);
        Entries.Remove(file);
        Folder.Files.Remove(file);
        _lastDeleted = new DeletedFile(file, index, folderIndex);
        CanUndo = true;
        SelectedEntry = null;
        Feedback = $"Deleted {file.Name}.";
        UpdateSummary();
        ApplyFilter();
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

        _all.Insert(Math.Min(deleted.Index, _all.Count), deleted.File);
        Folder.Files.Insert(Math.Clamp(deleted.FolderIndex, 0, Folder.Files.Count), deleted.File);
        _lastDeleted = null;
        CanUndo = false;
        UpdateSummary();
        ApplyFilter();
        SelectedEntry = Entries.Contains(deleted.File) ? deleted.File : null;
        Feedback = $"Restored {deleted.File.Name}.";
        OnPropertyChanged(nameof(IsFolderEmpty));
    }

    /// <summary>
    /// Explains that a command is not part of the sample, so that every command gives feedback.
    /// </summary>
    [RelayCommand]
    private void SampleOnly(string command) => Feedback = $"{command} isn't part of this sample.";

    [RelayCommand]
    private void Filter() => FilterRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Counts the files per status. A filter whose phrase disappears is turned off, so that rows never stay hidden by an invisible filter.
    /// </summary>
    private void UpdateSummary()
    {
        foreach (var item in Summary)
        {
            item.Count = _all.Count(entry => entry.Status == KindOf(item.Kind));
            if (item.Count == 0)
            {
                item.IsActive = false;
            }
        }
    }

    private static StatusKind KindOf(SummaryKind kind) => kind switch
    {
        SummaryKind.CheckedOut => StatusKind.CheckedOut,
        SummaryKind.Conflict => StatusKind.Conflict,
        _ => StatusKind.New,
    };

    private sealed record DeletedFile(FileItem File, int Index, int FolderIndex);
}
