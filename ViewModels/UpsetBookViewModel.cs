using BookShopping.Models;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace BookShopping.ViewModels;

public class UpsetBookViewModel 
{
    public Book Book { get; set; }
    public List<SelectListItem> Categories { get; set; }
    
    public IFormFile File { get; set; }
    public string OldFileName { get; set; }
}