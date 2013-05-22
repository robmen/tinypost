namespace RobMensching.TinyPost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Text;

    public enum PosterType
    {
        Unknown,
        Atom,
        Zendesk,
    };

    public class CommandLine
    {
        public PosterType Type { get; set; }

        public string Path { get; set; }

        public string Subdomain { get; set; }

        public int UnpublishedForumId { get; set; }

        public Uri Uri { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public static CommandLine Parse(string[] args)
        {
            CommandLine commandLine = new CommandLine();

            for (int i = 0; i < args.Length; ++i)
            {
                string arg = args[i];
                if (commandLine.Type == PosterType.Unknown)
                {
                    switch (arg.ToLowerInvariant())
                    {
                        case "atom":
                            commandLine.Type = PosterType.Atom;
                            break;

                        case "zendesk":
                            commandLine.Type = PosterType.Zendesk;
                            break;

                        default:
                            throw new ApplicationException(String.Format("Unknown action: {0}", arg));
                    }
                }
                else if (arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    string param = arg.Substring(1);
                    switch (param)
                    {
                        case "u":
                        case "user":
                            commandLine.User = args[++i];
                            break;

                        case "p":
                        case "password":
                            commandLine.Password = args[++i];
                            break;

                        default:
                            throw new ApplicationException(String.Format("Unknown command-line argument: {0}", arg));
                    }
                }
                else
                {
                    if (commandLine.Type == PosterType.Atom && commandLine.Uri == null)
                    {
                        commandLine.Uri = new Uri(arg);
                    }
                    else if (commandLine.Type == PosterType.Zendesk && commandLine.Subdomain == null)
                    {
                        commandLine.Subdomain = arg;
                    }
                    else if (String.IsNullOrEmpty(commandLine.Path))
                    {
                        commandLine.Path = arg;
                    }
                    else
                    {
                        throw new ApplicationException(String.Format("Unknown command-line argument: {0}", arg));
                    }
                }
            }

            if (commandLine.Type == PosterType.Atom && commandLine.Uri == null)
            {
                throw new ApplicationException("atom requires a URI to post to.");
            }
            else if (commandLine.Type == PosterType.Zendesk && commandLine.Subdomain == null)
            {
                throw new ApplicationException("zendesk requires a subdomain to post to.");
            }

            return commandLine;
        }
    }
}
