namespace FacilityInspection.Infrastructure.Configuration;

public class FrontendOptions
{
    public const string SectionName = "Frontend";

    public string BaseUrl { get; set; } = "http://localhost:4200";
}
