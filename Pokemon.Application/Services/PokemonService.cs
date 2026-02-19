using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Pokemon.Domain.Interfaces;

namespace Pokemon.Application.Services;

public class PokemonService
{
    private readonly IRepository<Domain.Models.Pokemon> _pokemonRepo;
    private readonly IUserOwnedRepository<Domain.Models.Pokemon> _userOwnedRepo;
    private readonly HttpClient _http;
    private readonly string _apiUrl;
    
    public PokemonService(IRepository<Domain.Models.Pokemon> pokemonRepo, IUserOwnedRepository<Domain.Models.Pokemon> userOwnedRepo, HttpClient http, IConfiguration config)
    {
        _pokemonRepo = pokemonRepo;
        _userOwnedRepo = userOwnedRepo;
        _http = http;
        _apiUrl = config["ApiSettings:PokemonApiUrl"];
        
        Console.WriteLine(_apiUrl);
    }

// __________________________________________DATABASE OPERATIONS________________________________________________________
    public async Task<List<Domain.Models.Pokemon>> GetUserTeamAsync(int userId) // ------------------------------------------------
    {
        // get all Pokémon in database with this userID
        return await _userOwnedRepo.GetByUserIdAsync((userId));
    }
    /*
    public async Task<List<Domain.Models.Pokemon>> GetUserTeamAsync(int userId) // ------------------------------------------------
    {
        return await _pokemonRepo.GetByUserIdAsync(userId);
    }
    */

    public async Task AddPokemonToTeamAsync(ApiPokemonDetails pokemon, int userId) // --------------
    {
        // check that team is not full (max of 3)
        var userTeam = await GetUserTeamAsync(userId);
        if (userTeam.Count >= 3)
        {
            throw new Exception($"Your team is full. You cannot have more than 3 pokemon at a time. ");
        }
        
        // create a Pokémon object using data from API item selected
        var newPokemon = new Domain.Models.Pokemon
        {
            Name = pokemon.Name,
            Type = pokemon.Types?.FirstOrDefault().Type.Name,
            Sprite = pokemon.Sprites.Front_Default,
            UserId = userId
        };
        
        // add Pokémon under this user to the database
        await _pokemonRepo.AddAsync(newPokemon);
        await _pokemonRepo.SaveChangesAsync();
    }

