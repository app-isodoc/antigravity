using System.Security.Cryptography;
using System.Text;

namespace Isodoc.Web.Helpers;

public static class PasswordGenerator
{
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string DigitChars = "0123456789";
    private const string SpecialChars = "!@#$%&*";

    /// <summary>
    /// Gera uma senha aleatória segura
    /// </summary>
    /// <param name="length">Tamanho da senha (mínimo 12)</param>
    /// <returns>Senha gerada</returns>
    public static string GenerateSecurePassword(int length = 12)
    {
        if (length < 12)
            length = 12;

        var allChars = LowercaseChars + UppercaseChars + DigitChars + SpecialChars;
        var password = new StringBuilder();

        // Garantir pelo menos um caractere de cada tipo
        password.Append(GetRandomChar(UppercaseChars));
        password.Append(GetRandomChar(LowercaseChars));
        password.Append(GetRandomChar(DigitChars));
        password.Append(GetRandomChar(SpecialChars));

        // Preencher o restante com caracteres aleatórios
        for (int i = 4; i < length; i++)
        {
            password.Append(GetRandomChar(allChars));
        }

        // Embaralhar a senha para não ter padrão previsível
        return Shuffle(password.ToString());
    }

    private static char GetRandomChar(string chars)
    {
        var randomIndex = RandomNumberGenerator.GetInt32(0, chars.Length);
        return chars[randomIndex];
    }

    private static string Shuffle(string input)
    {
        var array = input.ToCharArray();
        int n = array.Length;
        
        for (int i = n - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(0, i + 1);
            // Swap
            var temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
        
        return new string(array);
    }
}
