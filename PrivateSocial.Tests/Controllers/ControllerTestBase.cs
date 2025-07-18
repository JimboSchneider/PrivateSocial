using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrivateSocial.ApiService.Data;
using PrivateSocial.ApiService.Data.Entities;
using PrivateSocial.Tests.Helpers;

namespace PrivateSocial.Tests.Controllers;

public abstract class ControllerTestBase : IDisposable
{
    protected readonly ApplicationDbContext Context;

    protected ControllerTestBase()
    {
        Context = TestDbContextFactory.CreateInMemoryContext();
        TestDataBuilder.Reset();
    }

    protected static ControllerContext CreateControllerContext(int? userId = null, string? username = null)
    {
        var claims = new List<Claim>();
        
        if (userId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }
        
        if (!string.IsNullOrEmpty(username))
        {
            claims.Add(new Claim(ClaimTypes.Name, username));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    protected async Task<User> SeedUserAsync(User? user = null)
    {
        user ??= TestDataBuilder.CreateUser();
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();
        return user;
    }

    protected async Task<Post> SeedPostAsync(Post? post = null)
    {
        post ??= TestDataBuilder.CreatePost();
        
        // Ensure the user exists if not already seeded
        if (!await Context.Users.AnyAsync(u => u.Id == post.UserId))
        {
            var user = TestDataBuilder.CreateUser();
            user.Id = post.UserId;
            await Context.Users.AddAsync(user);
        }
        
        await Context.Posts.AddAsync(post);
        await Context.SaveChangesAsync();
        return post;
    }

    protected async Task<List<Post>> SeedMultiplePostsAsync(int count, int? userId = null)
    {
        var posts = new List<Post>();
        
        for (int i = 0; i < count; i++)
        {
            var post = TestDataBuilder.CreatePost(userId: userId);
            posts.Add(post);
        }
        
        // Ensure user exists
        if (userId.HasValue && !await Context.Users.AnyAsync(u => u.Id == userId))
        {
            var user = TestDataBuilder.CreateUser();
            user.Id = userId.Value;
            await Context.Users.AddAsync(user);
        }
        
        await Context.Posts.AddRangeAsync(posts);
        await Context.SaveChangesAsync();
        
        return posts;
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}