# Feature: UseCase Unit Test Generation for CQRS Modules

## 1. Objective

Generate a complete, isolated unit test project for any module's Application layer (Features). Given a module name, produce a `BaseTestSharedConfiguration`, per-UseCase handler test classes, and per-validator test classes that validate all execution paths through success and failure scenarios.

---

## 2. Requirements

- Test project per module: `Services.{Module}.Application.UnitTests`
- Location: `src/tests/Services.{Module}.Application.UnitTests/`
- Folder structure mirrors the `UseCases/` hierarchy of the Features project
- `.csproj` targets `net9.0` and uses central package management (no version numbers)
- `InternalsVisibleTo` must be added to the **Domain** project when entity factory methods or members are `internal`
- One `[Fact]` per distinct execution path per handler — success path always required; each fallible dependency call generates one `[Fact]` per distinct failure variant it can produce; independent failure combinations are NOT tested (see §4.9)
- Validators: one test per field per validation rule; prefer `[Theory] + [InlineData]` for multiple invalid inputs
- All mocks via NSubstitute — no `.Object` suffix needed
- All assertions via FluentAssertions
- Command handlers that write to repositories must verify `CreateAsync`/`UpdateAsync`/`DeleteAsync` with `Received(1)` + `Arg.Is<TEntity>` matchers on meaningful fields
- Cross-service validation calls via `IMessageQeueServices` must be verified with `Received(1)` + `Arg.Is<T>` matchers; both `NotFound` and `NullValue` failure paths are required per call

---

## 3. Constraints

- **Handler interface:** `ICommandHandler<TCmd, TResp>` / `IQueryHandler<TQuery, TResp>` from `CQRS.MediatR.Helper.Abstractions.Messaging`
- **Result pattern:** `Result` / `Result<T>` from `Shared.Common.Helper.ErrorsHandler`
- **Write architecture:** handlers write directly to repositories — `CreateAsync`, `UpdateAsync`, or `DeleteAsync` followed by `CommitAsync()` or `Commit()`; there is no message bus publish step for local persistence
- **Cross-service validation:** handlers call `IMessageQeueServices` methods (e.g., `GetDoctorByIdAsync`, `GetPatientByIdAsync`) to validate entities owned by other modules before performing local writes; these return `Result<TQueueResponse>` and must be mocked as interfaces
- **Search indexing:** some command handlers call `IElasticSearchService<TDto>.AddOrUpdateAsync(...)` as a post-write side effect; must be mocked and verified in success tests; verified with `DidNotReceive()` in failure tests before the write
- **Value Objects:** use `GuidObject.New()`, `GuidObject.Create(string)`, `StringObject.Create(...)`, `BooleanObject.CreateAsTrue()`, `IntegerObject.Create(...)` — never construct raw primitives where Value Objects are expected
- **Strong IDs:** module-specific strong IDs (e.g., `DoctorId`, `DiagnosisId`) may be created via `StrongId.Create(guid)` returning `Result<TId>` — this is a testable failure path if the handler calls it
- **Domain errors:** static fields on `{Entity}Errors` classes (e.g., `CredentialErrors.NotFound`)
- **Shared errors:** `Error.NullValue`, `Error.NotFound(code, msg)`, `Error.Unauthorized()`
- **`CancellationToken`:** always pass `default` in test invocations
- **Central package management:** `Directory.Packages.props` at `src/` manages all versions — `.csproj` uses `<PackageReference Include="..." />` only
- **Exception policy:** handlers do NOT catch exceptions — `ArgumentNullException.ThrowIfNull` guards are constructor-only; all domain failures are Result-based; do not test constructor null guards in handler tests

---

## 4. Best Practices 🏛️

### 4.1 AAA Discipline

Every test has exactly three sections, clearly commented. No logic in Assert. No side effects in Arrange.

```csharp
[Fact]
public async Task Handle_Should_ReturnSuccessResult()
{
    // arrange
    Set_Dependency_Success();

    // act
    Result<TResponse> result = await _handler.Handle(_command, default);

    // assert
    result.IsSuccess.Should().BeTrue();
}
```

### 4.2 Test Isolation

- Each test is completely independent — no shared mutable state between tests
- Mock returns are set up **inside each test's Arrange** (via `Set_` helpers) — never in the constructor
- Constructors only initialize immutable state: mocks, pre-built entities, handler, command/query

### 4.3 Deterministic Data

- Use `new Faker()` for test data generation
- For data that must match across tests (e.g., entity id used in command), use the pre-built entity from the base class — not `Guid.NewGuid()` inline
- Use `Faker<T>.CustomInstantiator(f => Entity.Create(...)).Generate(n)` for collection tests
- Use `const string` for values constrained by complex validation rules (e.g., passwords)

### 4.4 Mock Boundary Rule

Mock **interfaces only**. Never mock:

- Value Objects (`GuidObject`, `StringObject`, etc.)
- DTOs or response records
- Domain entities — construct them with real factory methods (`Entity.Create(...)`)
- Concrete classes — if a handler injects a concrete class instead of an interface, flag it as an architectural issue; it cannot be safely mocked

### 4.5 Naming Convention

Pattern: `Method_Should_ExpectedBehavior_WhenCondition`

