namespace Trackmate.Backend.Tracks;

public record UpdateTrackModel(string TrackId, string MimeType, Stream ImageData, Func<Task> CallBack);