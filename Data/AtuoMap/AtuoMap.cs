using AutoMapper;
using Person.Data.Model;
using Person.Data.ResponseDto;

namespace Form_Project.Mapper
{
    public class AtuoMap : Profile
    {
        public AtuoMap()
        {
            CreateMap<EPerson, ResponsPersonDTO>().ReverseMap();

        }
    }
}