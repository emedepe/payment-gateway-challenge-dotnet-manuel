namespace PaymentGateway.Api.Constants;

public static class ValidationMessages
{
    public static class PaymentRequest
    {
        public const string CARD_NUMBER_REQUIRED = "Card number is required.";
        public const string CARD_NUMBER_LENGTH = "Card number must be between 14 and 19 characters long.";
        public const string CARD_NUMBER_DIGITS_ONLY = "Card number must only contain numeric characters.";

        public const string EXPIRY_MONTH_RANGE = "Expiry month must be between 1 and 12.";
        public const string EXPIRY_DATE_IN_FUTURE = "Expiry date must be in the future.";

        public const string CURRENCY_REQUIRED = "Currency is required.";
        public const string CURRENCY_LENGTH = "Currency must be exactly 3 characters.";
        public const string CURRENCY_INVALID = "Currency must be one of the following: {0}.";

        public const string AMOUNT_POSITIVE = "Amount must be a positive integer representing the minor currency unit.";

        public const string CVV_LENGTH = "CVV must be 3-4 characters long.";

        public const string LOG_WARNING = "Validation failed with errors: {0}";
    }
}
