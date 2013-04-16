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

            this.PostsFolderName = "Posts";
            this.PublishFolderName = "Publish";
            this.WorkingFolderName = "Working" + " " + this.PublishAt.ToString("yyyy-MM-ddTHH.mm.ss");
        }

        public string PostsFolderName { get; set; }

        public string PublishFolderName { get; set; }

        public string WorkingFolderName { get; set; }

        public DateTime PublishAt { get; set; }

        public IEnumerable<Document> Process(string baseFolder)
        {
            string publishFolder = Path.Combine(baseFolder, this.PublishFolderName);
            string workingFolder = Path.Combine(baseFolder, this.WorkingFolderName);
            string postsFolder = Path.Combine(baseFolder, this.PostsFolderName);

            IEnumerable<Document> docs = FindReadyDocuments(publishFolder);
            docs = MoveToWorkingFolder(workingFolder, docs);
            docs = Post(postsFolder, docs);

            CleanWorkingFolder(workingFolder);

            return docs;
        }

        public IEnumerable<Document> FindReadyDocuments(string publishFolder)
        {
            List<Document> documents = new List<Document>();
            DirectoryInfo folder = new DirectoryInfo(publishFolder);
            if (folder.Exists)
            {
                foreach (FileInfo file in folder.EnumerateFiles("*.md"))
                {
                    try
                    {
                        using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                        {
                            Document doc = new Document(file.FullName).Load(stream);
                            if (doc.Date <= this.PublishAt)
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

        public IEnumerable<Document> MoveToWorkingFolder(string workingFolder, IEnumerable<Document> docs)
        {
            DirectoryInfo folder = new DirectoryInfo(workingFolder);
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

        public IEnumerable<Document> Post(string publishFolder, IEnumerable<Document> docs)
        {
            DirectoryInfo folder = new DirectoryInfo(publishFolder);
            if (!folder.Exists)
            {
                folder.Create();
            }

            List<Document> published = new List<Document>();
            foreach (Document doc in docs)
            {
                string location;
                doc.Published = this.Publish(doc.AuthorName, doc.AuthorEmail, doc.Title, doc.Slug, doc.Date, doc.Text, doc.RenderedText, doc.Tags, out location);
                doc.Location = doc.Location;

                string newFile = String.Concat(doc.Published.Value.ToString("yyyy-MM-dd"), " ", Path.GetFileName(doc.Path));
                string newPath = Path.Combine(folder.FullName, newFile);

                using (StreamWriter writer = new StreamWriter(newPath))
                {
                    doc.Save(writer);
                }

                File.Delete(doc.Path);
                doc.Path = newPath;
                published.Add(doc);
            }

            return published;
        }

        public void CleanWorkingFolder(string workingFolder)
        {
            try
            {
                Directory.Delete(workingFolder, false);
            }
            catch (IOException)
            {
                // TODO: print message.
            }
        }

        protected virtual DateTime Publish(string author, string email, string title, string slug, DateTime? date, string text, string html, string[] tags, out string location)
        {
            location = null;
            return date.HasValue ? date.Value : DateTime.Now;
        }
    }
}
