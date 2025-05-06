using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExSystemProject.MappinConfig
{
    public class MmappingProfile  : Profile
    {
        public MmappingProfile()
        {
            CreateMap<Student, MStudentDTO>().AfterMap((src, dest) =>
            {
                dest.id = src.StudentId;
                dest.UserId = (int)src.UserId;
                dest.Username = src.User?.Username;
                dest.Email = src.User?.Email;
                dest.Gender = src.User?.Gender;
                dest.Image = src.User?.Img;
                dest.TrackName = src.Track?.TrackName;
                dest.Isactive = src.Isactive;
                dest.EnrollmentDate = src.EnrollmentDate;             
            }).ReverseMap();
            CreateMap<Course, CourseNewDTO>()
             .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Ins != null && src.Ins.User != null ? src.Ins.User.Username : null))
            .ForMember(dest => dest.TrackName, opt => opt.MapFrom(src => src.Ins != null && src.Ins.Track != null ? src.Ins.Track.TrackName : null))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Ins != null && src.Ins.Track != null && src.Ins.Track.Branch != null ? src.Ins.Track.Branch.BranchName : null))
            .ForMember(dest => dest.TrackId, opt => opt.MapFrom(src => src.Ins != null ? src.Ins.TrackId : null))
            .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.Ins != null && src.Ins.Track != null ? src.Ins.Track.BranchId : null));
        }
    }
}
