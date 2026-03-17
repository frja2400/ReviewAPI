# Backend FOLIO

Backend REST API för FOLIO – en bokrecensionsplattform som hanterar sökning av böcker, bokrecensioner, favoriter och användare med JWT-autentisering och rollbaserad behörighet. Byggt med .NET Web API och SQLite.


## Länk

En liveversion av API:et finns tillgänglig på följande URL:

## Installation

För att installera och köra lokalt:

* `git clone https://github.com/frja2400/ReviewAPI.git`
* `cd ReviewAPI`
* `dotnet restore`
* Starta servern: `dotnet run`

## Datamodeller
```csharp
public class User
{
    public int Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "user";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Review
{
    public int Id { get; set; }

    [Required]
    public string BookId { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public string Text { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Favorite
{
    public int Id { get; set; }

    [Required]
    public string BookId { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
```

### Relationer & Cascade Delete
När en användare raderas tas även användarens recensioner och favoriter bort automatiskt via cascade delete.

## Användning

### Autentisering
| Metod | Ändpunkt | Beskrivning |
|-------|----------|-------------|
| POST | `/api/auth/register` | Registrerar ny användare |
| POST | `/api/auth/login` | Loggar in och returnerar JWT-token |

### Recensioner
| Metod | Ändpunkt | Beskrivning |
|-------|----------|-------------|
| GET | `/api/reviews/{bookId}` | Hämtar alla recensioner för en bok |
| GET | `/api/reviews/top-rated` | Hämtar 8 högst betygsatta böcker |
| GET | `/api/reviews/latest` | Hämtar 8 senast recenserade böcker |
| GET | `/api/reviews/user` | Hämtar inloggad användares recensioner |
| GET | `/api/reviews/{bookId}/user` | Hämtar inloggad användares recension för en specifik bok |
| POST | `/api/reviews` | Skapar ny recension |
| PUT | `/api/reviews/{id}` | Uppdaterar recension |
| DELETE | `/api/reviews/{id}` | Raderar recension |

### Favoriter
| Metod | Ändpunkt | Beskrivning |
|-------|----------|-------------|
| GET | `/api/favorites` | Hämtar inloggad användares favoriter |
| POST | `/api/favorites/{bookId}` | Lägger till bok i favoriter |
| DELETE | `/api/favorites/{bookId}` | Tar bort bok från favoriter |

### Admin
| Metod | Ändpunkt | Beskrivning |
|-------|----------|-------------|
| GET | `/api/admin/reviews` | Hämtar alla recensioner |
| DELETE | `/api/admin/reviews/{id}` | Raderar valfri recension |
| GET | `/api/admin/users` | Hämtar alla användare |
| DELETE | `/api/admin/users/{id}` | Raderar användare |

## Validering

API:et validerar inkommande data med Data Annotations:
* `Username` och `Email` är obligatoriska och måste vara unika.
* `Text` är obligatorisk för recensioner.
* `Rating` måste vara mellan 1 och 5.
* En användare kan bara lämna en recension per bok.
* Lösenord för användare måste vara minst 8 tecken och innehålla minst en siffra.

## CORS

API:et är konfigurerat för att tillåta anrop från:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://minfrontend.se", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

## JWT-autentisering

API:et använder rollbaserad behörighet med två roller — `user` och `admin`. Rollen är inbakad i token och kontrolleras automatiskt vid skyddade endpoints.

## Inloggningsuppgifter

API:et skapar automatiskt en admin-användare vid första uppstart.

- Email: `admin@folio.com`
- Lösenord: `admin123`

## Publicering
```bash
dotnet publish -c Release -o ./publish
```

Kopiera sedan `publish`-mappen till din server.