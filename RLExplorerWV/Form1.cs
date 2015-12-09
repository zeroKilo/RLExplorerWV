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
            rtb1.Text = TryReadProps(new MemoryStream(buff));
        }

        public string TryReadProps(MemoryStream m, int tab = 0, int start = 4)
        {
            StringBuilder sb = new StringBuilder();
            m.Seek(start, 0);
            string t = "";
            for (int i = 0; i < tab; i++)
                t += "\t";
            while (true)
            {
                string offset = m.Position.ToString("X");
                if (m.Position >= m.Length)
                    break;
                int name = Helpers.ReadInt(m);
                int tmp = Helpers.ReadInt(m);
                if (!upk.IsNameIndex(name) || upk.GetName(name) == "None")
                    break;
                int type = Helpers.ReadInt(m);
                tmp = Helpers.ReadInt(m);
                if (!upk.IsNameIndex(type))
                    break;
                int size = Helpers.ReadInt(m);
                tmp = Helpers.ReadInt(m);
                if (m.Position + size >= m.Length || size < 0)
                    break;
                byte[] buff;
                int clazz, value;
                switch(upk.GetName(type))
                {
                    case "StructProperty":
                        clazz = Helpers.ReadInt(m);
                        tmp = Helpers.ReadInt(m);
                        if (!upk.IsNameIndex(clazz))
                            return sb.ToString();
                        buff = new byte[size];
                        m.Read(buff, 0, size);
                        sb.AppendFormat("{0}<prop Offset=0x{1} Name='{2}' Type='{3}' Size={4} Class='{5}'>\n", t, offset, upk.GetName(name), upk.GetName(type), size, upk.GetName(clazz));
                        sb.Append(TryReadProps(new MemoryStream(buff), tab + 1, 0));
                        sb.AppendFormat("{0}<prop/>\n", t, upk.GetName(name), upk.GetName(type), size);
                        break;
                    case "BoolProperty":
                        size = 1;
                        buff = new byte[size];
                        m.Read(buff, 0, size);
                        sb.AppendFormat("{0}<prop Offset=0x{1} Name='{2}' Type='{3}' Size={4} value='{5}'/>\n", t, offset, upk.GetName(name), upk.GetName(type), size, Helpers.ByteArrayToHexString(buff));
                        break;
                    case "ByteProperty":
                        clazz = Helpers.ReadInt(m);
                        if (!upk.IsNameIndex(clazz))
                            return sb.ToString();
                        tmp = Helpers.ReadInt(m);
                        value = Helpers.ReadInt(m);
                        if (!upk.IsNameIndex(value))
                            return sb.ToString();
                        tmp = Helpers.ReadInt(m);
                        sb.AppendFormat("{0}<prop Offset=0x{1} Name='{2}' Type='{3}' Size={4} class='{5}' value='{6}'/>\n", t, offset, upk.GetName(name), upk.GetName(type), size, upk.GetName(clazz), upk.GetName(value));
                        break;
                    default:
                        buff = new byte[size];
                        m.Read(buff, 0, size);
                        sb.AppendFormat("{0}<prop Offset=0x{1} Name='{2}' Type='{3}' Size={4} value='{5}'/>\n", t, offset, upk.GetName(name), upk.GetName(type), size, Helpers.ByteArrayToHexString(buff));
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
