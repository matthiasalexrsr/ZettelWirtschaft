namespace DocuDesk.Domain.Entities;

public sealed class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; set; }
}
