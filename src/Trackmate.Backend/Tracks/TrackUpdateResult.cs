namespace Trackmate.Backend.Tracks;

public record TrackUpdateResult(TrackUpdateResultType type, string? instruction = null, Stream? instructionAudio = null)
{
    public static TrackUpdateResult NoLocation
        => new TrackUpdateResult(TrackUpdateResultType.NoLocation);

    public static TrackUpdateResult LocationUpdated
        => new TrackUpdateResult(TrackUpdateResultType.LocationUpdated);

    public static TrackUpdateResult NewInstruction(string instruction, Stream instructionAudio)
        => new TrackUpdateResult(TrackUpdateResultType.NewInstruction, instruction, instructionAudio);
}
