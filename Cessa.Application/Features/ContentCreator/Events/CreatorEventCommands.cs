namespace Cessa.Application.Features.ContentCreator.Events
{
    using MediatR;
    using Cessa.Domain.Entities.Content;
    using Cessa.Application.Common.Interfaces;
    using Cessa.Application.Common.Exceptions;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public record CreateEventCommand(string Title, string Description, DateTime EventDate, string? AdminCardNumber, Guid? CoverFileId, Guid CreatorId) : IRequest<Guid>;

    public class CreateEventCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateEventCommand, Guid>
    {
        public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users.FindAsync(new object[] { request.CreatorId }, cancellationToken);
            if (user != null && !user.IsVerifiedCreator)
                throw new UnauthorizedAccessException("حساب کاربری شما به عنوان تولیدکننده محتوا هنوز توسط ادمین تایید نشده است.");

            var newEvent = new Event
            {
                Title = request.Title,
                Description = request.Description,
                EventDate = request.EventDate,
                AdminCardNumber = request.AdminCardNumber,
                CoverFileId = request.CoverFileId,
                CreatorId = request.CreatorId
            };
            context.Events.Add(newEvent);
            await context.SaveChangesAsync(cancellationToken);
            return newEvent.Id;
        }
    }

    public record UpdateEventCommand(Guid Id, string Title, string Description, DateTime EventDate, string? AdminCardNumber, Guid? CoverFileId, Guid CreatorId) : IRequest<bool>;

    public class UpdateEventCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateEventCommand, bool>
    {
        public async Task<bool> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users.FindAsync(new object[] { request.CreatorId }, cancellationToken);
            if (user != null && !user.IsVerifiedCreator)
                throw new UnauthorizedAccessException("حساب کاربری شما به عنوان تولیدکننده محتوا هنوز توسط ادمین تایید نشده است.");

            // Verify event exists AND belongs to the requesting creator
            var ev = await context.Events.FirstOrDefaultAsync(e => e.Id == request.Id && e.CreatorId == request.CreatorId, cancellationToken)
                ?? throw new NotFoundException(nameof(Event), request.Id);

            ev.Title = request.Title;
            ev.Description = request.Description;
            ev.EventDate = request.EventDate;
            ev.AdminCardNumber = request.AdminCardNumber;
            ev.CoverFileId = request.CoverFileId;

            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public record DeleteEventCommand(Guid Id, Guid CreatorId) : IRequest<bool>;

    public class DeleteEventCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteEventCommand, bool>
    {
        public async Task<bool> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users.FindAsync(new object[] { request.CreatorId }, cancellationToken);
            if (user != null && !user.IsVerifiedCreator)
                throw new UnauthorizedAccessException("حساب کاربری شما به عنوان تولیدکننده محتوا هنوز توسط ادمین تایید نشده است.");

            // Verify event exists AND belongs to the requesting creator
            var ev = await context.Events.FirstOrDefaultAsync(e => e.Id == request.Id && e.CreatorId == request.CreatorId, cancellationToken)
                ?? throw new NotFoundException(nameof(Event), request.Id);

            ev.IsDeleted = true; // Soft Delete
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}