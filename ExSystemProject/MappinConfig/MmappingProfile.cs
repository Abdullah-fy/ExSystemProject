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

        }
    }
}
