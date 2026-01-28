using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Validators;
using FluentValidation.TestHelper;
using Xunit;
using System.Collections.Generic;

namespace EduShelf.Api.Tests.Validators
{
    public class QuizCreateDtoValidatorTests
    {
        private readonly QuizCreateDtoValidator _validator;

        public QuizCreateDtoValidatorTests()
        {
            _validator = new QuizCreateDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            var model = new QuizCreateDto { 
                Title = "", 
                Questions = new List<QuestionCreateDto>() 
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Too_Long()
        {
            var model = new QuizCreateDto { 
                Title = new string('a', 201), 
                Questions = new List<QuestionCreateDto>() 
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Have_Child_Validator_Error_When_Question_Is_Invalid()
        {
            var model = new QuizCreateDto
            {
                Title = "Valid Title",
                Questions = new List<QuestionCreateDto>
                {
                    new QuestionCreateDto { 
                        Text = "",
                        Answers = new List<AnswerCreateDto>() 
                    } // Invalid question
                }
            };
            var result = _validator.TestValidate(model);
            // Verify that the error comes from the nested validator logic
            result.ShouldHaveValidationErrorFor("Questions[0].Text");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Model_Is_Valid()
        {
            var model = new QuizCreateDto
            {
                Title = "Valid Quiz",
                Questions = new List<QuestionCreateDto>
                {
                    new QuestionCreateDto
                    {
                        Text = "Question 1",
                        Answers = new List<AnswerCreateDto>
                        {
                             new AnswerCreateDto { Text = "A1", IsCorrect = true },
                             new AnswerCreateDto { Text = "A2", IsCorrect = false }
                        }
                    }
                }
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
