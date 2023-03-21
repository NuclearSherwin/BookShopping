namespace BookShopping.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Cart
{
    [Key] public int Id { get; set; }
    [Required] public string UserId { get; set; }
    [Required] public int BookId { get; set; }
    [Required] public int Count { get; set; }
    
    public Cart()
    {
        Count = 1;
    }
    
    [ForeignKey("UserId")] 
    private User User { get; set; }
    [ForeignKey("BookId")] public Book Book { get; set; }
    [NotMapped] public double Price { get; set; }
}