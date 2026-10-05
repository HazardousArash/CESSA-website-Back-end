namespace Cessa.Application.Features.Admin.HallOfFame
{
    using FluentValidation;
    using MediatR;
    using Cessa.Application.Common.Interfaces;
    using Cessa.Application.Common.Exceptions;
    using Cessa.Domain.Entities.Admin;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public record AddHallOfFameMemberCommand(
        string FullName, 
        string Description, 
        string? GithubUrl, 
        string? LinkedInUrl, 
        string? TelegramId, 
        string? Email, 
        Guid? PhotoFileId,
        int? EntryYear,
        bool IsAssociationMember,
        string? Term,
        string? RoleTitle) : IRequest<Guid>;

    public class AddHallOfFameMemberValidator : AbstractValidator<AddHallOfFameMemberCommand>
    {
        public AddHallOfFameMemberValidator()
        {
            RuleFor(v => v.FullName).NotEmpty().WithMessage("نام و نام خانوادگی الزامی است.").MaximumLength(100);
            RuleFor(v => v.Description).NotEmpty().WithMessage("توضیحات الزامی است.").MaximumLength(1000);
            RuleFor(v => v.Email).EmailAddress().When(v => !string.IsNullOrEmpty(v.Email)).WithMessage("فرمت ایمیل نامعتبر است.");
            RuleFor(v => v.EntryYear).InclusiveBetween(1350, 1450).When(v => v.EntryYear.HasValue);
            RuleFor(v => v.Term).NotEmpty().Matches(@"^1[34]\d{2}-1[34]\d{2}$").When(v => v.IsAssociationMember);
        }
    }

    public class AddHallOfFameMemberHandler(IApplicationDbContext context) : IRequestHandler<AddHallOfFameMemberCommand, Guid>
    {
        public async Task<Guid> Handle(AddHallOfFameMemberCommand request, CancellationToken cancellationToken)
        {
            var member = new HallOfFameMember
            {
                FullName = request.FullName.Trim(),
                Description = request.Description.Trim(),
                GithubUrl = request.GithubUrl?.Trim(),
                LinkedInUrl = request.LinkedInUrl?.Trim(),
                TelegramId = request.TelegramId?.Trim(),
                Email = request.Email?.Trim(),
                PhotoFileId = request.PhotoFileId,
                EntryYear = request.EntryYear,
                IsAssociationMember = request.IsAssociationMember,
                Term = request.IsAssociationMember ? request.Term?.Trim() : null,
                RoleTitle = request.IsAssociationMember ? request.RoleTitle?.Trim() : null
            };

            context.HallOfFameMembers.Add(member);
            await context.SaveChangesAsync(cancellationToken);
            return member.Id;
        }
    }

    // دستور ویرایش
    public record UpdateHallOfFameMemberCommand(
        Guid Id,
        string FullName, 
        string Description, 
        string? GithubUrl, 
        string? LinkedInUrl, 
        string? TelegramId, 
        string? Email, 
        Guid? PhotoFileId,
        int? EntryYear,
        bool IsAssociationMember,
        string? Term,
        string? RoleTitle) : IRequest<bool>;

    public class UpdateHallOfFameMemberValidator : AbstractValidator<UpdateHallOfFameMemberCommand>
    {
        public UpdateHallOfFameMemberValidator()
        {
            RuleFor(v => v.Id).NotEmpty();
            RuleFor(v => v.FullName).NotEmpty().WithMessage("نام و نام خانوادگی الزامی است.").MaximumLength(100);
            RuleFor(v => v.Description).NotEmpty().WithMessage("توضیحات الزامی است.").MaximumLength(1000);
            RuleFor(v => v.Email).EmailAddress().When(v => !string.IsNullOrEmpty(v.Email)).WithMessage("فرمت ایمیل نامعتبر است.");
            RuleFor(v => v.EntryYear).InclusiveBetween(1350, 1450).When(v => v.EntryYear.HasValue);
            RuleFor(v => v.Term).NotEmpty().Matches(@"^1[34]\d{2}-1[34]\d{2}$").When(v => v.IsAssociationMember);
        }
    }

    public class UpdateHallOfFameMemberCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateHallOfFameMemberCommand, bool>
    {
        public async Task<bool> Handle(UpdateHallOfFameMemberCommand request, CancellationToken cancellationToken)
        {
            var member = await context.HallOfFameMembers.FindAsync(new object[] { request.Id }, cancellationToken)
                ?? throw new NotFoundException(nameof(HallOfFameMember), request.Id);

            member.FullName = request.FullName.Trim();
            member.Description = request.Description.Trim();
            member.GithubUrl = request.GithubUrl?.Trim();
            member.LinkedInUrl = request.LinkedInUrl?.Trim();
            member.TelegramId = request.TelegramId?.Trim();
            member.Email = request.Email?.Trim();

            if (request.PhotoFileId.HasValue)
            {
                member.PhotoFileId = request.PhotoFileId.Value;
            }

            member.EntryYear = request.EntryYear;
            member.IsAssociationMember = request.IsAssociationMember;
            member.Term = request.IsAssociationMember ? request.Term?.Trim() : null;
            member.RoleTitle = request.IsAssociationMember ? request.RoleTitle?.Trim() : null;

            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public record DeleteHallOfFameMemberCommand(Guid Id) : IRequest<bool>;

    public class DeleteHallOfFameMemberCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteHallOfFameMemberCommand, bool>
    {
        public async Task<bool> Handle(DeleteHallOfFameMemberCommand request, CancellationToken cancellationToken)
        {
            var member = await context.HallOfFameMembers.FindAsync(new object[] { request.Id }, cancellationToken)
                ?? throw new NotFoundException(nameof(HallOfFameMember), request.Id);

            member.IsDeleted = true;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
