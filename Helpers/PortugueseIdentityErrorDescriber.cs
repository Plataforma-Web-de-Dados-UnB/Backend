using Microsoft.AspNetCore.Identity;

namespace api.Helpers
{
    public class PortugueseIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() => new() { Code = nameof(DefaultError), Description = "Ocorreu um erro desconhecido." };
        public override IdentityError ConcurrencyFailure() => new() { Code = nameof(ConcurrencyFailure), Description = "Falha de concorrência. O registro foi modificado por outro usuário." };
        public override IdentityError PasswordMismatch() => new() { Code = nameof(PasswordMismatch), Description = "Senha incorreta." };
        public override IdentityError InvalidToken() => new() { Code = nameof(InvalidToken), Description = "Token inválido." };
        public override IdentityError LoginAlreadyAssociated() => new() { Code = nameof(LoginAlreadyAssociated), Description = "Já existe um usuário com este login." };
        public override IdentityError InvalidUserName(string userName) => new() { Code = nameof(InvalidUserName), Description = $"O nome de usuário '{userName}' é inválido. Deve conter apenas letras ou dígitos." };
        public override IdentityError InvalidEmail(string email) => new() { Code = nameof(InvalidEmail), Description = $"O e-mail '{email}' é inválido." };
        public override IdentityError DuplicateUserName(string userName) => new() { Code = nameof(DuplicateUserName), Description = "Este e-mail já está em uso." };
        public override IdentityError DuplicateEmail(string email) => new() { Code = nameof(DuplicateEmail), Description = "Este e-mail já está em uso." };
        public override IdentityError InvalidRoleName(string role) => new() { Code = nameof(InvalidRoleName), Description = $"A permissão '{role}' é inválida." };
        public override IdentityError DuplicateRoleName(string role) => new() { Code = nameof(DuplicateRoleName), Description = $"A permissão '{role}' já existe." };
        public override IdentityError UserAlreadyHasPassword() => new() { Code = nameof(UserAlreadyHasPassword), Description = "O usuário já possui uma senha definida." };
        public override IdentityError UserLockoutNotEnabled() => new() { Code = nameof(UserLockoutNotEnabled), Description = "O bloqueio de conta não está habilitado para este usuário." };
        public override IdentityError UserAlreadyInRole(string role) => new() { Code = nameof(UserAlreadyInRole), Description = $"O usuário já possui a permissão '{role}'." };
        public override IdentityError UserNotInRole(string role) => new() { Code = nameof(UserNotInRole), Description = $"O usuário não possui a permissão '{role}'." };
        
        // Passwords
        public override IdentityError PasswordTooShort(int length) => new() { Code = nameof(PasswordTooShort), Description = $"A senha deve ter pelo menos {length} caracteres." };
        public override IdentityError PasswordRequiresNonAlphanumeric() => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "A senha deve conter pelo menos um caractere especial (ex: !, @, #, etc.)." };
        public override IdentityError PasswordRequiresDigit() => new() { Code = nameof(PasswordRequiresDigit), Description = "A senha deve conter pelo menos um dígito ('0'-'9')." };
        public override IdentityError PasswordRequiresLower() => new() { Code = nameof(PasswordRequiresLower), Description = "A senha deve conter pelo menos uma letra minúscula ('a'-'z')." };
        public override IdentityError PasswordRequiresUpper() => new() { Code = nameof(PasswordRequiresUpper), Description = "A senha deve conter pelo menos uma letra maiúscula ('A'-'Z')." };
        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"A senha deve conter pelo menos {uniqueChars} caracteres únicos." };
    }
}