```
Handle_Should_ReturnSuccessResult
Handle_Should_ReturnFailedResult_WhenEntityNotFound
Handle_Should_ReturnFailedResult_WhenDoctorNotFound
Handle_Should_ReturnFailedResult_WhenDoctorIsNull
Handle_Should_ReturnFailedResult_WhenIdIsInvalid
Handle_Should_ReturnFailedResult_WhenDoctorIsNotTheOwner
Validate_Should_AllOk
Validate_Should_Fail_Name_WhenEmpty
Validate_Should_Fail_Name_WhenTooShort
```

### 4.6 Verify Interactions, Not Implementation

Use `Received(1)` on the mock interface to assert interactions. Do not assert internal handler state.

```csharp
// CORRECT — verify the repository write
await _repositoryMock.Received(1)
    .CreateAsync(Arg.Is<Entity>(e => e.Name.Value == _command.Name), default);

// CORRECT — verify cross-service call
await _messageQeueServicesMock.Received(1)
    .GetDoctorByIdAsync(Arg.Is<Guid>(id => id == _command.DoctorId), default);

// WRONG — leaks implementation details
_handler._internalField.Should().BeNull();
```

### 4.7 One Logical Concept per Fact

A `[Fact]` may contain multiple `Should()` calls if they assert the same logical outcome (e.g., `IsSuccess` + `Value.Id` of the same result). Never test two independent behaviors in one `[Fact]`.

### 4.8 Exception Handling Strategy

Handlers use the Result pattern for ALL domain-level failures — do not test exception propagation from handler logic.

- Exceptions in handlers signal programming errors (null dependencies), guarded by `ArgumentNullException.ThrowIfNull` in constructors — **not unit tested**
- If a mocked dependency is configured to throw, the exception propagates unhandled — this is correct behavior; do not write tests for it
- Only test `Result`-based failure paths (`.IsFailure == true` scenarios)

```csharp
// DO — test Result failure paths
[Fact]
public async Task Handle_Should_ReturnFailedResult_WhenEntityNotFound()
{
    Set_GetById_NotFound(); // mock returns Result.Failure<T>
    Result<TResponse> result = await _handler.Handle(_command, default);
    result.IsFailure.Should().BeTrue();
}

// DO NOT — do not test exception propagation
// [Fact] public async Task Handle_Should_Throw_WhenRepositoryThrows() { ... }
```

### 4.9 Execution Path Definition and Coverage Rule

**What is an execution path?**

An execution path is a unique sequence of statements from the first line of `Handle` to one `return` statement. Each distinct `return` is a separate path. There are four path categories:

| Category | Description | Test required? |
|---|---|---|
| **Success** | All checkpoints pass; reaches the final `return Map(...)` or `return value` | Always — exactly 1 `[Fact]` |
| **Failure** | An explicit guard or Result check exits early with `Result.Failure<T>` | Yes — 1 `[Fact]` per distinct failure variant |
| **Dependency-driven** | A dependency's outcome changes the error returned (same branch, different payload) | Yes — 1 `[Fact]` per distinct observable outcome |
| **Exception** | Unhandled exception propagates out of the handler | No — not tested in this architecture (see §4.8) |

**Coverage Completeness Rule:**

A handler's test suite is complete when ALL of the following hold:

1. Exactly one success `[Fact]` exists — the path where every checkpoint passes
2. Every explicit `return Result.Failure<T>(...)` in the handler body is reached by at least one `[Fact]`
3. Every dependency returning `Result<T>` has one `[Fact]` per distinct failure it can produce
4. No `[Fact]` tests a path that is unreachable given the handler's control flow

**Dependency Path Awareness:**

Each dependency call that returns `Result<T>` is a branch point. Enumerate its distinct failure outcomes:

| Dependency | Distinct failure variants |
|---|---|
| `IMessageQeueServices.GetXxxAsync` | `NotFound` + `NullValue` → **2 tests** |
| `repository.ByIdAsync` | `NotFound` → **1 test** |
| `StrongId.Create(id)` | Invalid input → **1 test** |
| `repository.ExistAsync` (boolean guard) | Guard condition met → **1 test** |
| `EmailAddress.Create(x)` / Value Object | Invalid format → **1 test** |

Only generate tests for variants that produce **different observable outcomes** in the handler (different `Result.Error` or different code path reached). Two dependency failures that ultimately return the same error propagated unchanged do not need separate tests.

**Implicit Paths:**

| Implicit path | Test it? | Reason |
|---|---|---|
| Dependency returns `Error.NullValue` | Yes — if `IMessageQeueServices` can produce it | It is an explicit, documented failure variant |
| Boolean guard where `true` exits early | Yes — one `[Fact]` for the exiting condition | The success test covers the non-exiting branch |
| `CancellationToken` cancelled mid-call | No | Framework behavior, not a handler responsibility |
| Invalid domain state after `Entity.Create(...)` | No | Domain factory enforces invariants; test the factory separately |
| `null` reference inside a `Result<T>` value | Only if the handler explicitly checks it | Otherwise covered by `Error.NullValue` variant of the producing dependency |

**Path Minimization — do not test failure combinations:**

