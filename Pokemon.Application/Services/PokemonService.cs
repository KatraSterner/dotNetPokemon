using System.Net.Http.Json;

using Pokemon.Domain.Interfaces;

namespace Pokemon.Application.Services;

public class PokemonService
{
    private readonly IRepository<Domain.Models.Pokemon> _pokemonRepo;
    private readonly HttpClient _http;
    
    public PokemonService(IRepository<Domain.Models.Pokemon> pokemonRepo, HttpClient http)
    {
        _pokemonRepo = pokemonRepo;
        _http = http;
    }
    
    // TODO: move to environment variable in azure when deployed-----------------------------------------------------------
    private string ApiUrl = "https://pokeapi.co/api/v2/";

// __________________________________________DATABASE OPERATIONS________________________________________________________
    public async Task<List<Domain.Models.Pokemon>> GetUserTeamAsync(int userId) // ------------------------------------------------
    {
        // get all Pokémon in database with this userID
        return await _pokemonRepo.GetByUserIdAsync(userId);
    }

    public async Task AddPokemonToTeamAsync(string name, string type, string sprite, int userId) // --------------
    {
        // check that team is not full (max of 3)
        var userTeam = await GetUserTeamAsync(userId);
        if (userTeam.Count >= 3)
        {
            throw new Exception($"Your team is full. You cannot have more than 3 pokemon at a time. ");
        }
        
        // create a Pokémon object using data from API item selected
        var pokemon = new Domain.Models.Pokemon
        {
            Name = name,
            Type = type,
            Sprite = sprite,
            UserId = userId
        };
        
        // add Pokémon under this user to the database
        await _pokemonRepo.AddAsync(pokemon);
        await _pokemonRepo.SaveChangesAsync();
    }

    public async Task RemovePokemonFromTeamAsync(int pokemonId, int userId) // -----------------------------------
    {
        // find the given Pokémon for this user and delete it from the database
        var team = await GetUserTeamAsync(userId);
        var pokemon = team.FirstOrDefault(p => p.Id == pokemonId);
        
        if (pokemon == null)
            throw new Exception($"Could not find pokemon with id: {pokemonId}, under user: {userId}");
        
        await _pokemonRepo.RemoveAsync(pokemon);
        await _pokemonRepo.SaveChangesAsync();
    }
    
    
// ________________________________________API OPERATIONS_______________________________________________________________
    public async Task<List<ApiPokemonResult>> GetPokemonFromApiAsync(int limit = 50) // --------------------------------
    {
        // get "all" Pokémon (top 50 by default) from API
        var response = await _http.GetFromJsonAsync<ApiPokemonResponse>($"{ApiUrl}pokemon?limit={limit}");
         
        return response.Results;
    }
    
    public async Task<ApiPokemonDetails> GetPokemonDetailsAsync(string url) // -----------------------------------------
    {
        // use the url from each ApiPokemonResult ^ to get more information about the Pokémon
        // "url": "https://pokeapi.co/api/v2/pokemon/1/" where the final number is the Pokémon ID
        return await _http.GetFromJsonAsync<ApiPokemonDetails>(url);
    }
    
