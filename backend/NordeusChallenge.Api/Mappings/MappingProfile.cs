using AutoMapper;
using NordeusChallenge.Api.Dtos;
using NordeusChallenge.Api.Models;

namespace NordeusChallenge.Api.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Stats, StatsDto>();
        CreateMap<MoveEffect, MoveEffectDto>();
        CreateMap<Move, MoveDto>();
    }
}
