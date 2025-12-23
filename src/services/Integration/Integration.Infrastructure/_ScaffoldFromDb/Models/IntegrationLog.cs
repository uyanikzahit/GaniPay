using System;
using System.Collections.Generic;

namespace GaniPay.Integration.Infrastructure._ScaffoldFromDb.Models;

public partial class IntegrationLog
{
    public Guid Id { get; set; }

    public Guid ProviderId { get; set; }

    public string Operation { get; set; } = null!;

    public string RequestPayload { get; set; } = null!;

    public string? ResponsePayload { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual IntegrationProvider Provider { get; set; } = null!;
}
