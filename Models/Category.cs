using System.ComponentModel.DataAnnotations;

namespace BookShopping.Models;

public class Category
{
    public enum StatusEnum
    {
        Pending,
        Approved,
        Rejected
    }
    [Key] public int Id { get; set; }
    
    
    [Required(ErrorMessage = "Category name is required")]
    public string Name { get; set; }
    
    
    
    [StringLength(200, ErrorMessage = "Description must be less than 200 characters")]
    public string Description { get; set; }

    [Required] public StatusEnum Status { get; set; }
}