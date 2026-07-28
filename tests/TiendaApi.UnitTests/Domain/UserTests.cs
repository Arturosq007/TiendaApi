using FluentAssertions;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.UnitTests.Domain;

public class UserTests
{
    private static User CreateUser(
        string username = "cajero01",
        UserRole role = UserRole.Cajero)
    {
        return User.Create(
            username: username,
            name: "Juan",
            lastName: "Pérez",
            passwordHash: "hash123",
            role: role);
    }

    // ─── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ShouldCreateActiveUser()
    {
        var user = CreateUser();

        user.IsActive.Should().BeTrue();
        user.Username.Should().Be("cajero01");
        user.RefreshTokenHash.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldStoreLowercaseUsername()
    {
        var user = User.Create(
            username: "ADMIN",
            name: "Admin",
            lastName: "Test",
            passwordHash: "hash",
            role: UserRole.Admin);

        user.Username.Should().Be("admin");
    }

    [Fact]
    public void Create_WithEmptyUsername_ShouldThrow()
    {
        var action = () => User.Create(
            username: "",
            name: "Juan",
            lastName: "Pérez",
            passwordHash: "hash",
            role: UserRole.Cajero);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithShortUsername_ShouldThrow()
    {
        var action = () => User.Create(
            username: "ab",
            name: "Juan",
            lastName: "Pérez",
            passwordHash: "hash",
            role: UserRole.Cajero);

        action.Should().Throw<DomainException>()
            .WithMessage("*3 caracteres*");
    }

    // ─── IsAdmin ─────────────────────────────────────────────────────────────

    [Fact]
    public void IsAdmin_WhenAdmin_ShouldReturnTrue()
    {
        var user = CreateUser(role: UserRole.Admin);

        user.IsAdmin().Should().BeTrue();
    }

    [Fact]
    public void IsAdmin_WhenCajero_ShouldReturnFalse()
    {
        var user = CreateUser(role: UserRole.Cajero);

        user.IsAdmin().Should().BeFalse();
    }

    // ─── RefreshToken ─────────────────────────────────────────────────────────

    [Fact]
    public void SetRefreshToken_WithValidData_ShouldStore()
    {
        var user = CreateUser();
        var expiry = DateTime.UtcNow.AddDays(7);

        user.SetRefreshToken("hash-token", expiry);

        user.RefreshTokenHash.Should().Be("hash-token");
        user.RefreshTokenExpiry.Should().Be(expiry);
    }

    [Fact]
    public void SetRefreshToken_WithPastExpiry_ShouldThrow()
    {
        var user = CreateUser();

        var action = () => user.SetRefreshToken(
            "hash",
            DateTime.UtcNow.AddMinutes(-1)); // ya expiró

        action.Should().Throw<DomainException>()
            .WithMessage("*futura*");
    }

    [Fact]
    public void HasValidRefreshToken_WithValidToken_ShouldReturnTrue()
    {
        var user = CreateUser();
        user.SetRefreshToken("mi-hash", DateTime.UtcNow.AddDays(7));

        user.HasValidRefreshToken("mi-hash").Should().BeTrue();
    }

    [Fact]
    public void HasValidRefreshToken_WithWrongHash_ShouldReturnFalse()
    {
        var user = CreateUser();
        user.SetRefreshToken("mi-hash", DateTime.UtcNow.AddDays(7));

        user.HasValidRefreshToken("otro-hash").Should().BeFalse();
    }

    [Fact]
    public void HasValidRefreshToken_AfterRevoke_ShouldReturnFalse()
    {
        var user = CreateUser();
        user.SetRefreshToken("mi-hash", DateTime.UtcNow.AddDays(7));
        user.RevokeRefreshToken();

        user.HasValidRefreshToken("mi-hash").Should().BeFalse();
    }

    // ─── ChangePassword ───────────────────────────────────────────────────────

    [Fact]
    public void ChangePassword_WithValidHash_ShouldUpdate()
    {
        var user = CreateUser();

        user.ChangePassword("nuevo-hash");

        user.PasswordHash.Should().Be("nuevo-hash");
    }

    [Fact]
    public void ChangePassword_WithEmpty_ShouldThrow()
    {
        var user = CreateUser();

        var action = () => user.ChangePassword(string.Empty);

        action.Should().Throw<DomainException>();
    }

    // ─── FullName ─────────────────────────────────────────────────────────────

    [Fact]
    public void FullName_ShouldCombineNameAndLastName()
    {
        var user = User.Create(
            username: "test",
            name: "Juan",
            lastName: "Pérez",
            passwordHash: "hash",
            role: UserRole.Cajero);

        user.FullName.Should().Be("Juan Pérez");
    }
}