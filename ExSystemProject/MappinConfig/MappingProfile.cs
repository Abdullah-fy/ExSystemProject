
using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExSystemProject.MappinConfig
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Course mappings
            CreateMap<Course, CourseDTO>()
                .ForMember(dest => dest.InstructorName,
                    opt => opt.MapFrom(src => src.Ins != null && src.Ins.User != null ?
                        src.Ins.User.Username : null));

            CreateMap<CourseDTO, Course>();

            // Question and Choice mappings
            CreateMap<Question, QuestionBankDTO>()
                .ForMember(dest => dest.ExamName,
                    opt => opt.MapFrom(src => src.Exam != null ? src.Exam.ExamName : null));

            CreateMap<QuestionBankDTO, Question>();

            CreateMap<Choice, ChoiceDTO>();
            CreateMap<ChoiceDTO, Choice>();

            // Student mappings
            CreateMap<Student, StudentDTO>()
                .ForMember(dest => dest.Username,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Username : null))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.Gender,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Gender : null))
                .ForMember(dest => dest.TrackName,
                    opt => opt.MapFrom(src => src.Track != null ? src.Track.TrackName : null));

            // Instructor mappings
            CreateMap<Instructor, InstructorDTO>()
                .ForMember(dest => dest.Username,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Username : null))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.Gender,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Gender : null))
                .ForMember(dest => dest.TrackName,
                    opt => opt.MapFrom(src => src.Track != null ? src.Track.TrackName : null))
                .ForMember(dest => dest.AssignedCourses,
                    opt => opt.MapFrom(src => src.Courses));

            // Exam mappings
            CreateMap<Exam, ExamDTO>()
                .ForMember(dest => dest.CourseName,
                    opt => opt.MapFrom(src => src.Crs != null ? src.Crs.CrsName : null))
                .ForMember(dest => dest.InstructorName,
                    opt => opt.MapFrom(src => src.Ins != null && src.Ins.User != null ?
                        src.Ins.User.Username : null));

            CreateMap<ExamDTO, Exam>();
        }
    }
}