    public async Task<ApiPokemonDetails> SearchPokemonAsync(string queryName)
    {
        // search the api for a specific Pokémon
        try
        {
            return await _http.GetFromJsonAsync<ApiPokemonDetails>($"{ApiUrl}pokemon/{queryName}");
        }
        catch
        {
            return null;
        }
    }
    
// -----DTOs------------------------------------------------------------------------------------------------------------
    public class ApiPokemonResponse
    {
        public List<ApiPokemonResult> Results { get; set; }
        /*  define the "results" section
        {
          "count": 1350,
          "next": "https://pokeapi.co/api/v2/pokemon?offset=50&limit=50",
          "previous": null,
          "results": [
                {
                  "name": "bulbasaur",
                  "url": "https://pokeapi.co/api/v2/pokemon/1/"
                }, ...
            ]
        } 
        */
    }
    public class ApiPokemonResult
    {
        public string Name { get; set; }
        public string Url { get; set; }
        /* define each item in the "results" section"
        {
            "name": "bulbasaur",
            "url": "https://pokeapi.co/api/v2/pokemon/1/"
        }, ...
        */
    }
    public class ApiPokemonDetails
    {
        public string Name { get; set; }
        public List<ApiTypeSlot> Types { get; set; }
        public ApiSprites Sprites { get; set; }
        /*
        {
            "abilities": [...],
            "cries": {...another url...},
            "forms": [...],
            "game_indices": [...],
            "height": 7,
            "held_items": [],
            "id": 1,
            "is_default": true,
            "location_area_encounters": "https://pokeapi.co/api/v2/pokemon/1/encounters",
            "moves": [...],
            "name": "bulbasaur",
            "order": 1,
            "past_abilities": [...],
            "past_stats": [...],
            "past_types": [],
            "species": {
                "name": "bulbasaur",
                "url": "https://pokeapi.co/api/v2/pokemon-species/1/"
            },
            "sprites": {...},
            "stats": [...],
            types": [...],
            "weight": 69
        }
        */
    }
    public class ApiTypeSlot
    {
        public ApiType Type { get; set; }
        /*
        {
            "slot": 1,
            "type": {...}
        }
        */
    }
    public class ApiType
    {
        public string Name { get; set; }
        /*
        {
            "name": "grass",
            "url": "https://pokeapi.co/api/v2/type/12/"
        }
        */
    }
    public class ApiSprites
    {
        public string Front_Default { get; set; }
        /*
        "sprites": {
            ...,
            "front_default": "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/1.png",
            "front_female": null,
            ...
        }
        */
    }
    
// _____________________________________CUSTOM FUNCTIONS________________________________________________________________
    public async Task<string> GymBattleAsync(int userId) // ---------------------------------------------------------
    {
        // Get all Pokémon on the user's team and battle them against a gym
        var userTeam = await GetUserTeamAsync(userId);

        // check that the user has a team of exactly 3
        if (userTeam.Count != 3)
            return "Your team must include exactly 3 Pokémon.";
        
        // TODO: convert to preset gyms in database instead of hardcoding it here?
        // build gym team with rock pokemon types
        var gymTeam = new List<Domain.Models.Pokemon>
        {
            new Domain.Models.Pokemon { Name = "Onix", Type = "rock" },
            new Domain.Models.Pokemon { Name = "Geodude", Type = "rock" },
            new Domain.Models.Pokemon { Name = "Sudowoodo", Type = "rock" }
        };
        
        // string to store outcome results
        string[] outcomes = { "", "", "" };
        // score to determine final winner
        var score = 0;
        // loop through each Pokémon on the team (3) and battle them in order against the gym team
        for (int i = 0; i < gymTeam.Count; i++)
        {
            if (IsTypeStrongAgainst(userTeam[i].Type, gymTeam[i].Type))
            {
                score += 1;
                outcomes[i] = $"Your {userTeam[i].Name} won!";
            }
            else if (IsTypeStrongAgainst(gymTeam[i].Type, userTeam[i].Type))
            {
                outcomes[i] = $"Your {userTeam[i].Name} lost.";
            }
            else
            {
                outcomes[i] = "It's a tie...";
            }
        }
        
        var result = score >= 2 ? "You Win!" : "You Lose!";
        return result +
               $"\nResults: " +
               $"\n\t{outcomes[0]}" +
               $"\n\t{outcomes[1]} " +
               $"\n\t{outcomes[2]}";
        
    }

    private bool IsTypeStrongAgainst(string type1, string type2) // - - - - - - - - - - - - - - - - - - - - - - - - - - 
    {
        // compare the types to determine the winner and return win status
        var attacker = type1.ToLower();
        var defender = type2.ToLower();
        
        return (attacker == "water" && defender == "fire")
               || (attacker == "fire" && defender == "grass")
               || (attacker == "grass" && defender == "water")
               || (attacker == "electric" && defender == "water")
               || (attacker == "rock" && defender == "fire");
        // if attacker is water and defender is fire, return true -> win
    }
}