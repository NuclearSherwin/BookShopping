using System.ComponentModel.DataAnnotations;

namespace BookShopping.Models;

public class FileModel
{
    [Key]
    private int Id { get; set; }
    public string Name { get; set; }
    public byte[] Data { get; set; }
}