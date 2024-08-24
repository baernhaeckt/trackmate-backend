namespace Trackmate.Backend.Models;

/// <summary>
/// 
/// </summary>
/// <param name="Alpha">Horziontal angle, 0° is north.</param>
/// <param name="Beta">Vertical angle.</param>
public record Orientation(double Alpha, double Beta);