using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ExSystemProject.Models;

public partial class ExSystemTestContext : DbContext
{
    public ExSystemTestContext()
    {
    }

    public ExSystemTestContext(DbContextOptions<ExSystemTestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Choice> Choices { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentAnswer> StudentAnswers { get; set; }

    public virtual DbSet<StudentCourse> StudentCourses { get; set; }

    public virtual DbSet<StudentExam> StudentExams { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<Track> Tracks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAssignment> UserAssignments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=ExSystemTest;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.BranchId).HasName("PK__Branch__E55E37DED1D72522");

            entity.ToTable("Branch", tb => tb.HasTrigger("trg_Branch_Status"));

            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.BranchName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("branch_name");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("location");
        });

        modelBuilder.Entity<Choice>(entity =>
        {
            entity.HasKey(e => e.ChoiceId).HasName("PK__Choice__33CAF83AB5062A90");

            entity.ToTable("Choice");

            entity.Property(e => e.ChoiceId).HasColumnName("choice_id");
            entity.Property(e => e.ChoiceText)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("choice_text");
            entity.Property(e => e.IsCorrect).HasColumnName("is_correct");
            entity.Property(e => e.QuesId).HasColumnName("ques_id");

            entity.HasOne(d => d.Ques).WithMany(p => p.Choices)
                .HasForeignKey(d => d.QuesId)
                .HasConstraintName("FK__Choice__ques_id__6754599E");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CrsId).HasName("PK__Courses__56CAA5D59FA5B4DC");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_Course_Status");
                    tb.HasTrigger("trg_Course_StudentCourse_Status");
                    tb.HasTrigger("trg_Course_Topic_Status");
                });

            entity.Property(e => e.CrsId).HasColumnName("Crs_Id");
            entity.Property(e => e.CrsName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Crs_Name");
            entity.Property(e => e.CrsPeriod).HasColumnName("Crs_period");
            entity.Property(e => e.InsId).HasColumnName("ins_id");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");

            entity.HasOne(d => d.Ins).WithMany(p => p.Courses)
                .HasForeignKey(d => d.InsId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Courses__ins_id__4F7CD00D");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.ExamId).HasName("PK__Exam__9C8C7BE936A40608");

            entity.ToTable("Exam", tb => tb.HasTrigger("trg_UpdatePassedGrade"));

            entity.HasIndex(e => new { e.ExamName, e.CrsId }, "UQ_ExamName_PerCourse").IsUnique();

            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.CrsId).HasColumnName("crs_id");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("endTime");
            entity.Property(e => e.ExamName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("exam_name");
            entity.Property(e => e.InsId).HasColumnName("ins_id");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.PassedGrade).HasColumnName("passedGrade");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("startTime");

            entity.HasOne(d => d.Crs).WithMany(p => p.Exams)
                .HasForeignKey(d => d.CrsId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Exam__crs_id__5EBF139D");

            entity.HasOne(d => d.Ins).WithMany(p => p.Exams)
                .HasForeignKey(d => d.InsId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Exam__ins_id__5FB337D6");
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.InsId).HasName("PK__Instruct__151409ED7C2DA571");

            entity.ToTable("Instructor", tb =>
                {
                    tb.HasTrigger("trg_Instructor_Deactive");
                    tb.HasTrigger("trg_prevent_instructor_to_be_student");
                });

            entity.Property(e => e.InsId).HasColumnName("Ins_Id");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Salary).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TrackId).HasColumnName("track_id");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Track).WithMany(p => p.Instructors)
                .HasForeignKey(d => d.TrackId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Instructo__track__45F365D3");

            entity.HasOne(d => d.User).WithMany(p => p.Instructors)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Instructo__userI__44FF419A");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuesId).HasName("PK__Question__5BF46E9B36ED1B1B");

            entity.ToTable("Question");

            entity.Property(e => e.QuesId).HasColumnName("ques_id");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.QuesScore).HasColumnName("ques_score");
            entity.Property(e => e.QuesText)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ques_text");
            entity.Property(e => e.QuesType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ques_type");

            entity.HasOne(d => d.Exam).WithMany(p => p.Questions)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Question__exam_i__6477ECF3");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Student__32C52B996985FF66");

            entity.ToTable("Student", tb =>
                {
                    tb.HasTrigger("trg_Student_StudentCourse_Status");
                    tb.HasTrigger("trg_prevent_student_to_be_instructor");
                    tb.HasTrigger("trg_prevent_student_to_be_superviosor_or_admin");
                });

            entity.Property(e => e.EnrollmentDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.TrackId).HasColumnName("track_id");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Track).WithMany(p => p.Students)
                .HasForeignKey(d => d.TrackId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Student__track_i__4BAC3F29");

            entity.HasOne(d => d.User).WithMany(p => p.Students)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Student__userId__4AB81AF0");
        });

        modelBuilder.Entity<StudentAnswer>(entity =>
        {
            entity.HasKey(e => e.Answerid).HasName("PK__Student___6837BD9C473F7ACB");

            entity.ToTable("Student_Answer");

            entity.Property(e => e.Answerid).HasColumnName("answerid");
            entity.Property(e => e.ChoiceId).HasColumnName("choice_id");
            entity.Property(e => e.QuesId).HasColumnName("ques_id");
            entity.Property(e => e.Studentid).HasColumnName("studentid");

            entity.HasOne(d => d.Choice).WithMany(p => p.StudentAnswers)
                .HasForeignKey(d => d.ChoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student_A__choic__6C190EBB");

            entity.HasOne(d => d.Ques).WithMany(p => p.StudentAnswers)
                .HasForeignKey(d => d.QuesId)
                .HasConstraintName("FK__Student_A__ques___6B24EA82");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentAnswers)
                .HasForeignKey(d => d.Studentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student_A__stude__6A30C649");
        });

        modelBuilder.Entity<StudentCourse>(entity =>
        {
            entity.HasKey(e => new { e.CrsId, e.StudentId });

            entity.ToTable("Student_Course");

            entity.Property(e => e.CrsId).HasColumnName("Crs_Id");
            entity.Property(e => e.EnrolledAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Grade).HasColumnName("grade");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Ispassed).HasColumnName("ispassed");

            entity.HasOne(d => d.Crs).WithMany(p => p.StudentCourses)
                .HasForeignKey(d => d.CrsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student_C__Crs_I__5441852A");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentCourses)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student_C__Stude__5535A963");
        });

        modelBuilder.Entity<StudentExam>(entity =>
        {
            entity.HasKey(e => e.StudentExamId).HasName("PK__Student___C5794976FC4AA087");

            entity.ToTable("Student_Exam", tb => tb.HasTrigger("trg_UpdatePassFail"));

            entity.HasIndex(e => new { e.ExamId, e.StudentId }, "unique_exam_student").IsUnique();

            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.ExaminationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("examination_date");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.PassFail)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("pass_fail");

            entity.HasOne(d => d.Exam).WithMany(p => p.StudentExams)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Student_E__exam___72C60C4A");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentExams)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student_E__Stude__71D1E811");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.TopicId).HasName("PK__Topic__D5DAA3E9457ABD66");

            entity.ToTable("Topic");

            entity.Property(e => e.TopicId).HasColumnName("topic_id");
            entity.Property(e => e.CrsId).HasColumnName("Crs_id");
            entity.Property(e => e.Descrtption)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descrtption");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.TopicName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("topic_name");

            entity.HasOne(d => d.Crs).WithMany(p => p.Topics)
                .HasForeignKey(d => d.CrsId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Topic__Crs_id__59063A47");
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasKey(e => e.TrackId).HasName("PK__Track__24ECC82E4A5CEF87");

            entity.ToTable("Track", tb => tb.HasTrigger("trg_Track_Status"));

            entity.Property(e => e.TrackId).HasColumnName("track_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.TrackDuration).HasColumnName("track_duration");
            entity.Property(e => e.TrackIntake).HasColumnName("track_intake");
            entity.Property(e => e.TrackName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("track_name");

            entity.HasOne(d => d.Branch).WithMany(p => p.Tracks)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Track_Branch");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__CB9A1CFFD17088C6");

            entity.ToTable(tb => tb.HasTrigger("trg_User_Status"));

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E6164A617A50E").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("gender");
            entity.Property(e => e.Img)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("img");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.Upassword)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__UserAssi__52C21820E129E02C");

            entity.ToTable("UserAssignment", tb => tb.HasTrigger("trg_prevent_superviosor_and_admin_to_be_student"));

            entity.Property(e => e.AssignmentId).HasColumnName("assignmentId");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.TrackId).HasColumnName("track_id");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Branch).WithMany(p => p.UserAssignments)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__UserAssig__branc__778AC167");

            entity.HasOne(d => d.Track).WithMany(p => p.UserAssignments)
                .HasForeignKey(d => d.TrackId)
                .HasConstraintName("FK__UserAssig__track__787EE5A0");

            entity.HasOne(d => d.User).WithMany(p => p.UserAssignments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserAssig__userI__76969D2E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
