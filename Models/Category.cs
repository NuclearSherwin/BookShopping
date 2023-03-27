using System.ComponentModel.DataAnnotations;

namespace BookShopping.Models;

public class Category
{
    [Key] public int Id { get; set; }
    [Required] 
    [Display(Name = "Type")]
    public string Name { get; set; }
    [Required] public string Description { get; set; }
}