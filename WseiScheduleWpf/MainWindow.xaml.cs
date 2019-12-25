﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CalendarGenerator;
using CalendarGenerator.PdfParse;
using Path = System.Windows.Shapes.Path;

namespace WseiScheduleWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".pdf", Filter = "Pdf Files (.pdf)|*.pdf"
            };

            var result = openFileDlg.ShowDialog();

            if (result == null || result != true) return;
            var savingResult = SaveICal(openFileDlg.FileName);
            StatusTextBlock.Text += savingResult + "\n";
        }

        private string SaveICal(string source)
        {
            try
            {
                using FileStream fileStream = File.Open(source, FileMode.Open, FileAccess.Read);
                var destinationDir = System.IO.Path.GetDirectoryName(source);
                var formattedTime = DateTime.Now.ToString(CultureInfo.InvariantCulture)
                    .Replace(".", "_").Replace(":", "_")
                    .Replace(" ", "_").Replace("/", "_");
                var generator = new CalGeneratorImpl();
                var iCal = generator.GenerateICalCalendar(fileStream);
                var destinationFileName = System.IO.Path.Combine(destinationDir, "calendar" + formattedTime + ".ical");
                File.WriteAllText(destinationFileName, iCal);
                return "Calendar saved: " + destinationFileName;
            }
            catch (ParsingException e)
            {
                return e.Message;
            }
        }
    }
}