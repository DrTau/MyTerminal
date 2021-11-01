﻿using System;
using System.IO;
using System.Text;

namespace ClassTerminal
{
    /// <summary>
    /// Class which provides acces to directories and works with them.
    /// </summary>/
    public class Terminal
    {
        DirectoryInfo currentDirectory = new DirectoryInfo(path: "C:\\");
        FileInfo bufferFile = null;
        bool isCut = false;

        /// <summary>
        /// Constructor for Terminal which sets starting directory. 
        /// </summary>
        /// <param name="path">starting directory for terminal.</param>
        public Terminal(string path)
        {
            currentDirectory = new DirectoryInfo(path: path);
        }

        /// <summary>
        /// Prints  elements of current directory.
        /// </summary>
        public void PrintElements()
        {
            // Prints directories
            foreach (var dirInfo in currentDirectory.GetDirectories())
            {
                try
                {
                    Console.Write("[" + dirInfo.Name + "]" + "\n");
                }
                catch (System.UnauthorizedAccessException)
                {
                    Console.Write("└───[Access error]\n");
                }

            }

            // Prints files
            foreach (var fileInfo in currentDirectory.GetFiles())
            {
                try
                {
                    Console.Write(fileInfo.Name + " (" + (fileInfo.Length / 8) + " B)" + "\n");
                }
                catch (IOException)
                {
                    Console.Write("{File access error}\n");
                }

            }
        }

        /// <summary>
        /// Changes current directory.
        /// </summary>
        /// <param name="dirName">Name of new directory.</param>
        public void ChangeDirectory(string dirName)
        {
            if (dirName == "..")
            {
                this.currentDirectory = this.currentDirectory.Parent;
                return;
            }
            foreach (var dirInfo in currentDirectory.GetDirectories())
            {
                if (dirInfo.Name == dirName)
                {
                    this.currentDirectory = new DirectoryInfo(path: dirInfo.FullName);
                    return;
                }
            }
            Console.WriteLine("There is no such directory");
        }

        /// <summary>
        /// Prints text file to console. Can use 3 different Encodings: UTF-8, ASCII and Unicode. 
        /// </summary>
        /// <param name="filename">Name of printing file.</param>
        /// <param name="encoding">Choosed encoding, default is UTF-8.</param>
        public void OpenTextFile(string filename, string encoding = "UTF-8")
        {
            Encoding encode;
            switch (encoding)
            {
                case "UTF-8":
                    encode = Encoding.UTF8;
                    break;
                case "ASCII":
                    encode = Encoding.ASCII;
                    break;
                case "Unicode":
                    encode = Encoding.Unicode;
                    break;
                default:
                    Console.WriteLine("Incorrect encoding");
                    return;
            }
            FileInfo fileInfo = new FileInfo(this.currentDirectory + "\\" + filename);
            if (fileInfo.Exists)
            {
                Console.WriteLine(File.ReadAllText(fileInfo.FullName), encode);
                return;
            }
            Console.WriteLine("Incorrect filename");
        }

        /// <summary>
        /// Add file to the file buffer. 
        /// </summary>
        /// <param name="filename">Name of coping file.</param>
        public void CopyFile(string filename)
        {
            FileInfo fileInfo = new FileInfo(this.currentDirectory + "\\" + filename);
            if (fileInfo.Exists)
            {
                this.bufferFile = fileInfo;
                Console.WriteLine("File copied");
                return;
            }
            Console.WriteLine("Incorrect filename");
        }

        /// <summary>
        /// Add file to the buffer. And turn cut flag on. 
        /// </summary>
        /// <param name="filename">Name of cutting file.</param>
        public void CutFile(string filename)
        {
            FileInfo fileInfo = new FileInfo(this.currentDirectory + "\\" + filename);
            if (fileInfo.Exists)
            {
                this.bufferFile = fileInfo;
                Console.WriteLine("File copied");
                this.isCut = true;
                return;
            }
            Console.WriteLine("Incorrect filename");
        }

        /// <summary>
        /// Paste file from buffert to current directory. 
        /// </summary>
        public void PasteFile()
        {
            if (this.bufferFile != null)
            {
                if (this.isCut)
                {
                    this.bufferFile.MoveTo(this.currentDirectory.FullName);
                    isCut = false;
                    this.bufferFile = null;
                }
                else
                {
                    this.bufferFile.CopyTo(this.currentDirectory.FullName + "\\" + this.bufferFile.Name, true);
                }
                Console.WriteLine("File pasted");
            }
            else
            {
                Console.WriteLine("No file in buffer");
            }
        }

        /// <summary>
        /// Delete file by filename. 
        /// </summary>
        /// <param name="filename">Name of the deliting file.</param>
        public void DeleteFile(string filename)
        {
            FileInfo fileInfo = new FileInfo(this.currentDirectory + "\\" + filename);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
                Console.WriteLine("File deleted");
                return;
            }
            Console.WriteLine("Incorrect filename");
        }

        /// <summary>
        /// Create text file and write some text there. User can choose one of the 3 encodings: UTF-8, ASCII, Unicode.
        /// </summary>
        /// <param name="filename">Name of creating file.</param>
        /// <param name="encoding">Choosed encoding.</param>
        public void CreateFile(string filename, string encoding = "UTF-8")
        {
            Encoding encode;
            switch (encoding)
            {
                case "UTF-8":
                    encode = Encoding.UTF8;
                    break;
                case "ASCII":
                    encode = Encoding.ASCII;
                    break;
                case "Unicode":
                    encode = Encoding.Unicode;
                    break;
                default:
                    Console.WriteLine("Incorrect encoding");
                    return;
            }
            Console.WriteLine("Enter your text:");
            string text = Console.ReadLine();
            using (StreamWriter sw = new StreamWriter(File.Open(filename, FileMode.Create), encode))
            {
                sw.Write(text);
            }
            File.Move(Directory.GetCurrentDirectory() + "\\" + filename, this.currentDirectory + "\\" + filename);
            Console.WriteLine("File successfully created.");

        }

        public void Concat(string firstFileName, string secondFileName)
        {
            this.OpenTextFile(firstFileName);
            this.OpenTextFile(secondFileName);
        }

    }
}