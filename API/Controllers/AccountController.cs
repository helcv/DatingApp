﻿using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API;

public class AccountController : BaseApiController
{
    private readonly DataContext _context;

    public AccountController(DataContext context){
        _context = context;
    }

    [HttpPost("register")]      //  api/Account/register
    [Produces("application/json")]
    public async Task<ActionResult<AppUser>> Register(RegisterDto registerDto){

        if(await UserExists(registerDto.Username)) return BadRequest("Username is taken");

        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            UserName = registerDto.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AppUser>> Login(LoginDto loginDto)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x =>
         x.UserName == loginDto.Username);          // We use .Find with primary key

         if(user == null) return Unauthorized("invalid username!");

         using var hmac = new HMACSHA512(user.PasswordSalt);

         var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

         for (int i = 0; i < computedHash.Length; i++)
         {
            if( user.PasswordHash[i] != computedHash[i])    return Unauthorized("invalid password!");
         }

         return  user;
    }

    public async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
}