namespace SpotifyNewReleases.Exceptions;

public class DeserializationException(Exception? innerException = null) : Exception("Error on deserialization", innerException)
{
}
