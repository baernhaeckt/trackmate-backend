namespace Trackmate.Backend.TrackNodes;

public record UploadPictureModel(Guid TrackNodeId, string ImageDataBase64, string MimeType);
