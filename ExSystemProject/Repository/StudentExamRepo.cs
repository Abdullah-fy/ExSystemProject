using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExSystemProject.Repository
{
    public class StudentExamRepo:GenaricRepo<StudentExam>
    {
        ExSystemTestContext _context;
        public StudentExamRepo(ExSystemTestContext context):base(context) 
        {
           _context = context;
        }
        public List<StudentExam> getStudentExamByStudentAndCourse(int studentId, int courseId)
        {
            var result = _context.StudentExams.FromSqlRaw("EXEC GetStudentExamByStudentAndCourse @studentId, @courseId",
                    new SqlParameter("@studentId",  studentId),
                    new SqlParameter("@courseId", courseId)
                    ).ToList();
            return result;
        }


        public IEnumerable<StudentExam> getActiveById(int examId)
        {
            return _context.StudentExams
                .Where(se => se.ExamId == examId && se.Isactive == true)
                .ToList();
        }
        public List<StudentExam> GetStudentExamsByExamId(int examId)
        {
            return _context.StudentExams
                .Include(se => se.Student)
                    .ThenInclude(s => s.User)
                .Where(se => se.ExamId == examId)
                .ToList();
        }

        public List<GetAssignExamToStudentDTO> GetAssignExamToStudent(int studentid)
        {
            var exams = new List<GetAssignExamToStudentDTO>();

            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "EXEC GetAssignedExamsForStudent @StudentID";
                    command.CommandType = CommandType.Text;

                    var param = command.CreateParameter();
                    param.ParameterName = "@StudentID";
                    param.Value = studentid;
                    param.DbType = DbType.Int32;
                    command.Parameters.Add(param);

                    _context.Database.OpenConnection();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var exam = new GetAssignExamToStudentDTO
                            {
                                ExamID = reader["exam_id"] != DBNull.Value ? Convert.ToInt32(reader["exam_id"]) : 0,
                                ExamName = reader["exam_name"]?.ToString(),
                                StartTime = reader["startTime"] != DBNull.Value ? Convert.ToDateTime(reader["startTime"]) : DateTime.MinValue,
                                EndTime = reader["endTime"] != DBNull.Value ? Convert.ToDateTime(reader["endTime"]) : DateTime.MinValue,
                                ExamDate = reader["examination_date"] != DBNull.Value
                                    ? DateOnly.FromDateTime(Convert.ToDateTime(reader["examination_date"]))
                                    : DateOnly.MinValue,
                                isactive = reader["isactive"] != DBNull.Value && Convert.ToBoolean(reader["isactive"])
                            };
                            exams.Add(exam);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAssignExamToStudent: {ex.Message}");
            }
            finally
            {
                _context.Database.CloseConnection();
            }

            return exams;
        }
        public List<QuestionDTO> GetExamQuestionsAndChoices(int examId)
        {
            var questionsDict = new Dictionary<int, QuestionDTO>();

            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "EXEC sp_GetExamQuestionsAndChoices @exam_id";
                command.CommandType = CommandType.Text;

                var param = command.CreateParameter();
                param.ParameterName = "@exam_id";
                param.Value = examId;
                param.DbType = DbType.Int32;
                command.Parameters.Add(param);

                _context.Database.OpenConnection();

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int quesId = Convert.ToInt32(reader["ques_id"]);

                    if (!questionsDict.ContainsKey(quesId))
                    {
                        questionsDict[quesId] = new QuestionDTO
                        {
                            QuesId = quesId,
                            QuesText = reader["ques_text"]?.ToString(),
                            QuesType = reader["ques_type"]?.ToString(),
                            QuesScore = reader["ques_score"] != DBNull.Value ? Convert.ToInt32(reader["ques_score"]) : 0
                        };
                    }

                    questionsDict[quesId].Choices.Add(new ChoiceDTO
                    {
                        ChoiceId = reader["choice_id"] != DBNull.Value ? Convert.ToInt32(reader["choice_id"]) : 0,
                        ChoiceText = reader["choice_text"]?.ToString(),
                        IsCorrect = reader["is_correct"] != DBNull.Value && Convert.ToBoolean(reader["is_correct"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetExamQuestionsAndChoices: {{ex.Message}}");
            }
            finally
            {
                _context.Database.CloseConnection();
            }

            return questionsDict.Values.ToList();
        }

        public string SubmitExamAnswer(SubmitAnswerDTO answerDto)
        {
            string resultMessage = "";

            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "sp_SubmitExamAnswers";
                command.CommandType = CommandType.StoredProcedure;

                var studentParam = command.CreateParameter();
                studentParam.ParameterName = "@student_id";
                studentParam.Value = answerDto.StudentId;
                command.Parameters.Add(studentParam);

                var examParam = command.CreateParameter();
                examParam.ParameterName = "@exam_id";
                examParam.Value = answerDto.ExamId;
                command.Parameters.Add(examParam);

                var quesParam = command.CreateParameter();
                quesParam.ParameterName = "@ques_id";
                quesParam.Value = answerDto.QuestionId;
                command.Parameters.Add(quesParam);

                var choiceParam = command.CreateParameter();
                choiceParam.ParameterName = "@choice_id";
                choiceParam.Value = answerDto.ChoiceId;
                command.Parameters.Add(choiceParam);

                _context.Database.OpenConnection();

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    resultMessage = reader["Message"].ToString();
                }
            }
            catch (Exception ex)
            {
                resultMessage = "Error submitting answer: " + ex.Message;
            }
            finally
            {
                _context.Database.CloseConnection();
            }

            return resultMessage;
        }

        public void DeactivateStudentExam(int studentId, int examId)
        {
            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "sp_DeactivateStudentExam";
                command.CommandType = CommandType.StoredProcedure;

                var studentParam = command.CreateParameter();
                studentParam.ParameterName = "@studentId";
                studentParam.Value = studentId;
                command.Parameters.Add(studentParam);

                var examParam = command.CreateParameter();
                examParam.ParameterName = "@examId";
                examParam.Value = examId;
                command.Parameters.Add(examParam);

                _context.Database.OpenConnection();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deactivating student exam: {ex.Message}");
            }
            finally
            {
                _context.Database.CloseConnection();
            }
        }

        public async Task<StudentExamResultsDTO> GetStudentExamResultsAsync(int examId, int? studentId = null)
        {
            var result = new StudentExamResultsDTO();

            try
            {
                await using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "sp_GetExamResults";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@ExamId", examId));
                command.Parameters.Add(new SqlParameter("@StudentId", studentId ?? (object)DBNull.Value));

                await _context.Database.OpenConnectionAsync();

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    result.ExamName = reader.GetString(0);
                    result.StartTime = reader.GetDateTime(1);
                    result.TotalMarks = reader.GetInt32(5);
                }

                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    result.Score = reader.GetInt32(2);
                    result.Percentage = reader.IsDBNull(4) ? null : (float?)reader.GetDouble(4); 
                    result.Result = reader.IsDBNull(3) ? null : reader.GetString(3);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error retrieving exam results", ex);
            }
            finally
            {
                if (_context.Database.GetDbConnection().State == ConnectionState.Open)
                {
                    await _context.Database.CloseConnectionAsync();
                }
            }
        }
        //public List<StudentExamResultsDTO> GetStudentExamResult(int examId, int studentId)
        //{
        //    var results = new List<StudentExamResultsDTO>();

        //    try
        //    {
        //        using (var command = _context.Database.GetDbConnection().CreateCommand())
        //        {
        //            command.CommandText = "sp_GetExamResults";
        //            command.CommandType = CommandType.StoredProcedure;

        //            var examParam = command.CreateParameter();
        //            examParam.ParameterName = "@ExamId";
        //            examParam.Value = examId;
        //            examParam.DbType = DbType.Int32;
        //            command.Parameters.Add(examParam);

        //            var studentParam = command.CreateParameter();
        //            studentParam.ParameterName = "@StudentId";
        //            studentParam.Value = studentId;
        //            studentParam.DbType = DbType.Int32;
        //            command.Parameters.Add(studentParam);

        //            _context.Database.OpenConnection();

        //            using (var reader = command.ExecuteReader())
        //            {
        //                // Skip the first result set (general exam info)
        //                if (reader.HasRows)
        //                {
        //                    reader.Read(); // we could read exam_name, startTime, etc., here if needed
        //                }

        //                // Move to second result set (student-specific result)
        //                if (reader.NextResult())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        var dto = new StudentExamResultsDTO
        //                        {
        //                            ExamName = reader["exam_name"]?.ToString(),
        //                            StartTime = reader["startTime"] != DBNull.Value ? Convert.ToDateTime(reader["startTime"]) : DateTime.MinValue,
        //                            TotalMarks = reader["TotalMarks"] != DBNull.Value ? Convert.ToInt32(reader["TotalMarks"]) : 0,
        //                            Score = reader["Score"] != DBNull.Value ? Convert.ToInt32(reader["Score"]) : 0,
        //                            Percentage = reader["Percentage"] != DBNull.Value ? Convert.ToInt32(reader["Percentage"]) : 0

        //                        };

        //                        results.Add(dto);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error: " + ex.Message);
        //    }
        //    finally
        //    {
        //        _context.Database.CloseConnection();
        //    }

        //    return results;
        //}


        internal dynamic GetByStudentId(int id)
        {
            throw new NotImplementedException();
        }


        public bool ExamAssigned(int studentId, int examId)
        {
            return _context.StudentExams
                .Any(se => se.StudentId == studentId && se.ExamId == examId);
        }


        public string SubmitEmptyExam(int studentId, int examId)
        {
            string resultMessage = "";

            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "sp_SubmitEmptyExam";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@student_id", studentId));
                command.Parameters.Add(new SqlParameter("@exam_id", examId));

                _context.Database.OpenConnection();

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    resultMessage = reader["Message"].ToString();
                }
            }
            catch (Exception ex)
            {
                resultMessage = "Error submitting empty exam: " + ex.Message;
            }
            finally
            {
                _context.Database.CloseConnection();
            }

            return resultMessage;
        }


    }
}

