using AutoMapper;
using ExSystemProject.DTOs.Instructor;
using ExSystemProject.Models;

namespace ExSystemProject.Mapping.NewFolder.InstructorProfile
{
    public class InstructorProfile : Profile
    {
        public InstructorProfile()
        {
            // Instructor <-> InstructorDTO
            CreateMap<Instructor, InstructorDTO>()
                .AfterMap((src, dest) =>
                {
                    dest.Username = src.User.Username;
                    dest.Email = src.User.Email;
                    dest.Gender = src.User.Gender;
                    dest.TrackName = src.Track != null ? src.Track.TrackName : "No Track";
                    dest.IsActive = src.Isactive ?? false;
                })
                .ReverseMap();

            // CreateInstructorDTO <-> User
            CreateMap<CreateInstructorDTO, User>()
                .AfterMap((src, dest) =>
                {
                    dest.Upassword = src.Password;
                    dest.Role = "instructor";
                    dest.Isactive = true;
                })
                .ReverseMap();

            // CreateInstructorDTO <-> Instructor
            CreateMap<CreateInstructorDTO, Instructor>()
                .AfterMap((src, dest) =>
                {
                    dest.Isactive = true;
                })
                .ReverseMap();

            // UpdateInstructorDTO <-> User
            CreateMap<UpdateInstructorDTO, User>()
                .AfterMap((src, dest) =>
                {
                    dest.Isactive = src.IsActive;
                })
                .ReverseMap();

            // UpdateInstructorDTO <-> Instructor
            CreateMap<UpdateInstructorDTO, Instructor>()
                .AfterMap((src, dest) =>
                {
                    dest.Isactive = src.IsActive;
                })
                .ReverseMap();
        }
    }
}
