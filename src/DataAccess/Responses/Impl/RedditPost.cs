﻿using System.Collections.Generic;

namespace DataAccess.Responses.Impl
{
    public class RedditPost : IApiCollection<IApiImage>
    {
        public string Domain { get; set; }
        public string Subreddit { get; set; }
        public string Author { get; set; }
        public bool Over_18 { get; set; }
        public string Permalink { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public bool Is_self { get; set; }
        public string Name { get; set; } //The posts 'Fullname'

        public string ShortTitle
        {
            get
            {
                if (Title == null) return null;
                return Title.Length <= 40 ? Title : Title.Substring(0, 40);
            }
        }

        public bool IsAlbum => Image == null && Album != null;

        public IApiImage Image { get; set; }
        public IApiCollection<IApiImage> Album { get; set; }

        /// <inheritdoc />
        public IEnumerable<IApiImage> GetImages()
        {
            if (Image != null)
            {
                return new List<IApiImage> { Image };
            }

            return Album?.GetImages() != null ? Album.GetImages() : new List<IApiImage>();
        }

    }
}
