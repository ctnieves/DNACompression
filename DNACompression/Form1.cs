﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
-._    _.--'"`'--._    _.--'"`'--._    _.--'"`'--._    _   DNA Compressor. . 
    '-:`.'|`|"':-.  '-:`.'|`|"':-.  '-:`.'|`|"':-.  '.` : '.   By Christian Nieves
  '.  '.  | |  | |'.  '.  | |  | |'.  '.  | |  | |'.  '.:   '.  '. http://www.ctnieves.com
  : '.  '.| |  | |  '.  '.| |  | |  '.  '.| |  | |  '.  '.  : '.  `.
  '   '.  `.:_ | :_.' '.  `.:_ | :_.' '.  `.:_ | :_.' '.  `.'   `.
         `-..,..-'       `-..,..-'       `-..,..-'       `         `
*/

namespace DNACompression
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        TextWriter _writer = null;

        private void Form1_Load(object sender, EventArgs e)
        {
            formatBox.SelectedIndex = 0;
            _writer = new TextBoxStreamWriter(outputBox);

            Console.SetOut(_writer);
            Console.WriteLine("\nDNA Compressor ready. ");
        }

        Dictionary<string, string> codonsList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            #region CODONS
            //{@"(\r[^\n])|([^\r]\n)", "65"},
            //{"\n", "66"},
            //{"\r", "65"},
            {"CCC", "00010101"},
            {"CCG", "00010110"},
            {"CCT", "00010111"},
            {"CCA", "00010100"},
            {"CGC", "00011001"},
            {"CGG", "00011010"},
            {"CGT", "00011011"},
            {"CGA", "00011000"},
            {"CTC", "00011101"},
            {"CTG", "00011110"},
            {"CTT", "00011111"},
            {"CTA", "00011100"},
            {"CAC", "00010001"},
            {"CAG", "00010010"},
            {"CAT", "00010011"},
            {"CAA", "00010000"},
            {"GCC", "00100101"},
            {"GCG", "00100110"},
            {"GCT", "00100111"},
            {"GCA", "00100100"},
            {"GGC", "00101001"},
            {"GGG", "00101010"},
            {"GGT", "00101011"},
            {"GGA", "00101000"},
            {"GTC", "00101101"},
            {"GTG", "00101110"},
            {"GTT", "00101111"},
            {"GTA", "00101100"},
            {"GAC", "00100001"},
            {"GAG", "00100010"},
            {"GAT", "00100011"},
            {"GAA", "00100000"},
            {"TCC", "00110101"},
            {"TCG", "00110110"},
            {"TCT", "00110111"},
            {"TCA", "00110100"},
            {"TGC", "00111001"},
            {"TGG", "00111010"},
            {"TGT", "00111011"},
            {"TGA", "00111000"},
            {"TTC", "00111101"},
            {"TTG", "00111110"},
            {"TTT", "00111111"},
            {"TTA", "00111100"},
            {"TAC", "00110001"},
            {"TAG", "00110010"},
            {"TAT", "00110011"},
            {"TAA", "00110000"},
            {"ACC", "00000101"},
            {"ACG", "00000110"},
            {"ACT", "00000111"},
            {"ACA", "00000100"},
            {"AGC", "00001001"},
            {"AGG", "00001010"},
            {"AGT", "00001011"},
            {"AGA", "00001000"},
            {"ATC", "00001101"},
            {"ATG", "00001110"},
            {"ATT", "00001111"},
            {"ATA", "00001100"},
            {"AAC", "00000001"},
            {"AAG", "00000010"},
            {"AAT", "00000011"},
            {"AAA", "00000000"},

                #endregion
        };

        Dictionary<string, string> codonList2 = new Dictionary<string, string>()
        {
            {"G\r\n", "10011010"},
            {"C\r\n", "10011001"},
            {"T\r\n", "10011000"},
            {"A\r\n", "10010111"},
        };

        public class TextBoxStreamWriter : TextWriter
        {
            TextBox _output = null;

            public TextBoxStreamWriter(TextBox output)
            {
                _output = output;
            }

            public override void Write(char value)
            {
                base.Write(value);
                _output.AppendText(value.ToString()); 
            }

            public override Encoding Encoding
            {
                get { return System.Text.Encoding.UTF8; }
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if(formatBox.SelectedIndex != 0)
            {
                Console.WriteLine("This format is still a work in progress. Unable to compress your file. ");
            }
            else
            {
                FileInfo file = new FileInfo(filenameBox.Text);
                if (file.Exists)
                {
                    startCompression();
                }
                else
                    Console.WriteLine("This file does not exist.");
            }
        }

        private void decompressButton_Click(object sender, EventArgs e)
        {
            if (formatBox.SelectedIndex != 0)
            {
                Console.WriteLine("This format is still a work in progress.");
            }
            else
            {
                FileInfo file = new FileInfo(filenameBox.Text);
                if (file.Exists)
                {
                    if (file.Extension == ".cDNA")
                        startDecompression();
                    else
                    {
                        Console.WriteLine("You must use a *.cDNA file for decompression, this is a " + file.Extension + " file");
                    }
                }
                else
                    Console.WriteLine("This file does not exist.");
            }
        }
        string dnaASCII = "`-:-.   ,-;\"`-:-.   ,-;\"`-:-.   ,-;\"`-:-.   ,-;\"  DNA Compressor\r\n   `=`,\'=/     `=`,\'=/     `=`,\'=/     `=`,\'=/ By Christian Nieves\r\n     y==/        y==/        y==/        y==/\r\n   ,=,-<=`.    ,=,-<=`.    ,=,-<=`.    ,=,-<=`.\r\n,-\'-\'   `-=_,-\'-\'   `-=_,-\'-\'   `-=_,-\'-\'   `-=_\n\n";
        public void startCompression()
        {
            outputBox.Text = dnaASCII;
            outputBox.Update();
            Console.WriteLine("\nOPERATION IN PROGRESS");
            filenameBox.Enabled = false;
            openButton.Enabled = false;
            startButton.Enabled = false;
            decompressButton.Enabled = false;
            cWorker.RunWorkerAsync();
        }

        public void startDecompression()
        {
            outputBox.Text = dnaASCII;
            outputBox.Update();
            Console.WriteLine("\nOPERATION IN PROGRESS");
            filenameBox.Enabled = false;
            openButton.Enabled = false;
            startButton.Enabled = false;
            decompressButton.Enabled = false;
            dWorker.RunWorkerAsync(); ;
        }

        string writeC = "";
        private void cWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            double baseCount = 0;
            var sw = Stopwatch.StartNew();
            string dir = filenameBox.Text;
            string dirO = Directory.GetParent(dir) + "\\" + Path.GetFileNameWithoutExtension(dir) + ".tDNA";
            string dirC = Directory.GetParent(dir) + "\\" + Path.GetFileNameWithoutExtension(dir) + ".cDNA"
            
            #region Byte Writer
            using (StreamReader sr = new StreamReader(dir))
            {
                char[] c = null;
                using (BinaryWriter b = new BinaryWriter(File.Open(dirO, FileMode.Create), Encoding.Unicode))
                {
                    while (sr.Peek() >= 0)
                    {
                        c = new char[1023];
                        sr.Read(c, 0, c.Length);
                        var regex = new Regex(String.Join("|", codonsList.Keys), RegexOptions.IgnoreCase);
                        var regex2 = new Regex(String.Join("|", codonList2.Keys), RegexOptions.IgnoreCase);
                        string cString = new string(c);

                        cString = cString.Replace("\0", "");
                        var q = regex.Replace(cString, m => codonsList[m.Value]);
                        baseCount += (q.Length / 8) * 3;
                        q = regex2.Replace(q, m => codonList2[m.Value]);

                        int numOfBytes = q.Length / 8;
                        byte[] bytes = new byte[numOfBytes];
                        for (int z = 0; z < numOfBytes; ++z)
                        {
                            bytes[z] = Convert.ToByte(q.Substring(8 * z, 8), 2);
                        }

                        foreach(byte bitSet in bytes)
                        {
                            b.Write(bitSet);
                        }
                        float progress = (float)sr.BaseStream.Position / (float)sr.BaseStream.Length;
                        cWorker.ReportProgress(Convert.ToInt32(progress * 100));
                    }
                }
            }
            #endregion
            Zip(dirO, dirC);
            File.Delete(dirO);
            FileInfo i = new FileInfo(dir);
            FileInfo o = new FileInfo(dirC);
            double seconds = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds).TotalSeconds;
            writeC = "------------------------------------------------------------------\n" +
                "Compressed " + string.Format("{0:#,###0}", baseCount) + " bases in " + string.Format("{0:#,###0}", seconds.ToString()) + " seconds\n" +
                "Original File(" + Math.Round((i.Length * 0.000976562), 1) + "kb) successfully compressed to " + Math.Round((o.Length * 0.000976562), 1) + "kb\n" +
                "Compression Ratio(bits per base) : " + (((float)o.Length * 8) / baseCount) + "\n" +
                "------------------------------------------------------------------\n" +
                "OPERATION COMPLETED";
        }

        private void dWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var nCodons = codonsList.ToDictionary(x => x.Value, x => x.Key);
            var nCodons2 = codonList2.ToDictionary(x => x.Value, x => x.Key);
            Dictionary<string, string> codons = new Dictionary<string, string>();

            var sw = Stopwatch.StartNew();
            string dir = filenameBox.Text;
            string dirT = Directory.GetParent(dir) + "\\" + Path.GetFileNameWithoutExtension(dir) + ".tDNA";
            string dirO = Directory.GetParent(dir) + "\\" + Path.GetFileNameWithoutExtension(dir) + ".oDNA";
            UnZip(dir, dirT);
            List<byte> bytes = new List<byte>();
            #region binary reader
            using (BinaryReader br = new BinaryReader(File.Open(dirT, FileMode.Open), Encoding.UTF8))
            {
                //char[] c = null;
                int pos = 0;
                int length = (int)br.BaseStream.Length;
                StreamWriter tw = new StreamWriter(dirO);
                while (pos < length)
                {
                    float progress = (float)br.BaseStream.Position / (float)br.BaseStream.Length;
                    dWorker.ReportProgress(Convert.ToInt32(progress * 100));
                    byte b = br.ReadByte();
                    var regex = new Regex(String.Join("|", nCodons.Keys));
                    var regex2 = new Regex(String.Join("|", nCodons2.Keys));
                    string c = Convert.ToString(b, 2).PadLeft(8, '0');
                    var q = regex.Replace(c, m => nCodons[m.Value]);
                        q = regex2.Replace(q, m => nCodons2[m.Value]);
                    tw.Write(q);
                     
                    pos += sizeof(byte);
                }
                tw.Dispose();
            }
            #endregion
            
            File.Delete(dirT);
            time = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds).TotalSeconds;
        }

        public string convert(int x)
        {
            char[] bits = new char[32];
            int i = 0;

            while (x != 0)
            {
                bits[i++] = (x & 1) == 1 ? '1' : '0';
                x >>= 1;
            }

            Array.Reverse(bits, 0, i);
            return new string(bits);
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                filenameBox.Text = dlg.FileName;
            }
        }

        public bool Zip(string file, string outputFile)
        {
            try
            {
                FileInfo fileToCompress = new FileInfo(file);
                if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden)
                != FileAttributes.Hidden & fileToCompress.Extension != ".cmp")
                {
                    using (var inFile = File.OpenRead(file))
                    {
                        using (var outFile = File.Create(outputFile))
                        {
                            using (var compress = new GZipStream(outFile, CompressionMode.Compress, false))
                            {
                                byte[] buffer = new byte[inFile.Length];

                                int read = inFile.Read(buffer, 0, buffer.Length);
                                while (read > 0)
                                {
                                    compress.Write(buffer, 0, read);
                                    read = inFile.Read(buffer, 0, buffer.Length);
                                }
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(string.Format("Error compressing file: {0}", ex.Message));
                return false;
            }
        }

        public bool UnZip(string source, string destFile)
        {
            byte[] buffer = new byte[2048];

            try
            {
                using (var inStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var outStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (var zipStream = new GZipStream(inStream, CompressionMode.Decompress, true))
                        {
                            while (true)
                            {
                                int count = zipStream.Read(buffer, 0, buffer.Length);

                                if (count != 0)
                                    outStream.Write(buffer, 0, count);

                                if (count != buffer.Length)
                                    break;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error decompressing file : " + ex.Message);
                return false;
            }
        }

        private void cWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 0;
            percentLabel.Text = "";
            string[] sepbyLine = writeC.Split('\n');
            foreach (string newLine in sepbyLine)
            {
                Console.WriteLine(newLine);
            }
            filenameBox.Enabled = true;
            openButton.Enabled = true;
            startButton.Enabled = true;
            decompressButton.Enabled = true;
        }
        double time = 0;
        private void dWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 0;
            percentLabel.Text = "";
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine("Decompressed in " + time + " seconds");
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine("OPERATION COMPLETE");
            filenameBox.Enabled = true;
            openButton.Enabled = true;
            startButton.Enabled = true;
            decompressButton.Enabled = true;
        }

        private void cWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            percentLabel.Text = e.ProgressPercentage.ToString() + "%";\
        }

        private void dWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            percentLabel.Text = e.ProgressPercentage.ToString() + "%";
        }
    }
}
