using Castle.Core.Internal;
using FluentAssertions;
using FluentAssertions.Extensions;
using CbUpdate.Configuration;
using CbUpdate.Domain.Entities;
using CbUpdate.Domain.Services.Interfaces;
using CbUpdate.Security;
using CbUpdate.Dto;
using CbUpdate.Dto.Authentication;
using CbUpdate.Test.Setup;
using CbUpdate.Crosscutting.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CbUpdate.Test.Controllers;

public class AccountResourceIntTest
{
    public AccountResourceIntTest()
    {
        _factory = new AppWebApplicationFactory<TestStartup>();
    }

    private readonly AppWebApplicationFactory<TestStartup> _factory;

    [Fact]
    public async Task TestActivateAccount()
    {
        var client = _factory.CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();
        var passwordHasher = _factory.GetRequiredService<IPasswordHasher<User>>();

        const string activationKey = "some activation key";
        var user = new User
        {
            Login = "activate-account",
            Email = "activate-account@example.com",
            PasswordHash = passwordHasher.HashPassword(null, TestUtil.RandomAlphabetic(60)),
            Activated = false,
            ActivationKey = activationKey
        };

        await userManager.CreateAsync(user);

        var response = await client.GetAsync($"/api/activate?key={activationKey}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        user = await userManager.FindByNameAsync(user.Login);
        //TODO FIX database refresh to prevent the usage of context/Reload
        user = Fixme.ReloadUser(_factory, user);
        user.Activated.Should().Be(true);
    }

    [Fact]
    public async Task TestActivateAccountWithWrongKey()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/activate?key=wrongActivationKey");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TestAuthenticatedUser()
    {
        var client = _factory.WithMockUser("test").CreateClient();
        var response = await client.GetAsync("/api/authenticate");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("test");
    }

    [Fact]
    public async Task TestChangePassword()
    {
        var client = _factory.WithMockUser("change-password").CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();
        var passwordHasher = _factory.GetRequiredService<IPasswordHasher<User>>();

        var currentPassword = TestUtil.RandomAlphabetic(60);
        var user = new User
        {
            Login = "change-password",
            Email = "change-password@example.com",
            PasswordHash = passwordHasher.HashPassword(null, currentPassword)
        };

        await userManager.CreateAsync(user);

        var response = await client.PostAsync("/api/account/change-password", TestUtil.ToJsonContent(
            new PasswordChangeDto
            {
                CurrentPassword = currentPassword,
                NewPassword = "new password"
            }));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedUser = await userManager.FindByNameAsync(user.Login);
        //TODO FIX database refresh to prevent the usage of context/Reload
        updatedUser = Fixme.ReloadUser(_factory, updatedUser);
        passwordHasher.VerifyHashedPassword(updatedUser, updatedUser.PasswordHash, "new password").Should()
            .Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public async Task TestChangePasswordEmpty()
    {
        var client = _factory.WithMockUser("change-password-empty").CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();
        var passwordHasher = _factory.GetRequiredService<IPasswordHasher<User>>();

        var currentPassword = TestUtil.RandomAlphabetic(60);
        var user = new User
        {
            Login = "change-password-empty",
            Email = "change-password-empty@example.com",
            PasswordHash = passwordHasher.HashPassword(null, currentPassword)
        };

        await userManager.CreateAsync(user);

        var response = await client.PostAsync("/api/account/change-password", TestUtil.ToJsonContent(
            new PasswordChangeDto
            {
                CurrentPassword = currentPassword,
                NewPassword = ""
            })
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var updatedUser = await userManager.FindByNameAsync(user.Login);
        //TODO FIX database refresh to prevent the usage of context/Reload
        updatedUser = Fixme.ReloadUser(_factory, updatedUser);
        updatedUser.PasswordHash.Should().Be(user.PasswordHash);
    }

    [Fact]
    public async Task TestChangePasswordTooLong()
    {
        var client = _factory.WithMockUser("change-password-too-long").CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();
        var passwordHasher = _factory.GetRequiredService<IPasswordHasher<User>>();

        var currentPassword = TestUtil.RandomAlphabetic(60);
        var user = new User
        {
            Login = "change-password-too-long",
            Email = "change-password-too-long@example.com",
            PasswordHash = passwordHasher.HashPassword(null, currentPassword)
        };

        await userManager.CreateAsync(user);

        var response = await client.PostAsync("/api/account/change-password", TestUtil.ToJsonContent(
            new PasswordChangeDto
            {
                CurrentPassword = currentPassword,
                NewPassword = TestUtil.RandomAlphabetic(101)
            }));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var updatedUser = await userManager.FindByNameAsync(user.Login);
        //TODO FIX database refresh to prevent the usage of context/Reload
        updatedUser = Fixme.ReloadUser(_factory, updatedUser);
        updatedUser.PasswordHash.Should().Be(user.PasswordHash);
    }

    [Fact]
    public async Task TestChangePasswordTooSmall()
    {
        var client = _factory.WithMockUser("change-password-too-small").CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();
        var passwordHasher = _factory.GetRequiredService<IPasswordHasher<User>>();

        var currentPassword = TestUtil.RandomAlphabetic(60);
        var user = new User
        {
            Login = "change-password-too-small",
            Email = "change-password-too-small@example.com",
            PasswordHash = passwordHasher.HashPassword(null, currentPassword)
        };

        await userManager.CreateAsync(user);

        var response = await client.PostAsync("/api/account/change-password", TestUtil.ToJsonContent(
            new PasswordChangeDto
            {
                CurrentPassword = currentPassword,
                NewPassword = TestUtil.RandomAlphabetic(3)
            }));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var updatedUser = await userManager.FindByNameAsync(user.Login);
        //TODO FIX database refresh to prevent the usage of context/Reload
        updatedUser = Fixme.ReloadUser(_factory, updatedUser);
        updatedUser.PasswordHash.Should().Be(user.PasswordHash);
    }

    [Fact]
    public async Task TestChangePasswordWrongExistingPassword()
    {
        var client = _factory.WithMockUser("change-password-wrong-existing-password").CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();
        var passwordHasher = _factory.GetRequiredService<IPasswordHasher<User>>();

        var currentPassword = TestUtil.RandomAlphabetic(60);
        var user = new User
        {
            Login = "change-password-wrong-existing-password",
            Email = "change-password-wrong-existing-password@example.com",
            PasswordHash = passwordHasher.HashPassword(null, currentPassword)
        };

        await userManager.CreateAsync(user);

        var response = await client.PostAsync("/api/account/change-password", TestUtil.ToJsonContent(
            new PasswordChangeDto
            {
                CurrentPassword = $"1{currentPassword}",
                NewPassword = "new password"
            }));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var updatedUser = await userManager.FindByNameAsync(user.Login);
        //TODO FIX database refresh to prevent the usage of context/Reload
        updatedUser = Fixme.ReloadUser(_factory, updatedUser);
        passwordHasher.VerifyHashedPassword(updatedUser, updatedUser.PasswordHash, "new password").Should()
            .Be(PasswordVerificationResult.Failed);
        passwordHasher.VerifyHashedPassword(updatedUser, updatedUser.PasswordHash, currentPassword).Should()
            .Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public async Task TestFinishPasswordReset()
    {
        var client = _factory.CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();
        var passwordHasher = _factory.GetRequiredService<IPasswordHasher<User>>();

        var user = new User
        {
            Login = "finish-password-reset",
            Email = "finish-password-reset@example.com",
            PasswordHash = passwordHasher.HashPassword(null, TestUtil.RandomAlphabetic(60)),
            ResetDate = DateTime.Now.Add(60.Seconds()),
            ResetKey = TestUtil.RandomAlphabetic(60)
        };

        await userManager.CreateAsync(user);

        var keyAndPassword = new KeyAndPasswordDto
        {
            Key = user.ResetKey,
            NewPassword = "new password"
        };

        var response = await client.PostAsync("/api/account/reset-password/finish",
            TestUtil.ToJsonContent(keyAndPassword));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedUser = await userManager.FindByNameAsync(user.Login);
        //TODO FIX database refresh to prevent the usage of context/Reload
        updatedUser = Fixme.ReloadUser(_factory, updatedUser);
        passwordHasher.VerifyHashedPassword(updatedUser, updatedUser.PasswordHash, keyAndPassword.NewPassword)
            .Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public async Task TestFinishPasswordResetTooSmall()
    {
        var client = _factory.CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();
        var passwordHasher = _factory.GetRequiredService<IPasswordHasher<User>>();

        var user = new User
        {
            Login = "finish-password-reset-too-small",
            Email = "finish-password-reset-too-small@example.com",
            PasswordHash = passwordHasher.HashPassword(null, TestUtil.RandomAlphabetic(60)),
            ResetDate = DateTime.Now.Add(60.Seconds()),
            ResetKey = TestUtil.RandomAlphabetic(60)
        };

        await userManager.CreateAsync(user);

        var keyAndPassword = new KeyAndPasswordDto
        {
            Key = user.ResetKey,
            NewPassword = TestUtil.RandomAlphabetic(3)
        };

        var response = await client.PostAsync("/api/account/reset-password/finish",
            TestUtil.ToJsonContent(keyAndPassword));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var updatedUser = await userManager.FindByNameAsync(user.Login);
        //TODO FIX database refresh to prevent the usage of context/Reload
        updatedUser = Fixme.ReloadUser(_factory, updatedUser);
        passwordHasher.VerifyHashedPassword(updatedUser, updatedUser.PasswordHash, keyAndPassword.NewPassword)
            .Should().Be(PasswordVerificationResult.Failed);
    }

    [Fact]
    public async Task TestFinishPasswordResetWrongKey()
    {
        var client = _factory.CreateClient();
        var keyAndPassword = new KeyAndPasswordDto
        {
            Key = TestUtil.RandomAlphabetic(60),
            NewPassword = TestUtil.RandomAlphabetic(60)
        };

        var response = await client.PostAsync("/api/account/reset-password/finish",
            TestUtil.ToJsonContent(keyAndPassword));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TestGetExistingAccount()
    {
        var client = _factory.WithMockUser("test").CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();

        var user = new User
        {
            Login = "test",
            FirstName = "john",
            LastName = "doe",
            Email = "john.doe@jhipster.com",
            ImageUrl = "http://placehold.it/50x50",
            LangKey = Constants.DefaultLangKey
        };

        await userManager.CreateAsync(user);
        await userManager.AddToRolesAsync(user, new[] { RolesConstants.ADMIN });

        var response = await client.GetAsync("/api/account");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = JToken.Parse(await response.Content.ReadAsStringAsync());
        json.SelectToken("$.login").Value<string>().Should().Be(user.Login);
        json.SelectToken("$.firstName").Value<string>().Should().Be(user.FirstName);
        json.SelectToken("$.lastName").Value<string>().Should().Be(user.LastName);
        json.SelectToken("$.email").Value<string>().Should().Be(user.Email);
        json.SelectToken("$.imageUrl").Value<string>().Should().Be(user.ImageUrl);
        json.SelectToken("$.langKey").Value<string>().Should().Be(user.LangKey);
        json.SelectToken("$.authorities").ToObject<IEnumerable<string>>()
            .Should().Contain(new[] { RolesConstants.ADMIN });
    }

    [Fact]
    public async Task TestGetUnknownAccount()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/account");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TestNonAuthenticatedUser()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/authenticate");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Be("");
    }

    [Fact]
    public async Task TestRegisterAdminIsIgnored()
    {
        var client = _factory.CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();

        var validUser = new ManagedUserDto
        {
            Login = "badguy",
            Password = "password",
            FirstName = "Bad",
            LastName = "Guy",
            Email = "badguy@example.com",
            Activated = true,
            ImageUrl = "http://placehold.it/50x50",
            LangKey = Constants.DefaultLangKey,
            Roles = new HashSet<string>
            {
                RolesConstants.ADMIN
            }
        };

        var response = await client.PostAsync("/api/register", TestUtil.ToJsonContent(validUser));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var user = await userManager.FindByNameAsync(validUser.Login);
        user.Should().NotBeNull();
        //            user.Roles.Count.Should().Be(1);
        //            user.Roles.Should().Contain(role => role.Name == RolesConstants.USER);
    }

    [Fact]
    public async Task TestRegisterDuplicateEmail()
    {
        var client = _factory.CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();

        // First user
        var firstUser = new ManagedUserDto
        {
            Login = "test-register-duplicate-email",
            Password = "password",
            FirstName = "Alice",
            LastName = "Test",
            Email = "test-register-duplicate-email@example.com",
            ImageUrl = "http://placehold.it/50x50",
            LangKey = Constants.DefaultLangKey,
            Roles = new HashSet<string>
            {
                RolesConstants.ADMIN
            }
        };

        // Register first user
        var response = await client.PostAsync("/api/register", TestUtil.ToJsonContent(firstUser));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var testUser1 = userManager.FindByNameAsync(firstUser.Login);
        testUser1.Should().NotBeNull();

        // Duplicate email, different login
        var secondUser = new ManagedUserDto
        {
            Login = "test-register-duplicate-email-2",
            Password = firstUser.Password,
            FirstName = firstUser.FirstName,
            LastName = firstUser.LastName,
            Email = firstUser.Email,
            ImageUrl = firstUser.ImageUrl,
            LangKey = firstUser.LangKey,
            Roles = new HashSet<string>(firstUser.Roles)
        };

        // Register second (non activated) user
        response = await client.PostAsync("/api/register", TestUtil.ToJsonContent(secondUser));
        response.StatusCode.Should().Be(HttpStatusCode.Created);


        var testUser2 = await userManager.FindByNameAsync(firstUser.Login);
        testUser2.Should().BeNull();

        var testUser3 = await userManager.FindByNameAsync(secondUser.Login);
        testUser3.Should().NotBeNull();

        // Duplicate email - with uppercase email address
        var userWithUpperCaseEmail = new ManagedUserDto
        {
            Id = firstUser.Id,
            Login = "test-register-duplicate-email-3",
            Password = firstUser.Password,
            FirstName = firstUser.FirstName,
            LastName = firstUser.LastName,
            Email = "TEST-register-duplicate-email@example.com",
            ImageUrl = firstUser.ImageUrl,
            LangKey = firstUser.LangKey,
            Roles = new HashSet<string>(firstUser.Roles)
        };

        // Register third (not activated) user
        response = await client.PostAsync("/api/register", TestUtil.ToJsonContent(userWithUpperCaseEmail));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var testUser4 = await userManager.FindByNameAsync(userWithUpperCaseEmail.Login);
        testUser4.Should().NotBeNull();
        testUser4.Email.Should().Be(firstUser.Email);

        testUser4.Activated = true;
        await userManager.UpdateAsync(testUser4);

        // Register 4th (already activated) user
        response = await client.PostAsync("/api/register", TestUtil.ToJsonContent(secondUser));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TestRegisterDuplicateLogin()
    {
        var client = _factory.CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();

        // First registration
        var firstUser = new ManagedUserDto
        {
            Login = "test-register-duplicate-email",
            Password = "password",
            FirstName = "Alice",
            LastName = "Something",
            Email = "alice@example.com",
            ImageUrl = "http://placehold.it/50x50",
            LangKey = Constants.DefaultLangKey,
            Roles = new HashSet<string>
            {
                RolesConstants.ADMIN
            }
        };

        // Duplicate login, different email
        var secondUser = new ManagedUserDto
        {
            Login = firstUser.Login,
            Password = firstUser.Password,
            FirstName = firstUser.FirstName,
            LastName = firstUser.LastName,
            Email = "alice2@example.com",
            ImageUrl = firstUser.ImageUrl,
            LangKey = firstUser.LangKey,
            Roles = new HashSet<string>(firstUser.Roles)
            //TODO add createdBy etc.
        };

        // First user
        var response = await client.PostAsync("/api/register", TestUtil.ToJsonContent(firstUser));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Second (non activated) user
        response = await client.PostAsync("/api/register", TestUtil.ToJsonContent(secondUser));
        response.StatusCode.Should().Be(HttpStatusCode.Created);


        var testUser = await userManager.FindByEmailAsync(secondUser.Email);
        testUser.Should().NotBeNull();
        testUser.Activated = true;

        await userManager.UpdateAsync(testUser);

        // Second (already activated) user
        response = await client.PostAsync("/api/register", TestUtil.ToJsonContent(secondUser));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TestRegisterInvalidEmail()
    {
        var client = _factory.CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();

        var invalidUser = new ManagedUserDto
        {
            Login = "bob",
            Password = "password",
            FirstName = "Bob",
            LastName = "Green",
            Email = "invalid", // <-- invalid
            Activated = true,
            ImageUrl = "http://placehold.it/50x50",
            LangKey = Constants.DefaultLangKey,
            Roles = new HashSet<string>
            {
                RolesConstants.USER
            }
        };

        var response = await client.PostAsync("/api/register", TestUtil.ToJsonContent(invalidUser));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var user = await userManager.FindByNameAsync(invalidUser.Login);
        user.Should().BeNull();
    }

    [Fact]
    public async Task TestRegisterInvalidLogin()
    {
        var client = _factory.CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();

        var invalidUser = new ManagedUserDto
        {
            Login = "funky-log!n", // <-- invalid
            Password = "password",
            FirstName = "Bob",
            LastName = "Green",
            Email = "bob@example.com",
            Activated = true,
            ImageUrl = "http://placehold.it/50x50",
            LangKey = Constants.DefaultLangKey,
            Roles = new HashSet<string>
            {
                RolesConstants.USER
            }
        };

        var response = await client.PostAsync("/api/register", TestUtil.ToJsonContent(invalidUser));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var user = await userManager.FindByNameAsync(invalidUser.Login);
        user.Should().BeNull();
    }

    [Fact]
    public async Task TestRegisterInvalidPassword()
    {
        var client = _factory.CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();

        var invalidUser = new ManagedUserDto
        {
            Login = "bob",
            Password = "123", // password with only 3 digits
            FirstName = "Bob",
            LastName = "Green",
            Email = "bob@example.com",
            Activated = true,
            ImageUrl = "http://placehold.it/50x50",
            LangKey = Constants.DefaultLangKey,
            Roles = new HashSet<string>
            {
                RolesConstants.USER
            }
        };

        var response = await client.PostAsync("/api/register", TestUtil.ToJsonContent(invalidUser));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var user = await userManager.FindByNameAsync(invalidUser.Login);
        user.Should().BeNull();
    }

    [Fact]
    public async Task TestRegisterNullPassword()
    {
        var client = _factory.CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();

        var invalidUser = new ManagedUserDto
        {
            Login = "bob",
            Password = null, // invalid null password
            FirstName = "Bob",
            LastName = "Green",
            Email = "bob@example.com",
            Activated = true,
            ImageUrl = "http://placehold.it/50x50",
            LangKey = Constants.DefaultLangKey,
            Roles = new HashSet<string>
            {
                RolesConstants.USER
            }
        };

        var response = await client.PostAsync("/api/register", TestUtil.ToJsonContent(invalidUser));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var user = await userManager.FindByNameAsync(invalidUser.Login);
        user.Should().BeNull();
    }

    [Fact]
    public async Task TestRegisterValid()
    {
        var client = _factory.CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();

        var validUser = new ManagedUserDto
        {
            Login = "test-register-valid",
            Password = "password",
            FirstName = "Alice",
            LastName = "Test",
            Email = "test-register-valid@example.com",
            ImageUrl = "http://placehold.it/50x50",
            LangKey = Constants.DefaultLangKey,
            Roles = new HashSet<string>
            {
                RolesConstants.USER
            }
        };
        var user = await userManager.FindByNameAsync(validUser.Login);
        user.Should().BeNull();

        var response = await client.PostAsync("/api/register", TestUtil.ToJsonContent(validUser));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        user = await userManager.FindByNameAsync(validUser.Login);
        user.Should().NotBeNull();
    }

    [Fact]
    public async Task TestRequestPasswordReset()
    {
        var client = _factory.CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();
        var passwordHasher = _factory.GetRequiredService<IPasswordHasher<User>>();

        var user = new User
        {
            Login = "password-reset",
            Email = "password-reset@example.com",
            PasswordHash = passwordHasher.HashPassword(null, TestUtil.RandomAlphabetic(60)),
            Activated = true
        };

        await userManager.CreateAsync(user);

        var response = await client.PostAsync("/api/account/reset-password/init",
            new StringContent("password-reset@example.com"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TestRequestPasswordResetUpperCaseEmail()
    {
        var client = _factory.CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();
        var passwordHasher = _factory.GetRequiredService<IPasswordHasher<User>>();

        var user = new User
        {
            Login = "password-reset",
            Email = "password-reset@example.com",
            PasswordHash = passwordHasher.HashPassword(null, TestUtil.RandomAlphabetic(60)),
            Activated = true
        };

        await userManager.CreateAsync(user);

        var response = await client.PostAsync("/api/account/reset-password/init",
            new StringContent("password-reset@EXAMPLE.COM"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TestRequestPasswordResetWrongEmail()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync("/api/account/reset-password/init",
            new StringContent("password-reset-wrong-email@example.com"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TestSaveAccount()
    {
        var client = _factory.WithMockUser("save-account").CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();
        var userService = _factory.GetRequiredService<IUserService>();
        var passwordHasher = _factory.GetRequiredService<IPasswordHasher<User>>();

        var user = new User
        {
            Login = "save-account",
            Email = "save-account@example.com",
            PasswordHash = passwordHasher.HashPassword(null, TestUtil.RandomAlphabetic(60)),
            Activated = true
        };

        await userManager.CreateAsync(user);

        var userDto = new UserDto
        {
            Login = "not-used",
            FirstName = "firstname",
            LastName = "lastname",
            Email = "save-account@example.com",
            Activated = false,
            ImageUrl = "http://placehold.it/50x50",
            LangKey = Constants.DefaultLangKey,
            Roles = new System.Collections.Generic.HashSet<string> {
                RolesConstants.USER
            }
        };

        var response = await client.PostAsync("/api/account", TestUtil.ToJsonContent(userDto));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedUser = await userService.GetByLogin(user.Login);

        //TODO FIX database refresh to prevent the usage of context/Reload
        updatedUser = Fixme.ReloadUser(_factory, updatedUser);
        updatedUser.FirstName.Should().Be(userDto.FirstName);
        updatedUser.LastName.Should().Be(userDto.LastName);
        updatedUser.Email.Should().Be(userDto.Email);
        updatedUser.LangKey.Should().Be(userDto.LangKey);
        updatedUser.ImageUrl.Should().Be(userDto.ImageUrl);
        updatedUser.PasswordHash.Should().Be(user.PasswordHash);
        updatedUser.Activated.Should().BeTrue();
        //updatedUser.UserRoles.IsNullOrEmpty().Should().BeTrue();
    }

    [Fact]
    public async Task TestSaveExistingEmail()
    {
        var client = _factory.WithMockUser("save-existing-email").CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();
        var passwordHasher = _factory.GetRequiredService<IPasswordHasher<User>>();

        var user = new User
        {
            Login = "save-existing-email",
            Email = "save-existing-email@example.com",
            PasswordHash = passwordHasher.HashPassword(null, TestUtil.RandomAlphabetic(60)),
            Activated = true
        };

        await userManager.CreateAsync(user);

        var anotherUser = new User
        {
            Login = "save-existing-email2",
            Email = "save-existing-email2@example.com",
            PasswordHash = passwordHasher.HashPassword(null, TestUtil.RandomAlphabetic(60)),
            Activated = true
        };

        await userManager.CreateAsync(anotherUser);


        var userDto = new UserDto
        {
            Login = "not-used",
            FirstName = "firstname",
            LastName = "lastname",
            Email = "save-existing-email2@example.com",
            Activated = false,
            ImageUrl = "http://placehold.it/50x50",
            LangKey = Constants.DefaultLangKey,
            Roles = new System.Collections.Generic.HashSet<string>
            {
                RolesConstants.USER
            }
        };

        var response = await client.PostAsync("/api/account", TestUtil.ToJsonContent(userDto));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var updatedUser = await userManager.FindByNameAsync(user.Login);
        updatedUser.Email.Should().Be("save-existing-email@example.com");
    }

    [Fact]
    public async Task TestSaveExistingEmailAndLogin()
    {
        var client = _factory.WithMockUser("save-existing-email-and-login").CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();
        var passwordHasher = _factory.GetRequiredService<IPasswordHasher<User>>();

        var user = new User
        {
            Login = "save-existing-email-and-login",
            Email = "save-existing-email-and-login@example.com",
            PasswordHash = passwordHasher.HashPassword(null, TestUtil.RandomAlphabetic(60)),
            Activated = true
        };

        await userManager.CreateAsync(user);

        var userDto = new UserDto
        {
            Login = "not-used",
            FirstName = "firstname",
            LastName = "lastname",
            Email = "save-existing-email-and-login@example.com",
            Activated = false,
            ImageUrl = "http://placehold.it/50x50",
            LangKey = Constants.DefaultLangKey,
            Roles = new System.Collections.Generic.HashSet<string>
            {
                RolesConstants.USER
            }
        };

        var response = await client.PostAsync("/api/account", TestUtil.ToJsonContent(userDto));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedUser = await userManager.FindByNameAsync(user.Login);
        updatedUser.Email.Should().Be("save-existing-email-and-login@example.com");
    }

    [Fact]
    public async Task TestSaveInvalidEmail()
    {
        var client = _factory.WithMockUser("save-invalid-email").CreateClient();
        var userManager = _factory.GetRequiredService<UserManager<User>>();
        var passwordHasher = _factory.GetRequiredService<IPasswordHasher<User>>();

        var user = new User
        {
            Login = "save-invalid-email",
            Email = "save-invalid-email@example.com",
            PasswordHash = passwordHasher.HashPassword(null, TestUtil.RandomAlphabetic(60)),
            Activated = true
        };

        await userManager.CreateAsync(user);

        var userDto = new UserDto
        {
            Login = "not-used",
            FirstName = "firstname",
            LastName = "lastname",
            Email = "invalid email",
            Activated = false,
            ImageUrl = "http://placehold.it/50x50",
            LangKey = Constants.DefaultLangKey,
            Roles = new HashSet<string>
            {
                RolesConstants.ADMIN
            }
        };

        var response = await client.PostAsync("/api/account", TestUtil.ToJsonContent(userDto));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        user = await userManager.FindByEmailAsync(userDto.Email);
        user.Should().BeNull();
    }
}
