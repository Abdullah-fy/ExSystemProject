using AutoMapper;
using ExSystemProject.DTOs.Student;
using ExSystemProject.Models;

namespace ExSystemProject.Profiles
{
    public class StudentProfile : Profile
    {
        public StudentProfile()
        {
            // Student -> UpdateStudentDTO
            CreateMap<Student, UpdateStudentDTO>()
                .AfterMap((src, dest) =>
                {
                    dest.StudentId = src.StudentId;
                    dest.Username = src.User.Username;
                    dest.Email = src.User.Email;
                    dest.Gender = src.User.Gender;
                    dest.TrackId = src.TrackId;
                    dest.IsActive = (bool)!src.Isactive;
                })
                .ReverseMap();

            // Student -> ShowStudentDTO
            CreateMap<Student, ShowStudentDTO>()
                .AfterMap((src, dest) =>
                {
                    dest.Username = src.User.Username;
                    dest.Email = src.User.Email;
                    dest.Gender = src.User.Gender;
                    dest.TrackName = src.Track != null ? src.Track.TrackName : "No Track";
                    dest.IsActive = !src.Isactive;
                })
                .ReverseMap();

            // CreateStudentDTO -> User
            CreateMap<CreateStudentDTO, User>()
                .AfterMap((src, dest) =>
                {
                    dest.Upassword = src.Password;
                    dest.Role = "student";
                    dest.Isactive = true;
                })
                .ReverseMap();

            // CreateStudentDTO -> Student
            CreateMap<CreateStudentDTO, Student>()
                .AfterMap((src, dest) =>
                {
                    dest.EnrollmentDate = DateOnly.FromDateTime(DateTime.Now);
                    dest.Isactive = false;
                })
                .ReverseMap();

            // UpdateStudentDTO -> User
            CreateMap<UpdateStudentDTO, User>()
                .AfterMap((src, dest) =>
                {
                    dest.Isactive = src.IsActive;
                })
                .ReverseMap();

            // UpdateStudentDTO -> Student
            CreateMap<UpdateStudentDTO, Student>()
                .AfterMap((src, dest) =>
                {
                    dest.Isactive = !src.IsActive;
                })
                .ReverseMap();
        }
    }
}
