using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace bizflow_desktop_app.Services;

public class FileSaveService : IFileSaveService
{
    private TopLevel? _topLevel;

    public void SetTopLevel(TopLevel topLevel) => _topLevel = topLevel;

    public async Task<bool> SaveFileAsync(string suggestedFileName, string defaultExtension, byte[] content, string fileTypeName = "PDF Document")
    {
        if (_topLevel is null)
            return false;

        var storage = _topLevel.StorageProvider;
        var fileType = new FilePickerFileType(fileTypeName) { Patterns = new[] { $"*.{defaultExtension}" } };
        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Receipt",
            DefaultExtension = defaultExtension,
            SuggestedFileName = suggestedFileName,
            FileTypeChoices = new[] { fileType }
        });

        if (file is null)
            return false;

        await using var stream = await file.OpenWriteAsync();
        await stream.WriteAsync(content);
        return true;
    }
}