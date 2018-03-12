using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatchManager
{
    public class InstructionSet
    {
        public static string errorString = "";
        public static Boolean error = false; 
        public static void Explore(string path)
        {
            if (path != "")
            {
                Process.Start("explorer.exe", "/select, " + path);
            }
            else
            {
                error = true;
                errorString= "File not verified or not exist";
            }
        }


    }


}
