using Application.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Behaviors
{
    public class ValidationBehavior<T> where T : class
    {
        private readonly IEnumerable<IValidator<T>> _validators;
        private readonly ILogger<ValidationBehavior<T>> _logger;

        public ValidationBehavior(
            IEnumerable<IValidator<T>> validators,
            ILogger<ValidationBehavior<T>> logger)
        {
            _validators = validators;
            _logger = logger;
        }

        public async Task<T> ValidateAsync(T request)
        {
            if (!_validators.Any())
            {
                return request;
            }

            var context = new ValidationContext<T>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count > 0)
            {
                _logger.LogWarning("Validation failures for request {RequestType} with errors: {Errors}",
                    typeof(T).Name, string.Join(", ", failures.Select(f => f.ErrorMessage)));

                throw new System.ComponentModel.DataAnnotations.ValidationException(failures.Select(f => f.ErrorMessage).ToString());
            }

            return request;
        }
    }
}