If C1 and C2 are independent checkpoints, there is no value in a test where both fail simultaneously. Short-circuit logic guarantees C2 is unreachable when C1 fails. The §4.17 algorithm enforces this: the failure test for CK always sets C1…C(K-1) as success, so only one variable changes per test.

```
// CORRECT — independent failures, tested independently
Test A: C1 fails                 (C2 never reached — no arrange needed for C2)
Test B: C1 succeeds, C2 fails    (C2 is now the variable)

// WRONG — combinatorial; adds zero coverage
Test C: C1 fails AND C2 also fails   ← never write this
```

**Quick mental model:** For each line in `Handle`, ask: *"What is called here? Can it fail? What distinct failures does it produce, and does the handler handle them differently?"* Each distinct "yes" is a checkpoint that generates at least one failure test.

See §4.17 for the deterministic algorithm that generates the full list with arrange sections.

### 4.10 Async Behavior Rules

- Always `await` handler calls in tests — never use `.Result` or `.Wait()`
- In NSubstitute setup: `.Returns(value)` works for `Task<Result<T>>` — NSubstitute wraps it automatically; use `.Returns(Task.FromResult(value))` when explicit is clearer
- For void tasks: `.Returns(Task.CompletedTask)`
- For synchronous `Commit()` calls: no `await` on the `Received()` assertion
- xUnit is async-native — never use `Thread.Sleep` or blocking waits

```csharp
// CORRECT
Result<TResponse> result = await _handler.Handle(_command, default);

// WRONG — causes deadlocks in xUnit async context
Result<TResponse> result = _handler.Handle(_command, default).Result;
```

### 4.11 Command vs Query Testing Rules

**Commands must:**
- Verify repository write calls (`Received(1)` on `CreateAsync`/`UpdateAsync`/`DeleteAsync`) in the success test
- Verify with `DidNotReceive()` on write calls in failure tests that exit before the write
- Verify cross-service validation calls (`Received(1)` on `IMessageQeueServices` methods) with matching arguments
- Verify `IElasticSearchService.AddOrUpdateAsync` when present in the success path

**Queries must:**
- Assert returned `result.Value` fields — at least two meaningful identity/distinguishing fields
- NOT assert write-side-effect calls (`Received()` on write repositories)

```csharp
// COMMAND SUCCESS — verify write + cross-service call
await _messageQeueServicesMock.Received(1)
    .GetDoctorByIdAsync(Arg.Is<Guid>(id => id == _command.DoctorId), default);

await _repositoryMock.Received(1)
    .CreateAsync(Arg.Is<Entity>(e => e.Name.Value == _command.Name), default);

// COMMAND FAILURE — verify write was NOT reached
await _repositoryMock.DidNotReceive()
    .CreateAsync(Arg.Any<Entity>(), default);

// QUERY SUCCESS — verify returned data
result.Value.Id.Should().Be(_validEntity.Id.Value);
result.Value.Name.Should().Be(_validEntity.Name.Value);
```

### 4.12 Mapping Validation

When a handler maps an entity or input to a response DTO, assert at least two meaningful fields on the response value in the success test:

```csharp
result.IsSuccess.Should().BeTrue();
result.Value.DoctorId.Should().Be(_command.DoctorId);   // from input
result.Value.Disease.Should().Be(_command.Disease);     // from input
```

Prefer `result.Value.Field.Should().Be(_command.Field)` for command responses and `result.Value.Field.Should().Be(_validEntity.Field.Value)` for query responses. Do not assert framework-generated values (auto-ids, audit timestamps) unless domain-critical.

### 4.13 Test Data Strategy

- **Pre-built entities in base class**: only when used by 2+ test classes in the same module
- **Inline data**: when data is unique to one test class or one `[Fact]` — build it in the test class constructor or directly in the test
- **`Faker<T>.CustomInstantiator`**: for collection scenarios requiring multiple domain entities
- **`const string`**: for fields constrained by complex regex (passwords, codes)
- **Never**: use raw `Guid.NewGuid()` where a Value Object is expected — always use `GuidObject.New()` or domain factory methods

```csharp
// COLLECTION — use Faker<T>
Faker<Diagnosis> diagnosisFaker = new Faker<Diagnosis>()
    .CustomInstantiator(f => Diagnosis.Create(
        GuidObject.Create(_validDoctor.Id.ToString()),
        GuidObject.Create(_validPatient.Id.ToString()),
        StringObject.Create(f.Commerce.ProductName()),
        StringObject.Create(f.Lorem.Paragraph(10)),
        DosageIntervals.EverySixHours.ToTimeSpan()));

IReadOnlyList<Diagnosis> collection = diagnosisFaker.Generate(5);

// EMPTY COLLECTION
IReadOnlyList<Diagnosis> empty = [];
```

### 4.14 What NOT to Test

Skip tests for:

- **Framework behavior**: `ArgumentNullException` from constructor guards, NSubstitute internals
- **Third-party libraries**: FluentValidation's own validation pipeline, EF Core, Bogus
- **Trivial pure returns**: static collection methods with no failure path (e.g., `GetSpecialtyCollection` that returns a hardcoded enum list)
- **Auto-generated audit fields**: `CreatedAt`, `UpdatedAt` timestamps
- **Static `Map()` methods in isolation**: test the handler's output; do not unit test the mapping method itself
- **Handlers with zero failure paths**: only the success `[Fact]` is needed

