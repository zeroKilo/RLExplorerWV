using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineClasses
{
    public class UPKFile
    {
        public class Header
        {
            public uint magic;
            public uint unk1;
            public uint unk2;
            public string NoneString;
            public uint unk3;
            public uint NameOffset;
            public uint NameCount;
            public uint ExportOffset;
            public uint ExportCount;
            public uint ImportOffset;
            public uint ImportCount;
        }

        public class NameListEntry
        {
            public string Text;
            public uint unk1;
            public uint unk2;

            public void ReadFromStream(Stream s)
            {
                Text = Helpers.ReadUnrealString(s);
                unk1 = Helpers.ReadUInt(s);
                unk2 = Helpers.ReadUInt(s);
            }

            public override string ToString()
            {
                return Text;
            }
        }

        public class ExportListEntry
        {
            public uint idxClass;
            public uint idxParent;
            public uint idxLink;
            public uint idxName;
            public uint unk5;
            public uint unk6;
            public uint unk7;
            public uint unk8;
            public uint Datasize;
            public uint Dataoffset;
            public uint unk11;
            public uint unk12;
            public uint unk13;
            public uint unk14;
            public uint unk15;
            public uint unk16;
            public uint unk17;

            public void ReadFromStream(Stream s)
            {
                idxClass = Helpers.ReadUInt(s);
                idxParent = Helpers.ReadUInt(s);
                idxLink = Helpers.ReadUInt(s);
                idxName = Helpers.ReadUInt(s);
                unk5 = Helpers.ReadUInt(s);
                unk6 = Helpers.ReadUInt(s);
                unk7 = Helpers.ReadUInt(s);
                unk8 = Helpers.ReadUInt(s);
                Datasize = Helpers.ReadUInt(s);
                Dataoffset = Helpers.ReadUInt(s);
                unk11 = Helpers.ReadUInt(s);
                unk12 = Helpers.ReadUInt(s);
                unk13 = Helpers.ReadUInt(s);
                unk14 = Helpers.ReadUInt(s);
                unk15 = Helpers.ReadUInt(s);
                unk16 = Helpers.ReadUInt(s);
                unk17 = Helpers.ReadUInt(s);
            }
        }

        public class ImportListEntry
        {
            public uint idxPackage; 
            public uint Unk1;  
            public uint idxClass;  
            public uint Unk2;       
            public uint idxLink;     
            public uint idxName;    
            public uint Unk3;

            public void ReadFromStream(Stream s)
            {
                idxPackage = Helpers.ReadUInt(s);
                Unk1 = Helpers.ReadUInt(s);
                idxClass = Helpers.ReadUInt(s);
                Unk2 = Helpers.ReadUInt(s);
                idxLink = Helpers.ReadUInt(s);
                idxName = Helpers.ReadUInt(s);
                Unk3 = Helpers.ReadUInt(s);
            }
        }

        public string MyPath;
        public Header header;
        public List<NameListEntry> NameList;
        public List<ExportListEntry> ExportList;
        public List<ImportListEntry> ImportList;
        public MemoryStream raw;
        public MemoryStream decrypted;
        public byte[] key = { 0xC7, 0xDF, 0x6B, 0x13, 0x25, 0x2A, 0xCC, 0x71, 
                              0x47, 0xBB, 0x51, 0xC9, 0x8A, 0xD7, 0xE3, 0x4B, 
                              0x7F, 0xE5, 0x00, 0xB7, 0x7F, 0xA5, 0xFA, 0xB2, 
                              0x93, 0xE2, 0xF2, 0x4E, 0x6B, 0x17, 0xE7, 0x79 };

        public UPKFile(string path)
        {
            MyPath = path;
            raw = new MemoryStream(File.ReadAllBytes(path));
            ReadHeader(raw);
            decrypted = new MemoryStream(Decrypt());
            ReadNameList();
            ReadExportList();
            ReadImportList();
        }

        private void ReadHeader(Stream s)
        {
            header = new Header();
            header.magic = Helpers.ReadUInt(s);
            header.unk1 = Helpers.ReadUInt(s);
            header.unk2 = Helpers.ReadUInt(s);
            header.NoneString = Helpers.ReadUnrealString(s);
            header.unk3 = Helpers.ReadUInt(s);
            header.NameCount = Helpers.ReadUInt(s);
            header.NameOffset = Helpers.ReadUInt(s);
            header.ExportCount = Helpers.ReadUInt(s);
            header.ExportOffset = Helpers.ReadUInt(s);
            header.ImportCount = Helpers.ReadUInt(s);
            header.ImportOffset = Helpers.ReadUInt(s);
        }

        private void ReadNameList()
        {
            decrypted.Seek(header.NameOffset, 0);
            NameList = new List<NameListEntry>();
            for (int i = 0; i < header.NameCount; i++)
            {
                NameListEntry e = new NameListEntry();
                e.ReadFromStream(decrypted);
                NameList.Add(e);
            }
        }

        private void ReadExportList()
        {
            decrypted.Seek(header.ExportOffset, 0);
            ExportList = new List<ExportListEntry>();
            for (int i = 0; i < header.ExportCount; i++)
            {
                ExportListEntry e = new ExportListEntry();
                e.ReadFromStream(decrypted);
                ExportList.Add(e);
            }
        }

        private void ReadImportList()
        {
            decrypted.Seek(header.ImportOffset, 0);
            ImportList = new List<ImportListEntry>();
            for (int i = 0; i < header.ImportCount; i++)
            {
                ImportListEntry e = new ImportListEntry();
                e.ReadFromStream(decrypted);
                ImportList.Add(e);
            }
        }

        byte[] Decrypt()
        {            
            int pos = (int)header.NameOffset;
            int size = (int)(raw.Length - pos);
            int size2 = size + (0x100 - size % 0x20);
            byte[] h = new byte[pos];
            raw.Seek(0, 0);
            raw.Read(h, 0, pos);
            byte[] buff = new byte[size2];
            raw.Read(buff, 0, size);
            buff = Helpers.AESDecrypt(buff, key);
            MemoryStream m = new MemoryStream();
            m.Write(h, 0, pos);
            m.Write(buff, 0, size);
            return m.ToArray();
        }

        public string GetName(int index)
        {
            string s = "";
            if (index >= 0 && index < header.NameCount)
                s = NameList[index].ToString();
            return s;
        }

        public string GetObjectName(int uindex)
        {
            if (uindex == 0)
                return "Class";
            if (uindex > 0)
                return GetName((int)ExportList[uindex - 1].idxName);
            else
                return GetName((int)ImportList[-uindex - 1].idxName);
        }
        public string GetObjectNamePath(int uindex)
        {
            string s = GetObjectName(uindex);
            if (uindex == 0)
                return s;
            if (uindex > 0)
                uindex = (int)ExportList[uindex - 1].idxLink;
            else
                uindex = (int)ImportList[-uindex - 1].idxLink;
            while (uindex != 0)
            {
                s = GetObjectName(uindex) + "." + s;
                if (uindex > 0)
                    uindex = (int)ExportList[uindex - 1].idxLink;
                else
                    uindex = (int)ImportList[-uindex - 1].idxLink;
            }
            return s;
        }
    }
}
