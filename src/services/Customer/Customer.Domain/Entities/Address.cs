using GaniPay.Customer.Domain.Enums;

namespace GaniPay.Customer.Domain.Entities;

public sealed class Address : AuditableEntity
{
    public Guid Id { get; set; }

    // DB: customer_id
    public Guid CustomerId { get; set; }

    // DB: address_type/city/district/postal_code/address_line_1
    public AddressType AddressType { get; set; }
    public string City { get; set; } = default!;
    public string District { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string AddressLine1 { get; set; } = default!;

    // Navigation
    public Customer Customer { get; set; } = default!;
}
