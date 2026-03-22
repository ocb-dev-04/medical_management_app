# Feature: UseCase Unit Test Generation for CQRS Modules

## 1. Objective

Generate unit tests for any service module's Application layer UseCases following the established patterns in this codebase. Given a module name, produce a complete test project with `BaseTestSharedConfiguration` and per-UseCase test classes that validate handler behavior through all success and failure paths.

## 2. Requirements

### 2.1 Test Project Structure

- Project naming: `Services.{Module}.Application.UnitTests`
- Location: `src/tests/Services.{Module}.Application.UnitTests/`
- Folder hierarchy mirrors the Application layer's `UseCases/` structure:
  ```
  Services.{Module}.Application.UnitTests/
  ├── BaseTestSharedConfiguration.cs
  ├── UseCases/
  │   ├── {UseCaseName}/
  │   │   ├── {UseCaseName}CommandHandlerTest.cs   (for Commands)
  │   │   ├── {UseCaseName}QueryHandlerTest.cs     (for Queries)
  │   │   └── {UseCaseName}CommandValidatorTest.cs  (if validator exists)
  ```

### 2.2 .csproj Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Bogus" Version="35.6.1" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="FluentAssertions" Version="7.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.0.1">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="relative-path-to-Application.csproj" />
    <ProjectReference Include="relative-path-to-Domain.csproj" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
</Project>
```

### 2.2a Internal Members — Granting Test Project Access

Before building the base class, scan all Domain entity files for `internal` members (methods, constructors, properties) that the test project needs to use.

If any are found, add `InternalsVisibleTo` to the **Domain project's** `.csproj`:

```xml
<ItemGroup>
  <InternalsVisibleTo Include="Services.{Module}.Application.UnitTests" />
</ItemGroup>
```

Alternatively, via `AssemblyInfo.cs` in the Domain project:

```csharp
[assembly: InternalsVisibleTo("Services.{Module}.Application.UnitTests")]
```

**Rules:**
- Add one `InternalsVisibleTo` entry per test project that needs access.
- Do NOT change `internal` members to `public` just to make tests compile — use this mechanism instead.
- If the Domain project already has an `<InternalsVisibleTo>` block, append the new entry rather than creating a duplicate.

### 2.3 Mocking Library: NSubstitute

```csharp
// Creation
_repoMock = Substitute.For<IRepository>();

// Setup — sync return
_repoMock.Method(Arg.Any<T>()).Returns(value);

// Setup — async return
_repoMock.MethodAsync(Arg.Any<T>(), Arg.Any<CancellationToken>()).Returns(value);

