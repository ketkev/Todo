using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Todo
{
    class CustomConsole
    {
        Type thisType;
        String CommandString;
        Dir CurrentDirectory;

        public CustomConsole(Dir dir)
        {
            thisType = this.GetType();
            CurrentDirectory = dir;

            RenderDirectory();
            WaitForCommand();
        }

        public void WaitForCommand()
        {
            while (true)
            {
                Console.WriteLine();
                Console.Write("> ");
                CommandString = Console.ReadLine();

                String[] Commands = Regex.Matches(CommandString, @"(?:\""(.+?)\"")|(\w+)")
                                                    .OfType<Match>()
                                                    .Select(m => m.Groups[0].Value)
                                                    .ToArray();

                for (int i = 0; i < Commands.Length; i++)
                {
                    Commands[i] = Commands[i].Replace("\"", "");
                }

                Type[] parameters = (from string item in Commands
                                     where item != Commands[0]
                                     select item.GetType()).ToArray();

                MethodInfo theMethod = thisType.GetMethod(Commands[0].ToLower(), parameters);
                if (theMethod != null)
                {
                    Commands = RemoveFirst(Commands);

                    if (theMethod.GetParameters().Length == Commands.Length)
                    {
                        theMethod.Invoke(this, Commands);
                    }
                    else
                    {
                        incorrectParameterCount(Commands.Length, theMethod.GetParameters().Length);
                    }
                }
                else
                {
                    unknownCommand(CommandString);
                }
            }
        }

        public string[] RemoveFirst(string[] input)
        {
            string[] output = new string[input.Length - 1];

            for (int i = 1; i < input.Length; i++)
            {
                output[i - 1] = input[i];
            }

            return output;
        }

        #region rendering
        public void RenderDirectory()
        {
            Console.Clear();
            ConsoleTable table = new ConsoleTable("name", "description", "planned hours", "actual hours", "done");
            table.Options.EnableCount = false;

            foreach (Todo item in CurrentDirectory.Content)
            {
                table.AddRow(item.Name, item.Description, item.PlannedHours, item.ActualHours, item.Done);
            }

            table.Write();
        }
        #endregion

        #region Commands
        public void add(string command)
        {
            if (!CurrentDirectory.Content.Exists(item => item.Name == command))
            {
                CurrentDirectory.Content.Add(new Todo(command));
                RenderDirectory();
            }
            else
            {
                Console.WriteLine($"item {command} already exists");
            }
        }

        public void add(string name, string description, string plannedHoursString)
        {
            decimal plannedHours = Decimal.Parse(plannedHoursString);
            CurrentDirectory.Content.Add(new Todo(name, description, plannedHours));
            RenderDirectory();
        }

        public void unknownCommand(string command)
        {
            Console.WriteLine($"Unknown command: {command}");
        }

        public void incorrectParameterCount(int provided, int required)
        {
            Console.WriteLine($"Invalid amount of parameters: {provided} provided, {required} required");
        }

        public void help()
        {
            Console.WriteLine("Commands:");

            Console.WriteLine("\tadd <item>");
            Console.WriteLine("\tAdds a new item\n");

            Console.WriteLine("\tremove <item>");
            Console.WriteLine("\tRemoves the specified item\n");

            Console.WriteLine("\tedit <item> <row> <value>");
            Console.WriteLine("\tChange the value of a todo item\n");

            Console.WriteLine("\tworked <item> <hours>");
            Console.WriteLine("\tIncreases the worked hours\n");

            Console.WriteLine("\tcd <dirname>");
            Console.WriteLine("\tChange directory\n");

            Console.WriteLine("\tmkdir <dirname>");
            Console.WriteLine("\tCreates a new directory\n");

            Console.WriteLine("\texport <filename>");
            Console.WriteLine("\tExport the list of todo's\n");

            Console.WriteLine("\timport <filename>");
            Console.WriteLine("\tImport a list of todo's\n");

            Console.WriteLine("\tlicense");
            Console.WriteLine("\tAll the open source licenses used in this project\n");

            Console.WriteLine("\tclear");
            Console.WriteLine("\tClears everything off the screen\n");

            Console.WriteLine("\tquit");
            Console.WriteLine("\tQuits the application\n");
        }

        public void license()
        {
            Console.WriteLine("ConsoleTables - Khalid Abuhakmeh\n");
            Console.WriteLine("The MIT License (MIT)\n\nCopyright(c) 2012 Khalid Abuhakmeh\n\nPermission is hereby granted, free of charge, to any person obtaining a copy\nof this software and associated documentation files(the \"Software\"), to deal\nin the Software without restriction, including without limitation the rights\nto use, copy, modify, merge, publish, distribute, sublicense, and / or sell\ncopies of the Software, and to permit persons to whom the Software is\nfurnished to do so, subject to the following conditions:\n\n\tThe above copyright notice and this permission notice shall be included in all\n\tcopies or substantial portions of the Software.\n\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR\nIMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,\nFITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE\nAUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER\nLIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,\nOUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE\nSOFTWARE.");
        }

        public void clear()
        {
            Console.Clear();
        }

        public void quit()
        {
            Console.WriteLine("Bye!");
            Environment.Exit(0);
        }
        #endregion
    }
}
