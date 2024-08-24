namespace Trackmate.Backend.Tracks;

public enum TrackUpdateResultType
{
    /// <summary>
    ///     The uploaded image could not be matched to any track node.
    /// </summary>
    NoLocation,

    /// <summary>
    ///    New instruction is passed to be provided to the user.
    /// </summary>
    NewInstruction,

    /// <summary>
    ///    The location of the track has been updated, no new instruction is needed.
    /// </summary>
    LocationUpdated
}