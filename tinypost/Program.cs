// <copyright file="Program.cs" company="RobMensching.com LLC">
//    Copyright (c) RobMensching.com LLC.  All rights reserved.
// </copyright>

namespace RobMensching.TinyPost
{
    using System;
    using System.Collections.Generic;
    using PosterApi;

    public class Program
    {
        public static void Main(string[] args)
        {
            Poster poster;
            string path;

            try
            {
                CommandLine cmdline = CommandLine.Parse(args);
                path = cmdline.Path ?? ".";

                if (cmdline.Type == PosterType.Atom)
                {
                    poster = new AtomPoster(cmdline.Uri, cmdline.User, cmdline.Password);
                }
                else if (cmdline.Type == PosterType.Zendesk)
                {
                    poster = new ZendeskPoster(cmdline.Subdomain, cmdline.User, cmdline.Password);
                }
                else
                {
                    throw new ApplicationException("Unknown action type.");
                }
            }
            catch (ApplicationException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            IEnumerable<Document> posted = poster.Process(path);
            foreach (var doc in posted)
            {
                Console.WriteLine("Posted: {1}\r\n at: {2}\r\n from: {0}\r\n", doc.Path, doc.Title, doc.Published);
            }

            Console.ReadKey(false);
        }
    }
}
