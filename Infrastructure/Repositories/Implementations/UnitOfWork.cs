namespace SplitwiseClone.Infrastructure.Repositories.Implementations;

using SplitwiseClone.Application.Interfaces.Repositories;
using SplitwiseClone.Infrastructure.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SplitwiseDbContext _context;

    public UnitOfWork(
        SplitwiseDbContext context,
        IUserRepository users,
        IGroupRepository groups,
        IGroupMemberRepository groupMembers,
        IExpenseRepository expenses,
        IExpenseSplitRepository expenseSplits)
    {
        _context = context;
        Users = users;
        Groups = groups;
        GroupMembers = groupMembers;
        Expenses = expenses;
        ExpenseSplits = expenseSplits;
    }

    public IUserRepository Users { get; }
    public IGroupRepository Groups { get; }
    public IGroupMemberRepository GroupMembers { get; }
    public IExpenseRepository Expenses { get; }
    public IExpenseSplitRepository ExpenseSplits { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
