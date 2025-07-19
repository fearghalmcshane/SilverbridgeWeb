using Microsoft.EntityFrameworkCore;
using SilverbridgeWeb.Modules.Ticketing.Domain.Customers;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.Ticketing.Infrastructure.Customers;

internal sealed class CustomerRepository(TicketingDbContext context) : ICustomerRepository
{
    public async Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Customers.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public void Insert(Customer customer)
    {
        context.Customers.Add(customer);
    }
}