### 4.15 Scalability of BaseTestSharedConfiguration

- One `BaseTestSharedConfiguration` per module test project
- Add to the base class only when a mock or entity is used by **2 or more** test classes
- If the base class exceeds ~150 lines, split by dependency group using intermediate abstract classes:

```csharp
// Tier 1 — shared Faker and common entities only
public abstract class BaseTestSharedConfiguration
{
    protected readonly Faker _faker;
    protected readonly Entity _validEntity;
    protected BaseTestSharedConfiguration() { ... }
}

// Tier 2 — handlers that also need message queue
public abstract class BaseMessageQueueTestConfiguration
    : BaseTestSharedConfiguration
{
    protected readonly IMessageQeueServices _messageQeueServicesMock;
    protected BaseMessageQueueTestConfiguration() : base() { ... }
}
```

- Never add a `Set_` method to the base class for a dependency that only one handler uses — inline it in the test class instead
- If a mock is only needed by one test class, declare and initialize it inside that class, not in the base

### 4.16 Stability and Maintainability

- Build test entities using only the fields required for the test scenario — do not over-specify
- Prefer matching on meaningful fields (`Arg.Is<T>(e => e.Name.Value == expected)`) rather than exhaustive field-by-field matching on every property
- When domain rules change, tests should only fail if observable behavior changed — avoid brittle assertions on implementation details
- Use `result.Error.Should().Be(EntityErrors.SpecificError)` for precise error matching; avoid matching only on `StatusCode` when multiple distinct errors share the same HTTP status code

### 4.17 Test Enumeration Algorithm

A deterministic process to generate the complete `[Fact]` list for any handler. Apply it to every `Handle` method before writing a single test.

> **Mental model before you start:** Read each line of `Handle` and ask *"What is called here? Can it return a failure? What distinct failures does it produce? Does this handler treat them differently?"* Each distinct answer is a checkpoint (C). Lines that cannot fail are not checkpoints.

**Step 1 — Extract checkpoints (C1…CN)**

Read the `Handle` body top to bottom. Each fallible operation is a checkpoint:

| Checkpoint type | Code pattern | Failure variants generated |
|---|---|---|
| Cross-service call | `IMessageQeueServices.GetXxxAsync` + `if (result.IsFailure)` | `NotFound` **+** `NullValue` → **2 tests** |
| Repository read | `repository.ByIdAsync` + `if (found.IsFailure)` | `NotFound` → **1 test** |
| Boolean guard | `if (exist) return Failure` | Guard triggered → **1 test** |
| Strong ID creation | `StrongId.Create(id)` + `if (id.IsFailure)` | Invalid value → **1 test** |
| Value Object creation | `EmailAddress.Create(x)` + `if (result.IsFailure)` | Invalid format → **1 test** |
| Ownership check | `if (entity.OwnerId != callerId)` | Mismatch → **1 test** |

Write operations (`CreateAsync`, `CommitAsync`, `Commit`, `AddOrUpdateAsync`) are **not** checkpoints — they appear only in assertions.

**Step 2 — Generate the test list**

```
SUCCESS test (always exactly 1):
  name:    Handle_Should_ReturnSuccessResult
  arrange: Set_C1_Success() … Set_CN_Success() + Set_Write_Success() if needed
  assert:  result.IsSuccess + mapping fields + Received(1) on every write op

FAILURE tests (one block per checkpoint):
  For K = 1 to N, for each failure variant V of CK:
    name:    Handle_Should_ReturnFailedResult_When{CK_description}
    arrange: Set_C1_Success() … Set_C(K-1)_Success()   ← all prior checkpoints succeed
             Set_CK_Failure_V()                          ← this checkpoint fails
    assert:  result.IsFailure
             result.Error match (specific error or StatusCode)
             DidNotReceive() on every write op that comes AFTER CK in the handler body
```

**Step 3 — Worked example (`CreateDiagnosisCommandHandler`)**

```
Handler body:
  C1: GetDoctorByIdAsync   (cross-service) → 2 failure tests
  C2: GetPatientByIdAsync  (cross-service) → 2 failure tests
  W:  CreateAsync + Commit (write op)      → verified in asserts only

Generated test list (5 total):

 1. Handle_Should_ReturnSuccessResult
    arrange: Set_DoctorMQ_Success(), Set_PatientMQ_Success()
    assert:  Received(1).CreateAsync(...), Received(1).Commit()
             result.IsSuccess, result.Value.DoctorId, result.Value.Disease

 2. Handle_Should_ReturnFailedResult_WhenDoctorNotFound       ← C1 variant 1
    arrange: Set_DoctorMQ_NotFoundFailure()
    assert:  result.IsFailure, Status404
             DidNotReceive().CreateAsync(...)

 3. Handle_Should_ReturnFailedResult_WhenDoctorIsNull         ← C1 variant 2
    arrange: Set_DoctorMQ_NullValueFailure()
    assert:  result.IsFailure, Error.NullValue
             DidNotReceive().CreateAsync(...)

 4. Handle_Should_ReturnFailedResult_WhenPatientNotFound      ← C2 variant 1
    arrange: Set_DoctorMQ_Success(), Set_PatientMQ_NotFoundFailure()
    assert:  result.IsFailure, Status404
             DidNotReceive().CreateAsync(...)

 5. Handle_Should_ReturnFailedResult_WhenPatientIsNull        ← C2 variant 2
    arrange: Set_DoctorMQ_Success(), Set_PatientMQ_NullValueFailure()
    assert:  result.IsFailure, Error.NullValue
             DidNotReceive().CreateAsync(...)
```

