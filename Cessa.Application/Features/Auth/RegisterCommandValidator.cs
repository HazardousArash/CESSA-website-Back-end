namespace Cessa.Application.Features.Auth
{
    using FluentValidation;

    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ایمیل الزامی است.")
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                .WithMessage("فرمت ایمیل نامعتبر است.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("رمز عبور الزامی است.")
                .MinimumLength(8).WithMessage("رمز عبور باید حداقل ۸ کاراکتر باشد.")
                .Matches(@"[0-9]").WithMessage("رمز عبور باید حداقل شامل یک عدد باشد.");

            RuleFor(x => x.FirstName).NotEmpty().WithMessage("نام الزامی است.");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("نام خانوادگی الزامی است.");
        }
    }
}
