using FRifugioBlog.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRifugioBlog.Services
{
    public interface IPostService
    {
        /// <summary>
        /// Returns a list of all the posts names
        /// </summary>
        Task<IEnumerable<string>> GetAllPostNamesAsync();
        
        Task<Post> GetPostAsync(string filename);

        /// <summary>
        /// Returns a post object with all valued properties, except for the Body
        /// </summary>
        /// <param name="path">Filename of the post (with extension)</param>
        Task<Post> GetPostMetadataAsync(string filename);
    }
}
