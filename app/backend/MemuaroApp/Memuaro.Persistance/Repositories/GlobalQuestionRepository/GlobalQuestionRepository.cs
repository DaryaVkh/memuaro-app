﻿using Memuaro.Persistance.Client;
using Memuaro.Persistance.Entities;
using Memuaro.Persistance.Repositories.Base;
using MongoDB.Driver;

namespace Memuaro.Persistance.Repositories.GlobalQuestionRepository;

public class GlobalQuestionRepository : BaseRepository<GlobalQuestion>, IGlobalQuestionRepository
{
    private const string CollectionName = "globalQuestions";
    
    public GlobalQuestionRepository(IDatabaseClient databaseClient) : base(databaseClient, CollectionName)
    {
    }

    public Task<GlobalQuestion> GetRandomGlobalQuestionWithExcept(HashSet<Guid> exceptIds) =>
        MongoCollection.Find(globalQuestion => !exceptIds.Contains(globalQuestion.Id)).FirstOrDefaultAsync();

    public Task<List<GlobalQuestion>> GetAllGlobalQuestions() => MongoCollection.Find(_ => true).ToListAsync();

    public Task<List<GlobalQuestion>> GetGlobalQuestionWithExcept(HashSet<Guid>? exceptIds = null, HashSet<Guid>? categories = null)
    {
        exceptIds ??= new HashSet<Guid>();
        return categories is {Count: > 0}
            ? MongoCollection.Find(gq => !exceptIds.Contains(gq.Id) && categories.Contains(gq.CategoryId)).ToListAsync()
            : MongoCollection.Find(gq => !exceptIds.Contains(gq.Id)).ToListAsync();
    }
}