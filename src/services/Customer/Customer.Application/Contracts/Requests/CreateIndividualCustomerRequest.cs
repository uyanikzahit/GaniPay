using System.Text.Json.Serialization;
using GaniPay.Customer.Application.Contracts.Enums;
using System.Text.Json.Serialization;

namespace GaniPay.Customer.Application.Contracts.Requests;

public sealed class CreateIndividualCustomerRequest
{
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public DateOnly BirthDate { get; init; }
    public string Nationality { get; init; } = default!;
    public string IdentityNumber { get; init; } = default!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CustomerSegment Segment { get; init; }
}
