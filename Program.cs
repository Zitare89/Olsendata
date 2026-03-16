using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.Use(async (context, next) =>
{
    await next();

    if (context.Response.ContentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) == true)
    {
        context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.Expires = "0";
    }
});

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        if (context.File.Name.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            context.Context.Response.ContentType = "text/html; charset=utf-8";
        }
    }
});

app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    if (path == "/hjem")
    {
        context.Response.Redirect("/");
        return;
    }

    if (path == "/tjenester")
    {
        context.Response.Redirect("/tjenester/");
        return;
    }

    if (path == "/om-oss")
    {
        context.Response.Redirect("/om-oss/");
        return;
    }

    if (path == "/kontakt")
    {
        context.Response.Redirect("/kontakt/");
        return;
    }

    await next();
});

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
        message = "Takk for meldingen. Vi svarer deg s\u00E5 raskt vi kan."
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
    [Required(ErrorMessage = "Navn er p\u00E5krevd.")]
    [StringLength(100, ErrorMessage = "Navn kan ikke v\u00E6re lengre enn 100 tegn.")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "E-post er p\u00E5krevd.")]
    [EmailAddress(ErrorMessage = "Skriv inn en gyldig e-postadresse.")]
    [StringLength(150, ErrorMessage = "E-post kan ikke v\u00E6re lengre enn 150 tegn.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Melding er p\u00E5krevd.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Meldingen m\u00E5 v\u00E6re mellom 10 og 2000 tegn.")]
    public string Message { get; init; } = string.Empty;
}
