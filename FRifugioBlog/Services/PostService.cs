using FRifugioBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using Markdig;
using Markdig.Syntax;
using Markdig.Extensions.Yaml;

namespace FRifugioBlog.Services
{
    public class PostService : IPostService
    {
        private readonly HttpClient _client;
        private const string _basePostPath = "assets/posts/documents/";

        public PostService(HttpClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<string>> GetAllPostsPathAsync()
        {
            var response = await _client.GetFromJsonAsync<JsonElement>("assets/posts/post-list.json");
            var postList = new List<string>();
            foreach (var post in response.GetProperty("post").EnumerateArray())
            {
                postList.Add(_basePostPath + post.GetString());
            }

            return postList;
        }

        public Task<Post> GetPostBodyFromPathAsync(string path)
        {
            throw new NotImplementedException();
        }

        public async Task<Post> GetPostMetadataFromPathAsync(string path)
        {

            // Retrieve and convert to string the file from the specified path
            var s = await _client.GetStringAsync(path);

            var pipeline = new MarkdownPipelineBuilder()
                .UseYamlFrontMatter()
                .Build();

            var document = Markdown.Parse(s, pipeline);

            var yamlLines = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault()
                .Lines.ToString();

            var yamlDeserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

            var post = yamlDeserializer.Deserialize<Post>(yamlLines);
            return post;
        }
    }
}
