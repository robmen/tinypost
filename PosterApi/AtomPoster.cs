// <copyright file="AtomPoster.cs" company="RobMensching.com LLC">
//    Copyright (c) RobMensching.com LLC.  All rights reserved.
// </copyright>

namespace PosterApi
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml.Linq;

    public class AtomPoster : Poster
    {
        private static readonly string AtompubContentType = "application/atom+xml;type=entry";
        private static readonly XNamespace AtomNamespace = "http://www.w3.org/2005/Atom";

        public AtomPoster(Uri uri, string username = null, string password = null)
            : this(uri, username, password, DateTime.Now)
        {
        }

        public AtomPoster(Uri uri, string username, string password, DateTime publishAt) :
            base(publishAt)
        {
            this.PrefixSuccessfulPostWithDate = true;

            this.Uri = uri;
            this.Username = username;
            this.Password = password;
        }

        public Uri Uri { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        protected override PublishResult Publish(string location, string author, string email, string title, string slug, DateTime? date, string text, string html, string[] tags)
        {
            bool create = true;
            if (String.IsNullOrEmpty(location))
            {
                location = this.Uri.AbsoluteUri;
            }
            else
            {
                create = false;
            }

            Uri uri = new Uri(location);
            XElement entryXml = CreateEntryXml(author, email, title, date, html, tags);
            byte[] entryBytes = Encoding.UTF8.GetBytes(entryXml.ToString());

            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;

            // Set credentials
            if (!String.IsNullOrEmpty(this.Username))
            {
                Uri authorityUri = new Uri(this.Uri.GetLeftPart(UriPartial.Authority));

                CredentialCache credentials = new CredentialCache();
                credentials.Add(authorityUri, "Basic", new NetworkCredential(this.Username, this.Password));
                credentials.Add(authorityUri, "Digest", new NetworkCredential(this.Username, this.Password));

                request.Credentials = credentials;
            }

            // Set slug.
            if (!String.IsNullOrEmpty(slug))
            {
                request.Headers.Add("Slug", slug);
            }

            // Content to send.
            request.Method = create ? "POST" : "PUT";
            request.ContentType = AtompubContentType;
            request.ContentLength = entryBytes.Length;

            Stream stream = request.GetRequestStream();
            stream.Write(entryBytes, 0, entryBytes.Length);
            stream.Close();

            // Send request.
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new ApplicationException("Failed to post entry to server.");
            }

            return new PublishResult()
            {
                Id = response.Headers["Location"] ?? location,
                Published = DateTime.Parse(entryXml.Element(AtomNamespace + "updated").Value),
            };
        }

        public XElement CreateEntryXml(string author, string email, string title, DateTime? date, string html, string[] tags)
        {
            XElement entryXml = new XElement(AtomNamespace + "entry",
                new XElement(AtomNamespace + "id", String.Format("uri:{0}", Guid.NewGuid())),
                new XElement(AtomNamespace + "title", title)
                );

            if (!String.IsNullOrEmpty(author))
            {
                entryXml.Add(new XElement(AtomNamespace + "author",
                                new XElement(AtomNamespace + "name", author),
                                String.IsNullOrEmpty(email) ? null : new XElement(AtomNamespace + "email", email)
                                ));
            }

            entryXml.Add(new XElement(AtomNamespace + "content",
                new XAttribute("type", "html"), 
                html));

            if (tags != null)
            {
                foreach (string tag in tags)
                {
                    entryXml.Add(new XElement(AtomNamespace + "category",
                        new XAttribute("term", tag)
                        ));
                }
            }

            DateTime updated = DateTime.UtcNow;
            if (date.HasValue)
            {
                updated = date.Value.ToUniversalTime();
                entryXml.Add(new XElement(AtomNamespace + "published", updated.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            }

            entryXml.Add(new XElement(AtomNamespace + "updated", updated.ToString("yyyy-MM-ddTHH:mm:ssZ")));

            return entryXml;
        }
    }
}
