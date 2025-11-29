using AfriPay.CORE.Entities;
using AfriPay.CORE.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
        Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Customer?> GetByBvnAsync(string bvn, CancellationToken cancellationToken = default);
        Task<Customer?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
        Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
        Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByBvnAsync(string bvn, CancellationToken cancellationToken = default);
    }
}
