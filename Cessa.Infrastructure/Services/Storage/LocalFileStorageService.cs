namespace Cessa.Infrastructure.Services.Storage
{
    using Cessa.Application.Common.Interfaces;
    using Microsoft.Extensions.Configuration;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats.Webp;
    using SixLabors.ImageSharp.Processing;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class LocalFileStorageService(IConfiguration config) : IFileStorageService
    {
        public async Task<string> SaveFileAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
        {
            var uploadDir = config["UploadSettings:Directory"] ?? "./uploads";
            var absoluteUploadFolder = Path.GetFullPath(uploadDir);

            if (!Directory.Exists(absoluteUploadFolder))
                Directory.CreateDirectory(absoluteUploadFolder);

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            bool isImage = ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".webp";
            
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}";

            if (isImage)
            {
                try 
                {
                    if (content.CanSeek) content.Position = 0;
                    using var image = await Image.LoadAsync(content, cancellationToken);
                    if (image.Width > 1920 || image.Height > 1920)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(1920, 1920) }));
                    }
                    var webpPath = Path.Combine(absoluteUploadFolder, uniqueFileName + ".webp");
                    await image.SaveAsWebpAsync(webpPath, new WebpEncoder { Quality = 80 }, cancellationToken);
                    return $"/uploads/{uniqueFileName}.webp";
                } 
                catch 
                {
                    // Fallback: If it's a corrupted image or fake extension, save with original extension!
                    var fallbackPath = Path.Combine(absoluteUploadFolder, uniqueFileName + ext);
                    if (content.CanSeek) content.Position = 0;
                    using var fileStream = new FileStream(fallbackPath, FileMode.Create);
                    await content.CopyToAsync(fileStream, cancellationToken);
                    return $"/uploads/{uniqueFileName}{ext}";
                }
            }
            else
            {
                var filePath = Path.Combine(absoluteUploadFolder, uniqueFileName + ext);
                if (content.CanSeek) content.Position = 0;
                using var fileStream = new FileStream(filePath, FileMode.Create);
                await content.CopyToAsync(fileStream, cancellationToken);
                return $"/uploads/{uniqueFileName}{ext}";
            }
        }
    }
}