This output matches `CreateDiagnosisCommandHandlerTest.cs` exactly. Apply the same algorithm to any handler to get its complete test list before writing any code.

---

## 5. Patterns Reference

### 5.1 .csproj Template

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Bogus" />
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="NSubstitute" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="relative-path-to-Features.csproj" />
    <ProjectReference Include="relative-path-to-Domain.csproj" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
</Project>
```

### 5.1a InternalsVisibleTo (Domain .csproj)

Add when Domain entity factory methods or members are `internal`:

```xml
<ItemGroup>
  <InternalsVisibleTo Include="Services.{Module}.Application.UnitTests" />
</ItemGroup>
```

Rule: do NOT change `internal` to `public` — use this mechanism instead.

### 5.2 BaseTestSharedConfiguration

```csharp
public abstract class BaseTestSharedConfiguration
{
    protected readonly Faker _faker;

    // Mocks — one per interface dependency shared by 2+ test classes
    protected readonly IEntityRepository _repositoryMock;
    protected readonly IMessageQeueServices _messageQeueServicesMock;
    protected readonly IHttpRequestProvider _httpRequestProviderMock;

    // Pre-built entities (used by 2+ test classes)
    protected readonly Entity _validEntity;
    protected readonly Entity _validEntityWithOtherOwner;

    // Pre-built cross-service responses
    protected readonly DoctorQueueResponse _validDoctor;
    protected readonly PatientQueueResponse _validPatient;

    protected BaseTestSharedConfiguration()
    {
        _faker = new();

        _repositoryMock = Substitute.For<IEntityRepository>();
        _messageQeueServicesMock = Substitute.For<IMessageQeueServices>();
        _httpRequestProviderMock = Substitute.For<IHttpRequestProvider>();

        // Build using domain factory + Value Objects — never raw primitives
        _validDoctor = DoctorQueueResponse.Map(
            Guid.NewGuid(),
            _faker.Person.FullName,
            _faker.Internet.UserName(),
            _faker.Random.Number(60),
            DateTimeOffset.UtcNow);

        _validPatient = PatientQueueResponse.Map(
            Guid.NewGuid(),
            _faker.Person.FullName,
            _faker.Random.Number(80),
            DateTimeOffset.UtcNow);

        _validEntity = Entity.Create(
            GuidObject.Create(_validDoctor.Id.ToString()),
            StringObject.Create(_faker.Commerce.ProductName()));

        _validEntityWithOtherOwner = Entity.Create(
            GuidObject.New(), // different owner id
            StringObject.Create(_faker.Commerce.ProductName()));
    }

    #region Repository

    public void Set_GetById_Success()
        => _repositoryMock
            .ByIdAsync(_validEntity.Id, Arg.Any<CancellationToken>())
            .Returns(_validEntity);

    public void Set_GetById_NotFound()
        => _repositoryMock
            .ByIdAsync(Arg.Any<GuidObject>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Entity>(EntityErrors.NotFound));

    public void Set_CreateAsync_Success()
        => _repositoryMock
            .CreateAsync(Arg.Any<Entity>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

    #endregion

    #region MessageQeueServices

    public void Set_DoctorMessageQueue_Success()
        => _messageQeueServicesMock
            .GetDoctorByIdAsync(_validDoctor.Id, Arg.Any<CancellationToken>())
            .Returns(_validDoctor);

    public void Set_DoctorMessageQueue_NotFoundFailure()
        => _messageQeueServicesMock
            .GetDoctorByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DoctorQueueResponse>(
                Error.NotFound("doctorNotFound", "The doctor was not found")));

    public void Set_DoctorMessageQueue_NullValueFailure()
        => _messageQeueServicesMock
            .GetDoctorByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DoctorQueueResponse>(Error.NullValue));

    #endregion

    #region HttpRequestProvider

    public void Set_GetContextCurrentUser_Success(CurrentRequestUser user)
        => _httpRequestProviderMock
            .GetContextCurrentUser()
            .Returns(Result.Success(user));

    public void Set_GetContextCurrentUser_UnauthorizedFailure()
        => _httpRequestProviderMock
            .GetContextCurrentUser()
            .Returns(Result.Failure<CurrentRequestUser>(Error.Unauthorized()));

    #endregion
}
```

**Rules:**

- Success `Set_` methods match on the specific pre-built entity/id
- Failure `Set_` methods use `Arg.Any<T>()` for broad matching
- Methods are single expression bodies where possible
- Organized by `#region` per dependency group
- Add `Set_CreateAsync_Success()` / `Set_UpdateAsync_Success()` / `Set_DeleteAsync_Success()` for write operations as needed

