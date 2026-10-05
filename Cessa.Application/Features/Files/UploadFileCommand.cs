namespace Cessa.Application.Features.Files
{
    using MediatR;
    using Cessa.Application.Common.Interfaces;
    using Cessa.Application.Common.Exceptions;
    using Cessa.Domain.Entities.Content;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public record UploadFileCommand(Stream Content, string FileName, string ContentType, Guid UserId, string Role, bool IsPrivate = false) : IRequest<Guid>;

    public class UploadFileCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage) : IRequestHandler<UploadFileCommand, Guid>
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".pdf"
        };

        public async Task<Guid> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            var ext = Path.GetExtension(request.FileName);
            if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "File", new[] { "فرمت فایل مجاز نیست. فقط فایل‌های JPG, PNG, WEBP و PDF پذیرفته می‌شوند." } }
                });
            }

            if (request.Role != "Admin")
            {
                var userFilesCount = await context.UploadedFiles
                    .CountAsync(f => f.UploadedById == request.UserId && !f.IsDeleted, cancellationToken);
                if (userFilesCount >= 100)
                {
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        { "Quota", new[] { "سقف مجاز آپلود شما (حداکثر ۱۰۰ فایل) تکمیل شده است. لطفاً فایل‌های قدیمی را حذف نمایید." } }
                    });
                }
            }

            var safeFileName = Path.GetFileName(request.FileName);
            var filePath = await fileStorage.SaveFileAsync(request.Content, safeFileName, cancellationToken);

            var fileEntity = new UploadedFile
            {
                FileName = safeFileName,
                ContentType = request.ContentType,
                FilePath = filePath,
                UploadedById = request.UserId
            };

            context.UploadedFiles.Add(fileEntity);
            await context.SaveChangesAsync(cancellationToken);

            return fileEntity.Id;
        }
    }
}
