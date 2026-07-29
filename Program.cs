using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/v1/parse-content", async (HttpRequest request, ParseContentRequest body) =>
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


    //Parsowanie zależne od typu
    if (contentType == ContentType.CSV)
    {
        return ParseCsv(decodedContent);
    }

    return ParseInternalJson(decodedContent);
});

app.Run();

static IResult ParseCsv(string csvContent)
{
    //Implementacja parsowania CSV
    string[] lines = csvContent
    .Replace("\r\n", "\n")
    .Split('\n', StringSplitOptions.RemoveEmptyEntries);

    if (lines.Length == 0)
    {
        return Results.BadRequest(new
        {
            status = "error",
            message = "Plik CSV jest pusty."
        });
    }

    // Zakładamy, że pierwsza linia zawiera nagłówki
    string[] headers = lines[0]
    .Split(',')
    .Select(header => header.Trim())
    .ToArray();

    if (headers.Length == 0 || headers.All(string.IsNullOrWhiteSpace))
    {
        return Results.BadRequest(new
        {
            status = "error",
            message = "Plik CSV nie zawiera nagłówków."
        });
    }

    var parsedRows = new List<Dictionary<string, string>>();

    for (int i = 1; i < lines.Length; i++)
    {
        string[] values = lines[i]
        .Split(',')
        .Select(value => value.Trim())
        .ToArray();

        if (values.Length != headers.Length)
        {
            return Results.BadRequest(new
            {
                status = "error",
                message = $"Wiersz numer {i + 1} ma niepoprawną liczbe kolumn"
            });
        }

        var row = new Dictionary<string, string>();
        for (int column = 0; column < headers.Length; column++)
        {
            row[headers[column]] = values[column];
        }

        parsedRows.Add(row);
    }

    return Results.Ok(new
    {
        status = "success",
        processedCount = parsedRows.Count,
        data = parsedRows
    });
}

static IResult ParseInternalJson(string jsonContent)
{
    try
    {
        using JsonDocument document = JsonDocument.Parse(jsonContent);
        JsonElement root = document.RootElement;
        int processedCount = root.ValueKind switch
        {
            JsonValueKind.Array => root.GetArrayLength(),
            JsonValueKind.Object => 1,
            _ => 1
        };

        //Potrzebny Clone, aby zwrócić dane w odpowiedzi
        JsonElement parsedData = root.Clone();

        return Results.Ok(new
        {
            status = "success",
            processedCount,
            data = parsedData
        });
    }
    catch (JsonException)
    {
        return Results.BadRequest(new
        {
            status = "error",
            message = "Nieprawidłowy format JSON."
        });
    }
}

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