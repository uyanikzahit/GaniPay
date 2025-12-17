using GaniPay.Customer.Domain.Enums;

namespace GaniPay.Customer.Domain.Entities;

public sealed class Phone : AuditableEntity
{
    public Guid Id { get; set; }

    // DB: customer_id
    public Guid CustomerId { get; set; }

    // DB: country_code/phone_number/type
    public string CountryCode { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public PhoneType Type { get; set; }

    // Navigation
    public Customer Customer { get; set; } = default!;
}
