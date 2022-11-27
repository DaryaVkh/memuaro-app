using Memuaro.Persistance.Entities;

namespace Memuaro.WebApi.Dtos.Question;

public class GlobalQuestionDto
{
    public string? Title { get; set; }
    public Guid CategoryId { get; set; }

    public GlobalQuestionDto(GlobalQuestion globalQuestion)
    {
        Title = globalQuestion.Title;
        CategoryId = globalQuestion.CategoryId;
    }
}