namespace Cessa.Application.Features.Advertisements
{
    using MediatR;
    using Cessa.Application.Common.Interfaces;
    using Cessa.Domain.Entities.Content;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Cessa.Application.Common.Exceptions;
    using System.Collections.Generic;
    using System.Linq;

    public record CreateAdCommand(string Title, string Description, string? LinkUrl, Guid? CoverFileId, bool IsActive) : IRequest<Guid>;

    public class CreateAdHandler(IApplicationDbContext context) : IRequestHandler<CreateAdCommand, Guid>
    {
        public async Task<Guid> Handle(CreateAdCommand request, CancellationToken cancellationToken)
        {
            var ad = new Advertisement
            {
                Title = request.Title,
                Description = request.Description,
                LinkUrl = request.LinkUrl,
                CoverFileId = request.CoverFileId,
                IsActive = request.IsActive
            };
            context.Advertisements.Add(ad);
            await context.SaveChangesAsync(cancellationToken);
            return ad.Id;
        }
    }

    public record UpdateAdCommand(Guid Id, string Title, string Description, string? LinkUrl, Guid? CoverFileId, bool IsActive) : IRequest<bool>;

    public class UpdateAdHandler(IApplicationDbContext context) : IRequestHandler<UpdateAdCommand, bool>
    {
        public async Task<bool> Handle(UpdateAdCommand request, CancellationToken cancellationToken)
        {
            var ad = await context.Advertisements.FindAsync(new object[] { request.Id }, cancellationToken)
                ?? throw new NotFoundException(nameof(Advertisement), request.Id);

            ad.Title = request.Title;
            ad.Description = request.Description;
            ad.LinkUrl = request.LinkUrl;
            ad.IsActive = request.IsActive;

            // FIX: Only overwrite CoverFileId if a new one is provided.
            // This prevents toggling status or editing text from wiping the image.
            if (request.CoverFileId.HasValue)
            {
                ad.CoverFileId = request.CoverFileId.Value;
            }

            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public record DeleteAdCommand(Guid Id) : IRequest<bool>;

    public class DeleteAdHandler(IApplicationDbContext context) : IRequestHandler<DeleteAdCommand, bool>
    {
        public async Task<bool> Handle(DeleteAdCommand request, CancellationToken cancellationToken)
        {
            var ad = await context.Advertisements.FindAsync(new object[] { request.Id }, cancellationToken)
                ?? throw new NotFoundException(nameof(Advertisement), request.Id);
            ad.IsDeleted = true;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    // FIX: Include CoverFileId in AdDto
    public record AdDto(
        Guid Id,
        string Title,
        string Description,
        string? LinkUrl,
        bool IsActive,
        Guid? CoverFileId,
        string? CoverFilePath
    );

    public record GetAllAdsQuery() : IRequest<List<AdDto>>;

    public class GetAllAdsHandler(IApplicationDbContext context) : IRequestHandler<GetAllAdsQuery, List<AdDto>>
    {
        public async Task<List<AdDto>> Handle(GetAllAdsQuery request, CancellationToken cancellationToken) =>
            await context.Advertisements.Include(a => a.CoverFile)
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AdDto(
                    a.Id,
                    a.Title,
                    a.Description,
                    a.LinkUrl,
                    a.IsActive,
                    a.CoverFileId,
                    a.CoverFile != null ? a.CoverFile.FilePath : null
                )).ToListAsync(cancellationToken);
    }

    public record GetActiveAdsQuery() : IRequest<List<AdDto>>;

    public class GetActiveAdsHandler(IApplicationDbContext context) : IRequestHandler<GetActiveAdsQuery, List<AdDto>>
    {
        public async Task<List<AdDto>> Handle(GetActiveAdsQuery request, CancellationToken cancellationToken) =>
            await context.Advertisements.Include(a => a.CoverFile)
                .Where(a => a.IsActive && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AdDto(
                    a.Id,
                    a.Title,
                    a.Description,
                    a.LinkUrl,
                    a.IsActive,
                    a.CoverFileId,
                    a.CoverFile != null ? a.CoverFile.FilePath : null
                )).ToListAsync(cancellationToken);
    }
}