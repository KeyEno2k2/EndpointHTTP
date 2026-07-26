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

}
);