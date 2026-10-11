namespace SplitwiseClone.Infrastructure.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IGroupRepository Groups { get; }
    IGroupMemberRepository GroupMembers { get; }
    IExpenseRepository Expenses { get; }
    IExpenseSplitRepository ExpenseSplits { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
