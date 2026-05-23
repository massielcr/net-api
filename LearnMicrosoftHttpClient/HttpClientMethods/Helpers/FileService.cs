namespace HttpClientMethods.Helpers
{
    public class FileService : IFileService
    {
        public async Task<bool> SaveFileAsync(byte[] data, string folderPath, string fileName, bool overwrite = false)
        {
            try
            {
                if (data == null || data.Length == 0) return false;

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fullPath = Path.Combine(folderPath, fileName);

                if (!overwrite && File.Exists(fullPath))
                {
                    return false; // File exists and overwrite not allowed
                }

                using var fileStream = new FileStream(
                                            fullPath,
                                            FileMode.Create,
                                            FileAccess.Write,
                                            FileShare.None,
                                            bufferSize: 4096,
                                            useAsync: true);

                await fileStream.WriteAsync(data.AsMemory()).ConfigureAwait(false);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
