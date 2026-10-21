namespace SplitwiseClone.Application.Interfaces.Repositories;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IGroupRepository Groups { get; }
    IGroupMemberRepository GroupMembers { get; }
    IExpenseRepository Expenses { get; }
    IExpenseSplitRepository ExpenseSplits { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
