# Rule: Code conventions and patterns

These are corrections and patterns enforced across this project. Apply them to all new and modified code.

---

## English only

All code, comments, identifiers, error messages, and log strings must be in English. No exceptions — this includes:

- Variable and parameter names
- XML/doc comments
- Log message templates (`_logger.LogInformation(...)`)
- Error messages in `Result.Failure(...)` or exception messages
- TODO/FIXME comments

---

## Targeted edits only

Never rewrite a file that was not just created. If a file needs fixing (missing `using`, wrong field name, etc.), make a targeted `Edit`. If fixing a file would require rewriting most of it, ask the user first.

---

## Async patterns

Use `Task.WhenAll` for independent parallel async operations — never `await` inside a loop when the iterations are independent.

```csharp
// WRONG — serial execution
foreach (var item in items)
    await _service.DoAsync(item);

// CORRECT — parallel execution
Task[] tasks = items.Select(item => _service.DoAsync(item)).ToArray();
await Task.WhenAll(tasks);
```

---

## IsNotCommand() in pipeline behaviors

`IsNotCommand()` in `BaseRequestPipelineBehavior` is intentional load-bearing code. Do not remove or "simplify" it. It gates behaviors that must not run for queries.

---

## Stripe graduated tiers

When creating overage prices in Stripe, always use graduated tiers — never flat `per_unit`:

```csharp
new PriceCreateOptions
{
    BillingScheme = "tiered",
    TiersMode = "graduated",
    Recurring = new PriceRecurringOptions
    {
        Interval = lifetimeAsString,
        UsageType = "metered",
        Meter = stripeMeterDecrypted,
    },
    Tiers =
    [
        new PriceTierOptions { UpTo = includedUnits, UnitAmount = 0 },
        new PriceTierOptions { UpTo = PriceTierUpTo.Inf, UnitAmount = overageCents },
    ],
}
```

`PriceTierUpTo.Inf` is the correct Stripe.net v50 value — not the string `"inf"`.

---

## SignalR hub architecture

- Hub class lives in the module's **Features** project (`Module.{X}.Features/Hubs/`)
- Hub client interface lives in the module's **Domain** project (`Module.{X}.Domain/Abstractions/Hubs/`)
- Hub discovery uses `AppDomain.CurrentDomain.GetAssemblies()` — not `typeof(X).Assembly`
- Handlers push to `Clients.Group(userId)` — not `Clients.User()` — to avoid user ID mapping issues
- `OnConnectedAsync` adds the connection to a group keyed by `Context.UserIdentifier!`

---

## DI registration for Stripe services

Register concrete Stripe services as `AddTransient` backed by `IStripeClient`:

```csharp
services.AddTransient(provider
    => new ProductService(provider.GetRequiredService<StripeClient>()));
services.AddTransient(provider
    => new PriceService(provider.GetRequiredService<StripeClient>()));
```

If a handler needs `SubscriptionService` or `SubscriptionItemService`, register them the same way.

---

## Static HttpClient for startup-time services

If a class must make HTTP calls during app startup (before the DI container is fully built), use `static readonly HttpClient`. Do not use `IHttpClientFactory` or `new HttpClient()` per-request.

```csharp
private static readonly HttpClient _httpClient = new();
```

---

## Command/Query instantiation before passing to sender

Never instantiate a command or query inline inside a method call. Always assign it to a named variable first — it improves readability and makes the intent explicit.

```csharp
// WRONG — inline instantiation
await _sender.Send(new SetUserAsBusinessCommand(@event.UserId.ToString()), cancellationToken);

// CORRECT — named variable
SetUserAsBusinessCommand command = new(@event.UserId.ToString());
await _sender.Send(command, cancellationToken);
```

This applies to `_sender.Send(...)`, `_messageBusService.PublishAsync(...)`, and any similar dispatch call.

---

## No third-party names in domain properties

Entity and domain properties must use domain-meaningful names, not the names of third-party services, SDKs, or APIs. The domain should not leak vendor specifics.

```csharp
// WRONG — vendor name in property
public StringObject StripePriceId { get; private set; }
public StringObject StripeMeterId { get; private set; }
public StringObject StripeEventName { get; private set; }
public StringObject PayPalOrderId { get; private set; }

// CORRECT — domain name
public StringObject PriceId { get; private set; }
public StringObject MeterId { get; private set; }
public StringObject MeterEventName { get; private set; }
public StringObject ExternalOrderId { get; private set; }
```

**Exception**: when the vendor name is unavoidable for disambiguation (e.g., `StripeInvoiceId` on an entity that also has an internal `InvoiceId`), prefer a domain-scoped prefix like `PaymentProviderInvoiceId` over a vendor-specific one. Never use vendor names in interface contracts, DTOs, or domain events unless the field literally carries the vendor's own identifier and there is no domain term that fits.

