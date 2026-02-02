using System.Net.Http.Json;

namespace Pokemon.Application.Services;

// TODO: import models and other files..................................................................................
//      IRepository
//      Pokemon
//      ApiPokemonResult

// assuming:
/*
    public class Pokemon
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string SpriteUrl { get; set; }
        public string UserId { get; set; }
    }

    public interface IRepository<T>
    {
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task RemoveAsync(T entity);
        Task SaveChangesAsync();
    }
*/

// TODO: 
//      - add DTOs for parsing JSON from the API

public class PokemonService
{
    
    private readonly IRepository<Pokemon> _pokemonRepo;
    private readonly HttpClient _http;
    
    public PokemonService(IRepository<Pokemon> pokemonRepo, HttpClient http)
    {
        _pokemonRepo = pokemonRepo;
        _http = http;
    }
    
    // TODO: move to environment variable in azure when deployed-----------------------------------------------------------
    private string ApiUrl = "https://pokeapi.co/api/v2/";

// __________________________________________DATABASE OPERATIONS________________________________________________________
    public async Task<List<Pokemon>> GetUserTeamAsync(string userId) // ------------------------------------------------
    {
        // get all pokemon in database with this userID
        return await _pokemonRepo.GetWhereAsync(p => p.UserId == userId);
    }

    public async Task AddPokemonToTeamAsync(string name, string type, string spriteURL, string userId) // --------------
    {
        // check that team is not full
        var userTeam = await GetUserTeamAsync(userId);
        if (userTeam.Count >= 3)
        {
            throw new Exception($"Your team is full. You cannot have more than 3 pokemon at a time. ");
        }
        
        // create a pokemon object using data from API item selected
        var pokemon = new Pokemon
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Type = type,
            SpriteURL = spriteURL,
            UserId = userId
        };
        
        // add pokemon under this user to the database
        await _pokemonRepo.AddAsync(pokemon);
        await _pokemonRepo.SaveChangesAsync();
    }

    public async Task RemovePokemonFromTeamAsync(string pokemonId, string userId) // -----------------------------------
    {
        // find the given pokemon for this user and delete it from the database
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
        // get "all" pokemon (top 50 by default) from API
        var response = await _http.GetFromJsonAsync<ApiPokemonResult>($"{ApiUrl}pokemon?limit={limit}");
        /* returns \/ \/ \/ \/ \/ \/ \/ 
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
        /\ /\ /\ /\ /\ /\ /\ /\ /\ /\ */
         
        return response.Results;
    }

    public async Task<ApiPokemonDetails> GetPokemonDetailsAsync(string url) // -----------------------------------------
    {
        // use the url from each ApiPokemonResult ^ to get more information about the pokemon
        // "url": "https://pokeapi.co/api/v2/pokemon/1/" where the final number is the pokemon ID
        return await _http.GetFromJsonAsync<ApiPokemonDetails>(url);
        
    }

    public async Task<ApiPokemonDetails> SearchPokemonAsync(string queryName)
    {
        // search the api for a specific pokemon
        try
        {
            // TODO: change to use private url to act like API key since there isn't one???????????????????????????????????????
            return await _http.GetFromJsonAsync<ApiPokemonDetails>($"{ApiUrl}pokemon/{queryName}");
        }
        catch
        {
            return null;
        }
    }
    
// _____________________________________CUSTOM FUNCTIONS________________________________________________________________
    public async Task<string> GymBattleAsync(string userId) // ---------------------------------------------------------
    {
        // Get all pokemon on the user's team and battle them against a gym
        var userTeam = await GetUserTeamAsync(userId);

        // TODO: check if user has 3 pokemon? (require them to have exactly 3?)????????????????????????????????????????????
        // check that the user has ANY pokemon to start with
        if (!userTeam.Any())
            return "You don't have any pokemon in your team to battle with!";
        
        // TODO: convert to preset gyms in database instead of hardcoding it here??????????????????????????????????????????
        // build gym team with rock pokemon types
        var gymTeam = List<>
        {
            { Name = "Onix", Type = "rock" },
            { Name = "Geodude", Type = "rock" },
            { Name = "SudoWoodo", Type = "rock"}
        };
        
        // string to store outcome results
        string[] outcomes = { "", "", "" };
        // score to determine final winner
        var score = 0;
        // loop through each pokemon on the team (3) and battle them in order against the gym team
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
                // TODO: replace with random winner????????????????????????????????????????????????????????????????????????
                outcomes[i] = "It's a tie...";
            }
        }

        if (score >= 2)
        {
            return $"You Win! " +
                   $"\nResults: " +
                   $"\n\t{outcomes[0]}" +
                   $"\n\t{outcomes[1]} " +
                   $"\n\t{outcomes[2]}";
        }
        else
        {
            return $"You Lose... " +
                   $"\nResults: " +
                   $"\n\t{outcomes[0]}" +
                   $"\n\t{outcomes[1]} " +
                   $"\n\t{outcomes[2]}";
        }
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