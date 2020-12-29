using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_QandA.Models;

namespace API_QandA.Data
{
    public interface IDataRepository
    {
        Task<IEnumerable<QuestionGetManyResponse>> GetQuestions();
        Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsBySearch(string search);
        Task<IEnumerable<QuestionGetManyResponse>> GetUnansweredQuestions();
        Task<QuestionGetSingleResponse> GetQuestion(int questionId);
        Task<bool> QuestionExists(int questionId);
        Task<AnswerGetResponse> GetAnswer(int answerId);

        // OTHERS

        Task<QuestionGetSingleResponse> PostQuestion(QuestionPostFullRequest question); //QuestionPostRequest was replaced for QuestionPostFullRequest
        Task<QuestionGetSingleResponse> PutQuestion(int questionId, QuestionPutRequest question);
        Task DeleteQuestion(int questionId);
        Task<AnswerGetResponse> PostAnswer(AnswerPostFullRequest answer);

        // OTHERS

        Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsWithAnswers();
        Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsBySearchWithPaging(string search,int pageNumber,int pageSize);

        // ASYNC

        Task<IEnumerable<QuestionGetManyResponse>>GetUnansweredQuestionsAsync();


    }
}
