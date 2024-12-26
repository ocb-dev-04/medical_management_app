using Bogus;
using Shared.Domain.Constants;
using FluentValidation.TestHelper;
using Services.Auth.Application.UseCases;

namespace Services.Auth.Application.UnitTests.UseCases.ChangePassword;

public sealed class ChangePasswordCommandValidatorTest
{
    private readonly Faker _faker;
    private readonly ChangePasswordCommandValidator _validator;

    private const string ValidPassword = "Qwerty1234@";
    private const string NewValidPassword = "Qwerty1234.";

    public ChangePasswordCommandValidatorTest()
    {
        _faker = new();
        _validator = new ChangePasswordCommandValidator();
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_AllOk()
    {
        // arrange
        ChangePasswordCommand command = new ChangePasswordCommand(ValidPassword, NewValidPassword, NewValidPassword);

        // act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Password_CantBeEmpty()
    {
        // arrange
        ChangePasswordCommand oldPasswordCommand = new ChangePasswordCommand(string.Empty, NewValidPassword, NewValidPassword);
        ChangePasswordCommand newPasswordCommand = new ChangePasswordCommand(ValidPassword, string.Empty, NewValidPassword);
        ChangePasswordCommand confirmNewPasswordCommand = new ChangePasswordCommand(ValidPassword, NewValidPassword, string.Empty);

        // act
        TestValidationResult<ChangePasswordCommand> oldPasswordResult = _validator.TestValidate(oldPasswordCommand);
        TestValidationResult<ChangePasswordCommand> newPasswordResult = _validator.TestValidate(newPasswordCommand);
        TestValidationResult<ChangePasswordCommand> confirmNewPasswordResult = _validator.TestValidate(confirmNewPasswordCommand);

        // assert
        oldPasswordResult.ShouldHaveValidationErrorFor(f => f.OldPassword);
        oldPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.FieldCantBeEmpty));

        newPasswordResult.ShouldHaveValidationErrorFor(f => f.NewPassword);
        newPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.FieldCantBeEmpty));

        confirmNewPasswordResult.ShouldHaveValidationErrorFor(f => f.ConfirmNewPassword);
        confirmNewPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.FieldCantBeEmpty));
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Password_RequiredOrCantBeNull()
    {
        // arrange
        ChangePasswordCommand oldPasswordCommand = new ChangePasswordCommand(string.Empty, NewValidPassword, NewValidPassword);
        ChangePasswordCommand newPasswordCommand = new ChangePasswordCommand(ValidPassword, string.Empty, NewValidPassword);
        ChangePasswordCommand confirmNewPasswordCommand = new ChangePasswordCommand(ValidPassword, NewValidPassword, string.Empty);

        // act
        TestValidationResult<ChangePasswordCommand> oldPasswordResult = _validator.TestValidate(oldPasswordCommand);
        TestValidationResult<ChangePasswordCommand> newPasswordResult = _validator.TestValidate(newPasswordCommand);
        TestValidationResult<ChangePasswordCommand> confirmNewPasswordResult = _validator.TestValidate(confirmNewPasswordCommand);

        // assert
        oldPasswordResult.ShouldHaveValidationErrorFor(f => f.OldPassword);
        oldPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.RequiredField));

        newPasswordResult.ShouldHaveValidationErrorFor(f => f.NewPassword);
        newPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.RequiredField));

        confirmNewPasswordResult.ShouldHaveValidationErrorFor(f => f.ConfirmNewPassword);
        confirmNewPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.RequiredField));
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Password_UppercaseLetterRequired()
    {
        // arrange
        string upperCaseReuired = "qwerty1234@";
        ChangePasswordCommand oldPasswordCommand = new ChangePasswordCommand(upperCaseReuired, NewValidPassword, NewValidPassword);
        ChangePasswordCommand newPasswordCommand = new ChangePasswordCommand(ValidPassword, upperCaseReuired, NewValidPassword);
        ChangePasswordCommand confirmNewPasswordCommand = new ChangePasswordCommand(ValidPassword, NewValidPassword, upperCaseReuired);

        // act
        TestValidationResult<ChangePasswordCommand> oldPasswordResult = _validator.TestValidate(oldPasswordCommand);
        TestValidationResult<ChangePasswordCommand> newPasswordResult = _validator.TestValidate(newPasswordCommand);
        TestValidationResult<ChangePasswordCommand> confirmNewPasswordResult = _validator.TestValidate(confirmNewPasswordCommand);

        // assert
        oldPasswordResult.ShouldHaveValidationErrorFor(f => f.OldPassword);
        oldPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.UppercaseLetterRequired));

        newPasswordResult.ShouldHaveValidationErrorFor(f => f.NewPassword);
        newPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.UppercaseLetterRequired));

        confirmNewPasswordResult.ShouldHaveValidationErrorFor(f => f.ConfirmNewPassword);
        confirmNewPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.UppercaseLetterRequired));
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Password_LowercaseLetterRequired()
    {
        // arrange
        string lowerCaseRequired = "QWERTY1234@";
        ChangePasswordCommand oldPasswordCommand = new ChangePasswordCommand(lowerCaseRequired, NewValidPassword, NewValidPassword);
        ChangePasswordCommand newPasswordCommand = new ChangePasswordCommand(ValidPassword, lowerCaseRequired, NewValidPassword);
        ChangePasswordCommand confirmNewPasswordCommand = new ChangePasswordCommand(ValidPassword, NewValidPassword, lowerCaseRequired);

        // act
        TestValidationResult<ChangePasswordCommand> oldPasswordResult = _validator.TestValidate(oldPasswordCommand);
        TestValidationResult<ChangePasswordCommand> newPasswordResult = _validator.TestValidate(newPasswordCommand);
        TestValidationResult<ChangePasswordCommand> confirmNewPasswordResult = _validator.TestValidate(confirmNewPasswordCommand);

        // assert
        oldPasswordResult.ShouldHaveValidationErrorFor(f => f.OldPassword);
        oldPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.LowercaseLetterRequired));

        newPasswordResult.ShouldHaveValidationErrorFor(f => f.NewPassword);
        newPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.LowercaseLetterRequired));

        confirmNewPasswordResult.ShouldHaveValidationErrorFor(f => f.ConfirmNewPassword);
        confirmNewPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.LowercaseLetterRequired));
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Password_DigitRequired()
    {
        // arrange
        string digitRequired = "Qwerty@";
        ChangePasswordCommand oldPasswordCommand = new ChangePasswordCommand(digitRequired, NewValidPassword, NewValidPassword);
        ChangePasswordCommand newPasswordCommand = new ChangePasswordCommand(ValidPassword, digitRequired, NewValidPassword);
        ChangePasswordCommand confirmNewPasswordCommand = new ChangePasswordCommand(ValidPassword, NewValidPassword, digitRequired);

        // act
        TestValidationResult<ChangePasswordCommand> oldPasswordResult = _validator.TestValidate(oldPasswordCommand);
        TestValidationResult<ChangePasswordCommand> newPasswordResult = _validator.TestValidate(newPasswordCommand);
        TestValidationResult<ChangePasswordCommand> confirmNewPasswordResult = _validator.TestValidate(confirmNewPasswordCommand);

        // assert
        oldPasswordResult.ShouldHaveValidationErrorFor(f => f.OldPassword);
        oldPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.DigitRequired));

        newPasswordResult.ShouldHaveValidationErrorFor(f => f.NewPassword);
        newPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.DigitRequired));

        confirmNewPasswordResult.ShouldHaveValidationErrorFor(f => f.ConfirmNewPassword);
        confirmNewPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.DigitRequired));
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Password_SpecialCharRequired()
    {
        // arrange
        string specialCharReqquired = "Qwerty1234";
        ChangePasswordCommand oldPasswordCommand = new ChangePasswordCommand(specialCharReqquired, NewValidPassword, NewValidPassword);
        ChangePasswordCommand newPasswordCommand = new ChangePasswordCommand(ValidPassword, specialCharReqquired, NewValidPassword);
        ChangePasswordCommand confirmNewPasswordCommand = new ChangePasswordCommand(ValidPassword, NewValidPassword, specialCharReqquired);

        // act
        TestValidationResult<ChangePasswordCommand> oldPasswordResult = _validator.TestValidate(oldPasswordCommand);
        TestValidationResult<ChangePasswordCommand> newPasswordResult = _validator.TestValidate(newPasswordCommand);
        TestValidationResult<ChangePasswordCommand> confirmNewPasswordResult = _validator.TestValidate(confirmNewPasswordCommand);

        // assert
        oldPasswordResult.ShouldHaveValidationErrorFor(f => f.OldPassword);
        oldPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.PasswordSpecialCharacterRequired));

        newPasswordResult.ShouldHaveValidationErrorFor(f => f.NewPassword);
        newPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.PasswordSpecialCharacterRequired));

        confirmNewPasswordResult.ShouldHaveValidationErrorFor(f => f.ConfirmNewPassword);
        confirmNewPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.PasswordSpecialCharacterRequired));
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Password_ToShort()
    {
        // arrange
        string toShort = "Qw1@";
        ChangePasswordCommand oldPasswordCommand = new ChangePasswordCommand(toShort, NewValidPassword, NewValidPassword);
        ChangePasswordCommand newPasswordCommand = new ChangePasswordCommand(ValidPassword, toShort, NewValidPassword);
        ChangePasswordCommand confirmNewPasswordCommand = new ChangePasswordCommand(ValidPassword, NewValidPassword, toShort);

        // act
        TestValidationResult<ChangePasswordCommand> oldPasswordResult = _validator.TestValidate(oldPasswordCommand);
        TestValidationResult<ChangePasswordCommand> newPasswordResult = _validator.TestValidate(newPasswordCommand);
        TestValidationResult<ChangePasswordCommand> confirmNewPasswordResult = _validator.TestValidate(confirmNewPasswordCommand);

        // assert
        oldPasswordResult.ShouldHaveValidationErrorFor(f => f.OldPassword);
        oldPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.ShortField));

        newPasswordResult.ShouldHaveValidationErrorFor(f => f.NewPassword);
        newPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.ShortField));

        confirmNewPasswordResult.ShouldHaveValidationErrorFor(f => f.ConfirmNewPassword);
        confirmNewPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.ShortField));
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Password_ToLong()
    {
        // arrange
        string toLong = $"Qwerty1234@{_faker.Random.String(64)}";
        ChangePasswordCommand oldPasswordCommand = new ChangePasswordCommand(toLong, NewValidPassword, NewValidPassword);
        ChangePasswordCommand newPasswordCommand = new ChangePasswordCommand(ValidPassword, toLong, NewValidPassword);
        ChangePasswordCommand confirmNewPasswordCommand = new ChangePasswordCommand(ValidPassword, NewValidPassword, toLong);

        // act
        TestValidationResult<ChangePasswordCommand> oldPasswordResult = _validator.TestValidate(oldPasswordCommand);
        TestValidationResult<ChangePasswordCommand> newPasswordResult = _validator.TestValidate(newPasswordCommand);
        TestValidationResult<ChangePasswordCommand> confirmNewPasswordResult = _validator.TestValidate(confirmNewPasswordCommand);

        // assert
        oldPasswordResult.ShouldHaveValidationErrorFor(f => f.OldPassword);
        oldPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.LongField));

        newPasswordResult.ShouldHaveValidationErrorFor(f => f.NewPassword);
        newPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.LongField));

        confirmNewPasswordResult.ShouldHaveValidationErrorFor(f => f.ConfirmNewPassword);
        confirmNewPasswordResult.Errors.Any(a => a.Equals(ValidationConstants.LongField));
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_NewPasswordCannotBeTheSameAsOldOne()
    {
        // arrange
        ChangePasswordCommand command = new ChangePasswordCommand(ValidPassword, ValidPassword, NewValidPassword);

        // act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // assert
        result.ShouldHaveValidationErrorFor(f => f.NewPassword);
        result.Errors.Any(a => a.Equals(ValidationConstants.NewPasswordCannotBeTheSameAsOldOne));
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_ConfirmPasswordDontMatchWithNewPassword()
    {
        // arrange
        ChangePasswordCommand command = new ChangePasswordCommand(ValidPassword, NewValidPassword, $"{NewValidPassword}@");

        // act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // assert
        result.ShouldHaveValidationErrorFor(f => f.ConfirmNewPassword);
        result.Errors.Any(a => a.Equals(ValidationConstants.ConfirmPasswordDontMatchWithNewPassword));
    }
}
