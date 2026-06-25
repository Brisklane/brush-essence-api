namespace BrushEssence.Application.Common.Exceptions;

/// <summary>Base type for expected, mappable application errors.</summary>
public abstract class AppException(string message) : Exception(message);

/// <summary>A requested resource does not exist (→ 404).</summary>
public sealed class NotFoundException(string message) : AppException(message);

/// <summary>The request conflicts with current state, e.g. duplicate email (→ 409).</summary>
public sealed class ConflictException(string message) : AppException(message);

/// <summary>The request is well-formed but invalid for business reasons (→ 400).</summary>
public sealed class BadRequestException(string message) : AppException(message);

/// <summary>Authentication failed, e.g. bad credentials or token (→ 401).</summary>
public sealed class AuthenticationException(string message) : AppException(message);
