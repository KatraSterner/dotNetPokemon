namespace Pokemon.Infrastructure.Data;

public class PokemonEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Sprite { get; set; } = string.Empty;
}