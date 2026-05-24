namespace HttpClientMethods.Helpers
{
    public class FileService : IFileService
    {
        public Task<byte[]> GetFileAsync(string folderPath, string fileName)
        {
            if (!Directory.Exists(folderPath)) 
            {
                throw new DirectoryNotFoundException("Folder not found.");
            }

            string fullPath = Path.Combine(folderPath, fileName);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("File not found.");
            }
            
            return File.ReadAllBytesAsync(fullPath);
        }

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

        public async Task<bool> SaveFileAsync(Stream data, string folderPath, string fileName, bool overwrite = false)
        {
            try
            {
                if (data == null) return false;

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

                await data.CopyToAsync(fileStream).ConfigureAwait(false);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetMimeTypeFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4) return "application/octet-stream";

            // JPEG: Starts with FF D8 FF
            if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            {
                return "image/jpeg";
            }

            // PNG: Starts with 89 50 4E 47
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                return "image/png";
            }

            // GIF: Starts with 47 49 46 38 ("GIF8")
            if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38)
            {
                return "image/gif";
            }

            // Default fallback if unknown binary pattern
            return "image/jpeg";
        }        
    }
}
