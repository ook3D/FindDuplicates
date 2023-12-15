using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ookii.Dialogs.WinForms;
using System.Reflection;

namespace FindDuplicates
{
    public partial class DuplicateFinder : Form
    {
        public DuplicateFinder()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string resourceName = new AssemblyName(args.Name).Name + ".dll";
                string resource = Array.Find(this.GetType().Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };

            InitializeComponent();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            using (VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolder = folderBrowserDialog.SelectedPath;
                    txtFolderPath.Text = selectedFolder;

                    string[] excludedExtensions = GetExcludedExtensions();

                    rtbResults.Clear();

                    Dictionary<string, List<string>> fileDict = new Dictionary<string, List<string>>();

                    foreach (var file in Directory.GetFiles(selectedFolder, "*", SearchOption.AllDirectories))
                    {
                        var fileName = Path.GetFileName(file);

                        if (fileDict.ContainsKey(fileName))
                        {
                            fileDict[fileName].Add(file);
                        }
                        else
                        {
                            fileDict[fileName] = new List<string> { file };
                        }
                    }

                    foreach (var duplicateEntry in fileDict.Where(entry => entry.Value.Count > 1))
                    {
                        string fileName = duplicateEntry.Key;
                        List<string> filePaths = duplicateEntry.Value;

                        if (!ShouldExcludeFile(fileName, excludedExtensions))
                        {
                            rtbResults.AppendText($"Duplicate found: {fileName}\n");

                            foreach (var filePath in filePaths)
                            {
                                rtbResults.AppendText($"   - {filePath}\n");
                            }

                            rtbResults.AppendText("\n");
                        }
                    }
                }
            }
        }

        private string[] GetExcludedExtensions()
        {
            string excludeText = rtbExclude.Text;
            char[] separators = { ',', ' ' };
            string[] excludedExtensions = excludeText.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < excludedExtensions.Length; i++)
            {
                excludedExtensions[i] = excludedExtensions[i].TrimStart('.');
            }

            return excludedExtensions;
        }

        private bool ShouldExcludeFile(string fileName, string[] excludedExtensions)
        {
            string fileExtension = Path.GetExtension(fileName)?.TrimStart('.');
            return excludedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }
    }
}