using ConsoleTables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace Todo
{
    public class CustomConsole
    {
        Type thisType;
        Dir CurrentDirectory;

        public CustomConsole(Dir dir)
        {
            thisType = this.GetType();
            CurrentDirectory = dir;

            import("default");

            RenderDirectory();
            WaitForCommand();
        }

        public void WaitForCommand()
        {
            while (true)
            {
                Console.WriteLine();
                Console.Write("> ");
                string CommandString = Console.ReadLine();

                String[] Commands = Regex.Matches(CommandString, @"(?:\""(.+?)\"")|(\w+|\.\.)")
                                                    .OfType<Match>()
                                                    .Select(m => m.Groups[0].Value)
                                                    .ToArray();

                for (int i = 0; i < Commands.Length; i++)
                {
                    Commands[i] = Commands[i].Replace("\"", "");
                }

                if (Commands.Length == 0)
                {
                    Console.WriteLine("Unknown command: ");
                    continue;
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
            ConsoleTable table = new ConsoleTable("Name", "Description", "PlannedHours", "ActualHours", "Done");
            table.Options.EnableCount = false;

            Dir[] dirs = Array.ConvertAll(CurrentDirectory.Content.FindAll(item => item is Dir).ToArray(), item => (Dir)item);
            Todo[] todos = Array.ConvertAll(CurrentDirectory.Content.FindAll(item => item is Todo).ToArray(), item => (Todo)item);

            foreach (Dir dir in dirs)
            {
                table.AddRow(dir.Name, "", "", "", "");
            }
            foreach (Todo todo in todos)
            {
                table.AddRow(todo.Name, todo.Description, todo.PlannedHours, todo.ActualHours, todo.Done);
            }

            table.Write();
        }
        #endregion

        public Dir findRootDir()
        {
            Dir rootDir = CurrentDirectory;
            while (rootDir != rootDir.root)
            {
                rootDir = rootDir.root;
            }
            return rootDir;
        }

        #region Commands

        public void export(string fileName)
        {
            FileStream file;

            IFormatter formatter = new BinaryFormatter();

            var path = Environment.CurrentDirectory + $"\\{fileName}.todo";
            if (File.Exists(path))
            {
                file = new FileStream(path, FileMode.Open, FileAccess.Write);
            }
            else
            {
                file = System.IO.File.Create(path);
            }

            Console.WriteLine($"Exported {fileName}");
            formatter.Serialize(file, findRootDir());
            file.Close();
        }

        public void import(string fileName)
        {
            FileStream file;

            IFormatter formatter = new BinaryFormatter();
            var path = Environment.CurrentDirectory + $"\\{fileName}.todo";

            if (File.Exists(path))
            {
                file = new FileStream(path, FileMode.Open, FileAccess.Read);
                CurrentDirectory = (Dir)formatter.Deserialize(file);
                file.Close();
                Console.WriteLine($"Imported {fileName}");
            }
            else
            {
                Console.WriteLine($"File {fileName} couldn't be found");
            }
        }

        public void add(string name, string description, string plannedHoursString)
        {
            if (!CurrentDirectory.Content.Exists(item => item.Name == name))
            {
                decimal plannedHours = Decimal.Parse(plannedHoursString);
                CurrentDirectory.Content.Add(new Todo(name, description, plannedHours));
                RenderDirectory();
            }
            else
            {
                Console.WriteLine($"item {name} already exists");
            }
        }
        public void add(string command) => add(command, "", "0");

        public void remove(string itemName)
        {
            if (CurrentDirectory.Content.Exists(item => item.Name == itemName))
            {
                CurrentDirectory.Content.Remove(CurrentDirectory.Content.Find(item => item.Name == itemName));
                RenderDirectory();
            }
            else
            {
                Console.WriteLine($"item {itemName} couldn't be found");
            }
        }

        public void edit(string itemName, string rowName, string value)

        {
            IItem currentItem = CurrentDirectory.Content.Find(item => item.Name == itemName);
            if (currentItem is Todo)
            {
                bool flag = currentItem.GetType()
                    .GetProperties()
                    .ToList()
                    .Exists(item => item.Name == rowName);
                try
                {
                    if (flag)
                    {
                        PropertyInfo property = ((Todo)CurrentDirectory.Content.Find(item => item.Name == itemName)).GetType().GetProperty(rowName);
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(currentItem, convertedValue);
                    }
                    RenderDirectory();
                }
                catch
                {
                    Console.WriteLine("Invalid argument value");
                }
            }
            else
            {
                Console.WriteLine($"item {itemName} couldn't be found");
            }
        }

        public void prune()
        {
            CurrentDirectory.Content.FindAll(item => item is Todo).ForEach(delegate (IItem item)
            {
                if (((Todo)item).Done)
                {
                    CurrentDirectory.Content.Remove(item);
                }
            });
            RenderDirectory();
        }

        public void finish(string itemName)
        {
            Todo item = CurrentDirectory.Content.Find(item => item.Name == itemName) as Todo;
            if (item != null)
            {
                item.Done = true;
                RenderDirectory();
            }
            else
            {
                Console.WriteLine($"item {itemName} couldn't be found");
            }
        }

        public void worked(string itemName, string hoursWorked)
        {
            decimal decimalHoursWorked = Decimal.Parse(hoursWorked);

            if (CurrentDirectory.Content.Exists(todoItem => todoItem.Name == itemName) && CurrentDirectory.Content.Find(todoItem => todoItem.Name == itemName) is Todo)
            {
                ((Todo)CurrentDirectory.Content.Find(todoItem => todoItem.Name == itemName)).ActualHours += decimalHoursWorked;
                RenderDirectory();
            }
            else
            {
                Console.WriteLine($"item {itemName} couldn't be found");
            }



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

            Console.WriteLine("\tfinish <item>");
            Console.WriteLine("\tFinishes the specified item\n");

            Console.WriteLine("\tcd <dirname>");
            Console.WriteLine("\tChange directory\n");

            Console.WriteLine("\tmkdir <dirname>");
            Console.WriteLine("\tCreates a new directory\n");

            Console.WriteLine("\texport <filename>");
            Console.WriteLine("\tExport the list of todo's\n");

            Console.WriteLine("\timport <filename>");
            Console.WriteLine("\tImport a list of todo's\n");

            Console.WriteLine("\tprune");
            Console.WriteLine("\tDeletes all finished items from the current directory\n");

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
            RenderDirectory();
        }

        public void quit()
        {
            export("default");
            Console.WriteLine("Bye!");
            Environment.Exit(0);
        }

        #region Directory management
        public void mkdir(string dirName)
        {
            if (!CurrentDirectory.Content.Exists(item => item.Name == dirName))
            {
                Dir newDir = new Dir(dirName, CurrentDirectory, new List<IItem>());
                CurrentDirectory.Content.Add(newDir);
                RenderDirectory();
            }
            else
            {
                Console.WriteLine($"item {dirName} already exists");
            }
        }

        public void cd(string dirName)
        {
            if (dirName == "..")
            {
                CurrentDirectory = CurrentDirectory.root;
                RenderDirectory();
            }
            else if (CurrentDirectory.Content.Exists(item => item.Name == dirName && item is Dir))
            {
                CurrentDirectory = (Dir)CurrentDirectory.Content.Find(item => item.Name == dirName);
                RenderDirectory();
            }
            else
            {
                Console.WriteLine($"directory {dirName} couldn't be found");
            }
        }
        #endregion

        #region commandAliases
        public void work(string itemName, string hoursWorked) => worked(itemName, hoursWorked);
        public void exit() => quit();
        #endregion
        #endregion
    }
}
