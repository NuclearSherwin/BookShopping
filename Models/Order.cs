namespace BookShopping.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Order
{
    [Key] public int Id { get; set; }
    [Required] public string UserId { get; set; }
    [Required] public string Address { get; set; }
    [Required] public double Total { get; set; }
    [Required] public DateTime OrderDate { get; set; }

    // foreign key
    [ForeignKey("UserId")] public User User { get; set; }
}