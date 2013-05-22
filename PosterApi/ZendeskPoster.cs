// <copyright file="ZendeskPoster.cs" company="RobMensching.com LLC">
//    Copyright (c) RobMensching.com LLC.  All rights reserved.
// </copyright>

namespace PosterApi
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Net;
    using System.Text;
    using Newtonsoft.Json;

    public class ZendeskPoster : Poster
    {
        private static readonly string ForumsUrlFormat = "https://{0}.zendesk.com/api/v2/forums.json";

        private static readonly string TopicsContentType = "application/json";
        private static readonly string TopicsUrlFormat = "https://{0}.zendesk.com/api/v2/topics.json";

        public ZendeskPoster(string subdomain, string username = null, string password = null)
            : this(subdomain, username, password, DateTime.Now)
        {
        }

        public ZendeskPoster(string subdomain, string username, string password, DateTime publishAt) :
            base(publishAt)
        {
            this.Subdomain = subdomain;
            this.Username = username;
            this.Password = password;
        }

        public string Subdomain { get; set; }

        public int StagedForumId { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Token { get; set; }

        protected override PublishResult Publish(string location, string author, string email, string title, string slug, DateTime? date, string text, string html, string[] tags)
        {
            bool create = true;
            int forumId = 0;
            if (String.IsNullOrEmpty(location))
            {
                location = String.Format(TopicsUrlFormat, this.Subdomain);

                if (this.StagedForumId == 0)
                {
                    this.StagedForumId = this.FindStagedForumId();
                }

                forumId = this.StagedForumId;
            }
            else
            {
                create = false;
                forumId = 0;
            }

            string topicJson = CreateTopicJson(forumId, author, email, title, date, html, tags);
            byte[] topicBytes = Encoding.UTF8.GetBytes(topicJson.ToString());

            Uri uri = new Uri(location);
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;

            // Set credentials
            if (!String.IsNullOrEmpty(this.Username))
            {
                Uri authorityUri = new Uri(uri.GetLeftPart(UriPartial.Authority));

                CredentialCache credentials = new CredentialCache();
                credentials.Add(authorityUri, "Basic", new NetworkCredential(this.Username, this.Password));

                request.Credentials = credentials;
            }

            // Content to send.
            request.Method = create ? "POST" : "PUT";
            request.Accept = TopicsContentType;
            request.ContentType = TopicsContentType;
            request.ContentLength = topicBytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(topicBytes, 0, topicBytes.Length);
            }

            // Send request.
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            if (create && response.StatusCode != HttpStatusCode.Created ||
                !create && response.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException("Failed to post topic to Zendesk: " + uri.AbsoluteUri);
            }

            using (Stream responseStream = response.GetResponseStream())
            {
                DateTime createdAt = this.TopicCreatedAt(responseStream);

                return new PublishResult()
                {
                    Id = response.Headers["Location"] ?? location,
                    Published = createdAt,
                };
            }
        }

        private int FindStagedForumId()
        {
            Uri uri = new Uri(String.Format(ForumsUrlFormat, this.Subdomain));
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;

            // Set credentials
            if (!String.IsNullOrEmpty(this.Username))
            {
                Uri authorityUri = new Uri(uri.GetLeftPart(UriPartial.Authority));

                CredentialCache credentials = new CredentialCache();
                credentials.Add(authorityUri, "Basic", new NetworkCredential(this.Username, this.Password));
                credentials.Add(authorityUri, "Digest", new NetworkCredential(this.Username, this.Password));

                request.Credentials = credentials;

                request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Concat(this.Username, ":", this.Password)));
            }

            // Content to send.
            request.Method = "GET";
            request.Accept = TopicsContentType;

            // Send request.
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException("Failed to get forums from Zendesk: " + uri.AbsoluteUri);
            }

            using (Stream responseStream = response.GetResponseStream())
            {
                Forum forum = this.GetStagedForum(responseStream);
                return forum.id;
            }
        }

        private Forum GetStagedForum(Stream stream)
        {
            ForumsMessage forumsMessage;

            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                string json = reader.ReadToEnd();
                forumsMessage = JsonConvert.DeserializeObject<ForumsMessage>(json);
            }

            return forumsMessage.forums.Where(f => f.name.Equals("staged", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        public string CreateTopicJson(int forumId, string author, string email, string title, DateTime? date, string html, string[] tags)
        {
            TopicMessage topicMessage = new TopicMessage(new Topic()
            {
                forum_id = forumId,
                title = title,
                body = html,
                tags = tags,
            });

            using (StringWriter json = new StringWriter())
            using (JsonWriter writer = new JsonTextWriter(json))
            {
                JsonSerializer.Create(new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }).Serialize(writer, topicMessage);
                return json.ToString();
            }
        }

        public DateTime TopicCreatedAt(Stream stream)
        {
            TopicMessage topicMessage;

            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                string json = reader.ReadToEnd();
                topicMessage = JsonConvert.DeserializeObject<TopicMessage>(json);
            }

            return DateTime.Parse(topicMessage.topic.created_at);
        }

        private class ForumsMessage
        {
            public Forum[] forums { get; set; }
        }

        private class Forum
        {
            public int id { get; set; }

            public string name { get; set; }
        };

        private class TopicMessage
        {
            public TopicMessage(Topic topic)
            {
                this.topic = topic;
            }

            public Topic topic { get; set; }
        }

        private class Topic
        {
            public int forum_id { get; set; }

            public string title { get; set; }

            public string body { get; set; }

            public string[] tags { get; set; }

            public string created_at { get; set; }
        }
    }
}
