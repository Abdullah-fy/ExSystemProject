﻿using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using System;
using System.Linq;

namespace ExSystemProject.MappinConfig
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DateOnly, DateTime>().ConvertUsing(src => src.ToDateTime(TimeOnly.MinValue));
            CreateMap<DateTime, DateOnly>().ConvertUsing(src => DateOnly.FromDateTime(src));
            CreateMap<DateOnly?, DateTime?>().ConvertUsing((src, dest) => src.HasValue ? src.Value.ToDateTime(TimeOnly.MinValue) : null);
            CreateMap<DateTime?, DateOnly?>().ConvertUsing((src, dest) => src.HasValue ? DateOnly.FromDateTime(src.Value) : null);




            CreateMap<UserAssignment, SupervisorDTO>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.BranchName))
                .ForMember(dest => dest.TrackName, opt => opt.MapFrom(src => src.Track.TrackName))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Isactive ?? false))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.User.Img));

            CreateMap<SupervisorDTO, UserAssignment>()
                .ForMember(dest => dest.Isactive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.User, opt => opt.Ignore());


            CreateMap<Course, CourseDTO>()
                .ForMember(dest => dest.InstructorName,
                    opt => opt.MapFrom(src => src.Ins != null && src.Ins.User != null ?
                        src.Ins.User.Username : null))
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(src => src.description));

            CreateMap<CourseDTO, Course>()
                .ForMember(dest => dest.description,
                    opt => opt.MapFrom(src => src.Description));


            CreateMap<Question, QuestionBankDTO>()
                .ForMember(dest => dest.ExamName,
                    opt => opt.MapFrom(src => src.Exam != null ? src.Exam.ExamName : null));

            CreateMap<QuestionBankDTO, Question>();

            CreateMap<Choice, ChoiceDTO>();
            CreateMap<ChoiceDTO, Choice>();

            CreateMap<Student, StudentDTO>()
              .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
              .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
              .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender))
              .ForMember(dest => dest.TrackName, opt => opt.MapFrom(src => src.Track.TrackName))
              .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.Track.BranchId))
              .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Track.Branch.BranchName))
                .ForMember(dest => dest.StudentCourses, opt => opt.MapFrom(src => src.StudentCourses))
                .ForMember(dest => dest.StudentExams, opt => opt.MapFrom(src => src.StudentExams));

            CreateMap<StudentCourse, StudentCourseDTO>()
                .ForMember(dest => dest.CourseName,
                    opt => opt.MapFrom(src => src.Crs != null ? src.Crs.CrsName : null))
                .ForMember(dest => dest.CoursePeriod,
                    opt => opt.MapFrom(src => src.Crs != null ? src.Crs.CrsPeriod : null));

            CreateMap<StudentExam, StudentExamDTO>()
                .ForMember(dest => dest.ExamName,
                    opt => opt.MapFrom(src => src.Exam != null ? src.Exam.ExamName : null))
                .ForMember(dest => dest.CourseName,
                    opt => opt.MapFrom(src => src.Exam != null && src.Exam.Crs != null ?
                        src.Exam.Crs.CrsName : null))
                .ForMember(dest => dest.ExaminationDate,
                    opt => opt.MapFrom(src => src.ExaminationDate));


            CreateMap<UserAssignment, ManagerDTO>()
                .ForMember(dest => dest.AssignmentId, opt => opt.MapFrom(src => src.AssignmentId))
                .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.BranchId))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.BranchName))
                .ForMember(dest => dest.Isactive, opt => opt.MapFrom(src => src.Isactive))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender))
                .ForMember(dest => dest.Img, opt => opt.MapFrom(src => src.User.Img));



            CreateMap<Instructor, InstructorDTO>()
    .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
    .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender))
    .ForMember(dest => dest.TrackName, opt => opt.MapFrom(src => src.Track.TrackName))
    .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.Track.BranchId))
    .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Track.Branch.BranchName))
    .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.User.Img))
    .ForMember(dest => dest.AssignedCourses, opt => opt.MapFrom(src => src.Courses));

            CreateMap<InstructorDTO, Instructor>()
                .ForPath(dest => dest.User.Username, opt => opt.MapFrom(src => src.Username))
                .ForPath(dest => dest.User.Email, opt => opt.MapFrom(src => src.Email))
                .ForPath(dest => dest.User.Gender, opt => opt.MapFrom(src => src.Gender));

            CreateMap<Exam, ExamDTO>()
                .ForMember(dest => dest.CourseName,
                    opt => opt.MapFrom(src => src.Crs != null ? src.Crs.CrsName : null))
                .ForMember(dest => dest.InstructorName,
                    opt => opt.MapFrom(src => src.Ins != null && src.Ins.User != null ?
                        src.Ins.User.Username : null));

            CreateMap<ExamDTO, Exam>();

            CreateMap<Track, TrackDTO>()
                .ForMember(dest => dest.branch_name,
                    opt => opt.MapFrom(src => src.Branch != null ? src.Branch.BranchName : null));

            CreateMap<TrackDTO, Track>();

            CreateMap<Branch, BranchDTO>();
            CreateMap<BranchDTO, Branch>();

            CreateMap<UserAssignment, SupervisorEditDTO>()
    .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
    .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender))
    .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.BranchName))
    .ForMember(dest => dest.TrackName, opt => opt.MapFrom(src => src.Track.TrackName))
    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Isactive ?? true))
    .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.User.Img));

            CreateMap<SupervisorEditDTO, UserAssignment>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Branch, opt => opt.Ignore())
                .ForMember(dest => dest.Track, opt => opt.Ignore())
                .ForMember(dest => dest.Isactive, opt => opt.MapFrom(src => src.IsActive));

        }
    }

    public class DateConverter : IValueConverter<object, DateTime?>
    {
        public DateTime? Convert(object sourceMember, ResolutionContext context)
        {
            if (sourceMember == null)
                return null;

            if (sourceMember is DateTime dateTime)
                return dateTime;

            if (sourceMember is DateOnly dateOnly)
                return dateOnly.ToDateTime(TimeOnly.MinValue);

            return null;
        }
    }
}