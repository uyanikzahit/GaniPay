using GaniPay.Customer.Domain.Enums;
using System.Net;
using System.Numerics;

namespace GaniPay.Customer.Domain.Entities;

public sealed class Customer : AuditableEntity
{
    public Guid Id { get; set; }

    // DB: customer_number
    public string CustomerNumber { get; set; } = default!;

    // DB: type/segment/status (string olarak persist edilecek)
    public CustomerType Type { get; set; }
    public CustomerSegment Segment { get; set; }
    public CustomerStatus Status { get; set; }

    // DB: open_date/close_date (date)
    public DateOnly OpenDate { get; set; }
    public DateOnly? CloseDate { get; set; }

    // DB: close_reason
    public string? CloseReason { get; set; }

    // Navigation
    public CustomerIndividual? Individual { get; set; }
    public List<Email> Emails { get; set; } = new();
    public List<Phone> Phones { get; set; } = new();
    public List<Address> Addresses { get; set; } = new();
}
