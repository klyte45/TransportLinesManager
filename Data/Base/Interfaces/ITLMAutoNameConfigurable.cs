namespace Klyte.TransportLinesManager.Xml
{
    public interface ITLMAutoNameConfigurable
    {
        bool UseInAutoName { get; set; }
        string NamingPrefix { get; set; }
    }

}
