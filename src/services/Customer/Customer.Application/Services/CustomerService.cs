using GaniPay.Customer.Application.Abstractions;
using GaniPay.Customer.Application.Contracts.Dtos;
using GaniPay.Customer.Application.Contracts.Requests;
using GaniPay.Customer.Domain.Entities;

using ContractEnums = GaniPay.Customer.Application.Contracts.Enums;
using DomainEnums = GaniPay.Customer.Domain.Enums;

namespace GaniPay.Customer.Application.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customers;
    private readonly IEmailRepository _emails;
    private readonly IPhoneRepository _phones;
    private readonly IAddressRepository _addresses;

    public CustomerService(
        ICustomerRepository customers,
        IEmailRepository emails,
        IPhoneRepository phones,
        IAddressRepository addresses)
    {
        _customers = customers;
        _emails = emails;
        _phones = phones;
        _addresses = addresses;
    }

    public async Task<CustomerDto> CreateIndividualAsync(CreateIndividualCustomerRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.IdentityNumber))
            throw new ArgumentException("IdentityNumber zorunludur.");

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            throw new ArgumentException("Ad/Soyad zorunludur.");

        var identity = request.IdentityNumber.Trim();
        if (await _customers.ExistsByIdentityNumberAsync(identity, ct))
            throw new InvalidOperationException("Bu identityNumber ile müşteri zaten mevcut.");

        var customerId = Guid.NewGuid();

        var customer = new Domain.Entities.Customer
        {
            Id = customerId,
            CustomerNumber = GenerateCustomerNumber(),
            Type = DomainEnums.CustomerType.Individual,
            Segment = MapSegment(request.Segment),
            Status = DomainEnums.CustomerStatus.Active,
            OpenDate = DateOnly.FromDateTime(DateTime.UtcNow),

            Individual = new CustomerIndividual
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                BirthDate = request.BirthDate,
                Nationality = request.Nationality.Trim(),
                IdentityNumber = identity
            }
        };

        await _customers.AddAsync(customer, ct);
        await _customers.SaveChangesAsync(ct);

        return ToCustomerDto(customer);
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid customerId, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(customerId, ct);
        return customer is null ? null : ToCustomerDto(customer);
    }

    public async Task AddEmailAsync(Guid customerId, AddEmailRequest request, CancellationToken ct)
    {
        _ = await _customers.GetByIdAsync(customerId, ct)
            ?? throw new KeyNotFoundException("Customer bulunamadı.");

        if (string.IsNullOrWhiteSpace(request.EmailAddress) || !request.EmailAddress.Contains('@'))
            throw new ArgumentException("Email formatı geçersiz.");

        var entity = new Email
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            EmailAddress = request.EmailAddress.Trim().ToLowerInvariant(),
            Type = MapEmailType(request.Type),
            IsVerified = false
        };

        await _emails.AddAsync(entity, ct);
        await _emails.SaveChangesAsync(ct);
    }

    public async Task AddPhoneAsync(Guid customerId, AddPhoneRequest request, CancellationToken ct)
    {
        _ = await _customers.GetByIdAsync(customerId, ct)
            ?? throw new KeyNotFoundException("Customer bulunamadı.");

        if (string.IsNullOrWhiteSpace(request.CountryCode) || string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new ArgumentException("Telefon alanları zorunludur.");

        var entity = new Phone
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CountryCode = request.CountryCode.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Type = MapPhoneType(request.Type)
        };

        await _phones.AddAsync(entity, ct);
        await _phones.SaveChangesAsync(ct);
    }

    public async Task AddAddressAsync(Guid customerId, AddAddressRequest request, CancellationToken ct)
    {
        _ = await _customers.GetByIdAsync(customerId, ct)
            ?? throw new KeyNotFoundException("Customer bulunamadı.");

        if (string.IsNullOrWhiteSpace(request.City) || string.IsNullOrWhiteSpace(request.District))
            throw new ArgumentException("Şehir/İlçe zorunludur.");

        if (string.IsNullOrWhiteSpace(request.AddressLine1))
            throw new ArgumentException("AddressLine1 zorunludur.");

        var entity = new Address
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            AddressType = MapAddressType(request.AddressType),
            City = request.City.Trim(),
            District = request.District.Trim(),
            PostalCode = request.PostalCode.Trim(),
            AddressLine1 = request.AddressLine1.Trim()
        };

        await _addresses.AddAsync(entity, ct);
        await _addresses.SaveChangesAsync(ct);
    }

    public async Task CloseAsync(Guid customerId, CloseCustomerRequest request, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(customerId, ct)
            ?? throw new KeyNotFoundException("Customer bulunamadı.");

        if (customer.Status == DomainEnums.CustomerStatus.Closed)
            return;

        customer.Status = DomainEnums.CustomerStatus.Closed;
        customer.CloseDate = DateOnly.FromDateTime(DateTime.UtcNow);
        customer.CloseReason = string.IsNullOrWhiteSpace(request.Reason) ? "N/A" : request.Reason.Trim();

        await _customers.SaveChangesAsync(ct);
    }

    // ✅ UI için detaylı endpoint response'u
    public async Task<CustomerDetailDto?> GetDetailByIdAsync(Guid customerId, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(customerId, ct);
        return customer is null ? null : ToCustomerDetailDto(customer);
    }

    // --------------------------
    // DTO MAP
    // --------------------------
    private static CustomerDto ToCustomerDto(Domain.Entities.Customer c)
        => new(
            c.Id,
            c.CustomerNumber,
            MapCustomerType(c.Type),
            MapCustomerSegment(c.Segment),
            MapCustomerStatus(c.Status),
            c.OpenDate,
            c.CloseDate,
            c.CloseReason
        );

    // ✅ NEW: Detail DTO map (Individual + Emails + Phones + Addresses)
    private static CustomerDetailDto ToCustomerDetailDto(Domain.Entities.Customer c)
        => new(
            c.Id,
            c.CustomerNumber,
            MapCustomerType(c.Type),
            MapCustomerSegment(c.Segment),
            MapCustomerStatus(c.Status),
            c.OpenDate,
            c.CloseDate,
            c.CloseReason,

            c.Individual is null
                ? null
                : new CustomerIndividualDto(
                    c.Id,
                    c.Individual.FirstName,
                    c.Individual.LastName,
                    c.Individual.BirthDate,
                    c.Individual.Nationality,
                    c.Individual.IdentityNumber
                ),

            c.Emails.Select(e => new EmailDto(
                e.Id,
                e.CustomerId,
                e.EmailAddress,
                MapEmailTypeToContract(e.Type),
                e.IsVerified
            )).ToList(),

            c.Phones.Select(p => new PhoneDto(
                p.Id,
                p.CustomerId,
                p.CountryCode,
                p.PhoneNumber,
                MapPhoneTypeToContract(p.Type)
            )).ToList(),

            c.Addresses.Select(a => new AddressDto(
                a.Id,
                a.CustomerId,
                MapAddressTypeToContract(a.AddressType),
                a.City,
                a.District,
                a.PostalCode,
                a.AddressLine1
            )).ToList()
        );

    private static string GenerateCustomerNumber()
        => $"CUST-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".Substring(0, 26);

    // --------------------------
    // ENUM MAP
    // --------------------------
    private static DomainEnums.CustomerSegment MapSegment(ContractEnums.CustomerSegment segment)
        => segment switch
        {
            ContractEnums.CustomerSegment.Standard => DomainEnums.CustomerSegment.Standard,
            ContractEnums.CustomerSegment.Premium => DomainEnums.CustomerSegment.Premium,
            ContractEnums.CustomerSegment.Vip => DomainEnums.CustomerSegment.Vip,
            _ => DomainEnums.CustomerSegment.Standard
        };

    private static DomainEnums.EmailType MapEmailType(ContractEnums.EmailType type)
        => type switch
        {
            ContractEnums.EmailType.Personal => DomainEnums.EmailType.Personal,
            ContractEnums.EmailType.Work => DomainEnums.EmailType.Work,
            _ => DomainEnums.EmailType.Personal
        };

    private static DomainEnums.PhoneType MapPhoneType(ContractEnums.PhoneType type)
        => type switch
        {
            ContractEnums.PhoneType.Mobile => DomainEnums.PhoneType.Mobile,
            ContractEnums.PhoneType.Home => DomainEnums.PhoneType.Home,
            ContractEnums.PhoneType.Work => DomainEnums.PhoneType.Work,
            _ => DomainEnums.PhoneType.Mobile
        };

    private static DomainEnums.AddressType MapAddressType(ContractEnums.AddressType type)
        => type switch
        {
            ContractEnums.AddressType.Home => DomainEnums.AddressType.Home,
            ContractEnums.AddressType.Work => DomainEnums.AddressType.Work,
            ContractEnums.AddressType.Other => DomainEnums.AddressType.Other,
            _ => DomainEnums.AddressType.Other
        };

    private static ContractEnums.CustomerType MapCustomerType(DomainEnums.CustomerType type)
        => type switch
        {
            DomainEnums.CustomerType.Individual => ContractEnums.CustomerType.Individual,
            DomainEnums.CustomerType.Corporate => ContractEnums.CustomerType.Corporate,
            _ => ContractEnums.CustomerType.Individual
        };

    private static ContractEnums.CustomerSegment MapCustomerSegment(DomainEnums.CustomerSegment segment)
        => segment switch
        {
            DomainEnums.CustomerSegment.Standard => ContractEnums.CustomerSegment.Standard,
            DomainEnums.CustomerSegment.Premium => ContractEnums.CustomerSegment.Premium,
            DomainEnums.CustomerSegment.Vip => ContractEnums.CustomerSegment.Vip,
            _ => ContractEnums.CustomerSegment.Standard
        };

    private static ContractEnums.CustomerStatus MapCustomerStatus(DomainEnums.CustomerStatus status)
        => status switch
        {
            DomainEnums.CustomerStatus.Active => ContractEnums.CustomerStatus.Active,
            DomainEnums.CustomerStatus.Suspended => ContractEnums.CustomerStatus.Suspended,
            DomainEnums.CustomerStatus.Closed => ContractEnums.CustomerStatus.Closed,
            _ => ContractEnums.CustomerStatus.Active
        };

    // ✅ NEW: Domain -> Contract Enum dönüşümleri (Detail response için)
    private static ContractEnums.EmailType MapEmailTypeToContract(DomainEnums.EmailType type)
        => type switch
        {
            DomainEnums.EmailType.Personal => ContractEnums.EmailType.Personal,
            DomainEnums.EmailType.Work => ContractEnums.EmailType.Work,
            _ => ContractEnums.EmailType.Personal
        };

    private static ContractEnums.PhoneType MapPhoneTypeToContract(DomainEnums.PhoneType type)
        => type switch
        {
            DomainEnums.PhoneType.Mobile => ContractEnums.PhoneType.Mobile,
            DomainEnums.PhoneType.Home => ContractEnums.PhoneType.Home,
            DomainEnums.PhoneType.Work => ContractEnums.PhoneType.Work,
            _ => ContractEnums.PhoneType.Mobile
        };

    private static ContractEnums.AddressType MapAddressTypeToContract(DomainEnums.AddressType type)
        => type switch
        {
            DomainEnums.AddressType.Home => ContractEnums.AddressType.Home,
            DomainEnums.AddressType.Work => ContractEnums.AddressType.Work,
            DomainEnums.AddressType.Other => ContractEnums.AddressType.Other,
            _ => ContractEnums.AddressType.Other
        };
}