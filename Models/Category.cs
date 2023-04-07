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
    [Required] 
    [Display(Name = "Type")]
    public string Name { get; set; }
    [Required] public string Description { get; set; }

    [Required] public StatusEnum Status { get; set; }
}