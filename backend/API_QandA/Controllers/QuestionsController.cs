using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using API_QandA.Data;
using API_QandA.Models;
using Microsoft.AspNetCore.SignalR;
using API_QandA.Hubs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;


namespace API_QandA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        // DATA QUESTION ANSWER
        private readonly IDataRepository _dataRepository;
        
        // SIGNAL R
        private readonly IHubContext<QuestionsHub> _questionHubContext;

        // CACHE
        private readonly IQuestionCache _cache;
            
        // AUTH
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _auth0UserInfo;


        public QuestionsController(
            IDataRepository dataRepository, 
            IHubContext<QuestionsHub> questionHubContext, 
            IQuestionCache questionCache,
            IHttpClientFactory clientFactory,
            IConfiguration configuration)
        {
            // TODO - set reference to _dataRepository
            _dataRepository = dataRepository;

            // TODO - set reference to _questionHubContext
            _questionHubContext = questionHubContext;

            // TODO - set reference to _cache
            _cache = questionCache;

            // TODO - set reference to _clientFactory
            _clientFactory = clientFactory;

            // TODO - set reference to _auth0UserInfo
            _auth0UserInfo = $"{configuration["Auth0:Authority"]}userinfo";
        }

        [HttpGet]
        public async Task<IEnumerable<QuestionGetManyResponse>> GetQuestions(string search, bool includeAnswers, int page = 1,int pageSize = 20)
        {
            if (string.IsNullOrEmpty(search))
            {
                if (includeAnswers)
                {
                    return await _dataRepository.GetQuestionsWithAnswers();
                }
                else
                {
                    return await _dataRepository.GetQuestions();
                }
            }
            else
            {
                /// TODO - call data repository question search
                // return _dataRepository.GetQuestionsBySearch(search);
                return await _dataRepository.GetQuestionsBySearchWithPaging(search,page,pageSize);
            }
        }

        [HttpGet("unanswered")]
        public async Task<IEnumerable<QuestionGetManyResponse>> GetUnansweredQuestions()
        {
            return await _dataRepository.GetUnansweredQuestionsAsync();
        }

        [HttpGet("{questionId}")]
        public async Task<ActionResult<QuestionGetSingleResponse>> GetQuestion(int questionId)
        {
            // FIRST IF QUESTION : CACHE
            var question = _cache.Get(questionId);
            if (question == null)
            {
                // SECOND IF QUESTION : RETRIEVE DATA 
                // TODO - call the data repository to get the question
                question = await _dataRepository.GetQuestion(questionId);
                // TODO - return HTTP status code 404 if the question isn't found
                if (question == null)
                {
                    return NotFound();
                }
                _cache.Set(question);
            }
            
            // TODO - return question in response with status code 200
            return question;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<QuestionGetSingleResponse>> PostQuestion(QuestionPostRequest questionPostRequest) // Removed (QuestionPostRequest questionPostRequest)
        {
            //var json = await new StreamReader(Request.Body).ReadToEndAsync();
            //var questionPostRequest =
            //JsonConvert.DeserializeObject<QuestionPostRequest>(json);

            // TODO - call the data repository to save the question
            var savedQuestion = await _dataRepository.PostQuestion(new QuestionPostFullRequest
            {
                Title = questionPostRequest.Title,
                Content = questionPostRequest.Content,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                UserName = await GetUserName(),
                Created = DateTime.UtcNow
            }
            ); //_dataRepository.PostQuestion(questionPostRequest);

            // TODO - return HTTP status code 201
            return CreatedAtAction(nameof(GetQuestion),
            new { questionId = savedQuestion.QuestionId },
            savedQuestion);
                // redirect ap/questions/3
        }

        //[HttpPost]
        //public async Task<ActionResult<QuestionGetSingleResponse>> PostQuestion(QuestionPostRequest questionPostRequest)
        //{
        //    var savedQuestion = await _dataRepository.PostQuestion(new QuestionPostFullRequest
        //    {
        //        Title = questionPostRequest.Title,
        //        Content = questionPostRequest.Content,
        //        UserName = "bob.test@test.com",
        //        UserId = "1",
        //        Created = DateTime.UtcNow
        //    });
        //    return CreatedAtAction(nameof(GetQuestion), new
        //    {
        //        questionId = savedQuestion.QuestionId
        //    }, savedQuestion);
        //}

        [Authorize(Policy = "MustBeQuestionAuthor")]
        [HttpPut("{questionId}")]
        public async Task<ActionResult<QuestionGetSingleResponse>> PutQuestion(int questionId,QuestionPutRequest questionPutRequest)
        {
            // TODO - get the question from the data repository
            var question = await _dataRepository.GetQuestion(questionId);

            // TODO - return HTTP status code 404 if the question isn't found
            if (question == null)
            {
                return NotFound();
            }

            // TODO - update the question model
            questionPutRequest.Title = 
                string.IsNullOrEmpty(questionPutRequest.Title) 
                ? question.Title 
                : questionPutRequest.Title;

            questionPutRequest.Content =
                string.IsNullOrEmpty(questionPutRequest.Content) 
                ? question.Content 
                : questionPutRequest.Content;

            // TODO - call the data repository with the updated question model to update the question in the database
            var savedQuestion = await _dataRepository.PutQuestion(questionId,questionPutRequest);

            // CACHE remove 
            _cache.Remove(savedQuestion.QuestionId);

            // TODO - return the saved question
            return savedQuestion;
        }

        [Authorize(Policy = "MustBeQuestionAuthor")]
        [HttpDelete("{questionId}")]
        public async Task<ActionResult> DeleteQuestion(int questionId)
        {
            var question = await _dataRepository.GetQuestion(questionId);
            if (question == null)
            {
                return NotFound();
            }
            
            await _dataRepository.DeleteQuestion(questionId);

            _cache.Remove(questionId);

            return NoContent();
        }

        #region Answers Controller
        
        [Authorize]
        [HttpPost("answer")]
        public async Task<ActionResult<AnswerGetResponse>>PostAnswer(AnswerPostRequest answerPostRequest)
        {
            var questionExists = await _dataRepository.QuestionExists(answerPostRequest.QuestionId.Value);//.value used when i used "?" on model 
            
            if (!questionExists)
            {
                return NotFound();
            }

            var savedAnswer = await _dataRepository.PostAnswer(new AnswerPostFullRequest 
            { 
                QuestionId= answerPostRequest.QuestionId.Value,
                Content = answerPostRequest.Content,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                UserName = await GetUserName(),
                Created = DateTime.UtcNow
            }
            );

            // CACHE
            _cache.Remove(answerPostRequest.QuestionId.Value);

            // SIGNAL R
            await _questionHubContext.Clients.Group($"Question-{answerPostRequest.QuestionId.Value}")
                .SendAsync("ReceiveQuestion",_dataRepository.GetQuestion(answerPostRequest.QuestionId.Value));
            // SIGNAL R

            return savedAnswer;
        }

        #endregion

        private async Task<string> GetUserName()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _auth0UserInfo);
            request.Headers.Add("Authorization", Request.Headers["Authorization"].First());

            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return user.Name;
            }
            else
            {
                return "";
            }
        }

    }
}
