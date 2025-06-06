using AutoMapper;
using FluentAssertions;
using CbUpdate.Domain.Entities;
using CbUpdate.Dto;
using CbUpdate.Security;
using CbUpdate.Configuration.AutoMapper;
using CbUpdate.Test.Setup;
using CbUpdate.Crosscutting.Constants;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CbUpdate.Test.Controllers;

public class UsersResourceIntTest
{
    public UsersResourceIntTest()
    {
        _factory = new AppWebApplicationFactory<TestStartup>().WithMockUser("test", new HashSet<string> { RolesConstants.ADMIN });
        _client = _factory.CreateClient();
        _userManager = _factory.GetRequiredService<UserManager<User>>();
        _passwordHasher = _userManager.PasswordHasher;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new AutoMapperProfile());
        });

        _mapper = config.CreateMapper();

        InitTest();
    }

    private const string DefaultLogin = "johndoe";
    private const string UpdatedLogin = "jhipster";

    private const string DefaultId = "id";

    private const string DefaultPassword = "passjohndoe";
    private const string UpdatedPassword = "passjhipster";

    private const string DefaultEmail = "johndoe@localhost";
    private const string UpdatedEmail = "jhipster@localhost";

    private const string DefaultFirstname = "john";
    private const string UpdatedFirstname = "jhipsterFirstName";

    private const string DefaultLastname = "doe";
    private const string UpdatedLastname = "jhipsterLastName";

    private const string DefaultImageurl = "http://placehold.it/50x50";
    private const string UpdatedImageurl = "http://placehold.it/40x40";

    private const string DefaultLangkey = "en";
    private const string UpdatedLangkey = "fr";

    private readonly AppWebApplicationFactory<TestStartup> _factory;
    private readonly HttpClient _client;


    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher<User> _passwordHasher;
    private User _user;


    private User CreateEntity()
    {
        return new User
        {
            Login = $"{DefaultLogin}{TestUtil.RandomAlphabetic(5)}",
            PasswordHash = _passwordHasher.HashPassword(null, TestUtil.RandomAlphabetic(60)),
            Activated = true,
            Email = $"{TestUtil.RandomAlphabetic(5)}{DefaultEmail}",
            FirstName = DefaultFirstname,
            LastName = DefaultLastname,
            ImageUrl = DefaultImageurl,
            LangKey = DefaultLangkey
        };
    }

    private void InitTest()
    {
        _user = CreateEntity();
        _user.Login = DefaultLogin;
        _user.Email = DefaultEmail;
    }

    [Fact]
    public async Task CreateUser()
    {
        var databaseSizeBeforeCreate = _userManager.Users.Count();

        // Create the User
        var managedUserDto = new ManagedUserDto
        {
            Login = DefaultLogin,
            Password = DefaultPassword,
            FirstName = DefaultFirstname,
            LastName = DefaultLastname,
            Email = DefaultEmail,
            Activated = true,
            ImageUrl = DefaultImageurl,
            LangKey = DefaultLangkey,
            Roles = new HashSet<string>
            {
                RolesConstants.USER
            }
        };

        var response = await _client.PostAsync("/api/admin/users", TestUtil.ToJsonContent(managedUserDto));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Validate the User in the database
        var userList = _userManager.Users.ToList();
        userList.Count().Should().Be(databaseSizeBeforeCreate + 1);
        var testUser = userList[userList.Count - 1];
        testUser.Login.Should().Be(DefaultLogin);
        testUser.FirstName.Should().Be(DefaultFirstname);
        testUser.LastName.Should().Be(DefaultLastname);
        testUser.Email.Should().Be(DefaultEmail);
        testUser.ImageUrl.Should().Be(DefaultImageurl);
        testUser.LangKey.Should().Be(DefaultLangkey);
    }

    [Fact]
    public async Task CreateUserWithExistingEmail()
    {
        // Initialize the database
        await _userManager.CreateAsync(_user);
        var databaseSizeBeforeCreate = _userManager.Users.Count();

        var managedUserDto = new ManagedUserDto
        {
            Login = "anotherlogin", // this email should already be used
            Password = DefaultPassword,
            FirstName = DefaultFirstname,
            LastName = DefaultLastname,
            Email = DefaultEmail,
            Activated = true,
            ImageUrl = DefaultImageurl,
            LangKey = DefaultLangkey,
            Roles = new HashSet<string>
            {
                RolesConstants.USER
            }
        };

        // Create the User
        var response = await _client.PostAsync("/api/admin/users", TestUtil.ToJsonContent(managedUserDto));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Validate the User in the database
        var userList = _userManager.Users.ToList();
        userList.Count().Should().Be(databaseSizeBeforeCreate);
    }

    [Fact]
    public async Task CreateUserWithExistingId()
    {
        var databaseSizeBeforeCreate = _userManager.Users.Count();

        var managedUserDto = new ManagedUserDto
        {
            Id = "id",
            Login = DefaultLogin,
            Password = DefaultPassword,
            FirstName = DefaultFirstname,
            LastName = DefaultLastname,
            Email = DefaultEmail,
            Activated = true,
            ImageUrl = DefaultImageurl,
            LangKey = DefaultLangkey,
            Roles = new HashSet<string> {
                RolesConstants.USER
            }
        };

        // An entity with an existing ID cannot be created, so this API call must fail
        var response = await _client.PostAsync("/api/admin/users", TestUtil.ToJsonContent(managedUserDto));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Validate the User in the database
        var userList = _userManager.Users.ToList();
        userList.Count().Should().Be(databaseSizeBeforeCreate);
    }

    [Fact]
    public async Task CreateUserWithExistingLogin()
    {
        // Initialize the database
        await _userManager.CreateAsync(_user);
        var databaseSizeBeforeCreate = _userManager.Users.Count();

        var managedUserDto = new ManagedUserDto
        {
            Login = DefaultLogin, // this login should already be used
            Password = DefaultPassword,
            FirstName = DefaultFirstname,
            LastName = DefaultLastname,
            Email = "anothermail@localhost",
            Activated = true,
            ImageUrl = DefaultImageurl,
            LangKey = DefaultLangkey,
            Roles = new HashSet<string> {
                RolesConstants.USER
            }
        };

        // Create the User
        var response = await _client.PostAsync("/api/admin/users", TestUtil.ToJsonContent(managedUserDto));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Validate the User in the database
        var userList = _userManager.Users.ToList();
        userList.Count().Should().Be(databaseSizeBeforeCreate);
    }

    [Fact]
    public async Task DeleteUser()
    {
        // Initialize the database
        await _userManager.CreateAsync(_user);
        var databaseSizeBeforeDelete = _userManager.Users.Count();

        var response = await _client.DeleteAsync($"/api/admin/users/{_user.Login}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Validate the database is empty
        var userList = _userManager.Users.ToList();
        userList.Count().Should().Be(databaseSizeBeforeDelete - 1);
    }

    [Fact]
    public async Task GetAllAuthorities()
    {
        var response = await _client.GetAsync("/api/admin/users/authorities");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = JToken.Parse(await response.Content.ReadAsStringAsync());
        var token = json.SelectToken("$");
        token.Should().BeOfType<JArray>();
        var array = token.ToObject<IEnumerable<string>>();
        array.Should().Contain(new[] { RolesConstants.USER, RolesConstants.ADMIN });
    }

    [Fact]
    public async Task GetAllUsers()
    {
        // Initialize the database
        await _userManager.CreateAsync(_user);

        // Get all the users
        var response = await _client.GetAsync("/api/admin/users?sort=id,desc");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = JToken.Parse(await response.Content.ReadAsStringAsync());
        json.SelectTokens("$.[*].login").Should().Contain(DefaultLogin);
        json.SelectTokens("$.[*].firstName").Should().Contain(DefaultFirstname);
        json.SelectTokens("$.[*].lastName").Should().Contain(DefaultLastname);
        json.SelectTokens("$.[*].email").Should().Contain(DefaultEmail);
        json.SelectTokens("$.[*].imageUrl").Should().Contain(DefaultImageurl);
        json.SelectTokens("$.[*].langKey").Should().Contain(DefaultLangkey);
    }

    [Fact]
    public async Task GetNonExistingUser()
    {
        var response = await _client.GetAsync("/api/admin/users/unknown");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUser()
    {
        // Initialize the database
        await _userManager.CreateAsync(_user);

        // Get the users
        var response = await _client.GetAsync($"/api/admin/users/{_user.Login}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = JToken.Parse(await response.Content.ReadAsStringAsync());
        json.SelectToken("$.login").Value<string>().Should().Be(DefaultLogin);
        json.SelectToken("$.firstName").Value<string>().Should().Be(DefaultFirstname);
        json.SelectToken("$.lastName").Value<string>().Should().Be(DefaultLastname);
        json.SelectToken("$.email").Value<string>().Should().Be(DefaultEmail);
        json.SelectToken("$.imageUrl").Value<string>().Should().Be(DefaultImageurl);
        json.SelectToken("$.langKey").Value<string>().Should().Be(DefaultLangkey);
    }

    [Fact]
    public void TestUserDtoToUser()
    {
        var userDto = new UserDto
        {
            Id = DefaultId,
            Login = DefaultLogin,
            FirstName = DefaultFirstname,
            LastName = DefaultLastname,
            Email = DefaultEmail,
            Activated = true,
            ImageUrl = DefaultImageurl,
            LangKey = DefaultLangkey,
            //TODO add CreatedDate, etc.
            Roles = new HashSet<string> {
                RolesConstants.USER
            }
        };
        var user = _mapper.Map<User>(userDto);
        user.Id.Should().Be(DefaultId);
        user.Login.Should().Be(DefaultLogin);
        user.FirstName.Should().Be(DefaultFirstname);
        user.LastName.Should().Be(DefaultLastname);
        user.Email.Should().Be(DefaultEmail);
        user.Activated.Should().Be(true);
        user.ImageUrl.Should().Be(DefaultImageurl);
        user.LangKey.Should().Be(DefaultLangkey);
        user.UserRoles.Select(it => it.Role.Name).Should().OnlyContain(it => it == RolesConstants.USER);
    }

    [Fact]
    public void TestUserEquals()
    {
        TestUtil.EqualsVerifier(typeof(User));
        var user1 = new User
        {
            Id = "user-1"
        };
        var user2 = new User
        {
            Id = user1.Id
        };
        user1.Should().Be(user2);
        user2.Id = "user-2";
        user1.Should().NotBe(user2);
        user1.Id = null;
        user1.Should().NotBe(user2);
    }

    [Fact]
    public void TestUserToUserDto()
    {
        _user.Id = DefaultId;
        //TODO set CreatedBy etc
        var userRoles = new HashSet<UserRole>
        {
            new UserRole
            {
                UserId = _user.Id,
                Role = new Role {Name = RolesConstants.USER}
            }
        };
        _user.UserRoles = userRoles;

        var userDto = _mapper.Map<UserDto>(_user);
        userDto.Id.Should().Be(DefaultId);
        userDto.Login.Should().Be(DefaultLogin);
        userDto.FirstName.Should().Be(DefaultFirstname);
        userDto.LastName.Should().Be(DefaultLastname);
        userDto.Email.Should().Be(DefaultEmail);
        userDto.Activated.Should().Be(true);
        userDto.ImageUrl.Should().Be(DefaultImageurl);
        userDto.LangKey.Should().Be(DefaultLangkey);
        //TODO check CreatedBy,
        userDto.Roles.Should().OnlyContain(it => it == RolesConstants.USER);
    }

    [Fact]
    public async Task UpdateUser()
    {
        // Initialize the database
        await _userManager.CreateAsync(_user);
        var databaseSizeBeforeUpdate = _userManager.Users.Count();

        //Update the user
        var updatedUser = await _userManager.FindByIdAsync(_user.Id);

        var managedUserDto = new ManagedUserDto
        {
            Id = updatedUser.Id,
            Login = updatedUser.Login,
            Password = UpdatedPassword,
            FirstName = UpdatedFirstname,
            LastName = UpdatedLastname,
            Email = UpdatedEmail,
            Activated = updatedUser.Activated,
            ImageUrl = UpdatedImageurl,
            LangKey = UpdatedLangkey,
            Roles = new HashSet<string>
            {
                RolesConstants.USER
            }
        };

        var response = await _client.PutAsync("/api/admin/users", TestUtil.ToJsonContent(managedUserDto));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Validate the User in the database
        var userList = _userManager.Users.ToList();
        userList.Count().Should().Be(databaseSizeBeforeUpdate);
        //            var testUser = userList[userList.Count - 1];
        //            TODO FIX database refresh to prevent the usage of context/Reload
        var testUser = Fixme.ReloadUser(_factory, updatedUser);
        testUser.FirstName.Should().Be(UpdatedFirstname);
        testUser.LastName.Should().Be(UpdatedLastname);
        testUser.Email.Should().Be(UpdatedEmail);
        testUser.ImageUrl.Should().Be(UpdatedImageurl);
        testUser.LangKey.Should().Be(UpdatedLangkey);
    }

    [Fact]
    public async Task UpdateUserExistingEmail()
    {
        // Initialize the database with 2 users
        await _userManager.CreateAsync(_user);

        var anotherUser = new User
        {
            Login = "jhipster",
            PasswordHash = _passwordHasher.HashPassword(null, TestUtil.RandomAlphabetic(60)),
            Activated = true,
            Email = "jhipster@localhost",
            FirstName = "java",
            LastName = "hipster",
            ImageUrl = "",
            LangKey = "en"
        };
        await _userManager.CreateAsync(anotherUser);

        //Update the user
        var updatedUser = await _userManager.FindByIdAsync(_user.Id);

        //TODO Add CreatedBy, CreatedDate, ModifiedBy, ModidfieDate
        var managedUserDto = new ManagedUserDto
        {
            Id = updatedUser.Id,
            Login = updatedUser.Login,
            Password = updatedUser.PasswordHash,
            FirstName = updatedUser.FirstName,
            LastName = updatedUser.LastName,
            Email = "jhipster@localhost", //this email should be already used by anotherUser
            Activated = updatedUser.Activated,
            ImageUrl = updatedUser.ImageUrl,
            LangKey = updatedUser.LangKey,
            Roles = new HashSet<string>
            {
                RolesConstants.USER
            }
        };

        var response = await _client.PutAsync("/api/admin/users", TestUtil.ToJsonContent(managedUserDto));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUserExistingLogin()
    {
        // Initialize the database
        await _userManager.CreateAsync(_user);

        var anotherUser = new User
        {
            Login = "jhipster",
            PasswordHash = _passwordHasher.HashPassword(null, TestUtil.RandomAlphabetic(60)),
            Activated = true,
            Email = "jhipster@localhost",
            FirstName = "java",
            LastName = "hipster",
            ImageUrl = "",
            LangKey = "en"
        };
        await _userManager.CreateAsync(anotherUser);

        //Update the user
        var updatedUser = await _userManager.FindByIdAsync(_user.Id);

        //TODO Add CreatedBy, CreatedDate, ModifiedBy, ModidfieDate
        var managedUserDto = new ManagedUserDto
        {
            Id = updatedUser.Id,
            Login = "jhipster", //this login should be already used by anotherUser
            Password = updatedUser.PasswordHash,
            FirstName = updatedUser.FirstName,
            LastName = updatedUser.LastName,
            Email = updatedUser.Email,
            Activated = updatedUser.Activated,
            ImageUrl = updatedUser.ImageUrl,
            LangKey = updatedUser.LangKey,
            Roles = new HashSet<string>
            {
                RolesConstants.USER
            }
        };

        var response = await _client.PutAsync("/api/admin/users", TestUtil.ToJsonContent(managedUserDto));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUserLogin()
    {
        // Initialize the database
        await _userManager.CreateAsync(_user);
        var databaseSizeBeforeUpdate = _userManager.Users.Count();

        //Update the user
        var updatedUser = await _userManager.FindByIdAsync(_user.Id);

        //TODO Add CreatedBy, CreatedDate, ModifiedBy, ModidfieDate
        var managedUserDto = new ManagedUserDto
        {
            Id = updatedUser.Id,
            Login = UpdatedLogin,
            Password = UpdatedPassword,
            FirstName = UpdatedFirstname,
            LastName = UpdatedLastname,
            Email = UpdatedEmail,
            Activated = updatedUser.Activated,
            ImageUrl = UpdatedImageurl,
            LangKey = UpdatedLangkey,
            Roles = new HashSet<string>
            {
                RolesConstants.USER
            }
        };

        var response = await _client.PutAsync("/api/admin/users", TestUtil.ToJsonContent(managedUserDto));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Validate the User in the database
        var userList = _userManager.Users.ToList();
        userList.Count().Should().Be(databaseSizeBeforeUpdate);
        //            var testUser = userList[userList.Count - 1];
        //            TODO FIX database refresh to prevent the usage of context/Reload
        var testUser = Fixme.ReloadUser(_factory, updatedUser);
        // Ignore case
        testUser.Login.Should().BeEquivalentTo(UpdatedLogin);
        testUser.FirstName.Should().Be(UpdatedFirstname);
        testUser.LastName.Should().Be(UpdatedLastname);
        testUser.Email.Should().Be(UpdatedEmail);
        testUser.ImageUrl.Should().Be(UpdatedImageurl);
        testUser.LangKey.Should().Be(UpdatedLangkey);
    }
}
