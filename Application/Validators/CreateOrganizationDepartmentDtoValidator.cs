using Application.DTOs.Organization;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class CreateOrganizationDepartmentDtoValidator : AbstractValidator<CreateOrganizationDepartmentDto>
    {
        public CreateOrganizationDepartmentDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Department name is required")
                .MinimumLength(2).WithMessage("Department name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters");
        }
    }
}
