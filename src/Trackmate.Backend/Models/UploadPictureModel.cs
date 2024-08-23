namespace Trackmate.Backend.Models;

public record UploadPictureModel(Guid TrackNodeId, string MimeType, Stream imageData);
