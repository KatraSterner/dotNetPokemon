namespace Pokemon.Domain.Models;

public class Pokemon
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public string Name { get; set; }
    
    public string Type { get; set; }
    
    public string Sprite { get; set; }
}