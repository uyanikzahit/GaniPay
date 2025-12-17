using GaniPay.Customer.Domain.Enums;

namespace GaniPay.Customer.Domain.Entities;

public sealed class Email : AuditableEntity
{
    public Guid Id { get; set; }

    // DB: customer_id
    public Guid CustomerId { get; set; }

    // DB: email_address/type/is_verified
    public string EmailAddress { get; set; } = default!;
    public EmailType Type { get; set; }
    public bool IsVerified { get; set; }

    // Navigation
    public Customer Customer { get; set; } = default!;
}
