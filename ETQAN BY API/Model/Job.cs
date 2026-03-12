public class Job
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public ICollection<Artisan>? Artisans { get; set; }
}