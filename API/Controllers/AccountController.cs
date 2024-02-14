using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API;

public class AccountController : BaseApiController
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IConfigurationSection _googleCredentials;
    public AccountController(DataContext context, ITokenService tokenService, IMapper mapper, IConfiguration config){
        _context = context;
        _tokenService = tokenService;
        _mapper = mapper;
        _googleCredentials = config.GetSection("GoogleClientId");
    }

    [HttpPost("register")]      //  api/Account/register
    [Produces("application/json")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto){

        if(await UserExists(registerDto.Username)) return BadRequest("Username is taken");

        var user = _mapper.Map<AppUser>(registerDto);

        using var hmac = new HMACSHA512();

        user.UserName = registerDto.Username.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        user.PasswordSalt = hmac.Key;


        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return  new UserDto{
            Username = user.UserName,
            Token = _tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
         };
    }

    [HttpPost("signin-google")]
    public async Task<ActionResult<UserDto>> GoogleRegister(GoogleSignInDTO googleDto){
        var payload = await GoogleJsonWebSignature.ValidateAsync(googleDto.GoogleToken);

        if (payload.Audience.ToString() != _googleCredentials.Value){
            throw new InvalidJwtException("Invalid token");
        }

        var user = _mapper.Map<AppUser>(googleDto);
        
        string[] parts = payload.Email.Split('@');
        var username = parts[0].ToLower();

        using var hmac = new HMACSHA512();

        user.UserName = username;
        user.KnownAs = username;
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(googleDto.Password));
        user.PasswordSalt = hmac.Key;

        if(await UserExists(username)) return BadRequest("Username is taken");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto{
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Token = _tokenService.CreateToken(user),
            Gender = googleDto.Gender
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _context.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);          // We use .Find with primary key

         if(user == null) return Unauthorized("invalid username!");

         using var hmac = new HMACSHA512(user.PasswordSalt);

         var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

         for (int i = 0; i < computedHash.Length; i++)
         {
            if( user.PasswordHash[i] != computedHash[i])    return Unauthorized("invalid password!");
         }

         return  new UserDto{
            Username = user.UserName,
            Token = _tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
         };
    }

    public async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
}
