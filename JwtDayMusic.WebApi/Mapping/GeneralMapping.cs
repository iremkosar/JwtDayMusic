using AutoMapper;
using JwtDayMusic.WebApi.Dtos;
using JwtDayMusic.WebApi.Entities;

namespace JwtDayMusic.WebApi.Mapping
{
    public class GeneralMapping:Profile
    {
        public GeneralMapping() 
        { 
            CreateMap<Artist,ResultArtistDto>().ReverseMap();
            CreateMap<Artist,CreateArtistDto>().ReverseMap();
        }
    }
}
