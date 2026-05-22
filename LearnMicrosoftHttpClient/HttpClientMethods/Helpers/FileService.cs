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

                if (File.Exists(fullPath))
                {
                    if (overwrite)
                    {
                        File.Delete(fullPath);
                    }
                    else
                    {
                        return false; // File exists and overwrite not allowed
                    }
                }

                await File.WriteAllBytesAsync(fullPath, data);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
