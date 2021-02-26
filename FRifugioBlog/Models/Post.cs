using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace FRifugioBlog.Models
{
    public class Post
    {
        [YamlMember(Alias = "title", ApplyNamingConventions = false)]
        public string Title { get; set; }
        
        [YamlMember(Alias = "author", ApplyNamingConventions = false)]
        public string Author { get; set; }
        
        [YamlMember(Alias = "publishDate", ApplyNamingConventions = false)]
        public DateTime PublishDate { get; set; }
        
        [YamlMember(Alias = "summary", ApplyNamingConventions = false)]
        public string Summary { get; set; }
        
        [YamlMember(Alias = "headImagePath", ApplyNamingConventions = false)]
        public string HeadImagePath { get; set; }
        
        [YamlMember(Alias = "categories", ApplyNamingConventions = false)]
        public List<string> Categories { get; set; }

        /// <summary>
        /// It contains the content of the post, already in HTML
        /// </summary>
        public string Body { get; set; }

        public Post()
        {
            Categories = new List<string>();
        }
    }
}
