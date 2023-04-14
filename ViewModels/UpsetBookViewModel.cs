using System.ComponentModel.DataAnnotations;
using BookShopping.Models;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace BookShopping.ViewModels;

public class UpsetBookViewModel 
{
    [Required]
    public Book Book { get; set; }
    [Required]
    public List<SelectListItem> Categories { get; set; }
    
    [Required]
    public IFormFile File { get; set; }
}