    public async Task RemovePokemonFromTeamAsync(int pokemonId, int userId) // -----------------------------------
    {
        // find the given Pokémon for this user and delete it from the database
        /*
        var team = await GetUserTeamAsync(userId);
        var pokemon = team.FirstOrDefault(p => p.Id == pokemonId);
        */
        var pokemon = await _userOwnedRepo
            .GetByUserIdAsync(userId)
            .ContinueWith(t => t.Result.FirstOrDefault(p => p.Id == pokemonId));
        
        if (pokemon == null)
            throw new Exception($"Could not find pokemon with id: {pokemonId}, under user: {userId}");
        
        //await _pokemonRepo.RemoveAsync(pokemon);
        //await _pokemonRepo.SaveChangesAsync();
        await _userOwnedRepo.RemoveAsync(pokemon);
        await _userOwnedRepo.SaveChangesAsync();
    }
    
    
// ________________________________________API OPERATIONS_______________________________________________________________
    public async Task<List<ApiPokemonResult>> GetPokemonFromApiAsync(int limit = 50) // --------------------------------
    {
        // get "all" Pokémon (top 50 by default) from API
        var response = await _http.GetFromJsonAsync<ApiPokemonResponse>($"{_apiUrl}pokemon?limit={limit}");
         
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
            return await _http.GetFromJsonAsync<ApiPokemonDetails>($"{_apiUrl}pokemon/{queryName}");
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
    
   private static readonly Dictionary<string, Dictionary<string, double>> TypeChart =
    new(StringComparer.OrdinalIgnoreCase)
{
    ["normal"] = new() { ["rock"] = 0.5, ["ghost"] = 0, ["steel"] = 0.5 },
    ["fire"] = new() {
        ["grass"] = 2, ["ice"] = 2, ["bug"] = 2, ["steel"] = 2,
        ["fire"] = 0.5, ["water"] = 0.5, ["rock"] = 0.5, ["dragon"] = 0.5
    },
    ["water"] = new() {
        ["fire"] = 2, ["ground"] = 2, ["rock"] = 2,
        ["water"] = 0.5, ["grass"] = 0.5, ["dragon"] = 0.5
    },
    ["electric"] = new() {
        ["water"] = 2, ["flying"] = 2,
        ["electric"] = 0.5, ["grass"] = 0.5, ["dragon"] = 0.5,
        ["ground"] = 0
    },
    ["grass"] = new() {
        ["water"] = 2, ["ground"] = 2, ["rock"] = 2,
        ["fire"] = 0.5, ["grass"] = 0.5, ["poison"] = 0.5,
        ["flying"] = 0.5, ["bug"] = 0.5, ["dragon"] = 0.5, ["steel"] = 0.5
    },
    ["ice"] = new() {
        ["grass"] = 2, ["ground"] = 2, ["flying"] = 2, ["dragon"] = 2,
        ["fire"] = 0.5, ["water"] = 0.5, ["ice"] = 0.5, ["steel"] = 0.5
    },
    ["fighting"] = new() {
        ["normal"] = 2, ["ice"] = 2, ["rock"] = 2, ["dark"] = 2, ["steel"] = 2,
        ["poison"] = 0.5, ["flying"] = 0.5, ["psychic"] = 0.5, ["bug"] = 0.5,
        ["fairy"] = 0.5, ["ghost"] = 0
    },
    ["poison"] = new() {
        ["grass"] = 2, ["fairy"] = 2,
        ["poison"] = 0.5, ["ground"] = 0.5, ["rock"] = 0.5, ["ghost"] = 0.5,
        ["steel"] = 0
    },
    ["ground"] = new() {
        ["fire"] = 2, ["electric"] = 2, ["poison"] = 2, ["rock"] = 2, ["steel"] = 2,
        ["grass"] = 0.5, ["bug"] = 0.5, ["flying"] = 0
    },
    ["flying"] = new() {
        ["grass"] = 2, ["fighting"] = 2, ["bug"] = 2,
        ["electric"] = 0.5, ["rock"] = 0.5, ["steel"] = 0.5
    },
    ["psychic"] = new() {
        ["fighting"] = 2, ["poison"] = 2,
        ["psychic"] = 0.5, ["steel"] = 0.5, ["dark"] = 0
    },
    ["bug"] = new() {
        ["grass"] = 2, ["psychic"] = 2, ["dark"] = 2,
        ["fire"] = 0.5, ["fighting"] = 0.5, ["poison"] = 0.5,
        ["flying"] = 0.5, ["ghost"] = 0.5, ["steel"] = 0.5, ["fairy"] = 0.5
    },
    ["rock"] = new() {
        ["fire"] = 2, ["ice"] = 2, ["flying"] = 2, ["bug"] = 2,
        ["fighting"] = 0.5, ["ground"] = 0.5, ["steel"] = 0.5
    },
    ["ghost"] = new() {
        ["psychic"] = 2, ["ghost"] = 2,
        ["dark"] = 0.5, ["normal"] = 0
    },
    ["dragon"] = new() {
        ["dragon"] = 2,
        ["steel"] = 0.5, ["fairy"] = 0
    },
    ["dark"] = new() {
        ["psychic"] = 2, ["ghost"] = 2,
        ["fighting"] = 0.5, ["dark"] = 0.5, ["fairy"] = 0.5
    },
    ["steel"] = new() {
        ["ice"] = 2, ["rock"] = 2, ["fairy"] = 2,
        ["fire"] = 0.5, ["water"] = 0.5, ["electric"] = 0.5, ["steel"] = 0.5
    },
    ["fairy"] = new() {
        ["fighting"] = 2, ["dragon"] = 2, ["dark"] = 2,
        ["fire"] = 0.5, ["poison"] = 0.5, ["steel"] = 0.5
    }
};

private bool IsTypeStrongAgainst(string attacker, string defender)
{
    if (!TypeChart.TryGetValue(attacker.ToLower(), out var matchups))
        return false;

    if (!matchups.TryGetValue(defender.ToLower(), out var multiplier))
        return false;

    return multiplier > 1.0;
}
    
    public class BattleOpponent
    {
        public string Name { get; set; } = string.Empty;
        public string SpriteUrl { get; set; } = string.Empty;
        public List<OpponentPokemon> Team { get; set; } = new();
        public bool IsBoss { get; set; }
    }

    public class OpponentPokemon
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string SpriteUrl { get; set; } = string.Empty;
    }

