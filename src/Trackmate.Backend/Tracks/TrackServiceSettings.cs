namespace Trackmate.Backend.Tracks;

public class TrackServiceSettings
{
    /// <summary>
    ///    No repetetive instructions will be given to the user within this time span.
    /// </summary>
    public TimeSpan InstructionTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    ///   The angle threshold in degrees to determine if a instruction is necessary.
    /// </summary>
    public double AngleThresholdInDegrees { get; set; } = 25;
}
