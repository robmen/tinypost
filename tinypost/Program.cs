namespace poster
{
    using System;
    using System.Collections.Generic;
    using PosterApi;

    public class Program
    {
        public static void Main(string[] args)
        {
            IEnumerable<Document> posted = new AtomPoster(new Uri("http://example.com")).Process(".");
            foreach (var doc in posted)
            {
                Console.WriteLine("Posted: {1}\r\nat: {2}\r\nfrom: {0}\r\n", doc.Path, doc.Title, doc.Published);
            }

            //Console.ReadKey(false);
        }
    }
}
