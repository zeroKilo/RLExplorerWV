using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Be.Windows.Forms;
using EngineClasses;

namespace RLExplorerWV
{
    public partial class Form1 : Form
    {
        UPKFile upk;

        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.upk|*.upk";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadFile(d.FileName);
            }
        }

        private void LoadFile(string s)
        {
            upk = new UPKFile(s);
            hb1.ByteProvider = new DynamicByteProvider(upk.decrypted.ToArray());
            listBox1.Items.Clear();
            int count = 0;
            foreach (UPKFile.NameListEntry e in upk.NameList)
                listBox1.Items.Add((count++).ToString("d5") + " : (0x" + e.unk1.ToString("X8") + " - 0x" + e.unk2.ToString("X8") + ") " + e);
            listBox2.Items.Clear();
            count = 0;
            foreach (UPKFile.ImportListEntry e in upk.ImportList)
            {
                listBox2.Items.Add(count.ToString("d5") + " : " + upk.GetObjectNamePath(-count - 1));
                count++;
            }
            listBox3.Items.Clear();
            count = 0;
            foreach (UPKFile.ExportListEntry e in upk.ExportList)
            {
                listBox3.Items.Add(count.ToString("d5") + " : " + upk.GetObjectNamePath(count + 1));
                count++;
            }
            Status.Text = "Loaded File : " + s;
        }

        private void saveRawDecryptedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (upk == null)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.upk_dec|*.upk_dec";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(d.FileName, upk.decrypted.ToArray());
                MessageBox.Show("Done.");
            }
        }

        private void saveExportEntryDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox3.SelectedIndex;
            if (n == -1)
                return;
            upk.decrypted.Seek(upk.ExportList[n].Dataoffset, 0);
            int size = (int)upk.ExportList[n].Datasize;
            byte[] buff = new byte[size];
            upk.decrypted.Read(buff, 0, size);
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(d.FileName, buff);
                MessageBox.Show("Done.");
            }
        }

        private void listBox3_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            int n = listBox3.SelectedIndex;
            if (n == -1)
                return;
            upk.decrypted.Seek(upk.ExportList[n].Dataoffset, 0);
            int size = (int)upk.ExportList[n].Datasize;
            byte[] buff = new byte[size];
            upk.decrypted.Read(buff, 0, size);
            hb2.ByteProvider = new DynamicByteProvider(buff);
            hb3.ByteProvider = new DynamicByteProvider(upk.ExportList[n].ToRaw());
        }
    }
}
