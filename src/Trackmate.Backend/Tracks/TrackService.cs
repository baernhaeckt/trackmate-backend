using Microsoft.Extensions.Logging;
using Trackmate.Backend.Embeddings;
using Trackmate.Backend.Models;
using Trackmate.Backend.TrackNodes;

namespace Trackmate.Backend.Tracks;

public class TrackService(
    ILogger<TrackService> logger, 
    ITrackDataSource trackDataSource, 
    ITrackNodeDataSource trackNodeDataSource,
    PictureEmbeddingClient pictureEmbeddingClient)
{
    public Task<TrackModel> StartTrackAsync(StartTrackModel startTrackModel)
    {

    }

    public Task<TrackModel> GetTrackAsync(string trackId)
    {

    }

    public Task<TrackModel> UpdateTrackAsync(UpdateTrackModel updateTrackModel)
    {

    }
}