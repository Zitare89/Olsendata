using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/hjem", () => Results.Redirect("/"));
app.MapGet("/tjenester", () => Results.Redirect("/tjenester/"));
app.MapGet("/om-oss", () => Results.Redirect("/om-oss/"));
app.MapGet("/kontakt", () => Results.Redirect("/kontakt/"));

app.MapPost("/api/contact", (ContactRequest request, ILogger<Program> logger) =>
{
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(request);

    var isValid = Validator.TryValidateObject(
        request,
        validationContext,
        validationResults,
        validateAllProperties: true);

    if (!isValid)
    {
        var errors = validationResults
            .GroupBy(result => result.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(
                group => ToCamelCase(group.Key),
                group => group
                    .Select(result => result.ErrorMessage ?? "Ugyldig verdi.")
                    .Distinct()
                    .ToArray());

        return Results.ValidationProblem(errors);
    }

    logger.LogInformation(
        "Ny kontakthenvendelse fra {Name} <{Email}>: {Message}",
        request.Name,
        request.Email,
        request.Message);

    return Results.Ok(new
    {
        message = "Takk for meldingen. Vi svarer deg sa raskt vi kan."
    });
});

app.Run();

static string ToCamelCase(string value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return string.Empty;
    }

    return char.ToLowerInvariant(value[0]) + value[1..];
}

sealed class ContactRequest
{
    [Required(ErrorMessage = "Navn er pakrevd.")]
    [StringLength(100, ErrorMessage = "Navn kan ikke vare lengre enn 100 tegn.")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "E-post er pakrevd.")]
    [EmailAddress(ErrorMessage = "Skriv inn en gyldig e-postadresse.")]
    [StringLength(150, ErrorMessage = "E-post kan ikke vare lengre enn 150 tegn.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Melding er pakrevd.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Meldingen ma vare mellom 10 og 2000 tegn.")]
    public string Message { get; init; } = string.Empty;
}
