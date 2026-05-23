namespace HttpClientMethods.Helpers
{
    public interface IFileService
    {
        Task<bool> SaveFileAsync(byte[] data, string folderPath, string fileName, bool overwrite = false);

        Task<bool> SaveFileAsync(Stream data, string folderPath, string fileName, bool overwrite = false);

        string GetMimeTypeFromBytes(byte[] bytes);
    }
}
