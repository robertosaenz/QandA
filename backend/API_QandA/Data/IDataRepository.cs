using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_QandA.Models;

namespace API_QandA.Data
{
    public interface IDataRepository
    {
        IEnumerable<QuestionGetManyResponse> GetQuestions();
        IEnumerable<QuestionGetManyResponse> GetQuestionsBySearch(string search);
        IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions();
        QuestionGetSingleResponse GetQuestion(int questionId);
        bool QuestionExists(int questionId);
        AnswerGetResponse GetAnswer(int answerId);

        // OTHERS

        QuestionGetSingleResponse PostQuestion(QuestionPostFullRequest question); //QuestionPostRequest was replaced for QuestionPostFullRequest
        QuestionGetSingleResponse PutQuestion(int questionId, QuestionPutRequest question);
        void DeleteQuestion(int questionId);
        AnswerGetResponse PostAnswer(AnswerPostFullRequest answer);

        // OTHERS

        IEnumerable<QuestionGetManyResponse> GetQuestionsWithAnswers();
        IEnumerable<QuestionGetManyResponse> GetQuestionsBySearchWithPaging(string search,int pageNumber,int pageSize);

        // ASYNC

        Task<IEnumerable<QuestionGetManyResponse>>GetUnansweredQuestionsAsync();


    }
}
