using PrivateSocial.ApiService.Models;

namespace PrivateSocial.ApiService.Services;

public interface IPostService
{
    Task<PagedResult<PostDto>> GetPostsAsync(int page, int pageSize);
    Task<PostDto?> GetPostByIdAsync(int id);
    Task<PostDto> CreatePostAsync(CreatePostRequest request, int userId);
    Task<PostDto?> UpdatePostAsync(int id, UpdatePostRequest request, int userId);
    Task<bool> DeletePostAsync(int id, int userId);
}
