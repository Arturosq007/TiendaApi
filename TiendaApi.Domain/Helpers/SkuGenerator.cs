namespace TiendaApi.Domain.Helpers;
public static class SkuGenerator
{
    public static string Generate(string categoryName, string productName, int sequence)
    {
        var category = Sanitize(categoryName, 3);
        var product = Sanitize(productName, 3);
        var number = sequence.ToString().PadLeft(4, '0');

        return $"{category}-{product}-{number}";
    }

    private static string Sanitize(string input, int length)
    {
        return input
            .ToUpper()
            .Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => c < 128 && char.IsLetter(c))
            .Take(length)
            .Aggregate(string.Empty, (acc, c) => acc + c);
    }
}