### 5.3 Handler Test Class

**Command handler (direct write + cross-service validation):**

```csharp
public sealed class CreateEntityCommandHandlerTest
    : BaseTestSharedConfiguration
{
    private readonly CreateEntityCommand _command;
    private readonly CreateEntityCommandHandler _handler;

    public CreateEntityCommandHandlerTest()
    {
        _command = new(
            _validDoctor.Id,
            _validPatient.Id,
            _faker.Commerce.ProductName(),
            _faker.Lorem.Paragraph(5));

        _handler = new(
            _repositoryMock,
            _messageQeueServicesMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult()
    {
        // arrange
        Set_DoctorMessageQueue_Success();
        Set_PatientMessageQueue_Success();
        Set_CreateAsync_Success();

        // act
        Result<EntityResponse> result = await _handler.Handle(_command, default);

        // assert
        await _messageQeueServicesMock.Received(1)
            .GetDoctorByIdAsync(Arg.Is<Guid>(id => id == _command.DoctorId), default);

        await _messageQeueServicesMock.Received(1)
            .GetPatientByIdAsync(Arg.Is<Guid>(id => id == _command.PatientId), default);

        await _repositoryMock.Received(1)
            .CreateAsync(Arg.Is<Entity>(e =>
                e.DoctorId.Value == _command.DoctorId &&
                e.Name.Value == _command.Name), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.DoctorId.Should().Be(_command.DoctorId);
        result.Value.Name.Should().Be(_command.Name);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailedResult_WhenDoctorNotFound()
    {
        // arrange
        Set_DoctorMessageQueue_NotFoundFailure();

        // act
        Result<EntityResponse> result = await _handler.Handle(_command, default);

        // assert
        await _messageQeueServicesMock.Received(1)
            .GetDoctorByIdAsync(Arg.Is<Guid>(id => id == _command.DoctorId), default);

        await _repositoryMock.DidNotReceive()
            .CreateAsync(Arg.Any<Entity>(), default);

        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailedResult_WhenDoctorIsNull()
    {
        // arrange
        Set_DoctorMessageQueue_NullValueFailure();

        // act
        Result<EntityResponse> result = await _handler.Handle(_command, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NullValue);
    }
}
```

**Query handler:**

```csharp
public sealed class GetEntityByIdQueryHandlerTest
    : BaseTestSharedConfiguration
{
    private readonly GetEntityByIdQuery _query;
    private readonly GetEntityByIdQueryHandler _handler;

    public GetEntityByIdQueryHandlerTest()
    {
        _query = new(_validEntity.Id.Value);
        _handler = new(_repositoryMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult()
    {
        // arrange
        Set_GetById_Success();

        // act
        Result<EntityResponse> result = await _handler.Handle(_query, default);

        // assert
        await _repositoryMock.Received(1)
            .ByIdAsync(Arg.Is<GuidObject>(id => id.Value == _query.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(_validEntity.Id.Value);
        result.Value.Name.Should().Be(_validEntity.Name.Value);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailedResult_WhenEntityNotFound()
    {
        // arrange
        Set_GetById_NotFound();

        // act
        Result<EntityResponse> result = await _handler.Handle(_query, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EntityErrors.NotFound);
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
```

**Assertion patterns:**

| Scenario | Assertion |
|---|---|
| Success | `result.IsSuccess.Should().BeTrue()` |
| Failure | `result.IsFailure.Should().BeTrue()` |
| HTTP status | `result.Error.StatusCode.Should().Be(StatusCodes.StatusXxx)` |
| Specific error | `result.Error.Should().Be(EntityErrors.SpecificError)` |
| Null value error | `result.Error.Should().Be(Error.NullValue)` |
| Collection has items | `result.Value.Any().Should().BeTrue()` |
| Collection is empty | `result.Value.Any().Should().BeFalse()` |
| Response field from input | `result.Value.Field.Should().Be(_command.Field)` |
| Response field from entity | `result.Value.Field.Should().Be(_validEntity.Field.Value)` |
| Async call verified | `await _mock.Received(1).MethodAsync(Arg.Is<T>(f => f.Prop == expected), default)` |
| Async call not made | `await _mock.DidNotReceive().MethodAsync(Arg.Any<T>(), default)` |
| Sync call verified | `_mock.Received(1).Method(Arg.Any<T>())` |

### 5.4 Validator Test Class

Validator tests are **standalone** — they do NOT inherit `BaseTestSharedConfiguration`.

```csharp
public sealed class CreateEntityCommandValidatorTest
{
    private readonly Faker _faker;
    private readonly CreateEntityCommandValidator _validator;

    public CreateEntityCommandValidatorTest()
    {
        _faker = new();
        _validator = new CreateEntityCommandValidator();
    }

    [Fact]
    public void Validate_Should_AllOk()
    {
        // arrange
        CreateEntityCommand command = new(_faker.Commerce.ProductName(), Guid.NewGuid());

        // act
        TestValidationResult<CreateEntityCommand> result = _validator.TestValidate(command);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")] // below minimum length
    public void Validate_Should_Fail_Name_WhenInvalid(string? name)
    {
        // arrange
        CreateEntityCommand command = new(name, Guid.NewGuid());

        // act
        TestValidationResult<CreateEntityCommand> result = _validator.TestValidate(command);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
```

