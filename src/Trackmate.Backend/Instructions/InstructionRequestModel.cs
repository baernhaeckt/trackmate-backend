using Trackmate.Backend.Models;

namespace Trackmate.Backend.Instructions;

public record InstructionRequestModel(GeoLocation Start, GeoLocation End);
