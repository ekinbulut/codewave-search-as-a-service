namespace Contracts;


public class IndexCreated 
{
    public Guid Id { get; set; }

    public DateTime Published => DateTime.UtcNow;

    public string Message { get; set; }

    public object? Data { get; set; }

}