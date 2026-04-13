namespace ServiceStackApp;

public static class MessageFormatter
{
    public static string Format(int partition, long offset, string text) =>
        $"Partition: {partition} | Offset: {offset} | Message: {text}";
}
