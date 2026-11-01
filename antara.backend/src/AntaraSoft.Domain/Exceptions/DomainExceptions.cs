using System;

namespace AntaraSoft.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
        public DomainException(string message, Exception inner) : base(message, inner) { }
    }

    public class NotFoundException : DomainException
    {
        public NotFoundException(string entity, object id)
            : base($"{entity} con id '{id}' no fue encontrado.") { }

        public NotFoundException(string message) : base(message) { }
    }

    public class ValidationException : DomainException
    {
        public ValidationException(string message) : base(message) { }
    }

    public class ConflictException : DomainException
    {
        public ConflictException(string message) : base(message) { }
    }
}

