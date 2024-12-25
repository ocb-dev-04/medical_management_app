using Bogus;
using Shared.Domain.Constants;
using FluentValidation.TestHelper;
using Services.Auth.Application.UseCases;

namespace Services.Auth.Application.UnitTests.UseCases.Signin;

public sealed class SigninCommandValidatorTest
{
    private readonly Faker _faker;
    private const string ValidPassword = "Qwerty1234@";

    public SigninCommandValidatorTest()
    {
        _faker = new();
    }

    [Fact]
    public void SigninCommandValidator_Should_AllOk()
    {
        // arrange
        SigninCommandValidator validator = new SigninCommandValidator();
        SigninCommand command = new SigninCommand(_faker.Person.Email, ValidPassword);

        // act
        TestValidationResult<SigninCommand> result = validator.TestValidate(command);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SigninCommandValidator_Should_Email_Invalid()
    {
        // arrange
        SigninCommandValidator validator = new SigninCommandValidator();
        SigninCommand command = new SigninCommand(_faker.Person.Email.Replace('@', '$'), ValidPassword);

        // act
        TestValidationResult<SigninCommand> result = validator.TestValidate(command);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void SigninCommandValidator_Should_Email_CantBeEmpty()
    {
        // arrange
        SigninCommandValidator validator = new SigninCommandValidator();
        SigninCommand command = new SigninCommand(string.Empty, ValidPassword);

        // act
        TestValidationResult<SigninCommand> result = validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.FieldCantBeEmpty));
    }

    [Fact]
    public void SigninCommandValidator_Should_Email_RequiredOrCantBeNull()
    {
        // arrange
        SigninCommandValidator validator = new SigninCommandValidator();
        SigninCommand command = new SigninCommand(null, ValidPassword);

        // act
        TestValidationResult<SigninCommand> result = validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.RequiredField));
    }
    
    [Fact]
    public void SigninCommandValidator_Should_Email_ToLong()
    {
        // arrange
        SigninCommandValidator validator = new SigninCommandValidator();
        SigninCommand command = new SigninCommand($"{_faker.Random.String(100)}@test.com", ValidPassword);

        // act
        TestValidationResult<SigninCommand> result = validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.LongField));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_CantBeEmpty()
    {
        // arrange
        SigninCommandValidator validator = new SigninCommandValidator();
        SigninCommand command = new SigninCommand(_faker.Person.Email, string.Empty);

        // act
        TestValidationResult<SigninCommand> result = validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.FieldCantBeEmpty));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_RequiredOrCantBeNull()
    {
        // arrange
        SigninCommandValidator validator = new SigninCommandValidator();
        SigninCommand command = new SigninCommand(_faker.Person.Email, null);

        // act
        TestValidationResult<SigninCommand> result = validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.RequiredField));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_UppercaseLetterRequired()
    {
        // arrange
        SigninCommandValidator validator = new SigninCommandValidator();
        SigninCommand command = new SigninCommand(_faker.Person.Email, "qwerty1234@");

        // act
        TestValidationResult<SigninCommand> result = validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.UppercaseLetterRequired));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_LowercaseLetterRequired()
    {
        // arrange
        SigninCommandValidator validator = new SigninCommandValidator();
        SigninCommand command = new SigninCommand(_faker.Person.Email, "QWERTY1234@");

        // act
        TestValidationResult<SigninCommand> result = validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.LowercaseLetterRequired));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_DigitRequired()
    {
        // arrange
        SigninCommandValidator validator = new SigninCommandValidator();
        SigninCommand command = new SigninCommand(_faker.Person.Email, "Qwerty@");

        // act
        TestValidationResult<SigninCommand> result = validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.DigitRequired));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_SpecialCharRequired()
    {
        // arrange
        SigninCommandValidator validator = new SigninCommandValidator();
        SigninCommand command = new SigninCommand(_faker.Person.Email, "Qwerty1234");

        // act
        TestValidationResult<SigninCommand> result = validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.DigitRequired));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_ToShort()
    {
        // arrange
        SigninCommandValidator validator = new SigninCommandValidator();
        SigninCommand command = new SigninCommand(_faker.Person.Email, "Qw1@");

        // act
        TestValidationResult<SigninCommand> result = validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.ShortField));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_ToLong()
    {
        // arrange
        SigninCommandValidator validator = new SigninCommandValidator();
        SigninCommand command = new SigninCommand(_faker.Person.Email, $"Qwerty1234@{_faker.Random.String(64)}");

        // act
        TestValidationResult<SigninCommand> result = validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.LongField));
    }
}
