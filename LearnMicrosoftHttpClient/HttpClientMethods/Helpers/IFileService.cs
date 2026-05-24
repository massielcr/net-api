namespace HttpClientMethods.Helpers
{
    public interface IFileService
    {
        Task<byte[]> GetFileAsync(string folderPath, string fileName);

        Task<bool> SaveFileAsync(string folderPath, string fileName, byte[] data,  bool overwrite = false);

        Task<bool> SaveFileAsync(string folderPath, string fileName, Stream data,  bool overwrite = false);

        string GetMimeTypeFromBytes(byte[] bytes);
    }
}
