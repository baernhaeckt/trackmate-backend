﻿using Microsoft.Extensions.Logging;
using Trackmate.Backend.Embeddings;
using Trackmate.Backend.Models;
using Trackmate.Backend.TrackNodes;

namespace Trackmate.Backend;

public class TrackNodeService(
    ILogger<TrackNodeService> logger, 
    PictureEmbeddingClient embeddingClient,
    ITrackNodeDataSource trackNodeDataSource)
{
    public async Task<TrackNodeModel> CreateTrackNodeAsync(CreateTrackNodeModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating track node with location: {location} and vector: {vector}", model.Location, model.Vector);
        return await trackNodeDataSource.CreateTrackNodeAsync(model, cancellationToken);
    }

    public async Task<TrackNodeModel> UploadTrackNodePictureAsync(UploadPictureModel uploadPictureModel, CancellationToken cancellationToken)
    {
        logger.LogInformation("Enriching track node (id:{trackNodeId} with picture embedding.", uploadPictureModel.TrackNodeId);

        PictureEmbeddingModel embedding = await embeddingClient.GeneratePictureEmbeddingAsync(uploadPictureModel.MimeType, uploadPictureModel.ImageDataBase64);
        return await trackNodeDataSource.AppendEmbeddingAsync(uploadPictureModel.TrackNodeId, embedding, cancellationToken);
    }

    public async Task<TrackNodeModel[]> GetAllTrackNodesAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting all track nodes.");
	    return await trackNodeDataSource.GetAllTrackNodesAsync(cancellationToken);
    }
}