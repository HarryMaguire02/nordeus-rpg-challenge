namespace NordeusChallenge.Api.Dtos;

// Wrapper required because Unity's JsonUtility cannot deserialize a root JSON array.
public record HeroListDto(List<HeroSummaryDto> Heroes);