---

## Variable declaration wrapping

When a variable declaration exceeds 120 characters, the assignment must be placed on the next line, indented once. Short declarations stay on a single line.

```csharp
// CORRECT — short declaration, same line
Guid reservationId = request.ReservationId.ExtractRawIdAsUuid();

// CORRECT — long declaration, wrapped
Result<CurrentTokenUserInformation> currentRequestUser
    = _jwtTokenProvider.ReadToken();

Result<Domain.Dtos.ReservationPaymentDto> payment
    = await _paymentReadRepository.GetByReservationIdAsync(reservationId, cancellationToken);

// WRONG — too long on one line
Result<CurrentTokenUserInformation> currentRequestUser = _jwtTokenProvider.ReadToken();
```

---

## Method chaining — one dot per line

Each chained method call or member access must be on its own line, indented once from the root object. This applies everywhere — not just LINQ.

```csharp
// CORRECT — LINQ
var activeUsers = users
    .Where(u => u.IsActive)
    .OrderBy(u => u.CreatedAt)
    .Select(u => u.Email)
    .ToList();

// CORRECT — fluent builders
var policy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(attempt));

// CORRECT — repository call
Result<Domain.Dtos.ReservationPaymentDto> payment
    = await _paymentReadRepository
        .GetByReservationIdAsync(reservationId, cancellationToken);

// WRONG — chain collapsed on one line
var emails = users.Where(u => u.IsActive).OrderBy(u => u.Name).Select(u => u.Email).ToList();
```

**Exception**: a single short member access (no method call chain) may stay on one line.

```csharp
// Acceptable — single access, no chaining
string name = user.Profile.FullName;
```

---

## Guard clause grouping

A guard clause must be placed immediately after the variable it validates — no blank line between them. A blank line goes after the full block (declaration + guard) to separate the next logical unit.

```csharp
// CORRECT — guard attached to its variable, blank line after the block
Result<CurrentTokenUserInformation> currentRequestUser
    = _jwtTokenProvider.ReadToken();
if (currentRequestUser.IsFailure)
    return Result.Failure<ReservationPaymentResponse>(currentRequestUser.Error);

Result<GetCustomerProfileByUserIdBusResponse> customerProfile
    = await _messageBusService.RequestAsync<...>(customerProfileQuery, cancellationToken);
if (customerProfile.IsFailure)
    return Result.Failure<ReservationPaymentResponse>(customerProfile.Error);

// WRONG — blank line between variable and its guard
Result<CurrentTokenUserInformation> currentRequestUser
    = _jwtTokenProvider.ReadToken();

if (currentRequestUser.IsFailure)
    return Result.Failure<ReservationPaymentResponse>(currentRequestUser.Error);
```

The variable and its guard form a single logical unit. Separating them visually breaks that relationship.

---

## One parameter per line

When a method or constructor call has multiple arguments, each argument must be on its own line, indented once. The closing parenthesis goes on its own line aligned with the opening statement.

```csharp
// CORRECT — one argument per line
_handler = new BusinessProfileUpdateCountriesAvailabilityQueueCommandHandler(
    _writeRepository,
    _entitiesEventsManagementProvider,
    _messageBusService,
    NullLogger<BusinessProfileUpdateCountriesAvailabilityQueueCommandHandler>.Instance);

// CORRECT — method call
Result<ReservationPaymentResponse> result = await _mediator.Send(
    command,
    cancellationToken);

// WRONG — all arguments on one line
_handler = new BusinessProfileUpdateCountriesAvailabilityQueueCommandHandler(_writeRepository, _entitiesEventsManagementProvider, _messageBusService, NullLogger<...>.Instance);
```

**Exception**: single-argument calls may stay on one line if the total length is within the 120-character limit.

```csharp
// Acceptable — single short argument
await _sender.Send(command, cancellationToken);
```

---

## Positional arguments over named arguments

Always use positional (implicit) arguments. Never use named parameters unless required to skip optional parameters and reach a specific one.

```csharp
// CORRECT — positional arguments
ReservationUpdatePaymentStatusQueueCommand command = new(
    Guid.NewGuid(),
    ReservationPaymentStatus.Paid);

// WRONG — unnecessary named arguments
ReservationUpdatePaymentStatusQueueCommand command = new(
    Id: Guid.NewGuid(),
    PaymentStatus: ReservationPaymentStatus.Paid);

// CORRECT — named argument required to skip optional parameters
Task<Result<GetItemCollectionByIdsBusResponse>> itemsRelatedTask
    = _messageBusService.RequestAsync<GetItemCollectionByIdsBusQuery, GetItemCollectionByIdsBusResponse>(
        itemBusQuery,
        cancellationToken: cancellationToken);
```

The only acceptable use of named arguments is when an optional parameter must be skipped to reach a later one — name only the parameter being targeted, not the others.
