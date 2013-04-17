// <copyright file="Program.cs" company="RobMensching.com LLC">
//    Copyright (c) RobMensching.com LLC.  All rights reserved.
// </copyright>

namespace poster
{
    using System;
    using System.Collections.Generic;
    using PosterApi;

    public class Program
    {
        public static void Main(string[] args)
        {
            Poster poster = null;
            string path = ".";

            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].Equals("atom", StringComparison.OrdinalIgnoreCase))
                {
                    Uri uri = new Uri(args[++i]);
                    string username = null;
                    string password = null;
                    if (args[i + 1].Equals("-u", StringComparison.OrdinalIgnoreCase))
                    {
                        ++i;
                        username = args[++i];
                    }

                    if (args[i + 1].Equals("-p", StringComparison.OrdinalIgnoreCase))
                    {
                        ++i;
                        password = args[++i];
                    }

                    poster = new AtomPoster(uri, username, password);
                }
                else
                {
                    path = args[i];
                }
            }

            if (poster == null)
            {
                return;
            }

            IEnumerable<Document> posted = poster.Process(path);
            foreach (var doc in posted)
            {
                Console.WriteLine("Posted: {1}\r\nat: {2}\r\nfrom: {0}\r\n", doc.Path, doc.Title, doc.Published);
            }

            //Console.ReadKey(false);
        }
    }
}
