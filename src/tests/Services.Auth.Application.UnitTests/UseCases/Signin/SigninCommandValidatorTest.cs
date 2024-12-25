using Bogus;
using Shared.Domain.Constants;
using FluentValidation.TestHelper;
using Services.Auth.Application.UseCases;
using System.ComponentModel.DataAnnotations;

namespace Services.Auth.Application.UnitTests.UseCases.Signin;

public sealed class SigninCommandValidatorTest
{
    private readonly Faker _faker;
    private readonly SigninCommandValidator _validator;

    private const string ValidPassword = "Qwerty1234@";

    public SigninCommandValidatorTest()
    {
        _faker = new();
        _validator = new SigninCommandValidator();
    }

    [Fact]
    public void SigninCommandValidator_Should_AllOk()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, ValidPassword);

        // act
        TestValidationResult<SigninCommand> result = _validator.TestValidate(command);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SigninCommandValidator_Should_Email_Invalid()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email.Replace('@', '$'), ValidPassword);

        // act
        TestValidationResult<SigninCommand> result = _validator.TestValidate(command);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void SigninCommandValidator_Should_Email_CantBeEmpty()
    {
        // arrange
        SigninCommand command = new SigninCommand(string.Empty, ValidPassword);

        // act
        TestValidationResult<SigninCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.FieldCantBeEmpty));
    }

    [Fact]
    public void SigninCommandValidator_Should_Email_RequiredOrCantBeNull()
    {
        // arrange
        SigninCommand command = new SigninCommand(null, ValidPassword);

        // act
        TestValidationResult<SigninCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.RequiredField));
    }
    
    [Fact]
    public void SigninCommandValidator_Should_Email_ToLong()
    {
        // arrange
        SigninCommand command = new SigninCommand($"{_faker.Random.String(100)}@test.com", ValidPassword);

        // act
        TestValidationResult<SigninCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.LongField));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_CantBeEmpty()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, string.Empty);

        // act
        TestValidationResult<SigninCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.FieldCantBeEmpty));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_RequiredOrCantBeNull()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, null);

        // act
        TestValidationResult<SigninCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.RequiredField));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_UppercaseLetterRequired()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, "qwerty1234@");

        // act
        TestValidationResult<SigninCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.UppercaseLetterRequired));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_LowercaseLetterRequired()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, "QWERTY1234@");

        // act
        TestValidationResult<SigninCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.LowercaseLetterRequired));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_DigitRequired()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, "Qwerty@");

        // act
        TestValidationResult<SigninCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.DigitRequired));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_SpecialCharRequired()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, "Qwerty1234");

        // act
        TestValidationResult<SigninCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.DigitRequired));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_ToShort()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, "Qw1@");

        // act
        TestValidationResult<SigninCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.ShortField));
    }

    [Fact]
    public void SigninCommandValidator_Should_Password_ToLong()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, $"Qwerty1234@{_faker.Random.String(64)}");

        // act
        TestValidationResult<SigninCommand> result = _validator.TestValidate(command);

        // assert
        result.Errors.Any(a => a.Equals(ValidationConstants.LongField));
    }
}
