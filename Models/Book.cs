using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace BookShopping.Models;

public class Book
{
    [Key] 
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Title is required")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Description is required")] 
    public string Description { get; set; }
    
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public double Price { get; set; }

    [Required(ErrorMessage = "Author is required")]
    public string Author { get; set; }

    
    [Required(ErrorMessage = "Number of pages is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Number of pages must be greater than 0")]
    public int NoPage { get; set; }
    
    public int FileId { get; set; }
    
    
    [ForeignKey("FileId")] 
    public FileModel FileModel { get; set; }
    
    
    
    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }
    
    
    //-----------------------------------------------
    [ForeignKey("CategoryId")]
    public Category Category { get; set; }
    
}