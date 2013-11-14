// <copyright file="Program.cs" company="RobMensching.com LLC">
//    Copyright (c) RobMensching.com LLC.  All rights reserved.
// </copyright>

namespace RobMensching.TinyPost
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using PosterApi;

    public class Program
    {
        public static void Main(string[] args)
        {
            Poster poster;
            string path;

            Program.Header();

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
                    CommandLine.Help();
                    return;
                }
            }
            catch (ApplicationException e)
            {
                CommandLine.Help(e.Message);
                return;
            }

            IEnumerable<Document> posted = poster.Process(path);
            foreach (var doc in posted)
            {
                Console.WriteLine("Posted: {1}\r\n at: {2}\r\n from: {0}\r\n", doc.Path, doc.Title, doc.Published);
            }

#if DEBUG
            Console.ReadKey(false);
#endif
        }

        private static void Header()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            AssemblyProductAttribute product = Program.GetAttribute<AssemblyProductAttribute>(assembly);
            AssemblyCopyrightAttribute copyright = Program.GetAttribute<AssemblyCopyrightAttribute>(assembly);

            Console.WriteLine("{0} version {1}", product.Product, fileVersion.FileVersion);
            Console.WriteLine(copyright.Copyright);
        }

        private static T GetAttribute<T>(Assembly assembly) where T : Attribute
        {
            object[] customAttributes = assembly.GetCustomAttributes(typeof(T), false);
            if (null != customAttributes && 0 < customAttributes.Length)
            {
                return customAttributes[0] as T;
            }

            return null;
        }
    }
}
