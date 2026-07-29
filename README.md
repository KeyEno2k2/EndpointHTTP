# EndpointHTTP

Prosta aplikacja REST API napisana w **ASP.NET Core Minimal API (.NET 9)**.

Aplikacja udostępnia endpoint HTTP, który przyjmuje dane zakodowane w formacie **Base64**, dekoduje je, parsuje w zależności od typu danych i zwraca wynik w ujednoliconej strukturze JSON.

## Technologie

- C#
- .NET 9
- ASP.NET Core Minimal API
- Swagger (Swashbuckle)
- System.Text.Json

---

## Uruchomienie projektu

### Wymagania

- .NET SDK 9.0 lub nowszy

### Uruchomienie

```bash
dotnet restore
dotnet run
```

Po uruchomieniu aplikacja będzie dostępna pod adresem:

```
http://localhost:5195
```

Dokumentacja Swagger:

```
http://localhost:5195/swagger
```

---

## Endpoint

### POST

```
/api/v1/parse-content
```

### Nagłówek

```
Content-Type: application/json
```

---

## Format żądania

```json
{
  "type": "CSV",
  "content": "Base64..."
}
```

### Parametry

| Pole    | Opis                                   |
|---------|----------------------------------------|
| type    | Typ danych (`CSV` lub `INTERNAL_JSON`) |
| content | Dane zakodowane w formacie Base64      |

---

## Przykład – CSV

Żądanie:

```json
{
  "type": "CSV",
  "content": "bmFtZSxhZ2UKQW5uYSwyNQpKYW4sMzA="
}
```

Odpowiedź:

```json
{
  "status": "success",
  "processedCount": 2,
  "data": [
    {
      "name": "Anna",
      "age": "25"
    },
    {
      "name": "Jan",
      "age": "30"
    }
  ]
}
```

---

## Przykład – INTERNAL_JSON

Żądanie:

```json
{
  "type": "INTERNAL_JSON",
  "content": "W3sibmFtZSI6IkFubmEiLCJhZ2UiOjI1fSx7Im5hbWUiOiJKYW4iLCJhZ2UiOjMwfV0="
}
```

Odpowiedź:

```json
{
  "status": "success",
  "processedCount": 2,
  "data": [
    {
      "name": "Anna",
      "age": 25
    },
    {
      "name": "Jan",
      "age": 30
    }
  ]
}
```

---

## Obsługiwane błędy

Aplikacja zwraca odpowiednie kody HTTP dla niepoprawnych danych.

Przykłady:

- `400 Bad Request`
  - nieobsługiwany typ danych
  - niepoprawny Base64
  - niepoprawny JSON
  - brak wymaganych pól

- `415 Unsupported Media Type`
  - niepoprawny nagłówek `Content-Type`

---

## Struktura projektu

```
EndpointHTTP
│
├── Program.cs
├── EndpointHTTP.csproj
├── appsettings.json
├── appsettings.Development.json
├── EndpointHTTP.http
├── README.md
└── Properties
```

---

## Autor - Igor Jurewicz

Projekt został wykonany jako zadanie rekrutacyjne z wykorzystaniem **ASP.NET Core Minimal API** oraz języka **C#**.