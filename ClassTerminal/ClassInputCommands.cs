using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using ClassTerminal;
using System.Linq;

namespace ClassInputCommands
{
    public static class InputCommands
    {
        static List<string> lastCommands = new List<string>();

        public static List<string> InputComds(Terminal terminal)
        {

            StringBuilder sb = new StringBuilder();
            ConsoleKeyInfo inputKey;
            int curPos = 0, historyPos = 0;
            string bufferForInput = "";

            inputKey = Console.ReadKey();
            while (inputKey.Key != ConsoleKey.Enter)
            {
                switch (inputKey.Key)
                {
                    case ConsoleKey.RightArrow:
                        if (curPos > sb.Length)
                            break;
                        curPos++;
                        sb = new StringBuilder(sb.ToString() + " ");
                        Console.CursorLeft++;
                        break;
                    case ConsoleKey.LeftArrow:
                        if (curPos == 0)
                            break;
                        curPos--;
                        Console.CursorLeft--;
                        break;
                    case ConsoleKey.UpArrow:
                        if (historyPos == lastCommands.Count)
                            break;
                        if (historyPos == 0)
                        {
                            bufferForInput = sb.ToString();
                            sb = new StringBuilder(lastCommands[0]);
                        }
                        else
                        {
                            sb = new StringBuilder(lastCommands[historyPos]);
                        }
                        historyPos++;
                        UpdateLine(sb);
                        curPos = sb.Length;
                        break;
                    case ConsoleKey.DownArrow:
                        if (historyPos == 0)
                            break;
                        if (historyPos == 1)
                        {
                            sb = new StringBuilder(bufferForInput);
                        }
                        else
                        {
                            sb = new StringBuilder(lastCommands[historyPos - 2]);
                        }
                        historyPos--;
                        UpdateLine(sb);
                        curPos = sb.Length;
                        break;
                    case ConsoleKey.Backspace:
                        if (curPos == 0)
                            break;
                        sb.Remove(--curPos, 1);
                        UpdateLine(sb);
                        break;
                    case ConsoleKey.Delete:
                        if (curPos >= sb.Length)
                            break;
                        sb.Remove(curPos, 1);
                        UpdateLine(sb);
                        break;
                    case ConsoleKey.Tab:
                        sb = AutoCompleteMethod(sb, terminal);
                        UpdateLine(sb);
                        curPos = sb.Length;
                        Console.CursorLeft = curPos;
                        break;
                    default:
                        sb.Insert(curPos++, inputKey.KeyChar);
                        UpdateLine(sb);
                        break;
                }
                Console.CursorLeft = curPos;
                inputKey = Console.ReadKey();
            }
            lastCommands.Insert(0, sb.ToString());
            List<string> returnsList = new List<string>(CutQuotesInString(sb.ToString().TrimEnd()));
            returnsList = returnsList.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            return returnsList;
        }

