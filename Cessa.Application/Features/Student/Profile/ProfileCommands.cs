namespace Cessa.Application.Features.Student.Profile
{
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Cessa.Domain.Entities.Auth;
    using Cessa.Application.Common.Exceptions;
    using Cessa.Application.Common.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public record ProfileDto(string FirstName, string LastName, string? StudentNumber, int? EntryYear, string? ProfilePictureUrl);

    public record GetMyProfileQuery(Guid UserId) : IRequest<ProfileDto>;

    public class GetMyProfileQueryHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<GetMyProfileQuery, ProfileDto>
    {
        public async Task<ProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await userManager.Users.Include(u => u.ProfilePicture).FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                ?? throw new NotFoundException(nameof(ApplicationUser), request.UserId);

            return new ProfileDto(user.FirstName, user.LastName, user.StudentNumber, user.EntryYear, user.ProfilePicture?.FilePath);
        }
    }

    public record UpdateProfileCommand(Guid UserId, string FirstName, string LastName, string? StudentNumber, int? EntryYear, string? NewPassword, Guid? ProfilePictureId) : IRequest<bool>;

    public class UpdateProfileHandler(UserManager<ApplicationUser> userManager, IApplicationDbContext context) : IRequestHandler<UpdateProfileCommand, bool>
    {
        public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString()) 
                ?? throw new NotFoundException(nameof(ApplicationUser), request.UserId);

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.StudentNumber = request.StudentNumber;
            user.EntryYear = request.EntryYear;

            // SECURITY FIX: Verify file ownership so users cannot assign private invoices as their avatar
            if (request.ProfilePictureId.HasValue)
            {
                var ownsFile = await context.UploadedFiles
                    .AnyAsync(f => f.Id == request.ProfilePictureId.Value && f.UploadedById == request.UserId && !f.IsDeleted, cancellationToken);
                if (!ownsFile)
                {
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        { "ProfilePicture", new[] { "تصویر انتخاب‌شده نامعتبر است یا متعلق به شما نمی‌باشد." } }
                    });
                }
                user.ProfilePictureId = request.ProfilePictureId;
            }

            await userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, token, request.NewPassword);
            }
            return true;
        }
    }
}
