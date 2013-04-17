// <copyright file="Poster.cs" company="RobMensching.com LLC">
//    Copyright (c) RobMensching.com LLC.  All rights reserved.
// </copyright>

namespace PosterApi
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class Poster
    {
        public Poster() :
            this(DateTime.Now)
        {
        }

        public Poster(DateTime publishAt)
        {
            this.PublishAt = publishAt;

            this.PostsRelativePath = "Posts";
            this.PublishRelativePath = "Publish";
            this.WorkingRelativePath = "Working" + " " + this.PublishAt.ToString("yyyy-MM-ddTHH.mm.ss");
        }

        public string PostsRelativePath { get; set; }

        public string PublishRelativePath { get; set; }

        public string WorkingRelativePath { get; set; }

        public DateTime PublishAt { get; set; }

        public IEnumerable<Document> Process(string baseFolder)
        {
            string publishFolder = Path.Combine(baseFolder, this.PublishRelativePath);
            string workingFolder = Path.Combine(baseFolder, this.WorkingRelativePath);
            string postsFolder = Path.Combine(baseFolder, this.PostsRelativePath);

            IEnumerable<Document> docs = FindPublishableDocuments(publishFolder);
            docs = MoveDocumentsToWorkingFolder(workingFolder, docs);
            docs = PostDocuments(postsFolder, docs);

            CleanWorkingFolder(workingFolder);

            return docs;
        }

        public IEnumerable<Document> FindPublishableDocuments(string publishPath)
        {
            List<Document> documents = new List<Document>();

            DirectoryInfo folder = new DirectoryInfo(publishPath);
            if (folder.Exists)
            {
                foreach (FileInfo file in folder.EnumerateFiles("*.md"))
                {
                    try
                    {
                        using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                        {
                            Document doc = new Document(file.FullName).Load(stream);
                            if (!doc.Date.HasValue || doc.Date <= this.PublishAt)
                            {
                                documents.Add(doc);
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        // TODO: warn.
                    }
                }
            }

            return documents;
        }

        public IEnumerable<Document> MoveDocumentsToWorkingFolder(string workingPath, IEnumerable<Document> docs)
        {
            DirectoryInfo folder = new DirectoryInfo(workingPath);
            if (!folder.Exists)
            {
                folder.Create();
            }

            List<Document> moved = new List<Document>();
            foreach (Document doc in docs)
            {
                string newPath = Path.Combine(folder.FullName, Path.GetFileName(doc.Path));

                File.Move(doc.Path, newPath);
                doc.Path = newPath;

                moved.Add(doc);
            }

            return moved;
        }

        public IEnumerable<Document> PostDocuments(string postsPath, IEnumerable<Document> docs)
        {
            DirectoryInfo folder = new DirectoryInfo(postsPath);
            if (!folder.Exists)
            {
                folder.Create();
            }

            List<Document> posted = new List<Document>();
            foreach (Document doc in docs)
            {
                PublishResult result = this.Publish(doc.AuthorName, doc.AuthorEmail, doc.Title, doc.Slug, doc.Date, doc.Text, doc.RenderedText, doc.Tags);
                doc.Id = result.Id;
                doc.Published = result.Published;

                string newFile = String.Concat(doc.Published.Value.ToString("yyyy-MM-dd"), " ", Path.GetFileName(doc.Path));
                string newPath = Path.Combine(folder.FullName, newFile);

                using (StreamWriter writer = new StreamWriter(newPath))
                {
                    doc.Save(writer);
                }

                // Saved posted document, delete the original.
                File.Delete(doc.Path);
                doc.Path = newPath;

                posted.Add(doc);
            }

            return posted;
        }

        public void CleanWorkingFolder(string workingFolder)
        {
            try
            {
                Directory.Delete(workingFolder, false);
            }
            catch (IOException)
            {
                // TODO: send warning message.
            }
        }

        protected virtual PublishResult Publish(string author, string email, string title, string slug, DateTime? date, string text, string html, string[] tags)
        {
            return new PublishResult()
            {
                Id = null,
                Published = DateTime.MinValue,
            };
        }
    }
}
