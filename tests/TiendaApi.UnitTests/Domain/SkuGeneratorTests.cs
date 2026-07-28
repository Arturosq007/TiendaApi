using FluentAssertions;
using TiendaApi.Domain.Helpers;

namespace TiendaApi.UnitTests.Domain;

public class SkuGeneratorTests
{
    [Fact]
    public void Generate_WithValidData_ShouldReturnFormattedSku()
    {
        var sku = SkuGenerator.Generate("Bebidas", "Coca Cola", 1);

        sku.Should().Be("BEB-COC-0001");
    }

    [Fact]
    public void Generate_WithAccentedCategory_ShouldRemoveAccents()
    {
        var sku = SkuGenerator.Generate("Lácteos", "Leche", 5);

        sku.Should().Be("LAC-LEC-0005");
    }

    [Fact]
    public void Generate_WithLargeSequence_ShouldPadCorrectly()
    {
        var sku = SkuGenerator.Generate("Snacks", "Papas", 100);

        sku.Should().Be("SNA-PAP-0100");
    }

    [Fact]
    public void Generate_WithSpecialChars_ShouldRemoveThem()
    {
        var sku = SkuGenerator.Generate("Frutas & Verduras", "Ñame", 1);

        // Solo letras ASCII
        sku.Should().NotContain("&");
        sku.Should().NotContain("Ñ");
    }

    [Theory]
    [InlineData("Bebidas", "Coca Cola", 1, "BEB-COC-0001")]
    [InlineData("Lácteos", "Leche Entera", 5, "LAC-LEC-0005")]
    [InlineData("Limpieza", "Lejía", 12, "LIM-LEJ-0012")]
    [InlineData("Abarrotes", "Arroz", 99, "ABA-ARR-0099")]
    public void Generate_WithVariousInputs_ShouldReturnCorrectSku(
        string category, string product, int sequence, string expected)
    {
        var sku = SkuGenerator.Generate(category, product, sequence);

        sku.Should().Be(expected);
    }
}