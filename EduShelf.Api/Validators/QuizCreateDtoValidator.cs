using EduShelf.Api.Models.Dtos;
using FluentValidation;

namespace EduShelf.Api.Validators
{
    public class AnswerCreateDtoValidator : AbstractValidator<AnswerCreateDto>
    {
        public AnswerCreateDtoValidator()
        {
            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Answer text is required.");
        }
    }

    public class QuestionCreateDtoValidator : AbstractValidator<QuestionCreateDto>
    {
        public QuestionCreateDtoValidator()
        {
            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Question text is required.");

            RuleFor(x => x.Answers)
                .NotEmpty().WithMessage("At least one answer is required.")
                .Must(x => x != null && x.Count > 0).WithMessage("At least one answer is required.");

            RuleForEach(x => x.Answers).SetValidator(new AnswerCreateDtoValidator());
        }
    }

    public class QuizCreateDtoValidator : AbstractValidator<QuizCreateDto>
    {
        public QuizCreateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

            RuleForEach(x => x.Questions).SetValidator(new QuestionCreateDtoValidator());
        }
    }
}
