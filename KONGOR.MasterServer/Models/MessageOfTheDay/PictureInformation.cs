namespace KONGOR.MasterServer.Models.MessageOfTheDay;

/// <summary>
///     What a picture's header says about it: its media type and its size in pixels.
/// </summary>
public record PictureInformation(string ContentType, int Width, int Height);
