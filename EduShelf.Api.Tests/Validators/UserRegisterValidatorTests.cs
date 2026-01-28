using EduShelf.Api.Models.Entities;
using EduShelf.Api.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace EduShelf.Api.Tests.Validators
{
    public class UserRegisterValidatorTests
    {
        private readonly UserRegisterValidator _validator;

        public UserRegisterValidatorTests()
        {
            _validator = new UserRegisterValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Username_Is_Empty()
        {
            var model = new UserRegister { 
                Username = "", 
                Email = "test@example.com", 
                Password = "password123" 
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            var model = new UserRegister { 
                Username = "user", 
                Email = "invalid_email", 
                Password = "password123" 
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Too_Short()
        {
            var model = new UserRegister { 
                Username = "user", 
                Email = "test@example.com", 
                Password = "123" 
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Model_Is_Valid()
        {
            var model = new UserRegister 
            { 
                Username = "validUser", 
                Email = "test@example.com", 
                Password = "strongPassword123" 
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
