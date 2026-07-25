using FacilityInspection.Application.Common.Security;
using FluentAssertions;
using Xunit;

namespace FacilityInspection.Tests.Application;

public class PasswordGeneratorTests
{
    [Fact]
    public void Generate_ReturnsPasswordMeetingPolicy()
    {
        var password = PasswordGenerator.Generate();

        password.Length.Should().BeGreaterThanOrEqualTo(12);
        password.Any(char.IsUpper).Should().BeTrue();
        password.Any(char.IsLower).Should().BeTrue();
        password.Any(char.IsDigit).Should().BeTrue();
        password.Any(c => !char.IsLetterOrDigit(c)).Should().BeTrue();
    }

    [Fact]
    public void Generate_ProducesUniqueValues()
    {
        var passwords = Enumerable.Range(0, 20).Select(_ => PasswordGenerator.Generate()).ToList();
        passwords.Distinct().Should().HaveCount(passwords.Count);
    }
}
