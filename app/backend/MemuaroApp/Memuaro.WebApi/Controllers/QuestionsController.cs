﻿using Memuaro.Auth;
using Memuaro.Persistance.Client;
using Memuaro.Persistance.Entities;
using Memuaro.Persistance.Models;
using Memuaro.Persistance.Repositories.GlobalQuestionRepository;
using Memuaro.Persistance.Repositories.QuestionRepository;
using Memuaro.WebApi.Dtos.Question;
using Memuaro.WebApi.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Memuaro.WebApi.Controllers;

[Authorize]
public class QuestionsController : BaseController
{
    private readonly IGlobalQuestionRepository _globalQuestionRepository;
    private readonly IQuestionRepository _questionRepository;

    public QuestionsController(
        IGlobalQuestionRepository globalQuestionRepository,
        IQuestionRepository questionRepository,
        AuthProvider authProvider) : base(authProvider)
    {
        _globalQuestionRepository = globalQuestionRepository;
        _questionRepository = questionRepository;
    }

    [HttpGet]
    [Route("global")]
    public async Task<ActionResult<GlobalQuestionsDto>> GetGlobalQuestions(
        [FromQuery] GetGlobalQuestionsRequestDto request)
    {
        var userIds = request.UserId.HasValue ? new HashSet<Guid> {request.UserId.Value} : new HashSet<Guid>();
        var globalQuestions = await _globalQuestionRepository.GetGlobalQuestionWithExcept(userIds,
            request.Categories?.ToHashSet());

        var globalQuestionsDto = globalQuestions
            .Select(gq => new GlobalQuestionDto(gq))
            .ToList();

        return Ok(new GlobalQuestionsDto {GlobalQuestions = globalQuestionsDto});
    }

    [HttpGet]
    [Route("new")]
    public async Task<ActionResult<QuestionDto>> GetNewQuestion([FromQuery] Guid userId)
    {
        CheckAccessForUser(userId);

        var questions = await _questionRepository.GetForUserAsync(userId);
        var newGlobalQuestion = await _globalQuestionRepository.GetRandomGlobalQuestionWithExcept(questions.Select(question => question.GlobalQuestionId).ToHashSet());

        if (newGlobalQuestion == null)
            throw new NotFoundException("There are no more globalQuestions");

        var userQuestion = new Question
        {
            Id = Guid.NewGuid(), GlobalQuestionId = newGlobalQuestion.Id, Title = newGlobalQuestion.Title,
            CategoryId = newGlobalQuestion.CategoryId, UserId = userId, Status = Status.Unanswered
        };

        await _questionRepository.CreateAsync(userQuestion);

        return Ok(new QuestionDto(userQuestion));
    }

    [HttpGet]
    [Route("")]
    public async Task<ActionResult<QuestionsDto>> GetAllQuestionsForUser([FromQuery] Guid userId)
    {
        CheckAccessForUser(userId);

        var allQuestions = await _questionRepository.GetForUserAsync(userId);
        var questions = allQuestions
            .Select(question => new QuestionDto(question))
            .ToList();

        return Ok(new QuestionsDto {Questions = questions});
    }

    [HttpPatch]
    [Route("{id}")]
    public async Task<ActionResult<QuestionDto>> PatchAnswer(string id, [FromBody] AnswerRequestDto request)
    {
        var question = await _questionRepository.GetAsync(Guid.Parse(id));
        if (question == null)
            throw new NotFoundException(nameof(Question), id);

        CheckAccessForQuestion(question);

        question.Answer = request.Answer;
        if (request.NewStatus != null) question.Status = request.NewStatus.Value;

        await _questionRepository.UpdateAsync(question.Id, question);

        return Ok(new QuestionDto(question));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Route("newGlobalQuestion")]
    public async Task<ActionResult<GlobalQuestionDto>> CreateNewGlobalQuestion([FromBody] CreateGlobalQuestionRequestDto request)
    {
        var globalQuestion = new GlobalQuestion {Id = Guid.NewGuid(), Title = request.Title, CategoryId = request.CategoryId ?? Guid.Parse("ea815826-0c02-e446-a984-00f62a687381")};
        await _globalQuestionRepository.CreateAsync(globalQuestion);

        return Ok(new GlobalQuestionDto(globalQuestion));
    }

    [HttpPost]
    [Authorize]
    [Route("new")]
    public async Task<ActionResult<QuestionDto>> CreateNewQuestion([FromBody] AddQuestionRequestDto request)
    {
        CheckAccessForUser(request.UserId);

        var question = new Question
        {
            Id = Guid.NewGuid(), Title = request.Title, CategoryId = Guid.Parse("ea815826-0c02-e446-a984-00f62a687381"), GlobalQuestionId = Guid.Empty,
            Status = Status.Unanswered, UserId = request.UserId
        };

        await _questionRepository.CreateAsync(question);

        return Ok(new QuestionDto(question));
    }

    private void CheckAccessForUser(Guid userId)
    {
        var accessToken = _authProvider.ParseAuthHeader(HttpContext.Request.Headers.Authorization);
        var userCred = _authProvider.GetCurrentUserCredential(accessToken);
        if (userCred.Id != userId) throw new ForbiddenException();
    }

    private void CheckAccessForQuestion(Question question)
    {
        var accessToken = _authProvider.ParseAuthHeader(HttpContext.Request.Headers.Authorization);
        var userCred = _authProvider.GetCurrentUserCredential(accessToken);
        if (question.UserId != userCred.Id) throw new ForbiddenException();
    }
}