// Setup — Task (no return value)
_repoMock.MethodAsync(Arg.Any<T>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

// Verification
await _repoMock.Received(1).MethodAsync(Arg.Is<T>(f => f.Equals(expected)), default);
```

### 2.4 BaseTestSharedConfiguration Pattern

This `abstract class` centralizes all shared state for every test class in the project.

**Structure:**
```csharp
public abstract class BaseTestSharedConfiguration
{
    protected readonly Faker _faker;

    // One field per interface dependency
    protected readonly IRepository _repositoryMock;
    protected readonly IExternalService _externalServiceMock;

    // Pre-built valid entities
    protected readonly Entity _validEntity;
    protected readonly Entity _validEntityWithOtherOwner; // for ownership checks

    protected BaseTestSharedConfiguration()
    {
        _faker = new();

        _repositoryMock = Substitute.For<IRepository>();
        _externalServiceMock = Substitute.For<IExternalService>();

        // Build entities using domain factory methods + Value Objects
        _validEntity = Entity.Create(
            GuidObject.New(),
            StringObject.Create(_faker.Commerce.ProductName()),
            ...);

        _validEntityWithOtherOwner = Entity.Create(
            GuidObject.New(), // different owner
            ...);
    }

    #region Repository

    public void Set_GetById_Success()
        => _repositoryMock.ByIdAsync(
                _validEntity.Id,
                Arg.Any<CancellationToken>())
            .Returns(_validEntity);

    public void Set_GetById_NotFound()
        => _repositoryMock.ByIdAsync(
                Arg.Any<EntityId>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Entity>(EntityErrors.NotFound));

    public void Set_Create_Success()
        => _repositoryMock.CreateAsync(
                Arg.Any<Entity>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

    #endregion
}
```

**Rules:**
- Success `Set_` methods use the specific pre-built entity/id for argument matching
- Failure `Set_` methods use `Arg.Any<T>()` for broad matching
- Methods are organized by `#region` per dependency group
- Each method is a single expression body (no braces unless multiple lines are needed)
- Return `Result.Failure<T>(DomainErrors.Xxx)`, `Result.Success(entity)`, or `Task.CompletedTask` as appropriate

**Constructor responsibilities:**
```
1. Initialize _faker = new()
2. Create all mocks via Substitute.For<T>()
3. Build valid entities using Value Objects (GuidObject.Create/New, StringObject.Create, EmailAddress.Create, etc.)
4. Build alternative entities for edge-case scenarios (e.g., different owner)
```

### 2.5 Handler Test Class Pattern

```csharp
public sealed class {UseCaseName}CommandHandlerTest
    : BaseTestSharedConfiguration
{
    private readonly {UseCaseName}Command _command;
    private readonly {UseCaseName}CommandHandler _handler;

    public {UseCaseName}CommandHandlerTest()
    {
        _command = new(/* use _validEntity fields and _faker data */);

        _handler = new(
            _repositoryMock,      // NSubstitute: no .Object needed
            _externalServiceMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult()
    {
        // arrange
        Set_Dependency1_Success();
        Set_Dependency2_Success();

        // act
        Result<TResponse> result = await _handler.Handle(_command, default);

        // assert
        await _repositoryMock.Received(1)
            .MethodAsync(Arg.Is<T>(f => f.Prop.Equals(_command.Prop)), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailedResult_{Reason}()
    {
        // arrange
        Set_Dependency1_FailureScenario();

        // act
        Result<TResponse> result = await _handler.Handle(_command, default);

        // assert
        await _repositoryMock.Received(1)
            .MethodAsync(Arg.Is<T>(f => f.Equals(_command.Prop)), default);

        result.IsFailure.Should().BeTrue();
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
| Specific error | `result.Error.Should().Be(DomainErrors.SpecificError)` |
| Null value error | `result.Error.Should().Be(Error.NullValue)` |
| Collection has items | `result.Value.Any().Should().BeTrue()` |
| Collection is empty | `result.Value.Any().Should().BeFalse()` |
| Call verified | `await _mock.Received(1).Method(Arg.Is<T>(f => ...), default)` |

### 2.6 Validator Test Class Pattern

Validator tests are **standalone** — they do NOT inherit `BaseTestSharedConfiguration`.

```csharp
public sealed class {Name}CommandValidatorTest
{
    private readonly Faker _faker;
    private readonly {Name}CommandValidator _validator;

    public {Name}CommandValidatorTest()
    {
        _faker = new();
        _validator = new {Name}CommandValidator();
    }

    [Fact]
    public void {Name}CommandValidator_Should_AllOk()
    {
        // arrange
        {Name}Command command = new(/* valid data */);

        // act
        TestValidationResult<{Name}Command> result = _validator.TestValidate(command);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void {Name}CommandValidator_Should_{Field}_{Rule}()
    {
        // arrange
        {Name}Command command = new(/* invalid field */);

        // act
        TestValidationResult<{Name}Command> result = _validator.TestValidate(command);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.{Field});
        result.Errors.Any(a => a.Equals(ValidationConstants.{Rule}));
    }
}
```

**Validation rules to cover per field** (from `ValidationConstants`):
- `FieldCantBeEmpty` — empty string `""`
- `RequiredField` — `null`
- `ShortField` — below minimum length
- `LongField` — above maximum length
- `UppercaseLetterRequired` — password without uppercase
- `LowercaseLetterRequired` — password without lowercase
- `DigitRequired` — password without digit
- `PasswordSpecialCharacterRequired` — password without special char
- `NewPasswordCannotBeTheSameAsOldOne` — cross-field rule
- `ConfirmPasswordDontMatchWithNewPassword` — cross-field rule

### 2.7 Test Coverage Requirements per UseCase

**Command/Query Handlers:**
1. Happy path — all dependencies succeed, verify all calls with `Received(1)`, assert `IsSuccess`
2. One failure test per dependency call in the handler's execution flow
3. Ownership/authorization failures — if handler checks the requesting user owns the resource
4. `NullValue` failures — if handler validates null responses from external services
5. Empty collection — for collection queries, test success with zero items

**Validators:**
1. Happy path — no validation errors
2. One test per field per validation rule

## 3. Constraints

### Technical
- .NET 8.0 · xUnit · FluentAssertions 7.x · Bogus 35.x · NSubstitute 5.x · FluentValidation
- CQRS: handlers implement `IRequestHandler<TRequest, TResponse>` with `Handle(TRequest, CancellationToken)`
- Result pattern: `Result` / `Result<T>` from `Shared.Common.Helper.ErrorsHandler`
- Value Objects: `GuidObject`, `StringObject`, `EmailAddress`, etc. — from `Value.Objects.Helper`
- Domain errors: static fields on `{Entity}Errors` classes (e.g., `DiagnosisErrors.NotFound`)
- Shared errors: `Error.NullValue`, `Error.NotFound(code, msg)`, `Error.Unauthorized()`
- `CancellationToken` is always passed as `default` in test invocations

### Business
- Each module has its own Domain: Entities, Errors, Abstractions (repositories/services), Enums
- Cross-module data uses message queue services (`IMessageQeueServices`) returning `{Entity}QueueResponse`
- Some commands require ownership validation (requesting user must own the resource)

## 4. Thinking Framework 🧠

- **Step 1: Discover dependencies.** Read `Domain/Abstractions/` to identify all repository and service interfaces. Read each handler's constructor to know which interfaces it receives.
- **Step 2: Discover entities and value objects.** Read `Domain/Entities/` to understand factory methods (`Entity.Create(...)`). Note all parameters and their Value Object types.
- **Step 3: Check for `internal` members.** Scan the Domain entity files for any method, constructor, or property marked `internal`. If any exist that the test project needs to invoke, add `InternalsVisibleTo` to the Domain `.csproj` — see Section 2.2a.
- **Step 4: Discover errors.** Read `Domain/Errors/` to find all static error fields. Each error = one failure test case.
- **Step 5: Map handler flow to test cases.** Read each `Handle` method. Each dependency call that can fail = one failure test. All calls succeeding = the success test.
- **Step 6: Build BaseTestSharedConfiguration.** One mock per dependency, pre-built entities, `Set_` methods for every scenario found in steps 1–5.
- **Step 7: Build handler tests.** Constructor builds command/query + handler. One `[Fact]` per execution path.
- **Step 8: Build validator tests (if applicable).** Check if the Command file contains an inline validator class. If yes, create standalone validator test class covering all rules per field.

## 5. Implementation Plan

- **Phase 1 — Scaffold:** Create project folder, `.csproj`, add project references to Application and Domain, add to solution. Scan Domain entities for `internal` members and add `InternalsVisibleTo` to the Domain `.csproj` if needed (see Section 2.2a).
- **Phase 2 — BaseTestSharedConfiguration:** Read all handler constructors, entity factories, and error classes. Write the base class.
- **Phase 3 — Handler Tests:** One file per UseCase handler. Cover all execution paths.
- **Phase 4 — Validator Tests:** One file per validator, standalone class.
- **Phase 5 — Verify:** `dotnet build` then `dotnet test`.

## 6. Edge Cases

- **Entities that need a service to construct** (e.g., `Credential.Create` requires `IHashingService`): pass the mock directly as a constructor argument.
- **Bogus password limitations:** Bogus cannot generate passwords matching complex regex — use hardcoded `const string ValidPassword = "Qwerty1234@"`.
- **Cross-module entities:** Build `{Entity}QueueResponse.Map(...)` using Bogus data inside the base class constructor.
- **Multiple entity variants:** Always create `_validEntity` + `_validEntityWithOtherOwner` (using `GuidObject.New()` for the owner field) to cover ownership validation tests.
- **Collection queries:** Use `Faker<T>.CustomInstantiator(...)` + `.Generate(N)` for non-empty, and `Enumerable.Empty<T>().ToList().AsReadOnly()` for empty.
- **Handlers with no failure path:** Only the success test is needed (e.g., simple enum/static collection queries).
- **Inline mock setup in `[Fact]`:** Acceptable when the setup is unique to a single test and does not belong in a reusable `Set_` method.
