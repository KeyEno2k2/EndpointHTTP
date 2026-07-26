using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/api/v1/parse-content", async (HttpRequest request) =>
{
    // Sprawdzenie nagłówka Content-Type
    if (!request.HasJsonContentType())
    {
        return Results.Json(
            new
            {
                status = "error",
                message = "Content-Type musi mieć wartość application/json."
            },
            statusCode: StatusCodes.Status415UnsupportedMediaType
        );
    }
    ParseContentRequest? body;

    try
    {
        body = await request.ReadFromJsonAsync<ParseContentRequest>();
    }
    catch (JsonException)
    {
        return Results.Json(
            new
            {
                status = "error",
                message = "Nieprawidłowy format JSON."
            },
            statusCode: StatusCodes.Status400BadRequest
        );
    }

    if (body == null)
    {
        return Results.BadRequest(new
        {
            status = "error",
            message = "Nie przesłano danych"
        });
    }

    if (string.IsNullOrWhiteSpace(body.Content))
    {
        return Results.BadRequest(new
        {
            status = "error",
            message = "Pole 'Content' nie może być puste."
        });
    }

    //Sprawdzanie obsługiwanych typów
    if (!Enum.TryParse<ContentType>(
        body.Type,
        ignoreCase: true,
        out var contentType))
        {
            return Results.BadRequest(new
            {
                status = "error",
                message = "Nieobsługiwany typ. Dozwolone typy to: CSV, INTERNAL_JSON."
            });
        }

    //Dekodowanie Base64
    string decodedContent;
        try
        {
            byte[] decodedBytes = Convert.FromBase64String(body.Content);
            decodedContent = Encoding.UTF8.GetString(decodedBytes);
        }
        catch (FormatException)
        {
        return Results.BadRequest(new
        {
            status = "error",
            message = "Pole COntent nie zawiera poprawnych danych Base64"
        });
        }
});


enum ContentType
{
    CSV,
    INTERNAL_JSON
}

class ParseContentRequest
{
    public string? Type {get; set;}
    public string? Content {get; set;}
}