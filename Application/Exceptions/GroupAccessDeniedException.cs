namespace SplitwiseClone.Application.Exceptions;

using SplitwiseClone.Domain.Exceptions;

public sealed class GroupAccessDeniedException(int userId, int groupId)
    : ConflictException($"User {userId} is not a member of group {groupId}.");
