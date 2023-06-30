/*
 *  CSSALT FileManager for the MR-Suite.
 *  Kaizad Avari.
 */

using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class FileManager : MonoBehaviour
{

    /* -------------------------------------------------------------------------------------------------------------------------
     * Comparator class-within-a-class is used to compare the files by date
     * 
     */

    private class FileComparer : IComparer<FileInfo>
    {
        public int Compare(FileInfo x, FileInfo y)
        {
            return x.LastWriteTime.CompareTo(y.LastWriteTime);
        }
    }

    public static FileManager ME;

    /*
     * -------------------------------------------------------------------------------------------------------------------------
     */

    //FileManager starts here.

    private void Awake()
    {

        if (ME != null)
            GameObject.Destroy(ME);
        else
            ME = this;

        DontDestroyOnLoad(this);
    }

    public static List<FileInfo> GetFileList(string dir)
    {
        //Uses default ordering of items.
        DirectoryInfo di = new DirectoryInfo(dir);
        FileInfo[] txtFiles = di.GetFiles("*.txt", SearchOption.TopDirectoryOnly);
        FileInfo[] cssaltFiles = di.GetFiles("*.cssalt", SearchOption.TopDirectoryOnly);
        List<FileInfo> list = new List<FileInfo>(txtFiles);
        list.AddRange(cssaltFiles);
        return list;
    }

    public static List<FileInfo> GetFileListOrdered(string dir)
    {
        List<FileInfo> list = GetFileList(dir);
        list.Sort(new FileComparer());
        return list;
    }

    public static void WriteFile(string data, string path)
    {
        if (!File.Exists(path))
            File.CreateText(path).Close();
        File.WriteAllText(path, data);
    }

    public static string ReadFile(string path)
    {
        return File.ReadAllText(path);
    }

    public static bool RemoveRecord(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception) { return false; }
            return true;
        }
        else return false;
    }

    public void UpdateFileLine(string lineContentToSearchFor, string lineContentToWrite, string filePath)
    {
        //the line to replace with the updated time
        int lineToOverwrite = 1;

        //Copy in the current file
        string[] lines = File.ReadAllLines(filePath);

        try
        {

            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;

                while ((line = sr.ReadLine()) != null && !line.Contains(lineContentToSearchFor))
                {
                    lineToOverwrite++;
                }

            }

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                for (int currentLine = 1; currentLine <= lines.Length; ++currentLine)
                {
                    if (currentLine == lineToOverwrite)
                    {
                        sw.WriteLine(lineContentToWrite);
                    }
                    else
                    {
                        sw.WriteLine(lines[currentLine - 1]);
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Let the user know what went wrong.
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }
    }

    public void InsertLine(string lineToInsertBefore, string newLineToAdd, string filePath) //npcName = "item1"
    {
        var fileName = filePath;
        //var endTag = String.Format("[/{0}]", lineToInsertAfter);
        string endTag = lineToInsertBefore;
        var lineToAdd = newLineToAdd;

        List<string> txtLines = File.ReadAllLines(fileName).ToList();   //Fill a list with the lines from the txt file.
        txtLines.Insert(txtLines.IndexOf(endTag), lineToAdd);  //Insert the line you want to add last under the tag 'item1'.
        File.WriteAllLines(fileName, txtLines.ToArray());                //Add the lines including the new one.
    }

    /// <summary>
    /// Appends data to a file, or creates a new file if none exists.
    /// </summary>
    /// <param name="data">Data to append</param>
    /// <param name="path">Path to the file</param>
    public static void AppendLine(string data, string path)
    {
        File.AppendAllText(path, "\n" + data);
    }

    public static void ReplaceLine(string path, int line, string data)
    {
        string[] contents = File.ReadAllLines(path);
        if (line < contents.Length)
        {
            contents[line] = data;
            File.WriteAllLines(path, contents);
        }
        else
            AppendLine(data, path);
    }
}
