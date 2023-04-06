namespace BookShopping.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Cart
{
    public Cart()
    {
        Count = 1;
    }
    
    [Key] 
    public int Id { get; set; }
    
    [Required] 
    public string UserId { get; set; }
    [ForeignKey("UserId")]
    private User User { get; set; }
    
    [Required] 
    public int BookId { get; set; }
    [ForeignKey("BookId")] 
    public Book Book { get; set; }
    
    [Required] 
    public int Count { get; set; }
    
    [NotMapped] 
    public double Price { get; set; }
}