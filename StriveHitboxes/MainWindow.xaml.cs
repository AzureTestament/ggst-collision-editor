using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
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
using Microsoft.Win32;
using System.Drawing;
using System.ComponentModel;

namespace StriveHitboxes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string colPath;
        PACFile col;
        OverlaidImage oi;
        List<PACFile.PACEntry> editedBoxes = new List<PACFile.PACEntry>();
        private int oldListIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PAC files (*.pac)|*.pac|All files (*.*)|*.*";
            openFileDialog.Title = "Open Collision PAC";
            if (openFileDialog.ShowDialog() == true) 
            {
                File.Delete(colPath + ".tmp");
                colPath = openFileDialog.FileName;
                col = new PACFile(openFileDialog.FileName);
                byte[] oldpac = File.ReadAllBytes(colPath);
                File.WriteAllBytes(colPath + ".tmp", oldpac);
                spriteList.Items.Clear();
                boxList.Items.Clear();
                foreach(var entry in col.pacentries)
                {
                    spriteList.Items.Add(entry);
                }
            }
        }
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void spriteList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (spriteList.Items.Count > 0)
            {
                PACFile.PACEntry colentry = (PACFile.PACEntry)spriteList.Items[spriteList.SelectedIndex];
                var coloffset = col.getOffsetByName(colentry.name);
                if (oldListIndex > 0)
                {
                    var oldEntry = (PACFile.PACEntry)spriteList.Items[oldListIndex];
                    int idx = editedBoxes.BinarySearch(oldEntry);
                    if (idx < 0) idx = ~idx;
                    editedBoxes.Insert(idx, oldEntry);
                }
                oldListIndex = spriteList.SelectedIndex;
                oi = new OverlaidImage(col.path, coloffset);
                boxList.Items.Clear();
                foreach (var box in oi.hurtboxes)
                    boxList.Items.Add(box);
                foreach (var box in oi.hitboxes)
                    boxList.Items.Add(box);
                if (boxList.Items.Count != 0)
                    boxList.SelectedIndex = 0;
                spriteBox.Source = BitmapToImageSource(oi.boxbitmap);
            }
            else
            {
                spriteList.Items.Clear();
                boxList.Items.Clear();
                editedBoxes.Clear();
            }
        }

        private void boxList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (boxList.Items.Count > 0)
            {
                JonbinBox currbox = (JonbinBox)boxList.Items[boxList.SelectedIndex];
                xPos.TextChanged -= xPos_TextChanged;
                yPos.TextChanged -= yPos_TextChanged;
                xScl.TextChanged -= xScl_TextChanged;
                yScl.TextChanged -= yScl_TextChanged;
                xPos.Text = currbox.x.ToString();
                yPos.Text = currbox.y.ToString();
                xScl.Text = currbox.width.ToString();
                yScl.Text = currbox.height.ToString();
                xPos.TextChanged += xPos_TextChanged;
                yPos.TextChanged += yPos_TextChanged;
                xScl.TextChanged += xScl_TextChanged;
                yScl.TextChanged += yScl_TextChanged;
            }
        }

        private void xPos_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (boxList.Items.Count > 0)
            {
                JonbinBox currbox = (JonbinBox)boxList.Items[boxList.SelectedIndex];
                float newx;
                if (float.TryParse(xPos.Text, out newx))
                {
                    currbox.x = newx;
                    oi.renderBoxes();
                    spriteBox.Source = BitmapToImageSource(oi.boxbitmap);
                }
            }
        }

        private void yPos_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (boxList.Items.Count > 0)
            {
                JonbinBox currbox = (JonbinBox)boxList.Items[boxList.SelectedIndex];
                float newY;
                if (float.TryParse(yPos.Text, out newY))
                {
                    currbox.y = newY;
                    oi.renderBoxes();
                    spriteBox.Source = BitmapToImageSource(oi.boxbitmap);
                }
            }
        }

        private void xScl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (boxList.Items.Count > 0)
            {
                JonbinBox currbox = (JonbinBox)boxList.Items[boxList.SelectedIndex];
                float newW;
                if (float.TryParse(xScl.Text, out newW))
                {
                    currbox.width = newW;
                    oi.renderBoxes();
                    spriteBox.Source = BitmapToImageSource(oi.boxbitmap);
                }
            }
        }

        private void yScl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (boxList.Items.Count > 0)
            {
                JonbinBox currbox = (JonbinBox)boxList.Items[boxList.SelectedIndex];
                float newH;
                if (float.TryParse(yScl.Text, out newH))
                {
                    currbox.height = newH;
                    oi.renderBoxes();
                    spriteBox.Source = BitmapToImageSource(oi.boxbitmap);
                }
            }
        }

        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PAC files (*.pac)|*.pac|All files (*.*)|*.*";
            saveFileDialog.Title = "Save edited PAC file";
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                PACFile.PACEntry colentry = (PACFile.PACEntry)spriteList.Items[spriteList.SelectedIndex];
                int idx = editedBoxes.BinarySearch(colentry);
                if (idx < 0) idx = ~idx;
                editedBoxes.Insert(idx, colentry);
                byte[] oldpac = File.ReadAllBytes(col.path);
                foreach (var jb in editedBoxes)
                {
                    int writestart = (int)(jb.offset + col.data_start);
                    int writeend = writestart;
                    writeend += 4;
                    writeend += 2 + BitConverter.ToInt16(oldpac, writeend) * 0x20 + 3;
                    var chunkssize = BitConverter.ToInt32(oldpac, writeend) * 0x50;
                    writeend += 10 + 41 * 2 + chunkssize;
                    int boxcount = boxList.Items.Count;
                    byte[] currfloat = { 0, 0, 0, 0 };
                    for (var i = 0; i < boxcount; i++)
                    {
                        writeend += 4;
                        var currbox = (JonbinBox)boxList.Items[i];
                        currfloat = System.BitConverter.GetBytes(currbox.x);
                        Array.Copy(currfloat, 0, oldpac, writeend, 4);
                        writeend += 4;
                        currfloat = System.BitConverter.GetBytes(currbox.y);
                        Array.Copy(currfloat, 0, oldpac, writeend, 4);
                        writeend += 4; currfloat = System.BitConverter.GetBytes(currbox.width);
                        Array.Copy(currfloat, 0, oldpac, writeend, 4);
                        writeend += 4; currfloat = System.BitConverter.GetBytes(currbox.height);
                        Array.Copy(currfloat, 0, oldpac, writeend, 4);
                        writeend += 4;
                    }
                }
                File.WriteAllBytes(saveFileDialog.FileName, oldpac);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(File.Exists(colPath + ".tmp"))
            {
                File.Delete(colPath + ".tmp");
            }
        }

        private void CloseItem_Click(object sender, RoutedEventArgs e)
        {
            spriteList.Items.Clear();
            boxList.Items.Clear();
            editedBoxes.Clear();
        }
    }
}
