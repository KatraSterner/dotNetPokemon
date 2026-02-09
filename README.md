# dotNetPokemon

---
## Application Outline
### Purpose
This application utilizes .NET 9 to access the pokemon API. It gets the first 50 pokemon by
default to display to the user but also allows for searching by pokemon name. Each pokemon can 
be added to the user's "team" which consists of up to 3 pokemon. Once the user has 3 pokemon
in their team they can choose to "battle" against preset gyms. The battle will occur in order
of the pokemon being added and compare them to the gym leader's pokemon and display a final
battle resul to the user after the battle. 

### Tech Stack
- .NET 9
- Neon DB
- PokeAPI
- OpenIddict (planned)
- GitHub
- JetBrains Rider

### Developers
- Katra Sterner
- Clayton Williams
- Ylyas Movlyamov
- Marcie Benacka

---
## To Run: 
### insert user-secrets
api key:
```
dotnet user-secrets set "ApiSettings:PokemonApiUrl" "https://pokeapi.co/api/v2/"
```
database connection:
```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=ep-wispy-sound-a8s6owj4-pooler.eastus2.azure.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_XnENT4zRevA6;SslMode=Require;"
```
to run the project in development mode NOT PRODUCTION
```
dotnet run
``` 