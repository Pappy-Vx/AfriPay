namespace AfriPay.CORE.Interfaces
{
    /// <summary>
    /// Defines the application database context contract
    /// </summary>
    public interface IApplicationDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
