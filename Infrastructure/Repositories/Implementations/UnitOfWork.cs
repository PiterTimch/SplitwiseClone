namespace SplitwiseClone.Infrastructure.Repositories.Implementations;

using SplitwiseClone.Infrastructure.Data;
using SplitwiseClone.Infrastructure.Repositories.Interfaces;

public class UnitOfWork : IUnitOfWork
{
    private readonly SplitwiseDbContext _context;
    private IUserRepository? _users;
    private IGroupRepository? _groups;
    private IGroupMemberRepository? _groupMembers;
    private IExpenseRepository? _expenses;
    private IExpenseSplitRepository? _expenseSplits;

    public UnitOfWork(SplitwiseDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IGroupRepository Groups => _groups ??= new GroupRepository(_context);
    public IGroupMemberRepository GroupMembers => _groupMembers ??= new GroupMemberRepository(_context);
    public IExpenseRepository Expenses => _expenses ??= new ExpenseRepository(_context);
    public IExpenseSplitRepository ExpenseSplits => _expenseSplits ??= new ExpenseSplitRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