**Validation rules to cover per field** (from `ValidationConstants`):

- `FieldCantBeEmpty` — `""`
- `RequiredField` — `null`
- `ShortField` — below minimum length
- `LongField` — above maximum length
- `UppercaseLetterRequired`, `LowercaseLetterRequired`, `DigitRequired`, `PasswordSpecialCharacterRequired` — password rules
- `NewPasswordCannotBeTheSameAsOldOne`, `ConfirmPasswordDontMatchWithNewPassword` — cross-field rules

### 5.5 NSubstitute Quick Reference

```csharp
// Create mock
_mock = Substitute.For<IInterface>();

// Setup sync return
_mock.Method(Arg.Any<T>()).Returns(value);

// Setup async return (NSubstitute wraps Task automatically)
_mock.MethodAsync(Arg.Any<T>(), Arg.Any<CancellationToken>()).Returns(value);

// Setup async return (explicit Task)
_mock.MethodAsync(Arg.Any<T>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(value));

// Setup Result failure return
_mock.MethodAsync(Arg.Any<T>(), Arg.Any<CancellationToken>())
    .Returns(Result.Failure<Dto>(EntityErrors.NotFound));

// Setup void Task
_mock.MethodAsync(Arg.Any<T>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

// Verify async called once with condition
await _mock.Received(1).MethodAsync(Arg.Is<T>(x => x.Id == expected), default);

// Verify async never called
await _mock.DidNotReceive().MethodAsync(Arg.Any<T>(), default);

// Verify sync called once
_mock.Received(1).Method(Arg.Any<T>());
```

### 5.6 Cross-Service Validation Pattern

When a handler calls `IMessageQeueServices` to validate entities from other modules, always test three scenarios per call: success, `NotFound`, and `NullValue`.

```csharp
// Base class — both failure variants required
public void Set_DoctorMessageQueue_NotFoundFailure()
    => _messageQeueServicesMock
        .GetDoctorByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
        .Returns(Result.Failure<DoctorQueueResponse>(
            Error.NotFound("doctorNotFound", "The doctor was not found")));

public void Set_DoctorMessageQueue_NullValueFailure()
    => _messageQeueServicesMock
        .GetDoctorByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
        .Returns(Result.Failure<DoctorQueueResponse>(Error.NullValue));

// Test class — both [Fact]s required for each cross-service call
[Fact]
public async Task Handle_Should_ReturnFailedResult_WhenDoctorNotFound()
{
    Set_DoctorMessageQueue_NotFoundFailure();
    Result<TResponse> result = await _handler.Handle(_command, default);
    await _messageQeueServicesMock.Received(1)
        .GetDoctorByIdAsync(Arg.Is<Guid>(id => id == _command.DoctorId), default);
    result.IsFailure.Should().BeTrue();
    result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
}

[Fact]
public async Task Handle_Should_ReturnFailedResult_WhenDoctorIsNull()
{
    Set_DoctorMessageQueue_NullValueFailure();
    Result<TResponse> result = await _handler.Handle(_command, default);
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(Error.NullValue);
}
```

If a handler validates two cross-service entities sequentially (e.g., doctor then patient), the second entity's failure tests must also set up the first call as a success.

### 5.7 ElasticSearch Side Effect Pattern

When a command handler calls `IElasticSearchService<TDto>.AddOrUpdateAsync(...)` after the repository write:

```csharp
// Base class mock (add only if 2+ handlers in the module use it)
protected readonly IElasticSearchService<EntityDto> _elasticSearchServiceMock;

// Setup
public void Set_ElasticSearch_AddOrUpdate_Success()
    => _elasticSearchServiceMock
        .AddOrUpdateAsync(
            Arg.Any<string>(),
            Arg.Any<EntityDto>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
        .Returns(Task.CompletedTask);

// Assert in success test — verify the DTO fields
await _elasticSearchServiceMock.Received(1)
    .AddOrUpdateAsync(
        Arg.Any<string>(),
        Arg.Is<EntityDto>(dto => dto.Name.Value == _command.Name),
        Arg.Any<string>(),
        default);

// Assert NOT called in failure tests that exit before the write
await _elasticSearchServiceMock.DidNotReceive()
    .AddOrUpdateAsync(Arg.Any<string>(), Arg.Any<EntityDto>(), Arg.Any<string>(), default);
```

---

## 6. Thinking Framework 🧠

