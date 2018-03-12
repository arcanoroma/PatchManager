using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Drawing;
using System.Security.Principal;

namespace PatchManager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        bool isKeyPressed = false;
        private void parse(int idPatch)
        {
            // verify is the process run in admin mode
            if (mypatch[idPatch].admin)
            {
                if (!IsRunAsAdmin())
                {
                    goadmin ga = new goadmin();
                    ga.title = mypatch[idPatch].title;
                    ga.ShowDialog();
                    if (ga.isAccept)
                    {
                        ProcessStartInfo proc = new ProcessStartInfo();
                        proc.UseShellExecute = true;
                        proc.WorkingDirectory = Environment.CurrentDirectory;
                        proc.FileName = Assembly.GetEntryAssembly().CodeBase;

                        proc.Verb = "runas";
                        Process.Start(proc);
                        Application.Exit();
                    }
                    else
                    {
                        writeReport("HALT - This script run only in Admin mode");
                        return;
                    }
                }
            }
            id_log log = mypatch[idPatch].log;
            if (log == id_log.Verbose)
                writeReport("Begin title:");
            string banner = "*** " + mypatch[idPatch].title + " ***";
            if (mypatch[idPatch].author.Length > mypatch[idPatch].title.Length)
                mypatch[idPatch].author = mypatch[idPatch].author.Substring(0, mypatch[idPatch].title.Length);
            writeReport(string.Concat(Enumerable.Repeat("*", banner.Length)));
            writeReport(banner);
            writeReport("***" + string.Concat(Enumerable.Repeat(" ", banner.Length - 10 - mypatch[idPatch].author.Length)) + "by " + mypatch[idPatch].author + " ***");
            writeReport(string.Concat(Enumerable.Repeat("*", banner.Length)));
            //String applicationArguments = mypatch[idPatch].arguments;
            //string sha256 = mypatch[idPatch].sha256;
            //long applicationLen = mypatch[idPatch].length;
            Process process = null;
            Pointer p = null;
            string path = "";
            FileStream fs = null;

            for (int i = 0; i < mypatch[idPatch].istruction.Count; i++)
            {
                if (log == id_log.Verbose)
                    writeReport("Step" + (i + 1).ToString());
                string[] my_istr = mypatch[idPatch].istruction[i].Split('"');
                switch (mypatch[idPatch].idIstruction[i])
                {
                    //                   pause = 1,
                    //openfile,
                    //closefile,
                    //writefile,
                    case id_instruction.openfile:
                        if (path != "")
                        {
                            if (log == id_log.Verbose)
                                writeReport("Opening file: " + path);
                            if (my_istr[1].ToLower() != "false")
                            {
                                string backfile = path + ".bak";
                                if (!File.Exists(backfile))
                                {
                                    if (log == id_log.Verbose)
                                        writeReport("Make copy backup: " + backfile);
                                    File.Copy(path, backfile);
                                    //    if (log == id_log.Verbose)
                                    //        writeReport("Delete old backup " + backfile);
                                    //    File.Delete(backfile);
                                }
                            }
                            fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
                        }
                        else
                        {
                            if (log == id_log.Verbose || log == id_log.onError)
                            {
                                writeReport("HALT - File not exist");
                                fs = null;
                            }
                            return;
                        }
                        break;
                    case id_instruction.writefile:
                        if (path != "" && fs != null)
                        {
                            VerifyAndWrite(fs, Convert.ToInt32(my_istr[1], 16), my_istr[3],  my_istr[5], log);
                        } else
                        {
                            if (log == id_log.Verbose || log == id_log.onError)
                            {
                                writeReport("HALT - File not exist or File not open");
                                path = "";
                                fs = null;
                            }

                        }
                        break;
                    case id_instruction.closefile:
                        if (log == id_log.Verbose)
                        {
                            writeReport("Close File");
                            fs = null;
                        }
                        break;
                    case id_instruction.pause:
                        string caption = "Press any key to contunue";
                        if (my_istr[1] != "")
                            caption = my_istr[1];
                        writeReport(caption);
                        isKeyPressed = false;
                        button1.Show();
                       while (!isKeyPressed)

                          Application.DoEvents();

                        button1.Hide();
                        break;
                    case id_instruction.explore:
                        InstructionSet.Explore("jj");

                        if (InstructionSet.error)
                            showError(log);
                        break;
                    case id_instruction.messagebox:
                        if (log == id_log.Verbose)
                            writeReport("Show MessageBox");
                        message_box mb = new message_box();
                        mb.title = mypatch[idPatch].title;
                        mb.textBox2.Text = my_istr[1].Replace("<NEWLINE>", Environment.NewLine );
                        mb.textBox1.Text = my_istr[3].Replace("<NEWLINE>", Environment.NewLine);
                        mb.ShowDialog();
                        break;
                    case id_instruction.regdeletevalue:
                        string keyRoot = my_istr[1];
                        string keyName = my_istr[3];
                        string keyValue = my_istr[5];
                        RegistryDeleteValue(keyRoot, keyName, keyValue, log);
                        break;
                    case id_instruction.verify:
                        path = Verify(my_istr[1], Convert.ToInt64( my_istr[3]), my_istr[5],log);
                        break;
                    case id_instruction.exec:
                        if (path != "")
                        {
                            process = loadProcess(path, my_istr[1], log);
                            if (process == null)
                            {
                                if (log == id_log.Verbose || log == id_log.onError)
                                    writeReport("HALT - handle process is invalid");
                                return;
                            }
                            p = new Pointer(process);
                        }
                        else
                        {
                            if (log == id_log.Verbose || log == id_log.onError)
                                writeReport("The file is not found or different");
                            return;
                        }

                        break;
                    case id_instruction.wait:
                        wait(Convert.ToInt32(my_istr[1]), log);
                        break;
                    case id_instruction.patch:
                        VerifyAndPatch(p, my_istr[1], my_istr[3], my_istr[5], log);
                        break;
                    case id_instruction.print:
                        writeReport("*** " + my_istr[1] + " ***");
                        break;
                    case id_instruction.exit:
                        if (log == id_log.Verbose)
                            writeReport("EXIT - Closing PatchManager");
                        Application.Exit();
                        break;
                }
            }
            if (log == id_log.Verbose)
                writeReport("End patching");
        }

        private void showError(id_log log)
        {
            if (log == id_log.Verbose || log == id_log.onError)
                writeReport(InstructionSet.errorString);
        }

        private void wait(int millisecond, id_log log)

        {
            if (log == id_log.Verbose)
                writeReport("Sleep " + ((float)millisecond / 1000).ToString() + " second(s)");
            Thread.Sleep(millisecond);
        }

        private Boolean RegistryDeleteValue(string keyRoot, string keyName, string keyValue, id_log log)
        {
            Boolean isDone = true;
            if (log == id_log.Verbose)
            {
                writeReport("Delete Registry Value");
                writeReport(" Root Registry Key: " + keyRoot);
                writeReport(" Registry Key: " + keyName);
                writeReport(" Registry Value: " + keyValue);
            }
            RegistryKey r = null;
            if (keyRoot == "CU")
                r = Registry.CurrentUser;
            if (keyRoot == "LM")
                r = Registry.LocalMachine;
            try
            {
                using (RegistryKey key = r.OpenSubKey(keyName, true))
                    if (key != null)
                    {
                        key.DeleteValue(keyValue);
                        if (log == id_log.Verbose)
                            writeReport("Done");
                    }
                    else
                    {
                        if (log == id_log.Verbose || log == id_log.onError)
                            writeReport("ALERT - Value not exist. Ignore command");
                    }
            }
            catch (Exception ex)
            {
                if (log == id_log.Verbose || log == id_log.onError)
                    writeReport("ERROR - " + ex.Message);
                isDone = false;
            }
            return isDone;
        }

        private String Verify(string applicationPath, long applicationLen, string sha256, id_log log)
        {
            if (log == id_log.Verbose)
            {
                writeReport("Verify file");
                writeReport(" Application Path: " + applicationPath);
                writeReport(" Length: " + applicationLen);
                writeReport(" SHA265: " + sha256);
            }
            // render 
            string[] sPath = applicationPath.Split('%');
            if (sPath.Length == 3)
            {
                string programFiles = Environment.ExpandEnvironmentVariables("%" + sPath[1] + "%");
                applicationPath = programFiles + sPath[2];
                if (log == id_log.Verbose)
                    writeReport(" Resolve path: " + applicationPath);
            }
            // verify path and Length       
            if (File.Exists(applicationPath))
            {
                if (log == id_log.Verbose)
                    writeReport(" FIle exist");
                FileInfo f = new FileInfo(applicationPath);
                if (f.Length == applicationLen)
                {
                    if (log == id_log.Verbose)
                        writeReport(" Length correct");
                    string hash = GetChecksum(applicationPath);
                    if (hash == sha256)
                    {
                        if (log == id_log.Verbose)
                        {
                            writeReport(" SHA256 correct");
                            writeReport(" File is matched");
                        }
                    }
                    else
                    {
                        if (log == id_log.Verbose || log == id_log.onError)
                        {
                            writeReport("ERROR - Wrong SHA256 hash: expected " + sha256 + " found " + hash);
                            applicationPath = "";
                        }
                    }

                }
                else
                {
                    if (log == id_log.Verbose || log == id_log.onError)
                    {
                        writeReport("ERROR - Wrong executable lenght: expected " + applicationLen.ToString() + " found " + f.Length);
                        applicationPath = "";
                    }
                }
            }
            else
            {
                if (log == id_log.Verbose || log == id_log.onError)
                {
                    writeReport("ERROR - Path not exist");
                    applicationPath = "";
                }
            }

            return applicationPath;
        }

        private Process loadProcess(string applicationPath, string applicationArguments, id_log log)
        {
            if (log == id_log.Verbose)
            {
                writeReport("Load Process");
                writeReport(" Application Path: " + applicationPath);
                if (applicationArguments != "")
                    writeReport(" Arguments: " + applicationArguments);
            }
            Process p = null;
            // render 
 
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = applicationPath;
                        startInfo.Arguments = applicationArguments;
                        p = Process.Start(startInfo);
                        if (log == id_log.Verbose)
                        {
                            writeReport(" PID: " + p.Id.ToString() + " (" + toHex(p.Id) + ")");
                            writeReport(" Process name: " + p.ProcessName);
                            writeReport(" Main window title: " + p.MainWindowTitle);
                        }
            return p;
        }

        private static Process waitForProcess(String processName)
        {
            int index1 = -1;
            Boolean isFound = false;
            Process[] processes = null;
            while (!isFound)
            {
                processes = Process.GetProcesses();
                for (int index3 = 0; index3 < processes.Length; ++index3)
                {
                    if (processes[index3].ProcessName.ToLower() == processName)
                    {
                        index1 = index3;
                        isFound = true;
                        break;
                    }
                }
                Application.DoEvents();
                Thread.Sleep(100);
            }
            return processes[index1];
        }

        List<String> iset = new List<String>();
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = versione + " - ";
            if (IsRunAsAdmin())
                this.Text += "Admin Mode";
            else
                this.Text += "User Mode";
            EnumPatch();
            // populate istruction set
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream template = assembly.GetManifestResourceStream("PatchManager.Resources.help.txt");
            StreamReader sr = new StreamReader(template);
            string spiegazione = "";
            int nrist = 0;
            toolStripComboBox3.Items.Add("Structure naked");
            toolStripComboBox3.Items.Add("Verify command");
                while (!sr.EndOfStream)
            {
                string buffer = sr.ReadLine();
                if (buffer.Length > 0)
                {
                    if (buffer.Substring(0, 1) == "*")
                    {
                        if (nrist > 0)
                        {
                            iset.Add(spiegazione);
                        }
                        spiegazione = "";
                        toolStripComboBox2.Items.Add(buffer.Substring(1));
                        nrist++;
                    }
                    else
                    {
                        spiegazione += buffer + "\r\n";
                    }
                }

            }
            if (nrist > 0)
            {
                iset.Add(spiegazione);
            }
            sr.Close();
            template.Close();


        }

        string versione = "PatchManager v.0.1b";

        //private bool VerifyAndPatch(Pointer p, string offset, string byteOrig, string bytePatch, id_log log)
        //{
        //    // indirizzamento offset CheatEngine style  "\"mz004\"+4BD2B"
        //    bool flag = false;
        //    IntPtr address1 = p.GetAddress(offset);
        //    int size = (byteOrig.Length + 1) / 3;
        //    byte[] first = new byte[size];
        //    byte[] second = patch.string2Byte(byteOrig);
        //    byte[] buffer1 = patch.string2Byte(bytePatch);
        //    Pointer pointer = p;
        //    IntPtr address2 = address1;
        //    byte[] buffer2 = first;
        //    int length = buffer2.Length;
        //    pointer.ReadMemory(address2, buffer2, length);
        //    if (patch.compareArray(first, second) && patch.compareArray(first, second))
        //    {
        //        p.WriteMemory(address1, buffer1, size);
        //        flag = true;
        //    }
        //    return flag;
        //}

        private bool VerifyAndWrite(FileStream fs, int offset, string byteOrig, string bytePatch, id_log log)
        {
            if (log == id_log.Verbose)
                writeReport("Check match byte");
            bool flag = false;
            int size = (byteOrig.Length + 1) / 3;
            byte[] first = new byte[size];
            byte[] second = patch.string2Byte(byteOrig);
            byte[] buffer1 = patch.string2Byte(bytePatch);
            byte[] buffer2 = first;
            int length = buffer2.Length;
            fs.Position = offset;
            fs.Read(buffer2, 0, length);
            if (patch.compareArray(first, second) && patch.compareArray(first, second))
            {
                fs.Position = offset;
                fs.Write(buffer1,0, size);
                flag = true;
                for (int i = 0; i < first.Length; i++)
                {
                        if (log == id_log.Verbose)
                            writeReport(" " + toHex((int)offset + i) + " = " + toHex(first[i]) + " -> " + toHex(buffer1[i]));
                }
            }
            else
            {
                for (int i = 0; i < first.Length; i++)
                {
                        if (log == id_log.Verbose || log == id_log.onError)
                            writeReport(" " + toHex(offset + i) + " expected: " + toHex(second[i]) + " found: " + toHex(first[i]));
                }
                if (log == id_log.Verbose || log == id_log.onError)
                    writeReport("Alert - Byte mismatch - Ignore command");

            }
            return flag;
        }

        private bool VerifyAndPatch(Pointer p, string offset, string byteOrig, string bytePatch, id_log log)
        {
            // indirizzamento offset assoluto 0x834234
            if (log == id_log.Verbose)
                writeReport("Patching Memory");
            bool flag = false;
            int size = (byteOrig.Length + 1) / 3;
            byte[] first = new byte[size];
            byte[] second = patch.string2Byte(byteOrig);
            byte[] buffer1 = patch.string2Byte(bytePatch);
            Pointer pointer = p;
            byte[] buffer2 = first;
            int length = buffer2.Length;
            IntPtr iptr = (IntPtr)Convert.ToInt32(offset, 16);

            pointer.ReadMemory(iptr, buffer2, length);
            if (patch.compareArray(first, second) && patch.compareArray(first, second))
            {
                p.WriteMemory(iptr, buffer1, size);
                flag = true;
                for (int i = 0; i < first.Length; i++)
                {
                    unsafe
                    {
                        if (log == id_log.Verbose)
                            writeReport(" " + toHex((int)iptr.ToPointer() + i) + " = " + toHex(first[i]) + " -> " + toHex(buffer1[i]));
                    }
                }
            }
            else
            {
                for (int i = 0; i < first.Length; i++)
                {
                    unsafe
                    {
                        if (log == id_log.Verbose || log == id_log.onError)
                            writeReport(" " + toHex((int)iptr.ToPointer() + i) + " expected: " + toHex(second[i]) + " found: " + toHex(first[i]));
                    }
                    if (log == id_log.Verbose || log == id_log.onError)
                        writeReport("Alert - Byte mismatch - Ignore command");
                }

            }
            return flag;
        }

        private string toHex(byte value)
        {
            return "0x" + value.ToString("x").PadLeft(2, '0');
        }

        private string toHex(long value)
        {
            return "0x" + value.ToString("x").PadLeft(8, '0');
        }


        //private bool VerifyAndPatch(Pointer p, string offset, string byteOrig)
        //{
        //    bool flag = false;

        //    IntPtr address1 = p.GetAddress(offset);
        //    int size = (byteOrig.Length + 1) / 3;
        //    byte[] first = new byte[size];
        //    byte[] second = patch.string2Byte(byteOrig);
        //    string str = "";
        //    for (int index = 0; index < size; ++index)
        //        str += "90 ";
        //    byte[] buffer1 = patch.string2Byte(str.Substring(0, str.Length - 1));
        //    Pointer pointer = p;
        //    IntPtr address2 = address1;
        //    byte[] buffer2 = first;
        //    int length = buffer2.Length;
        //    pointer.ReadMemory(address2, buffer2, length);
        //    pointer.ReadMemory((IntPtr) 0x820d2b, buffer2, length);
        //    if (patch.compareArray(first, second) && patch.compareArray(first, second))
        //    {
        //        p.WriteMemory(address1, buffer1, size);
        //        flag = true;
        //    }
        //    return flag;
        //}

        private void timer1_Tick(object sender, EventArgs e)
        {
            //    this.PRIMO();
        }

        string ListPatchPath = "patch_list.txt";

        class applicationPatch
        {
            public string title = "";
            public string author = "";
            public Image image = null;
            public Boolean admin = false;
            public id_log log = id_log.Verbose;
            public List<String> istruction = new List<string>();
            public List<id_instruction> idIstruction;
        }

        class istruction
        {
            public string command = "";
            public int lenSplit;
            public id_instruction idIstruction;
        }


        List<applicationPatch> mypatch = new List<applicationPatch>();

        enum id_instruction
        {
            pause=1,
            print,
            regdeletevalue,
            exec,
            wait,
            exit,
            explore,
            verify,
            patch,
            openfile,
            closefile,
            writefile,
            messagebox,
        }

        enum id_log
        {
            Verbose,
            onError,
            None
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            EnumPatch();
        }

        private void EnumPatch()
        {
            mypatch = new List<applicationPatch>();
            List<istruction> myistruction = new List<istruction>();
            // statement
            myistruction.Add(new istruction { command = "title", lenSplit = 3 });
            myistruction.Add(new istruction { command = "author", lenSplit = 3});
            myistruction.Add(new istruction { command = "log", lenSplit = 3 });
            myistruction.Add(new istruction { command = "icon", lenSplit = 3 });
            myistruction.Add(new istruction { command = "goadmin", lenSplit = 1 });
            myistruction.Add(new istruction { command = "endtitle", lenSplit = 1 });
            // istruction
            myistruction.Add(new istruction { command = "patch", lenSplit = 7,  idIstruction = id_instruction.patch });
            myistruction.Add(new istruction { command = "pause", lenSplit = 3, idIstruction = id_instruction.pause });
            myistruction.Add(new istruction { command = "print", lenSplit = 3,  idIstruction = id_instruction.print });
            myistruction.Add(new istruction { command = "regdeletevalue", lenSplit = 7, idIstruction = id_instruction.regdeletevalue });
            myistruction.Add(new istruction { command = "exec", lenSplit = 3,  idIstruction = id_instruction.exec });
            myistruction.Add(new istruction { command = "wait", lenSplit = 3,  idIstruction = id_instruction.wait });
            myistruction.Add(new istruction { command = "exit", lenSplit = 1,  idIstruction = id_instruction.exit });
            myistruction.Add(new istruction { command = "explore", lenSplit = 1, idIstruction = id_instruction.explore });
            myistruction.Add(new istruction { command = "verify", lenSplit = 7,  idIstruction = id_instruction.verify });
            myistruction.Add(new istruction { command = "openfile", lenSplit = 3,  idIstruction = id_instruction.openfile });
            myistruction.Add(new istruction { command = "closefile", lenSplit = 1,  idIstruction = id_instruction.closefile });
            myistruction.Add(new istruction { command = "writefile", lenSplit = 7,  idIstruction = id_instruction.writefile });
            myistruction.Add(new istruction { command = "messagebox", lenSplit = 5,  idIstruction = id_instruction.messagebox});

            textBox1.Text = "";
            string path = Application.StartupPath + "\\" + ListPatchPath;

            if (File.Exists(path))
            {
                StreamReader sr = new StreamReader(path);
                writeReport("Script patch found");
                applicationPatch ap = new applicationPatch();
                while (!sr.EndOfStream)
                {
                    String buffer = sr.ReadLine().Trim();
                    // ignora le rem

                    if (buffer.Length > 0)
                    {
                        if (buffer.Substring(0, 1) != "#")
                        {
                            for (int i = 0; i < myistruction.Count; i++)
                            {
                                if (buffer.Length >= myistruction[i].command.Length)
                                {
                                    if (buffer.Substring(0, myistruction[i].command.Length).ToLower() == myistruction[i].command)
                                    {
                                        string[] myLine = buffer.Split('"');
                                        if (myLine.Length == myistruction[i].lenSplit)
                                        {
                                            switch (myistruction[i].command)
                                            {
                                                case "title":
                                                    ap = new applicationPatch();
                                                    ap.istruction = new List<string>();
                                                    ap.idIstruction = new List<id_instruction>();
                                                    ap.title = myLine[1];
                                                    writeReport("Add title: " + ap.title);
                                                    break;
                                                case "author":
                                                    ap.author = myLine[1];
                                                    break;
                                                case "goadmin":
                                                    ap.admin = true;
                                                    break;
                                                case "icon":
                                                 string hex = myLine[1];
                                                    byte[] b = Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
                                                    var ms = new MemoryStream(b);
                                                    ap.image = Image.FromStream(ms);
                                                    break;
                                                case "log":
                                                    switch (myLine[1].ToLower())
                                                    {
                                                        case "verbose":
                                                            ap.log = id_log.Verbose;
                                                            break;
                                                        case "onerror":
                                                            ap.log = id_log.onError;
                                                            break;
                                                        case "none":
                                                            ap.log = id_log.None;
                                                            break;
                                                    }
                                                    break;
                                                case "endtitle":
                                                    mypatch.Add(ap);
                                                    break;
                                                default:
                                                    ap.istruction.Add(buffer);
                                                    ap.idIstruction.Add(myistruction[i].idIstruction);
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            writeReport("ERROR - Parse command " + myistruction[i].command.ToUpper() + "\r\n -> " + buffer);
                                        }
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
                sr.Close();
                toolStripComboBox1.Items.Clear();
                for (int i = 0; i < mypatch.Count; i++)
                {
                    toolStripComboBox1.Items.Add(mypatch[i].title);
                }
            }
            else
            {
                writeReport("ERROR - Script patch missing\r\n ->" + path);
            }
        }

        private bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void writeReport(string info)
        {
            textBox1.AppendText(info + "\r\n");
            Application.DoEvents();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (toolStripComboBox1.SelectedIndex != -1)
            {
                textBox1.Text = "";
                parse(toolStripComboBox1.SelectedIndex);
            }
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {

        }
        private string GetChecksum(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }
        private void panel1_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBox2.SelectedIndex > -1)
            {
                textBox3.Text = iset[toolStripComboBox2.SelectedIndex];
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBox1.SelectedIndex > -1)
            {
                if (mypatch[toolStripComboBox1.SelectedIndex].image != null)
                    toolStripLabel1.Image = mypatch[toolStripComboBox1.SelectedIndex].image;

            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
        //    isKeyPressed = true;
        }

        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {
            if (toolStripComboBox3.SelectedIndex > -1)
            {
                string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (FileList.Length == 1)
                {
                    textBox2.Text = "";
                    string filename = FileList[0];
                    string buffer = "";
                    if (toolStripComboBox3.SelectedIndex == 0)
                    {
                        Assembly assembly = Assembly.GetExecutingAssembly();
                        Stream template = assembly.GetManifestResourceStream("PatchManager.Resources.template.txt");
                        StreamReader sr = new StreamReader(template);

                        buffer = sr.ReadToEnd();
                        sr.Close();
                        template.Close();
                        // get icon
                        Icon ico = Icon.ExtractAssociatedIcon(filename);
                        Bitmap b = ico.ToBitmap();
                        Bitmap resized = new Bitmap(b, new Size(15, 16));
                        // convert in byte 
                        ImageConverter converter = new ImageConverter();
                        byte[] bmpByte = (byte[])converter.ConvertTo(resized, typeof(byte[]));
                        string byteS = bmpByte.Aggregate(new System.Text.StringBuilder(), (sb, v) => sb.Append(v.ToString("X2"))).ToString();
                        buffer = buffer.Replace("%%ICON%%", byteS);
                    }
                    if (toolStripComboBox3.SelectedIndex ==1)
                    {

                        buffer = "Verify = \"%%PATH%%\", \"%%LEN%%\", \"%%SHA256%%\"";
                        FileInfo f = new FileInfo(filename);
                        buffer = buffer.Replace("%%LEN%%", f.Length.ToString());
                        buffer = buffer.Replace("%%SHA256%%", GetChecksum(filename));

                        string[] env = new string[] { "ALLUSERSPROFILE", "APPDATA", "CommonProgramFiles", "CommonProgramFiles(x86)", "CommonProgramW6432", "HOMEPATH", "ProgramData", "ProgramFiles", "ProgramFiles(x86)", "ProgramW6432", "PUBLIC", "SystemRoot", "TEMP", "TMP", "USERPROFILE" };

                        for (int i = 0; i < env.Length; i++)
                        {
                            string value = Environment.ExpandEnvironmentVariables("%" + env[i] + "%");
                            if (filename.Length >= value.Length)
                            {
                                if (filename.Substring(0, value.Length).ToLower() == value.ToLower())
                                {
                                    filename = "%" + env[i] + "%" + filename.Substring(value.Length);
                                    break;
                                }
                            }
                        }
                        buffer = buffer.Replace("%%PATH%%", filename);
                    }
                    textBox2.Text = buffer;
                }
                textBox2.AllowDrop = false;
                textBox2.BackColor = Color.White;
            }
        }

        private void textBox2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (FileList.Length == 1)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                    e.Effect = DragDropEffects.None;
            }
        }

        private void toolStripComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBox3.SelectedIndex > -1)
            {
                textBox2.Text = "Drag the file to patch here";
                textBox2.AllowDrop = true;
                textBox2.BackColor = Color.Yellow;
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            toolStripComboBox3.SelectedIndex = -1;
            textBox2.AllowDrop = false;
            textBox2.BackColor = Color.White;
            textBox2.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            isKeyPressed = true;
        }
    }
}
