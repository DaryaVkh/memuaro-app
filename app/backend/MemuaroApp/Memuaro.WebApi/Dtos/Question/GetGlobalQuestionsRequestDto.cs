namespace Memuaro.WebApi.Dtos.Question;

public class GetGlobalQuestionsRequestDto
{
    public Guid? UserId { get; set; }
    public Guid[]? Categories { get; set; }
}