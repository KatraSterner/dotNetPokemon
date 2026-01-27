namespace Pokemon.Application.Services;

// using domain layer models and interface and repository

// use httpCleint to talk to chosen public API
// inject this service into controller/blazor component

public class PokemonService
{

    private readonly IRepository<Pokemon> _pokemonRepo;

    // constructor for repo/service
    public PokemonService(IRepository<Pokemon> pokemonRepo)
    {
        _pokemonRepo = pokemonRepo;
    }
    
    // TODO: search function
    // TODO: get all (from DB) function
    // TODO: get all (from API) function

    public async Task AddPokemonToTeam(String name, String type, String url, String userId)
    {
        
    }
    // TODO: add pokemon to list        - addPokemonToTeam(pokemon attributes)
    //      - create new "pokemon" object
    //      - add to database under a user-id
    //      - use the currently logged in user-id

    public async Task RemovePokemonFromTeam(String id, String userId)
    {
        
    }
    // TODO: remove pokemon from list   - removePokemonFromTeam(id)
    //      - remove this pokemon from the current user's team
    // TODO: gym battle                 - gymBattle(pokemon object, pokemon object)
    //      - battle default sets (gyms?)
    // TODO: battle other users?
    
}