namespace RobMensching.TinyPost
{
    using System;

    public enum PosterType
    {
        Unknown,
        Help,
        Atom,
        Zendesk,
    };

    public class CommandLine
    {
        public PosterType Type { get; set; }

        public string Path { get; set; }

        public string Subdomain { get; set; }

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

                        case "help":
                        case "-?":
                        case "/?":
                            commandLine.Type = PosterType.Help;
                            break;

                        default:
                            throw new ApplicationException(String.Format("Unknown action: '{0}'. Valid actions are: atom or zendesk.", arg));
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
                throw new ApplicationException("zendesk requires a sub-domain to post to.");
            }

            return commandLine;
        }

        public static void Help(string error = null)
        {
            Console.WriteLine("");
            Console.WriteLine("  tinypost.exe [atom|zendesk] [-u username] [-p password] uri [path]");
            Console.WriteLine("    atom    - posts to an atompub end-point at uri.");
            Console.WriteLine("    zendesk - posts to an sub-domain on Zendesk at uri");
            Console.WriteLine("");

            if (!String.IsNullOrEmpty(error))
            {
                ConsoleColor original = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(error);

                Console.ForegroundColor = original;
                Console.WriteLine("");
            }
        }
    }
}
