namespace GastroErp.Application.Common.Options;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Provider { get; set; } = "SqlServer";
    public string ConnectionStringName { get; set; } = "DefaultConnection";
}