    public class BattleResult
    {
        public string OverallResult { get; set; } = string.Empty;
        public List<PokemonRoundResult> Rounds { get; set; } = new();
    }

    public class PokemonRoundResult
    {
        public string UserPokemonName { get; set; } = string.Empty;
        public string UserPokemonType { get; set; } = string.Empty;
        public string OpponentPokemonName { get; set; } = string.Empty;
        public string OpponentPokemonType { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty; // Win / Lose / Tie
    }
    
    private OpponentPokemon MakeOpponentPokemon(string name, string type, int pokedexId)
    {
        return new OpponentPokemon
        {
            Name = name,
            Type = type,
            SpriteUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{pokedexId}.png"
        };
    }
    
    private List<BattleOpponent> GetAllBossOpponents()
{
    return new List<BattleOpponent>
    {
        // ------------------ GYM LEADERS ------------------

        new BattleOpponent
        {
            Name = "Brock",
            SpriteUrl = "/images/trainers/brock.png",
            IsBoss = true,
            Team = new()
            {
                MakeOpponentPokemon("Geodude", "rock", 74),
                MakeOpponentPokemon("Onix", "rock", 95),
                MakeOpponentPokemon("Rhyhorn", "rock", 111)
            }
        },

        new BattleOpponent
        {
            Name = "Misty",
            SpriteUrl = "/images/trainers/misty.png",
            IsBoss = true,
            Team = new()
            {
                MakeOpponentPokemon("Staryu", "water", 120),
                MakeOpponentPokemon("Starmie", "water", 121),
                MakeOpponentPokemon("Psyduck", "water", 54)
            }
        },

        new BattleOpponent
        {
            Name = "Lt. Surge",
            SpriteUrl = "/images/trainers/surge.png",
            IsBoss = true,
            Team = new()
            {
                MakeOpponentPokemon("Voltorb", "electric", 100),
                MakeOpponentPokemon("Pikachu", "electric", 25),
                MakeOpponentPokemon("Raichu", "electric", 26)
            }
        },

        new BattleOpponent
        {
            Name = "Erika",
            SpriteUrl = "/images/trainers/erika.png",
            IsBoss = true,
            Team = new()
            {
                MakeOpponentPokemon("Victreebel", "grass", 71),
                MakeOpponentPokemon("Tangela", "grass", 114),
                MakeOpponentPokemon("Vileplume", "grass", 45)
            }
        },

        new BattleOpponent
        {
            Name = "Koga",
            SpriteUrl = "/images/trainers/koga.png",
            IsBoss = true,
            Team = new()
            {
                MakeOpponentPokemon("Koffing", "poison", 109),
                MakeOpponentPokemon("Muk", "poison", 89),
                MakeOpponentPokemon("Weezing", "poison", 110)
            }
        },

        new BattleOpponent
        {
            Name = "Sabrina",
            SpriteUrl = "/images/trainers/sabrina.png",
            IsBoss = true,
            Team = new()
            {
                MakeOpponentPokemon("Kadabra", "psychic", 64),
                MakeOpponentPokemon("Mr. Mime", "psychic", 122),
                MakeOpponentPokemon("Alakazam", "psychic", 65)
            }
        },

        new BattleOpponent
        {
            Name = "Blaine",
            SpriteUrl = "/images/trainers/blaine.png",
            IsBoss = true,
            Team = new()
            {
                MakeOpponentPokemon("Growlithe", "fire", 58),
                MakeOpponentPokemon("Ponyta", "fire", 77),
                MakeOpponentPokemon("Arcanine", "fire", 59)
            }
        },

        new BattleOpponent
        {
            Name = "Giovanni",
            SpriteUrl = "/images/trainers/giovanni.png",
            IsBoss = true,
            Team = new()
            {
                MakeOpponentPokemon("Rhyhorn", "ground", 111),
                MakeOpponentPokemon("Dugtrio", "ground", 51),
                MakeOpponentPokemon("Nidoking", "ground", 34)
            }
        },

        // ------------------ ELITE FOUR ------------------

        new BattleOpponent
        {
            Name = "Lorelei",
            SpriteUrl = "/images/trainers/lorelei.png",
            IsBoss = true,
            Team = new()
            {
                MakeOpponentPokemon("Dewgong", "ice", 87),
                MakeOpponentPokemon("Cloyster", "ice", 91),
                MakeOpponentPokemon("Lapras", "ice", 131)
            }
        },

        new BattleOpponent
        {
            Name = "Bruno",
            SpriteUrl = "/images/trainers/bruno.png",
            IsBoss = true,
            Team = new()
            {
                MakeOpponentPokemon("Onix", "rock", 95),
                MakeOpponentPokemon("Hitmonlee", "fighting", 106),
                MakeOpponentPokemon("Machamp", "fighting", 68)
            }
        },

        new BattleOpponent
        {
            Name = "Agatha",
            SpriteUrl = "/images/trainers/agatha.png",
            IsBoss = true,
            Team = new()
            {
                MakeOpponentPokemon("Gengar", "ghost", 94),
                MakeOpponentPokemon("Haunter", "ghost", 93),
                MakeOpponentPokemon("Arbok", "poison", 24)
            }
        },

        new BattleOpponent
        {
            Name = "Lance",
            SpriteUrl = "/images/trainers/lance.png",
            IsBoss = true,
            Team = new()
            {
                MakeOpponentPokemon("Gyarados", "water", 130),
                MakeOpponentPokemon("Dragonair", "dragon", 148),
                MakeOpponentPokemon("Dragonite", "dragon", 149)
            }
        },

        // ------------------ CHAMPION ------------------

        new BattleOpponent
        {
            Name = "Blue",
            SpriteUrl = "/images/trainers/blue.png",
            IsBoss = true,
            Team = new()
            {
                MakeOpponentPokemon("Pidgeot", "flying", 18),
                MakeOpponentPokemon("Alakazam", "psychic", 65),
                MakeOpponentPokemon("Charizard", "fire", 6)
            }
        }
    };
    
    
}
    
