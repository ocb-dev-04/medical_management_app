# Rule: Unit test conventions

These rules are derived from corrections made during this project. Apply them every time a test is written or modified.

---

## Logger injection

Never `Substitute.For<ILogger<THandler>>()` when `THandler` is `internal sealed`.
Always use `NullLogger<THandler>.Instance`.

```csharp
// WRONG — throws Castle.DynamicProxy strong-name error at runtime
ILogger<MyHandler> logger = Substitute.For<ILogger<MyHandler>>();

// CORRECT
_handler = new MyHandler(dep, NullLogger<MyHandler>.Instance);
```

`Substitute.For<ILogger<T>>()` is only safe when `T` is `public`. Event handlers are `public sealed`, so they can use either — but `NullLogger` is always preferred since logging is never asserted.

---

## Collection empty checks

Use `.Any()` / `!.Any()`, not `.Count == 0` / `.Count > 0`.

```csharp
// WRONG
if (collection.Count == 0) return;

// CORRECT
if (!collection.Any()) return;
```

---

## Value object construction in tests

Use static factory methods for all value objects. Do NOT use `.As*Object()` extensions except for
`.AsUuidObject()`, which is the canonical way to construct a `UuidObject` from any source:

```csharp
// WRONG — extensions on non-UUID value objects
"some-string".AsStringObject()
42.AsIntegerObject()

// CORRECT
StringObject.Create("some-string")
IntegerObject.Create(42)
UuidObject.New()                   // generate fresh ID
someGuid.AsUuidObject()            // from Guid
someString.AsUuidObject()          // from string (with or without schema prefix — handled internally)
(UShortIntegerObject)(ushort)10    // for UShortIntegerObject
```

`UuidObject.From()` does NOT exist.

---

## Verify field names before writing assertions

Before writing `Arg.Is<SomeCommand>(c => c.FieldName == ...)`, read the record definition to confirm the exact property names. Do not guess from handler code — the command record is the authoritative source.

---

## Stripe service mocking

Stripe services (`ProductService`, `PriceService`, etc.) are concrete classes that accept `IStripeClient` in their constructor. Mock via the interface, not the service class.

```csharp
IStripeClient stripeClient = Substitute.For<IStripeClient>();
ProductService productService = new(stripeClient);
PriceService priceService = new(stripeClient);
```

Configure `RequestAsync<T>` per return type:

```csharp
// StripeList<Price> — for ListAsync calls
stripeClient
    .RequestAsync<StripeList<Price>>(
        Arg.Any<HttpMethod>(), Arg.Any<string>(),
        Arg.Any<BaseOptions>(), Arg.Any<RequestOptions>(),
        Arg.Any<CancellationToken>())
    .Returns(new StripeList<Price> { Data = [] });

// Price — for CreateAsync calls (multiple returns for successive calls)
stripeClient
    .RequestAsync<Price>(...)
    .Returns(new Price { Id = "price_base" }, new Price { Id = "price_overage" });
```

---

## Hub context mocking (SignalR)

Mock the full chain: `IHubContext<THub, TClient>` → `IHubClients<TClient>` → `TClient`.

```csharp
IHubContext<MyHub, IMyClientHub> hubContext = Substitute.For<IHubContext<MyHub, IMyClientHub>>();
IHubClients<IMyClientHub> hubClients = Substitute.For<IHubClients<IMyClientHub>>();
IMyClientHub clientProxy = Substitute.For<IMyClientHub>();

hubContext.Clients.Returns(hubClients);
hubClients.Group(Arg.Any<string>()).Returns(clientProxy);
clientProxy.SomeMethod(Arg.Any<...>()).Returns(Task.CompletedTask);
```

---

## IMessageBusService.PublishAsync overload disambiguation

When asserting on `PublishAsync`, use the specific command type to avoid ambiguity:

```csharp
await _messageBusService.Received(1)
    .PublishAsync(Arg.Is<MyQueueCommand>(c => c.Id == expectedId));
```

`DidNotReceiveWithAnyArgs()` is preferred over `DidNotReceive()` when checking that nothing was published at all.

---

## IMemoryDatabaseService.AddOrUpdateAsync signature

Full signature: `AddOrUpdateAsync<T>(string key, T value, TimeSpan expiry, JsonSerializerSettings? settings = null, CancellationToken cancellationToken = default) where T : class`.

Always use the **3-arg form** in tests (key, value, TimeSpan). The optional params default to `null`/`default` in production code and NSubstitute matches accordingly.

```csharp
// WRONG — CancellationToken lands in JsonSerializerSettings? slot (CS1503)
_memoryDatabaseService
    .AddOrUpdateAsync(Arg.Any<string>(), Arg.Any<T>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
    .Returns(Task.CompletedTask);

// CORRECT — 3-arg form, add where T : class constraint
protected void Set_AddOrUpdateAsync_Success<T>() where T : class
    => _memoryDatabaseService
        .AddOrUpdateAsync(Arg.Any<string>(), Arg.Any<T>(), Arg.Any<TimeSpan>())
        .Returns(Task.CompletedTask);
```

`RemoveAsync(string key, CancellationToken)` does take CancellationToken — keep `Arg.Any<CancellationToken>()` there.

`IMessageBusService.RequestAsync<TQ,TR>(query, bool, TimeSpan, CancellationToken)` takes CancellationToken at position 4 — keep it there too.
