﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API_QandA.Data;
using API_QandA.Models;
using Microsoft.AspNetCore.SignalR;
using API_QandA.Hubs;


namespace API_QandA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;
        private readonly IHubContext<QuestionsHub> _questionHubContext;


        public QuestionsController(IDataRepository dataRepository, IHubContext<QuestionsHub> questionHubContext)
        {
            // TODO - set reference to _dataRepository
            _dataRepository = dataRepository;

            // TODO - set reference to _questionHubContext
            _questionHubContext = questionHubContext;

        }

        [HttpGet]
        public IEnumerable<QuestionGetManyResponse> GetQuestions(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                // TODO - get questions from data repository

                var questions = _dataRepository.GetQuestions();

                // TODO - return questions in the response

                return questions;

                //return _dataRepository.GetQuestions();
            }
            else
            {
                /// TODO - call data repository question search
                return _dataRepository.GetQuestionsBySearch(search);
            }

        }

        [HttpGet("unanswered")]
        public IEnumerable<QuestionGetManyResponse>GetUnansweredQuestions()
        {
            return _dataRepository.GetUnansweredQuestions();
        }

        [HttpGet("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> GetQuestion(int questionId)
        {
            // TODO - call the data repository to get the question
            var question = _dataRepository.GetQuestion(questionId);
            // TODO - return HTTP status code 404 if the question isn't found
            if (question == null)
            {
                return NotFound();
            }
            // TODO - return question in response with status code 200
            return question;
        }

        [HttpPost]
        public ActionResult<QuestionGetSingleResponse> PostQuestion(QuestionPostRequest questionPostRequest)
        {
            // TODO - call the data repository to save the question
            var savedQuestion = _dataRepository.PostQuestion( new QuestionPostFullRequest 
            { 
                Title=questionPostRequest.Title,
                Content=questionPostRequest.Content,
                UserId="1",
                UserName="test@test.com",
                Created=DateTime.UtcNow
            }
            );; //_dataRepository.PostQuestion(questionPostRequest);

            // TODO - return HTTP status code 201
            return CreatedAtAction(nameof(GetQuestion),
            new { questionId = savedQuestion.QuestionId },
            savedQuestion);
                // redirect ap/questions/3
        }

        [HttpPut("{questionId}")]
        public ActionResult<QuestionGetSingleResponse>PutQuestion(int questionId,QuestionPutRequest questionPutRequest)
        {
            // TODO - get the question from the data repository
            var question = _dataRepository.GetQuestion(questionId);

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
            var savedQuestion = _dataRepository.PutQuestion(questionId,questionPutRequest);

            // TODO - return the saved question
            return savedQuestion;
        }

        [HttpDelete("{questionId}")]
        public ActionResult DeleteQuestion(int questionId)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if (question == null)
            {
                return NotFound();
            }
            _dataRepository.DeleteQuestion(questionId);
            return NoContent();
        }

        #region Answers Controller

        [HttpPost("answer")]
        public ActionResult<AnswerGetResponse>PostAnswer(AnswerPostRequest answerPostRequest)
        {
            var questionExists = _dataRepository.QuestionExists(answerPostRequest.QuestionId.Value);//.value used when i used "?" on model 
            
            if (!questionExists)
            {
                return NotFound();
            }

            var savedAnswer = _dataRepository.PostAnswer(new AnswerPostFullRequest 
            { 
                QuestionId= answerPostRequest.QuestionId.Value,
                Content = answerPostRequest.Content,
                UserId = "1",
                UserName = "bob.test@test.com",
                Created = DateTime.UtcNow
            }
            );

            // SIGNAL R
            _questionHubContext.Clients.Group($"Question-{answerPostRequest.QuestionId.Value}")
                .SendAsync("ReceiveQuestion",_dataRepository.GetQuestion(answerPostRequest.QuestionId.Value));
            // SIGNAL R

            return savedAnswer;
        }

        #endregion

    }
}
