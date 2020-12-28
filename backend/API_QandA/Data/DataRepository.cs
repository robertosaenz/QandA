using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Dapper;
using API_QandA.Models;


namespace API_QandA.Data
{
    public class DataRepository : IDataRepository
    {
        private readonly string _connectionString;

        public DataRepository(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionStrings:DefaultConnection"];
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestions()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<QuestionGetManyResponse>
                    (
                    @"EXEC dbo.Question_GetMany"
                    );
            }
        }

        public AnswerGetResponse GetAnswer(int answerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<AnswerGetResponse>
                (
                    @"EXEC dbo.Answer_Get_ByAnswerId @AnswerId = @AnswerId",
                    new { AnswerId = answerId }
                );
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestionsBySearch(string search)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<QuestionGetManyResponse>
                (
                    @"EXEC dbo.Question_GetMany_BySearch @Search = @Search",
                    new { Search = search } //focus
                );
            }

        }

        public QuestionGetSingleResponse GetQuestion(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // SLOW RESPONSE

                //var question = connection.QueryFirstOrDefault<QuestionGetSingleResponse>
                //(
                //    @"EXEC dbo.Question_GetSingle @QuestionId = @QuestionId",
                //    new { QuestionId = questionId }
                //);
                //// TODO - Get the answers for the question
                //if (question != null)
                //{
                //    question.Answers = connection.Query<AnswerGetResponse>
                //    (
                //        @"EXEC dbo.Answer_Get_ByQuestionId
                //        @QuestionId = @QuestionId",
                //        new { QuestionId = questionId }
                //    );
                //}
                //return question;

                using (GridReader results =
connection.QueryMultiple(
@"EXEC dbo.Question_GetSingle
@QuestionId = @QuestionId;
EXEC dbo.Answer_Get_ByQuestionId
@QuestionId = @QuestionId",
new { QuestionId = questionId }
)
)
                {
                    var question =
                    results.Read<QuestionGetSingleResponse>().FirstOrDefault();
                    if (question != null)
                    {
                        question.Answers =
                        results.Read<AnswerGetResponse>().ToList();
                    }
                    return question;
                }
            }

        }

        public IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<QuestionGetManyResponse>
                (
                   "EXEC dbo.Question_GetUnanswered"
                );

            }
        }

        public bool QuestionExists(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.QueryFirst<bool>
                (
                    @"EXEC dbo.Question_Exists @QuestionId = @QuestionId",
                    new { QuestionId = questionId }
                );
            }
        }

        // OTHERS 

        public void DeleteQuestion(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                connection.Execute(
                @"EXEC dbo.Question_Delete
                    @QuestionId = @QuestionId",
                new { QuestionId = questionId }
                );
            }

        }

        public AnswerGetResponse PostAnswer(AnswerPostFullRequest answer)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.QueryFirst<AnswerGetResponse>(
                @"EXEC dbo.Answer_Post
                    @QuestionId = @QuestionId, @Content = @Content,
                    @UserId = @UserId, @UserName = @UserName,
                    @Created = @Created",
                answer
                );
            }

        }

        public QuestionGetSingleResponse PostQuestion(QuestionPostFullRequest question) //QuestionPostRequest was replaced for QuestionPostFullRequest
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var questionId = connection.QueryFirst<int>(
                @"EXEC dbo.Question_Post 
                    @Title = @Title, 
                    @Content = @Content,
                    @UserId = @UserId, @UserName = @UserName,
                    @Created = @Created",
                question
                );
                return GetQuestion(questionId);
            }
        }

        public QuestionGetSingleResponse PutQuestion(int questionId, QuestionPutRequest question)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                connection.Execute(
                @"EXEC dbo.Question_Put
                    @QuestionId = @QuestionId, 
                    @Title = @Title, 
                    @Content = @Content",
                new { QuestionId = questionId, question.Title, question.Content }
                );
                return GetQuestion(questionId);
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestionsWithAnswers()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // SLOW RESPONSE 

                //var questions =
                //connection.Query<QuestionGetManyResponse>(
                //"EXEC dbo.Question_GetMany");
                //foreach (var question in questions)
                //{
                //    question.Answers =
                //    connection.Query<AnswerGetResponse>
                //    (
                //        @"EXEC dbo.Answer_Get_ByQuestionId
                //            @QuestionId = @QuestionId",
                //        new { QuestionId = question.QuestionId }
                //    )
                //    .ToList();
                //}
                //return questions;

                var questionDictionary = new Dictionary<int, QuestionGetManyResponse>();
                return connection.Query
                <QuestionGetManyResponse,
                AnswerGetResponse,
                QuestionGetManyResponse>
                (
                    "EXEC dbo.Question_GetMany_WithAnswers",
                    map: (q, a) =>
                    {
                        QuestionGetManyResponse question;
                        if (!questionDictionary.TryGetValue(q.QuestionId, out question))
                        {
                            question = q;
                            question.Answers = new List<AnswerGetResponse>();
                            questionDictionary.Add(question.QuestionId, question);
                        }
                        question.Answers.Add(a);
                        return question;
                    },
                    splitOn: "QuestionId"
                )
                .Distinct()
                .ToList();
            }
        }


    }
}
