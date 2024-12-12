namespace Shared.Domain.Constants;

public static class ValidationConstants
{
    public static string FieldCantBeEmpty = "fieldCantBeEmpty";
    public static string RequiredField = "requiredField";
    public static string LongField = "longField";
    public static string ShortField = "shortField";
    public static string CantBeNegativeOrZero = "cantBeNegativeOrZero";
    public static string UppercaseLetterRequired = "uppercaseLetterRequired";
    public static string PasswordSpecialCharacterRequired = "passwordSpecialCharacterRequired";

    public static string InvalidEmail = "invalidEmail";

    public static string LowercaseLetterRequired = "lowercaseLetterRequired";
    public static string DigitRequired = "digitRequired";

    public static string NewPasswordCannotBeTheSameAsOldOne = "newPasswordCannotBeTheSameAsOldOne";
    public static string ConfirmPasswordDontMatchWithNewPassword = "confirmPasswordDontMatchWithNewPassword";
}
