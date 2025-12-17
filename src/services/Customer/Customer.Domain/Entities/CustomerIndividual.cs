namespace GaniPay.Customer.Domain.Entities;

public sealed class CustomerIndividual : AuditableEntity
{
    public Guid Id { get; set; }

    // DB: customer_id
    public Guid CustomerId { get; set; }

    // DB: first_name/last_name/birth_date/nationality/identity_number
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateOnly BirthDate { get; set; }
    public string Nationality { get; set; } = default!;
    public string IdentityNumber { get; set; } = default!;

    // Navigation
    public Customer Customer { get; set; } = default!;
}
