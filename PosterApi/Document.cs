namespace PosterApi
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class Document
    {
        public Document()
        {
        }

        public Document(string path)
        {
            this.Path = path;
        }

        public string AuthorName { get; set; }

        public string AuthorEmail { get; set; }

        public DateTime? Date { get; set; }

        public string Location { get; set; }

        public bool Post { get; set; }

        public DateTime? Published { get; set; }

        public string RenderedText { get; set; }

        public string Slug { get; set; }

        public string[] Tags { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public string Path { get; set; }

        public Document Load(Stream stream)
        {
            Regex r = new Regex(@"^(?<key>\w+):\s?(?<value>.+)$", RegexOptions.Compiled);

            StreamReader reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Eat any blank lines or comments at the top of the document or in the header.
                if (String.IsNullOrEmpty(line) || line.StartsWith(";") || line.StartsWith("//"))
                {
                    continue;
                }

                Match m = r.Match(line);
                if (m.Success)
                {
                    string key = m.Groups["key"].Value.ToLowerInvariant();
                    string value = m.Groups["value"].Value.Trim();
                    switch (key)
                    {
                        case "date":
                            this.Date = DateTime.Parse(value);
                            break;

                        case "email":
                            this.AuthorEmail = value;
                            break;

                        case "name":
                            this.AuthorName = value;
                            break;

                        case "location":
                            this.Location = value;
                            break;

                        case "post":
                            this.Post = value.ToLowerInvariant().Equals("true");
                            break;

                        case "slug":
                            this.Slug = value;
                            break;

                        case "tag":
                        case "tags":
                            this.Tags = line.Substring(5).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(t => t.Trim()).Where(t => !String.IsNullOrEmpty(t)).ToArray();
                            break;

                        case "title":
                            this.Title = value;
                            break;

                        default:
                            throw new ApplicationException(String.Format("Unexpected header tag: {0}", key));
                    }
                }
                else
                {
                    break;
                }
            }

            // If the title wasn't explicitly stated, pull the implicit title.
            if (this.Title == null)
            {
                this.Title = line.Trim(new[] { ' ', '#' });
                line = reader.ReadLine();
            }

            this.Text = String.Concat(line, "\r\n", reader.ReadToEnd()).Trim();

            var md = new MarkdownSharp.Markdown(new MarkdownSharp.MarkdownOptions() { AutoHyperlink = true });
            //var md = new MarkdownDeep.Markdown();
            //md.SafeMode = false;
            //md.ExtraMode = false;
            //md.AutoHeadingIDs = true;
            //md.SectionHeader = "<a href='{0}'>";
            //md.SectionHeadingSuffix = "---";
            //md.SectionFooter = "<hr>";
            //md.HtmlClassFootnotes = "footnotes";

            this.RenderedText = md.Transform(this.Text).Trim();
            return this;
        }

        public Document Save(StreamWriter writer)
        {
            bool header = false;
            //StreamWriter writer = new StreamWriter(stream);

            if (this.Post)
            {
                writer.WriteLine("post: true");
                header = true;
            }

            if (!String.IsNullOrEmpty(this.Slug))
            {
                writer.WriteLine("slug: ", this.Slug);
                header = true;
            }

            if (this.Date.HasValue)
            {
                writer.WriteLine("date: {0}", this.Date.Value.ToString("yyyy-MM-ddTHH:mm"));
                header = true;
            }

            if (this.Tags != null && this.Tags.Length > 0)
            {
                writer.WriteLine("tags: {0}", String.Join(", ", this.Tags));
                header = true;
            }

            if (this.Published.HasValue)
            {
                writer.WriteLine("published: {0}", this.Published.Value.ToString("yyyy-MM-ddTHH:mm"));
                header = true;
            }

            if (!String.IsNullOrEmpty(this.Location))
            {
                writer.WriteLine("location: {0}", this.Location);
                header = true;
            }

            if (!String.IsNullOrEmpty(this.Title))
            {
                if (header)
                {
                    writer.WriteLine();
                }

                writer.WriteLine("# {0}", this.Title);
                header = true;
            }

            if (!String.IsNullOrEmpty(this.Text))
            {
                if (header)
                {
                    writer.WriteLine();
                }

                writer.WriteLine(this.Text);
            }

            return this;
        }
    }
}
