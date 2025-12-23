using System;
using System.Collections.Generic;

namespace GaniPay.Integration.Infrastructure._ScaffoldFromDb.Models;

public partial class IntegrationProvider
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string BaseUrl { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<IntegrationLog> IntegrationLogs { get; set; } = new List<IntegrationLog>();
}