        private static bool IsQuoteInString(string checkString)
        {
            for (int i = 0; i < checkString.Length; i++)
            {
                if (checkString[i] != ' ')
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Clears input line and puts new text there. 
        /// </summary>
        /// <param name="sb">StringBuilder with new text.</param>
        public static void UpdateLine(StringBuilder sb)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\r");
            Console.Write(new string(' ', Console.BufferWidth - 1));
            Console.Write("\r");
            Console.Write(sb.ToString());
            Console.ResetColor();
        }

        /// <summary>
        /// Finds all matches in files and directories that locate in current or child directory.
        /// </summary>
        /// <param name="inputLine">Line with commands.</param>
        /// <param name="terminal">Working  terminal.</param>
        /// <returns>List of all matches.</returns>
        public static List<String> SearchForMatches(List<String> inputLine, Terminal terminal)
        {
            List<string> matchedLines = new List<string>();
            if (inputLine.Count == 1)
            {
                matchedLines = GetMachedLines(inputLine[0], terminal.GetAvailableComands());
            }
            else if (inputLine.Count > 1 && IsPartOfPathChecker(inputLine[inputLine.Count - 1]))
            {
                Tuple<string, string> pathOfPath = GetPartOfPath(inputLine[inputLine.Count - 1]);
                matchedLines = GetMachedLines(pathOfPath.Item2, GetFileFromDirectory(terminal.GetCurrentDirectory() + pathOfPath.Item1));
            }
            else if (inputLine.Count > 1)
            {
                if (terminal.GetIsDirectory())
                {
                    matchedLines = GetMachedLines(inputLine[inputLine.Count - 1], terminal.GetDirectoriesInCurrentPosition());
                    matchedLines = matchedLines.Concat(GetMachedLines(inputLine[inputLine.Count - 1], terminal.GetFilesInCurrentPosition())).ToList();
                }
                else
                    matchedLines = GetMachedLines(inputLine[inputLine.Count - 1], terminal.GetDrives());
            }

            return matchedLines;
        }

        /// <summary>
        /// Complite input string to existing command or file. 
        /// </summary>
        /// <param name="sb">Input string.</param>
        /// <param name="terminal">Working terminal.</param>
        /// <returns>New string with completed input.</returns>
        public static StringBuilder AutoCompleteMethod(StringBuilder sb, Terminal terminal)
        {
            List<string> inputLine = CutQuotesInString(sb.ToString());
            bool hasQuotes = inputLine[inputLine.Count - 1].Contains(' ') || inputLine[inputLine.Count - 1].Contains('"');
            inputLine[inputLine.Count - 1] = inputLine[inputLine.Count - 1].Trim('"');
            List<string> matchedLines = SearchForMatches(inputLine, terminal);
            if (matchedLines.Count > 1)
            {
                Console.WriteLine();
                PrintElementsOfList(matchedLines);
                return sb;
            }

            if (matchedLines.Count == 1 && IsPartOfPathChecker(inputLine[inputLine.Count - 1]))
            {
                inputLine[inputLine.Count - 1] = GetPartOfPath(inputLine[inputLine.Count - 1]).Item1 + matchedLines[0];
                if (hasQuotes)
                    inputLine[inputLine.Count - 1] = '"' + inputLine[inputLine.Count - 1] + '"';
                sb = new StringBuilder(ConcatStrings(inputLine) + "\\");
                return sb;
            }
            if (matchedLines.Count == 1)
                inputLine[inputLine.Count - 1] = matchedLines[0];
            if (matchedLines.Count == 1 && !IsFileChecker(inputLine[inputLine.Count - 1]) && inputLine.Count > 1)
            {
                if (hasQuotes)
                    inputLine[inputLine.Count - 1] = '"' + inputLine[inputLine.Count - 1] + '"';
                sb = new StringBuilder(ConcatStrings(inputLine) + "\\");
                return sb;
            }
            if (matchedLines.Count == 1)
            {
                if (hasQuotes)
                    inputLine[inputLine.Count - 1] = '"' + inputLine[inputLine.Count - 1] + '"';
                sb = new StringBuilder(ConcatStrings(inputLine) + " ");
                return sb;
            }

            return sb;
        }

        /// <summary>
        /// Prints elements of list with strings in one row. 
        /// </summary>
        /// <param name="elements">List of strings.</param>
        public static void PrintElementsOfList(List<string> elements)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var element in elements)
            {
                Console.Write(element + " ");
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Returns list of mached lines. 
        /// </summary>
        /// <param name="line">Line to find.</param>
        /// <param name="linesForSearch">List of lines to search in.</param>
        /// <returns>List of mached lines.</returns>
        public static List<string> GetMachedLines(string line, List<string> linesForSearch)
        {
            int lenOfInputedLine = line.Length;
            List<string> matchedLines = new List<string>();
            foreach (var lineInList in linesForSearch)
            {
                if (lineInList.Length >= lenOfInputedLine && line == lineInList.Substring(0, lenOfInputedLine))
                {
                    matchedLines.Add(lineInList);
                }
            }

            return matchedLines;
        }

        /// </summary>
        /// <param name="inStr">String to split.</param>
        /// <returns>List of separated strings.</returns>
        public static List<string> CutQuotesInString(string inStr)
        {
            char[] charsInString = inStr.ToCharArray();
            bool quoteFlag = false;

            for (int i = 0; i < charsInString.Length - 1; i++)
            {
                if (charsInString[i] == '"')
                {
                    quoteFlag = !quoteFlag;
                }
                if (!quoteFlag && charsInString[i] == ' ' && charsInString[i + 1] != ' ')
                {
                    charsInString[i] = '|';
                }
            }

            List<string> returnsList = new List<string>((new string(charsInString)).Split('|'));
            for (int i = 0; i < returnsList.Count; i++)
            {
                // returnsList[i] = returnsList[i].Trim('"');
            }

            return returnsList;
        }

        /// <summary>
        /// Concats a banch of string at one.
        /// </summary>
        /// <param name="inStr">Massive of strings.</param>
        /// <returns>One string which consists of string form inStr.</returns>
        public static string ConcatStrings(List<string> inStr)
        {
            string output = "";

            foreach (string line in inStr)
            {
                output += line + " ";
            }

            return output.TrimEnd();
        }

        /// <summary>
        ///  Checks is input name file or directory.
        /// </summary>
        /// <returns>True if it is file, false otherwise.</returns>/
        private static bool IsFileChecker(string name)
        {
            return name.Contains('.');
        }

        /// <summary>
        ///  Checks if input name is a part of path. 
        /// </summary>
        /// <returns>True if it is a part of path, false otherwise.</returns>
        private static bool IsPartOfPathChecker(string name)
        {
            return name.Contains('\\');
        }

        /// <summary>
        /// Cut the end of unfinished path. 
        /// </summary>
        /// <param name="partOfPath">Part of path to cut.</param>
        /// <returns>Part of correct path.</returns>
        private static Tuple<string, string> GetPartOfPath(string partOfPath)
        {
            string[] pathInMas = partOfPath.Split('\\');
            string path = "";
            for (int i = 0; i < pathInMas.Length - 1; i++)
            {
                path += pathInMas[i] + "\\";
            }
            return new Tuple<string, string>(path, pathInMas[pathInMas.Length - 1]);
        }

        /// <summary>
        /// Gets all directory's elements. 
        /// </summary>
        /// <param name="path">Full  path to directory.</param>
        /// <returns>List with elements names.</returns>
        public static List<string> GetFileFromDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            List<string> elements = new List<string>();
            if (dir.Exists)
            {
                foreach (var element in dir.GetDirectories())
                {
                    elements.Add(element.Name);
                }
                foreach (var element in dir.GetFiles())
                {
                    elements.Add(element.Name);
                }
            }
            return elements;
        }
    }
}