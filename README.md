# dotNetPokemon

---
## Application Outline
### Purpose
This application utilizes .NET 9 to access the Pokémon API. It gets the first 50 Pokémon by
default to display to the user but also allows for searching by Pokémon name. Each Pokémon can 
be added to the user's "team" which consists of up to 3 Pokémon. Once the user has 3 Pokémon
in their team they can choose to "battle" against preset gyms. The battle will occur in order
of the Pokémon being added and compare them to the gym leader's Pokémon and display a final
battle resul to the user after the battle. 

### Tech Stack
- .NET 9
- Blazor Web
- Neon DB
- [PokeAPI](https://pokeapi.co)
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
check user-secrets were created correctly(check teams):
```
dotnet user-secrets list
```
to run the project in development mode NOT PRODUCTION
```
dotnet run
``` 
---
