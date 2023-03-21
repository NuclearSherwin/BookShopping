namespace BookShopping.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class OrderDetail
{
    [Key] public int Id { get; set; }
    [Required] public int OrderId { get; set; }
    [Required] public int BookId { get; set; }
    [Required] public int Quantity { get; set; }
    [Required] public double Price { get; set; }

    // foreign keys
    [ForeignKey("OrderId")] public Order Order { get; set; }
    [ForeignKey("BookId")] public Book Book { get; set; }
}