using BookShopping.Models;

namespace BookShopping.ViewModels;

public class ShoppingCartVM
{
    public IEnumerable<Cart> ListCart { get; set; }
    public Order Orders { get; set; }
}