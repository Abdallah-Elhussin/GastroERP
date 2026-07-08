using GastroErp.Domain.Entities.HR;
using GastroErp.Domain.Common.Exceptions;

namespace GastroErp.Domain.UnitTests.Hr;

public class EmployeeNameRulesTests
{
    [Theory]
    [InlineData("محمد أحمد العلي")]
    [InlineData("Mohammed Ahmed Al-Ali")]
    public void IsTripleName_accepts_valid_names(string name)
        => Assert.True(EmployeeNameRules.IsTripleName(name));

    [Theory]
    [InlineData("محمد")]
    [InlineData("محمد أحمد")]
    [InlineData("")]
    public void IsTripleName_rejects_invalid_names(string name)
        => Assert.False(EmployeeNameRules.IsTripleName(name));

    [Fact]
    public void Hire_requires_triple_name()
    {
        var ex = Assert.Throws<BusinessException>(() =>
            Employee.Hire(Guid.NewGuid(), Guid.NewGuid(), "EMP-001", "محمد أحمد", null));
        Assert.Equal("Validation.InvalidTripleName", ex.ErrorCode);
    }

    [Fact]
    public void Hire_succeeds_with_triple_name()
    {
        var employee = Employee.Hire(
            Guid.NewGuid(), Guid.NewGuid(), "EMP-001", "محمد أحمد العلي");
        Assert.Equal("محمد أحمد العلي", employee.Name);
    }
}
