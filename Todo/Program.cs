using System;
using System.Collections.Generic;

namespace Todo
{
    class Program
    {
        static void Main(string[] args)
        {
            Dir rootDir = new Dir("..", new List<IItem>());
            CustomConsole console = new CustomConsole(rootDir);
        }
    }
}
