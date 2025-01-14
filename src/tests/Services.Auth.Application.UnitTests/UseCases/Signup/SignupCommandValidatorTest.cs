using Bogus;
using FluentValidation.TestHelper;
using Services.Auth.Application.UseCases;
using Shared.Domain.Constants;

namespace Services.Auth.Application.UnitTests.UseCases.Signup;

public sealed class SignupCommandValidatorTest
{
    private readonly Faker _faker;
    private readonly SignupCommandValidator _validator;

    private const string ValidPassword = "Qwerty1234@";

    public SignupCommandValidatorTest()
    {
        _faker = new();
        _validator = new SignupCommandValidator();
    }

    [Fact]
    public void SignupCommandValidator_Should_AllOk()
    {
        // arrange
        SignupCommand command = new SignupCommand(_faker.Person.Email, ValidPassword);

        // act
        TestValidationResult<SignupCommand> result = _validator.TestValidate(command);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SignupCommandValidator_Should_Email_Invalid()
    {
        // arrange
        SignupCommand command = new SignupCommand(_faker.Person.Email.Replace('@', '$'), ValidPassword);

        // act
        TestValidationResult<SignupCommand> result = _validator.TestValidate(command);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void SignupCommandValidator_Should_Email_CantBeEmpty()
    {
        // arrange
        SignupCommand command = new SignupCommand(string.Empty, ValidPassword);

        // act
        TestValidationResult<SignupCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.FieldCantBeEmpty));
    }

    [Fact]
    public void SignupCommandValidator_Should_Email_RequiredOrCantBeNull()
    {
        // arrange
        SignupCommand command = new SignupCommand(null, ValidPassword);

        // act
        TestValidationResult<SignupCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.RequiredField));
    }

    [Fact]
    public void SignupCommandValidator_Should_Email_ToLong()
    {
        // arrange
        SignupCommand command = new SignupCommand($"{_faker.Random.String(100)}@test.com", ValidPassword);

        // act
        TestValidationResult<SignupCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.LongField));
    }

    [Fact]
    public void SignupCommandValidator_Should_Password_CantBeEmpty()
    {
        // arrange
        SignupCommand command = new SignupCommand(_faker.Person.Email, string.Empty);

        // act
        TestValidationResult<SignupCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.FieldCantBeEmpty));
    }

    [Fact]
    public void SignupCommandValidator_Should_Password_RequiredOrCantBeNull()
    {
        // arrange
        SignupCommand command = new SignupCommand(_faker.Person.Email, null);

        // act
        TestValidationResult<SignupCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.RequiredField));
    }

    [Fact]
    public void SignupCommandValidator_Should_Password_UppercaseLetterRequired()
    {
        // arrange
        SignupCommand command = new SignupCommand(_faker.Person.Email, "qwerty1234@");

        // act
        TestValidationResult<SignupCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.UppercaseLetterRequired));
    }

    [Fact]
    public void SignupCommandValidator_Should_Password_LowercaseLetterRequired()
    {
        // arrange
        SignupCommand command = new SignupCommand(_faker.Person.Email, "QWERTY1234@");

        // act
        TestValidationResult<SignupCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.LowercaseLetterRequired));
    }

    [Fact]
    public void SignupCommandValidator_Should_Password_DigitRequired()
    {
        // arrange
        SignupCommand command = new SignupCommand(_faker.Person.Email, "Qwerty@");

        // act
        TestValidationResult<SignupCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.DigitRequired));
    }

    [Fact]
    public void SignupCommandValidator_Should_Password_SpecialCharRequired()
    {
        // arrange
        SignupCommand command = new SignupCommand(_faker.Person.Email, "Qwerty1234");

        // act
        TestValidationResult<SignupCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.DigitRequired));
    }

    [Fact]
    public void SignupCommandValidator_Should_Password_ToShort()
    {
        // arrange
        SignupCommand command = new SignupCommand(_faker.Person.Email, "Qw1@");

        // act
        TestValidationResult<SignupCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.ShortField));
    }

    [Fact]
    public void SignupCommandValidator_Should_Password_ToLong()
    {
        // arrange
        SignupCommand command = new SignupCommand(_faker.Person.Email, $"Qwerty1234@{_faker.Random.String(64)}");

        // act
        TestValidationResult<SignupCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.LongField));
    }
}
