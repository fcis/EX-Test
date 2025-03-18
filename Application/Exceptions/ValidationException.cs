using System;
using System.Collections.Generic;

namespace Application.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException() : base("One or more validation failures have occurred.")
        {
            Errors = new List<string>();
        }

        public ValidationException(IEnumerable<string> errors) : this()
        {
            Errors = new List<string>(errors);
        }

        public ValidationException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }

        public List<string> Errors { get; }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException() : base()
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string name, object key) : base($"Entity \"{name}\" ({key}) was not found.")
        {
        }
    }

    public class BadRequestException : Exception
    {
        public BadRequestException() : base()
        {
        }

        public BadRequestException(string message) : base(message)
        {
        }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base("You are not authorized to perform this action.")
        {
        }

        public UnauthorizedException(string message) : base(message)
        {
        }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException() : base("You do not have permission to perform this action.")
        {
        }

        public ForbiddenException(string message) : base(message)
        {
        }
    }

    public class AlreadyExistsException : Exception
    {
        public AlreadyExistsException() : base()
        {
        }

        public AlreadyExistsException(string message) : base(message)
        {
        }

        public AlreadyExistsException(string name, object key) : base($"Entity \"{name}\" ({key}) already exists.")
        {
        }
    }
}