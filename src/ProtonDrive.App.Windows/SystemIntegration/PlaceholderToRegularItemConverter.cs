using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using ProtonDrive.App.SystemIntegration;
using ProtonDrive.Shared.Extensions;
using ProtonDrive.Shared.IO;
using ProtonDrive.Sync.Windows.FileSystem;
using ProtonDrive.Sync.Windows.FileSystem.CloudFiles;
using static Vanara.PInvoke.CldApi;

namespace ProtonDrive.App.Windows.SystemIntegration;

internal sealed class PlaceholderToRegularItemConverter : IPlaceholderToRegularItemConverter
{
    private readonly ILogger<PlaceholderToRegularItemConverter> _logger;

    private readonly EnumerationOptions _enumerationOptions = new()
    {
        RecurseSubdirectories = false,
        AttributesToSkip = FileAttributes.None, // By default, Hidden and System attributes are skipped
    };

    public PlaceholderToRegularItemConverter(ILogger<PlaceholderToRegularItemConverter> logger)
    {
        _logger = logger;
    }

    public bool TryConvertToRegularFolder(string path, bool skipRoot)
    {
        if (!Directory.Exists(path))
        {
            return true;
        }

        try
        {
            TryConvertFolder(path, skipRoot, out _);

            return true;
        }
        catch (Exception ex) when (ex.IsFileAccessException() || ex is COMException)
        {
            _logger.LogError("Failed to convert to classic folder: {ExceptionType}: {ErrorCode}", ex.GetType().Name, ex.GetRelevantFormattedErrorCode());

            return false;
        }
    }

    public bool TryConvertToRegularFile(string path)
    {
        if (!File.Exists(path))
        {
            return true;
        }

        try
        {
            TryConvertFile(path, out _);

            return true;
        }
        catch (Exception ex) when (ex.IsFileAccessException() || ex is COMException)
        {
            _logger.LogError("Failed to convert to classic file: {ExceptionType}: {ErrorCode}", ex.GetType().Name, ex.GetRelevantFormattedErrorCode());

            return false;
        }
    }

    private static bool IsPlaceholder(FileSystemObject fileSystemObject)
    {
        var placeholderState = fileSystemObject.GetPlaceholderState();

        return placeholderState.HasFlag(PlaceholderState.Placeholder);
    }

    private static bool IsPartial(FileSystemObject fileSystemObject)
    {
        var placeholderState = fileSystemObject.GetPlaceholderState();

        return placeholderState.HasFlag(PlaceholderState.Partial);
    }

    private static void RemoveReadOnlyAttribute(FileSystemFile file)
    {
        if (file.Attributes.HasFlag(FileAttributes.ReadOnly))
        {
            file.Attributes &= ~FileAttributes.ReadOnly;
        }
    }

    private void TryExcludeFromSync(FileSystemObject fileSystemObject)
    {
        try
        {
            fileSystemObject.SetPinState(CF_PIN_STATE.CF_PIN_STATE_EXCLUDED, CF_SET_PIN_FLAGS.CF_SET_PIN_FLAG_NONE);
        }
        catch (COMException ex) when (ex.HResultContainsWin32ErrorCode(Win32SystemErrorCode.ErrorInvalidFunction))
        {
            _logger.LogWarning("Failed to exclude from sync: {ExceptionType}: {ErrorCode}", ex.GetType().Name, ex.GetRelevantFormattedErrorCode());
        }
    }

    private void TryConvertFile(string filePath, out bool deleted)
    {
        using var file = FileSystemFile.Open(filePath, FileSystemFileAccess.WriteAttributes | FileSystemFileAccess.Delete);

        if (IsPartial(file))
        {
            RemoveReadOnlyAttribute(file);

            file.Delete();
            deleted = true;

            return;
        }

        deleted = false;

        if (IsPlaceholder(file))
        {
            file.RevertPlaceholder();
        }

        TryExcludeFromSync(file);
    }

    private void TryConvertFolder(string folderPath, bool skipRoot, out bool deleted)
    {
        using var folder = FileSystemDirectory.Open(folderPath, FileSystemFileAccess.Read | FileSystemFileAccess.Delete);

        var entries = folder.EnumerateFileSystemEntries(options: _enumerationOptions);

        var folderIsEmpty = true;

        foreach (var entry in entries)
        {
            var entryFullPath = Path.Combine(folder.FullPath, entry.Name);

            bool entryDeleted;

            if (entry.Attributes.HasFlag(FileAttributes.Directory))
            {
                TryConvertFolder(entryFullPath, skipRoot: false, out entryDeleted);
            }
            else
            {
                TryConvertFile(entryFullPath, out entryDeleted);
            }

            folderIsEmpty &= entryDeleted;
        }

        deleted = false;

        if (skipRoot)
        {
            return;
        }

        if (folderIsEmpty)
        {
            folder.Delete(recursive: false);
            deleted = true;

            return;
        }

        if (IsPlaceholder(folder))
        {
            folder.RevertPlaceholder();
        }

        TryExcludeFromSync(folder);
    }
}
