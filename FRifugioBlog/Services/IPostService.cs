using FRifugioBlog.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRifugioBlog.Services
{
    public interface IPostService
    {
        /// <summary>
        /// Returns a list of all the posts filepath, in descending order by name
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetAllPostsPathAsync();
        Task<Post> GetPostBodyFromPathAsync(string path);

        /// <summary>
        /// Returns a post object with all valued properties, except for the Body
        /// </summary>
        /// <param name="path">Path of the post</param>
        Task<Post> GetPostMetadataFromPathAsync(string path);
    }
}