- **Step 1 — Dependencies:** Read the handler constructor — identify every injected type → one mock per **interface** in `BaseTestSharedConfiguration`; flag any concrete class injection (e.g., `MessageQeueServices` without interface) as untestable via mocking
- **Step 2 — Entity factory:** Read `Domain/Entities/` to find `Create(...)` signature → build `_validEntity` and any cross-service responses using exact Value Object types (`GuidObject`, `StringObject`, `IntegerObject`, etc.)
- **Step 3 — Internal access:** Scan Domain entity files for `internal` members → if any are needed by tests, add `InternalsVisibleTo` to Domain `.csproj`
- **Step 4 — Error map:** Read `Domain/Errors/` → each error = one `Set_Xxx_Failure()` method + one `[Fact]`
- **Step 5 — Handler flow:** Apply the §4.17 Test Enumeration Algorithm — extract all checkpoints in order, then generate the success test and one failure test per checkpoint variant; the arrange of each failure test sets all prior checkpoints as success
- **Step 6 — Strong ID check:** Does the handler call `StrongId.Create(request.Id)`? → that call returns `Result<TId>`; add a `[Fact]` for invalid ID failure
- **Step 7 — Write operations:** Does the handler call `CreateAsync`/`UpdateAsync`/`DeleteAsync`? → verify with `Received(1)` + `Arg.Is<TEntity>` on meaningful fields in the success test; verify with `DidNotReceive()` in failure tests that exit before the write; check whether `Commit()` is sync or `CommitAsync()` is async
- **Step 8 — Cross-service check:** Does the handler call `IMessageQeueServices.GetXxx`? → add `NotFound` failure `[Fact]` + `NullValue` failure `[Fact]` per call; set up prior calls as success in sequential-dependency failure tests; verify with `Received(1)` in success and in relevant failure tests
- **Step 9 — Search indexing:** Does the handler call `IElasticSearchService<T>.AddOrUpdateAsync`? → mock it, verify `Received(1)` in the success test and `DidNotReceive()` in failure tests before the write
- **Step 10 — Ownership check:** Does the handler compare a caller's id against the entity's owner field? → create `_validEntityWithOtherOwner` using a different `GuidObject.New()` and add an unauthorized/bad-request failure `[Fact]`
- **Step 11 — Validators:** Does the Command file contain an inline `*CommandValidator`? → create standalone validator test class with `[Theory]` for each field rule

---

## 7. Implementation Plan

**Phase 1 — Scaffold**

- Create `src/tests/Services.{Module}.Application.UnitTests/`
- Create `.csproj` (central packages, project refs to Features + Domain)
- Add project to solution via `dotnet sln add`
- If Domain has internal members needed, add `InternalsVisibleTo` to Domain `.csproj`

**Phase 2 — BaseTestSharedConfiguration**

- Read all handler constructors, entity factories, and error classes
- Identify mocks and entities needed by 2+ test classes → add only those to base class
- Write abstract base class with mocks, pre-built entities, `Set_` methods organized by `#region`

**Phase 3 — Handler Tests**

- One file per UseCase handler under mirrored `UseCases/{UseCaseName}/` folder
- Cover all execution paths per §4.9 (success + one `[Fact]` per failure)
- Commands: verify writes + cross-service calls; Queries: verify returned data fields

**Phase 4 — Validator Tests**

- One file per UseCase that has an inline `*CommandValidator`
- Standalone class, `[Theory]` per field rule

**Phase 5 — Verify**

- `dotnet build src/tests/Services.{Module}.Application.UnitTests`
- `dotnet test src/tests/Services.{Module}.Application.UnitTests`
- All tests green; zero skipped

---

## 8. Edge Cases

- **Entity constructor is `internal`:** `Entity.Create(...)` may call `new Entity(...)` internally. If the test project cannot resolve it, add `InternalsVisibleTo` — do not make `internal` → `public`
- **Password fields:** Bogus cannot reliably generate strings matching complex regex — use `const string ValidPassword = "Qwerty1234@"` hardcoded in the validator test and base class
- **Cross-module queue responses:** Build `TQueueResponse` using its static `Map(...)` method with Bogus data inside the base constructor; mock `IMessageQeueServices` to return them
- **Collection queries:** Use `Faker<T>.CustomInstantiator(f => Entity.Create(...)).Generate(n)` for non-empty; `IReadOnlyList<T> empty = []` for empty; note these repository methods return `IReadOnlyCollection<T>` directly, not `Result<T>`
- **Handlers with no failure path:** Only the success `[Fact]` needed (e.g., pure static collection queries)
- **Inline mock setup in `[Fact]`:** Acceptable when setup is unique to one test and does not belong in a reusable `Set_` method
- **`IClockProvider`:** Mock and use `Set_Clock_Returns(DateTimeOffset fixedTime)` so time-sensitive domain logic is deterministic
- **Ownership validation:** Create `_validEntityWithOtherOwner` using a different owner `GuidObject.New()`; stub `IHttpRequestProvider.GetContextCurrentUser()` to return the mismatched user id; assert `result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest)` or `Status401Unauthorized` depending on the domain rule
- **Strong ID creation failure:** If the handler calls `DoctorId.Create(request.Id)` — which returns `Result<TId>` — add a `[Fact]` that passes an invalid GUID string to hit this failure path
- **Sync `Commit()` vs async `CommitAsync()`:** Some repositories expose sync `Commit()` — verify with `_repositoryMock.Received(1).Commit()` (no `await`); check the actual interface signature before writing the assertion
- **Concrete class injection:** Some handlers inject `MessageQeueServices` as a concrete class rather than `IMessageQeueServices` — this cannot be mocked with NSubstitute; flag it and test only what can be isolated; prefer refactoring to inject the interface
- **Sequential cross-service validation:** When a handler validates doctor then patient, the patient failure tests must call `Set_DoctorMessageQueue_Success()` in their arrange so the handler reaches the patient check
