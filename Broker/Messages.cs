namespace Broker;

public static class Messages
{
    public static Queue<byte[]> Queue;

    static Messages()
    {
        Queue = new Queue<byte[]>();
    }
}