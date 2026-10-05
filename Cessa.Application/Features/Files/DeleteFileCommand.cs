namespace Cessa.Application.Features.Files;

using MediatR;
using Cessa.Application.Common.Interfaces;
using Cessa.Application.Common.Exceptions;
using Cessa.Domain.Entities.Content;
using System;
using System.Threading;
using System.Threading.Tasks;

public record DeleteFileCommand(Guid FileId, Guid RequestingUserId, string Role) : IRequest<bool>;

public class DeleteFileCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteFileCommand, bool>
{
    public async Task<bool> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var file = await context.UploadedFiles.FindAsync(new object[] { request.FileId }, cancellationToken);
        if (file == null) throw new NotFoundException(nameof(UploadedFile), request.FileId);

        // Allow Admins to delete any file; otherwise, restrict to the owner
        if (request.Role != "Admin" && file.UploadedById != request.RequestingUserId)
            throw new UnauthorizedAccessException("شما مجوز حذف این فایل را ندارید.");

        file.IsDeleted = true; // Soft delete
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
