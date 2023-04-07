﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace BookShopping.Models;

public class Book
{
    [Key] 
    public int Id { get; set; }
    
    [Required] 
    public string Name { get; set; }
    
    [Required] 
    public string Description { get; set; }
    
    [Required] 
    [Range(10, 10000)] 
    public double Price { get; set; }

    [Required] 
    public string Author { get; set; }

    [Required] 
    public int NoPage { get; set; }
    
    public int FileId { get; set; }
    [ForeignKey("FileId")] 
    public FileModel FileModel { get; set; }
    
    [Required] 
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public Category Category { get; set; }
    
}