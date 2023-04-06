using BookShopping.Models;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace BookShopping.Areas.Authenticated.ViewMode;


public class UpsetBookViewModel 
{
    public Book Book { get; set; }
    public List<SelectListItem> Categories { get; set; }
}