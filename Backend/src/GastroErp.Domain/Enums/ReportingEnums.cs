namespace GastroErp.Domain.Enums;

public enum ReportModule
{
    General = 1,
    Sales = 2,
    Kitchen = 3,
    Delivery = 4,
    Inventory = 5,
    Finance = 6,
    HR = 7,
    CRM = 8,
    Purchasing = 9
}

public enum ReportCategory
{
    Operational = 1,
    Financial = 2,
    Analytical = 3,
    Compliance = 4,
    Executive = 5
}

public enum ReportStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

public enum WidgetType
{
    KpiCard = 1,
    LineChart = 2,
    BarChart = 3,
    PieChart = 4,
    AreaChart = 5,
    DonutChart = 6,
    Gauge = 7,
    Table = 8,
    HeatMap = 9
}

public enum ChartType
{
    Bar = 1,
    Line = 2,
    Pie = 3,
    Area = 4,
    Donut = 5,
    Scatter = 6,
    HeatMap = 7,
    Gauge = 8
}

public enum ReportExportFormat
{
    Csv = 1,
    Excel = 2,
    Pdf = 3,
    Json = 4
}

public enum KpiTrend
{
    Up = 1,
    Down = 2,
    Stable = 3,
    Unknown = 4
}

public enum ScheduleFrequency
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Cron = 4
}
