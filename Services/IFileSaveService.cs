namespace bizflow_desktop_app.Services;

public interface IFileSaveService
{
    void SetTopLevel(Avalonia.Controls.TopLevel topLevel);
    Task<bool> SaveFileAsync(string suggestedFileName, string defaultExtension, byte[] content, string fileTypeName = "PDF Document");
}