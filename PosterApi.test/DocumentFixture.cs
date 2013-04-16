namespace PosterApi.test
{
    using System;
    using System.IO;
    using System.Text;
    using Xunit;

    public class DocumentFixture
    {
        [Fact]
        public void CanParseDocumentWithHeader()
        {
            string s = "post: true\r\ndate:2013-04-16T12:10\r\ntitle:Test title\r\n\r\nThis is document text.\r\n";
            Document doc = LoadDocFromString(s);
            Assert.True(doc.Post);
            Assert.Equal(new DateTime(2013, 4, 16, 12, 10, 0), doc.Date);
            Assert.Equal("Test title", doc.Title);
            Assert.Equal("This is document text.", doc.Text);
            Assert.Equal("<p>This is document text.</p>", doc.RenderedText);
        }

        [Fact]
        public void CanParseDocumentWithoutHeader()
        {
            string s = "# Test title\r\n\r\n## This is a header\r\nThis is document text.\r\n";
            Document doc = LoadDocFromString(s);
            Assert.Equal("Test title", doc.Title);
            Assert.Equal("## This is a header\r\nThis is document text.", doc.Text);
            Assert.Equal("<h2>This is a header</h2>\n\n<p>This is document text.</p>", doc.RenderedText);
        }

        [Fact]
        public void CanParseDocumentWithComments()
        {
            string s = ";post: true\r\ntitle:Test title\r\n\r\nThis is document text.\r\n";
            Document doc = LoadDocFromString(s);
            Assert.False(doc.Post);
            Assert.Equal("Test title", doc.Title);
            Assert.Equal("This is document text.", doc.Text);
            Assert.Equal("<p>This is document text.</p>", doc.RenderedText);
        }

        [Fact]
        public void FailsParseDocument()
        {
            string s = "post: true\r\ninvalid: Invalid data\r\n\r\nThis is document text.\r\n";
            Assert.Throws<ApplicationException>(delegate {
                LoadDocFromString(s);
            });
        }

        private Document LoadDocFromString(string documentString)
        {
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(documentString)))
            {
                return new Document().Load(stream);
            }
        }
    }
}
