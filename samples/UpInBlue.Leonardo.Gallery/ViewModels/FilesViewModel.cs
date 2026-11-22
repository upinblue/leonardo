using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UpInBlue.Leonardo.Gallery.Models;

namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// The Files scenario: a folder tree, the files of the selected folder and the details of the selected file.
/// </summary>
public sealed partial class FilesViewModel : ObservableObject
{
    private DeletedFile? _lastDeleted;

    /// <summary>
    /// Creates the view model with the sample folders and opens Hofmann Logistics / Contracts.
    /// </summary>
    public FilesViewModel()
    {
        Folders = SampleData.Folders();
        SelectedFolder = Folders[0].Children[0];
    }

    /// <summary>The top-level folders.</summary>
    public IReadOnlyList<FolderNode> Folders { get; }

    /// <summary>The open folder.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFolderEmpty))]
    public partial FolderNode SelectedFolder { get; set; }

    /// <summary>The selected file, or null.</summary>
    [ObservableProperty]
    public partial FileItem? SelectedFile { get; set; }

    /// <summary>What just happened, for example "Deleted Price list 2027.xlsx."; empty if nothing.</summary>
    [ObservableProperty]
    public partial string Feedback { get; set; } = string.Empty;

    /// <summary>Whether the last deletion can be undone.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UndoCommand))]
    public partial bool CanUndo { get; set; }

    /// <summary>Whether the open folder contains no files.</summary>
    public bool IsFolderEmpty => SelectedFolder.Files.Count == 0;

    /// <summary>
    /// Deletes the selected file. The deletion can be undone. The command stays available and explains
    /// what to do when no file is selected, instead of being disabled without a reason.
    /// </summary>
    [RelayCommand]
    private void Delete()
    {
        if (SelectedFile is null)
        {
            Feedback = "Select a file first, then delete it.";
            return;
        }

        DeleteFile(SelectedFile);
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

        deleted.Folder.Files.Insert(Math.Min(deleted.Index, deleted.Folder.Files.Count), deleted.File);
        _lastDeleted = null;
        CanUndo = false;
        SelectedFolder = deleted.Folder;
        SelectedFile = deleted.File;
        Feedback = $"Restored {deleted.File.Name}.";
        OnPropertyChanged(nameof(IsFolderEmpty));
    }

    /// <summary>
    /// Explains that a command is not part of the sample, so that every command gives feedback.
    /// </summary>
    [RelayCommand]
    private void SampleOnly(string command) => Feedback = $"{command} isn't part of this sample.";

    /// <summary>
    /// Deletes a file, for example from its context menu.
    /// </summary>
    public void DeleteFile(FileItem? file)
    {
        if (file is null)
        {
            return;
        }

        var index = SelectedFolder.Files.IndexOf(file);
        if (index < 0)
        {
            return;
        }

        SelectedFolder.Files.RemoveAt(index);
        _lastDeleted = new DeletedFile(file, SelectedFolder, index);
        CanUndo = true;
        SelectedFile = null;
        Feedback = $"Deleted {file.Name}.";
        OnPropertyChanged(nameof(IsFolderEmpty));
    }

    partial void OnSelectedFolderChanged(FolderNode value)
    {
        SelectedFile = null;
        if (!CanUndo)
        {
            Feedback = string.Empty;
        }
    }

    private sealed record DeletedFile(FileItem File, FolderNode Folder, int Index);
}
