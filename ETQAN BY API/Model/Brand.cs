using ETQAN.API.Models;

public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string UrlImage { get; set; }

    // ✅ السطر ده ضروري عشان ميثود GetBrandWithProducts تشتغل صح
    public ICollection<Product> Products { get; set; } = new List<Product>();
}