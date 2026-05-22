namespace HttpClientMethods.Helpers
{
    public interface IFileService
    {
        Task<bool> SaveFileAsync(byte[] data, string folderPath, string fileName, bool overwrite = false);
    }
}