    private async Task<BattleOpponent> GetRandomRouteTrainerAsync()
    {
        var ids = new HashSet<int>();
        while (ids.Count < 3)
            ids.Add(Random.Shared.Next(1, 152)); // Gen 1

        var team = new List<OpponentPokemon>();

        foreach (var id in ids)
        {
            var details = await _http.GetFromJsonAsync<ApiPokemonDetails>($"{_apiUrl}pokemon/{id}");
            if (details == null) continue;

            team.Add(new OpponentPokemon
            {
                Name = details.Name,
                Type = details.Types.First().Type.Name,
                SpriteUrl = details.Sprites.Front_Default
            });
        }

        return new BattleOpponent
        {
            Name = "Random Trainer",
            SpriteUrl = "/images/trainers/random.png",
            IsBoss = false,
            Team = team
        };
    }
    
    
    public async Task<BattleOpponent> GetRandomOpponentAsync()
    {
        var roll = Random.Shared.Next(0, 100);

        if (roll < 40)
            return await GetRandomRouteTrainerAsync();

        var bosses = GetAllBossOpponents();
        return bosses[Random.Shared.Next(bosses.Count)];
    }
    
    public async Task<BattleResult> BattleAsync(int userId, BattleOpponent opponent)
    {
        var userTeam = await GetUserTeamAsync(userId);

        var result = new BattleResult();
        var rounds = new List<PokemonRoundResult>();

        if (userTeam.Count != 3 || opponent.Team.Count != 3)
        {
            result.OverallResult = "You must have exactly 3 Pokémon.";
            return result;
        }

        var score = 0;

        for (int i = 0; i < 3; i++)
        {
            var userMon = userTeam[i];
            var oppMon = opponent.Team[i];

            var userType = userMon.Type.ToLower();
            var oppType = oppMon.Type.ToLower();

            var userStrong = IsTypeStrongAgainst(userType, oppType);
            var oppStrong = IsTypeStrongAgainst(oppType, userType);

            string outcome;

            if (userStrong && !oppStrong)
            {
                score++;
                outcome = "Win";
            }
            else if (oppStrong && !userStrong)
            {
                score--;
                outcome = "Lose";
            }
            else
            {
                outcome = "Tie";
            }

            rounds.Add(new PokemonRoundResult
            {
                UserPokemonName = userMon.Name,
                UserPokemonType = userType,
                OpponentPokemonName = oppMon.Name,
                OpponentPokemonType = oppType,
                Outcome = outcome
            });
        }

        result.Rounds = rounds;
        result.OverallResult =
            score > 0 ? "You Win!" :
            score < 0 ? "You Lose!" :
            "It's a Tie!";

        return result;
    }
    
    

    
}