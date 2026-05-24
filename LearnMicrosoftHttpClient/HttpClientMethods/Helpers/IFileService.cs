namespace HttpClientMethods.Helpers
{
    public interface IFileService
    {
        Task<byte[]> GetFileAsync(string folderPath, string fileName);

        Task<bool> SaveFileAsync(byte[] data, string folderPath, string fileName, bool overwrite = false);

        Task<bool> SaveFileAsync(Stream data, string folderPath, string fileName, bool overwrite = false);

        string GetMimeTypeFromBytes(byte[] bytes);
    }
}
