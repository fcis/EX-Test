using Application.DTOs.Organization;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class CreateOrganizationDtoValidator : AbstractValidator<CreateOrganizationDto>
    {
        public CreateOrganizationDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(3).WithMessage("Name must be at least 3 characters")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Website)
                .NotEmpty().WithMessage("Website is required")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Website must be a valid URL");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("A valid email address is required");

            RuleFor(x => x.Phone)
                .Matches(@"^\+?[0-9\s\-\(\)]+$").When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage("Phone number is not in a valid format");

            RuleFor(x => x.Departments)
                .NotEmpty().WithMessage("At least one department is required")
                .Must(d => d.Count > 0).WithMessage("At least one department is required");

            RuleForEach(x => x.Departments)
                .SetValidator(new CreateOrganizationDepartmentDtoValidator());
        }
    }
}
