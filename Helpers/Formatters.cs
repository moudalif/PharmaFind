using System.Globalization;
using System.Text;

namespace PharmaFind.Helpers;

public static class AppCulture
{
    /// <summary>Culture d'affichage. fr-FR donne le séparateur de milliers « espace fine » attendu pour le FCFA.</summary>
    public static readonly CultureInfo Fr = new("fr-FR");
}

/// <summary>Prix en franc CFA d'Afrique centrale (XAF), sans décimales.</summary>
public static class PriceFormatter
{
    public static string Format(int amountXaf) => amountXaf.ToString("N0", AppCulture.Fr) + " FCFA";
}

/// <summary>Numéros camerounais : 9 chiffres nationaux, indicatif +237.</summary>
public static class PhoneFormatter
{
    public const string CountryCode = "+237";

    public static string Digits(string? input)
    {
        var digits = new string((input ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.StartsWith("00237")) digits = digits[5..];
        else if (digits.StartsWith("237") && digits.Length == 12) digits = digits[3..];
        return digits;
    }

    /// <summary>Numéro valide : 9 chiffres commençant par 6 (mobile) ou 2 (fixe).</summary>
    public static bool IsValid(string? input)
    {
        var d = Digits(input);
        return d.Length == 9 && (d[0] == '6' || d[0] == '2');
    }

    /// <summary>+237 6 99 12 34 56</summary>
    public static string Format(string? input)
    {
        var d = Digits(input);
        if (d.Length != 9) return input ?? string.Empty;
        return $"{CountryCode} {d[0]} {d[1..3]} {d[3..5]} {d[5..7]} {d[7..9]}";
    }

    public static string ForDialer(string? input) => CountryCode + Digits(input);
}

public static class DateFormatter
{
    public static string Relative(DateTime date)
    {
        var diff = DateTime.Now - date;
        if (diff.TotalMinutes < 1) return "à l'instant";
        if (diff.TotalMinutes < 60) return $"il y a {(int)diff.TotalMinutes} min";
        if (diff.TotalHours < 24 && date.Date == DateTime.Today) return $"il y a {(int)diff.TotalHours} h";
        if (date.Date == DateTime.Today.AddDays(-1)) return $"hier à {date:HH\\:mm}";
        if (diff.TotalDays < 7) return $"il y a {(int)Math.Ceiling(diff.TotalDays)} j";
        return "le " + date.ToString("dd/MM/yyyy", AppCulture.Fr);
    }

    public static string Full(DateTime date) => date.ToString("dd/MM/yyyy 'à' HH:mm", AppCulture.Fr);
}

/// <summary>Recherche tolérante : insensible à la casse et aux accents, avec suggestions pour les fautes de frappe.</summary>
public static class TextSearch
{
    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var decomposed = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static bool Matches(string query, params string[] fields)
    {
        var q = Normalize(query);
        if (q.Length == 0) return true;
        return fields.Any(f => Normalize(f).Contains(q));
    }

    public static int Levenshtein(string a, string b)
    {
        var d = new int[a.Length + 1, b.Length + 1];
        for (var i = 0; i <= a.Length; i++) d[i, 0] = i;
        for (var j = 0; j <= b.Length; j++) d[0, j] = j;
        for (var i = 1; i <= a.Length; i++)
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        return d[a.Length, b.Length];
    }

    /// <summary>Distance entre la requête et le début du mot le plus proche d'un champ.</summary>
    public static int Closeness(string query, string field)
    {
        var q = Normalize(query);
        var best = int.MaxValue;
        foreach (var word in Normalize(field).Split(' ', '/', '-'))
        {
            if (word.Length == 0) continue;
            var prefix = word.Length > q.Length ? word[..q.Length] : word;
            best = Math.Min(best, Levenshtein(q, prefix));
        }
        return best;
    }
}
