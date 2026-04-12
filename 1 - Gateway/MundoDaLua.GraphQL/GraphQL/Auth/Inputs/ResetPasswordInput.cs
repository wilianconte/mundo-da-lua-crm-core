namespace MyCRM.GraphQL.GraphQL.Auth.Inputs;

public record ResetPasswordInput(string Token, string NewPassword, string NewPasswordConfirmation);
