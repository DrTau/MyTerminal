﻿using System;
using ClassTerminal;
namespace MyTerminal
{
    class Program
    {
        static void Main(string[] args)
        {
            // string input = Console.ReadLine();
            Terminal terminal = new Terminal("C:\\Users\\drtta\\Documents\\infa");
            Console.WriteLine();
            terminal.Concat("test copy.txt", "test.txt");
        }
    }
}
