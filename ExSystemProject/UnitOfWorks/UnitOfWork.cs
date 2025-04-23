using ExSystemProject.Models;
using ExSystemProject.Repository;

namespace ExSystemProject.UnitOfWorks
{
    public class UnitOfWork
    {
        public ExSystemTestContext context { get; }
        BranchRepo BranchRepo;
        ChoicesRepo ChoicesRepo;
        CourseRepo CourseRepo;
        ExamRepo ExamRepo;
        InstructorRepo InstructorRepo;
        QuestionRepo QuestionRepo;
        StudentRepo StudentRepo;
        StudentAnswerRepo StudentAnswerRepo;
        StudentCourseRepo StudentCourseRepo;
        StudentExamRepo StudentExamRepo;
        TopicRepo TopicRepo;
        TrackRepo TrackRepo;
        UserRepo UserRepo;
        UserAssignmentRepo UserAssignmentRepo;

        // Admin repositories
        AdminBranchRepo AdminBranchRepo;
        AdminChoicesRepo AdminChoicesRepo;
        AdminCourseRepo AdminCourseRepo;
        AdminExamRepo AdminExamRepo;
        AdminInstructorRepo AdminInstructorRepo;
        AdminQuestionRepo AdminQuestionRepo;
        AdminStudentRepo AdminStudentRepo;
        AdminTopicRepo AdminTopicRepo;
        AdminTrackRepo AdminTrackRepo;

        public UnitOfWork(ExSystemTestContext context)
        {
            this.context = context;
        }

        public BranchRepo branchRepo
        {
            get
            {
                if (BranchRepo == null)
                {
                    BranchRepo = new BranchRepo(context);
                }
                return BranchRepo;
            }
        }
        public ChoicesRepo choicesRepo
        {
            get
            {
                if (ChoicesRepo == null)
                {
                    ChoicesRepo = new ChoicesRepo(context);
                }
                return ChoicesRepo;
            }
        }
        public CourseRepo courseRepo
        {
            get
            {
                if (CourseRepo == null)
                {
                    CourseRepo = new CourseRepo(context);
                }
                return CourseRepo;
            }
        }
        public ExamRepo examRepo
        {
            get
            {
                if (ExamRepo == null)
                {
                    ExamRepo = new ExamRepo(context);
                }
                return ExamRepo;
            }
        }
        public InstructorRepo instructorRepo
        {
            get
            {
                if (InstructorRepo == null)
                {
                    InstructorRepo = new InstructorRepo(context);
                }
                return InstructorRepo;
            }
        }
        public QuestionRepo questionRepo
        {
            get
            {
                if (QuestionRepo == null)
                {
                    QuestionRepo = new QuestionRepo(context);
                }
                return QuestionRepo;
            }
        }
        public StudentRepo studentRepo
        {
            get
            {
                if (StudentRepo == null)
                {
                    StudentRepo = new StudentRepo(context);
                }
                return StudentRepo;
            }
        }
        public StudentAnswerRepo studentAnswerRepo
        {
            get
            {
                if (StudentAnswerRepo == null)
                {
                    StudentAnswerRepo = new StudentAnswerRepo(context);
                }
                return StudentAnswerRepo;
            }
        }
        public StudentCourseRepo studentCourseRepo
        {
            get
            {
                if (StudentCourseRepo == null)
                {
                    StudentCourseRepo = new StudentCourseRepo(context);
                }
                return StudentCourseRepo;
            }
        }
        public StudentExamRepo studentExamRepo
        {
            get
            {
                if (StudentExamRepo == null)
                {
                    StudentExamRepo = new StudentExamRepo(context);
                }
                return StudentExamRepo;
            }
        }
        public TopicRepo topicRepo
        {
            get
            {
                if (TopicRepo == null)
                {
                    TopicRepo = new TopicRepo(context);
                }
                return TopicRepo;
            }
        }
        public TrackRepo trackRepo
        {
            get
            {
                if (TrackRepo == null)
                {
                    TrackRepo = new TrackRepo(context);
                }
                return TrackRepo;
            }
        }
        public UserRepo userRepo
        {
            get
            {
                if (UserRepo == null)
                {
                    UserRepo = new UserRepo(context);
                }
                return UserRepo;
            }
        }
        public UserAssignmentRepo userAssignmentRepo
        {
            get
            {
                if (UserAssignmentRepo == null)
                {
                    UserAssignmentRepo = new UserAssignmentRepo(context);
                }
                return UserAssignmentRepo;
            }
        }

        // Admin repositories properties
        public AdminBranchRepo adminBranchRepo
        {
            get
            {
                if (AdminBranchRepo == null)
                {
                    AdminBranchRepo = new AdminBranchRepo(context);
                }
                return AdminBranchRepo;
            }
        }

        public AdminChoicesRepo adminChoicesRepo
        {
            get
            {
                if (AdminChoicesRepo == null)
                {
                    AdminChoicesRepo = new AdminChoicesRepo(context);
                }
                return AdminChoicesRepo;
            }
        }

        public AdminCourseRepo adminCourseRepo
        {
            get
            {
                if (AdminCourseRepo == null)
                {
                    AdminCourseRepo = new AdminCourseRepo(context);
                }
                return AdminCourseRepo;
            }
        }

        public AdminExamRepo adminExamRepo
        {
            get
            {
                if (AdminExamRepo == null)
                {
                    AdminExamRepo = new AdminExamRepo(context);
                }
                return AdminExamRepo;
            }
        }

        public AdminInstructorRepo adminInstructorRepo
        {
            get
            {
                if (AdminInstructorRepo == null)
                {
                    AdminInstructorRepo = new AdminInstructorRepo(context);
                }
                return AdminInstructorRepo;
            }
        }

        public AdminQuestionRepo adminQuestionRepo
        {
            get
            {
                if (AdminQuestionRepo == null)
                {
                    AdminQuestionRepo = new AdminQuestionRepo(context);
                }
                return AdminQuestionRepo;
            }
        }

        public AdminStudentRepo adminStudentRepo
        {
            get
            {
                if (AdminStudentRepo == null)
                {
                    AdminStudentRepo = new AdminStudentRepo(context);
                }
                return AdminStudentRepo;
            }
        }

        public AdminTopicRepo adminTopicRepo
        {
            get
            {
                if (AdminTopicRepo == null)
                {
                    AdminTopicRepo = new AdminTopicRepo(context);
                }
                return AdminTopicRepo;
            }
        }

        public AdminTrackRepo adminTrackRepo
        {
            get
            {
                if (AdminTrackRepo == null)
                {
                    AdminTrackRepo = new AdminTrackRepo(context);
                }
                return AdminTrackRepo;
            }
        }

        public void save()
        {
            context.SaveChanges();
        }
    }
}
