using Microsoft.Extensions.Logging;
using Trackmate.Backend.Models;

namespace Trackmate.Backend;

public class TrackNodeService(ILogger<TrackNodeService> Logger)
{
    public async Task<TrackNodeModel> CreateTrackNode(CreateTrackNodeModel model)
    {
        Logger.LogInformation("Creating TrackNode with Location: {Location} and Vector: {Vector}", model.Location, model.Vector);
        return new TrackNodeModel(Guid.NewGuid(), model.Location, model.Vector);
    }

    public Task UploadPicture(UploadPictureModel uploadPictureModel)
    {
        return Task.CompletedTask;
    }
}