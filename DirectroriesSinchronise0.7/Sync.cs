using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.IO;
using System.Collections;
using System.Security;
using System.Security.AccessControl;
namespace DirectroriesSinchronise0._6
{
    public class Sync
    {
        private bool compared;
        private bool Ord;//only root directory
        private string disk1, disk2;
        public TreeNode[] CmpTn;
        public string[] parasite = { "Thumbs.db" , "desktop.ini"};

        private string[] disk 
        { 
            get
            { 
                return new string[]{disk1, disk2}; 
            }
        }
        TextBox tb;

        public Sync(string disk1, string disk2, TextBox tb, bool Ord = false)
        {
            this.disk1 = disk1;
            this.disk2 = disk2;
            CmpTn = new TreeNode[disk.Length];
            this.tb = tb;
            compared = false;
            this.Ord = Ord;
        }

        /*
         * Сравнение папок, инициализация TND[i], TNF[i]
         */
        public void Comparing()
        {
            if (!OkDisk())
            {
                WriteLn("warn: Choose existing disks");
                return;
            }

            TreeNode[] All = new TreeNode[disk.Length];

            for (int i = 0; i < disk.Length; i++)
            {
                All[i] = FileTree(disk[i]);
                WriteLn(String.Format("FileSystem {0} found", (i + 1).ToString()));
            }
            for (int i = 0; i < disk.Length; i++)
            {
                int j = (i == 0) ? 1 : 0;
                CmpTn[i] = DifferFils(All[i], All[j], i, j);
                CmpTn[i].Text = disk[i];
                WriteLn(String.Format("Differens {0} found", (i + 1).ToString()));
            }
            compared = true;
        }
        //Построение файловой системы для данного каталога
        private TreeNode FileTree(string DirNow)
        {
            TreeNode res = new TreeNode(NameThis(DirNow));
            string[] dDirs, dFils;
            dDirs = Directory.GetDirectories(DirNow, "*", SearchOption.TopDirectoryOnly);
            dFils = Directory.GetFiles(DirNow, "*", SearchOption.TopDirectoryOnly);
            Array.Sort(dDirs);
            Array.Sort(dFils);
            for (int i = 0; i < dDirs.Length; i++)
            {
                try
                {
                    res.Nodes.Add(FileTree(dDirs[i]));
                }
                catch (UnauthorizedAccessException e)
                {
                    WriteLn(String.Format("No root {0} {1}", dDirs[i], e.Message));
                }
                res.LastNode.Name = "0";
            }
            for (int i = 0; i < dFils.Length; i++)
                if (isParasite(NameThis(dFils[i])) == null)
                {
                    res.Nodes.Add(NameThis(dFils[i]));
                    res.LastNode.Name = "1";
                }
            return res;
        }
        //Поиск различных файлов
        private TreeNode DifferFils(TreeNode tn1, TreeNode tn2, int d1, int d2)
        {
            TreeNode res = new TreeNode(tn1.Text);
            for (int i = 0; i < tn1.Nodes.Count; i++)
            {
                bool was = false;
                bool DifAtr = false;
                string pathToThis = FullName(tn1.Nodes[i], disk[d1]);
                for (int j = 0; j < tn2.Nodes.Count && !was; j++)
                    if (tn1.Nodes[i].Text == tn2.Nodes[j].Text)
                    {
                        was = true;
                        if (IsFile(tn1.Nodes[i]) && !EqualAtrs(pathToThis, FullName(tn2.Nodes[j], disk[d2])))
                            DifAtr = true;
                        if (!Ord)
                        {
                            TreeNode adding = DifferFils(tn1.Nodes[i], tn2.Nodes[j], d1, d2);
                            if (adding.Nodes.Count > 0)
                                myAdd(ref res, adding, tn1.Nodes[i].Name);
                        }
                    }
                if (!was || DifAtr)
                    if (IsFile(tn1.Nodes[i]) || DifAtr)
                    {
                        string atrs = (DifAtr) ? Atrs(pathToThis) : "";
                        res.Nodes.Add(tn1.Nodes[i].Text + ((DifAtr) ? atrs : ""));
                        res.LastNode.Name = tn1.Nodes[i].Name + ((DifAtr) ? tn1.Nodes[i].Text.Length.ToString() : "");
                    }
                    else
                        myAdd(ref res, tn1.Nodes[i], tn1.Nodes[i].Name);
            }
            return res;
        }

        /*
         * Синхронизация дисков
         */
        public void Synchronize()
        {
            if (!compared)
            {
                WriteLn("warn: Compare After Sync!");
                return;
            }
            for (int i = 0; i < disk.Length; i++)
                if (LowDisk(i))
                {
                    WriteLn(String.Format("error: Too small disk {0}", disk[i]));
                    return;
                }
                for (int i = 0; i < disk.Length; i++)
                {
                    int j = (i == 0) ? 1 : 0; ;
                    CopyFiles(CmpTn[i], disk[i], disk[j], i);
                    WriteLn(String.Format("Different Files from Disk {0} to Disk {1} Copied.", (i + 1).ToString(), (j + 1).ToString()));
                }
            compared = false;
        }
        //Определяет, недостаточно ли места на диске для копирования файлов
        private bool LowDisk(int i)
        {
            return false;
        }
        //Копирование каталогов и файлов
        private void CopyFiles(TreeNode tn, string dOld, string dNew, int dNum)
        {
            for (int i = 0; i < tn.Nodes.Count; i++)
                if (tn.Nodes[i].Checked)
                {
                    tn.Nodes[i].ForeColor = Color.DarkRed;
                    if (IsFile(tn.Nodes[i]))
                    {
                        string FtnP = FullTreeNodePath(tn.Nodes[i]);
                        string sufftnp = FtnP;
                        if (tn.Nodes[i].Name.Length > 1)
                        {
                            string size = tn.Nodes[i].Name.Remove(0, 1);
                            FtnP = FtnP.Remove(int.Parse(size) + 1 + (FtnP.Length - tn.Nodes[i].Text.Length));
                            sufftnp = WithSuffix(dNum, FtnP, dOld, dNew);
                        }
                        if (sufftnp == "")
                        {
                            WriteLn(String.Format("File was not copied, You are testing me!!! {0}{1}", dOld, FtnP));
                            continue;
                        }
                        string oldF = dOld + FtnP;
                        try
                        {
                            File.Copy(oldF, dNew + sufftnp);
                            File.Move(oldF, dOld + sufftnp);
                            WriteLn(String.Format("File {0} copied.", oldF));
                        }
                        catch (Exception ex)
                        {
                            WriteLn(String.Format("Error copying file {0}. Exception: {1}", oldF, ex.Message));
                        }
                    }
                    else
                    {
                        string fn = FullName(tn.Nodes[i], dNew);
                        if (!Directory.Exists(fn))
                        {
                                Directory.CreateDirectory(fn);
                                WriteLn(string.Format("Directory {0} created.", fn));
                        }
                        CopyFiles(tn.Nodes[i], dOld, dNew, dNum);
                    }
                    tn.Nodes[i].ForeColor = Color.Blue;
                }
        }
        //Добавлени суффикса к имени файла ftnp с диска с индексом dNum при копировании со старого диска с именем dOld на новый диск с именем dNew
        private string WithSuffix(int dNum, string ftnp, string dOld, string dNew)
        {
            string[] suf = { "D", "F", "Disk", "Dir", "Cat", "Fold", "Folder", "Directory", "Catalog", "Disket", "Direct", "" };
            string[] spl = { "", " ", "_", "-", "." };
            string num = dNum.ToString();
            int idx = 0;
            for (int i = ftnp.Length - 1; i >= 0; i--)
            {
                if (ftnp[i] == '.')
                {
                    idx = i;
                    break;
                }
                if (ftnp[i] == '\\')
                {
                    idx = i + 1;
                    break;
                }
            }
            for (int addNum = 0; addNum < 10000; addNum++)
                for (int i = 0; i < suf.Length; i++)
                    for (int j = 0; j < spl.Length; j++)
                    {
                        string res = ftnp.Insert(idx, spl[j] + suf[0] + num + ((addNum > 0) ? addNum.ToString() : ""));
                        if (!File.Exists(dOld + res) && !File.Exists(dNew + res))
                            return res;
                    }
            return "";
        }

        //Добавление нода adding к ноду dif с присвоением имени Name
        private void myAdd(ref TreeNode dif, TreeNode adding, string Name)
        {
            dif.Nodes.Add(adding);
            dif.LastNode.Name = Name;
        }
        //Возвращает строку, в которой описываются аттрибуты(размер, время изменения) файла с именем path
        private string Atrs(string path)
        {
            String res = "    ";
            FileInfo fi = new FileInfo(path);
            res += stringSize(fi.Length) + "    ";
            res += "LWT: " + fi.LastWriteTime;
            return res;
        }
        //Возвращает строку, в которой удобно для пользователя описывается размер ln файла, razdel - разделитель между числовым значением и единицей измерения
        private string stringSize(long ln, string razdel = " ")
        {
            String[] sz = { "B", "KB", "MB", "GB", "TB" };
            if (ln == 0)
                return "0" + razdel + sz[0];
            int dgs = (int)Math.Log10(ln) + 1;
            string size = (dgs >= 0) ? ln.ToString().Substring(0, (dgs % 3 == 0) ? 3 : dgs % 3) : "0";
            return size + razdel + sz[(dgs - 1) / 3];
        }

        //Возвращает Имя данного каталога или файла, имеющего полное имя(с путем) Full
        private string NameThis(string Full)
        {
            for (int i = Full.Length - 1; i >= 0; i--)
                if (Full[i] == '\\')
                    return Full.Remove(0, i + 1);
            return Full;
        }
        //Возвращает полное имя(с путем) к файлу или каталогу в данных TreeNode и disk
        private string FullName(TreeNode tn, string disk)
        {
            return disk + FullTreeNodePath(tn);
        }
        //Возвращает полный путь к данному TreeNode
        private string FullTreeNodePath(TreeNode tn)
        {
            if (tn.Parent == null)
                return "";
            return FullTreeNodePath(tn.Parent) + '\\' + tn.Text;
        }
        //Возвращает значение, описывающее одинаковость аттрибутов файлов name1
        private bool EqualAtrs(string name1, string name2)
        {
            FileInfo fi1 = new FileInfo(name1);
            FileInfo fi2 = new FileInfo(name2);
            return fi1.Length == fi2.Length;
        }
        //Возвращает значение, определяющее, является ли данный TreeNode файлом
        private bool IsFile(TreeNode tn)
        {
            return tn.Name[0] == '1';
        }
        //Определяет, существуют ли данные диски
        private bool OkDisk()
        {
            for (int i = 0; i < disk.Length; i++)
                if (!Directory.Exists(disk[i]))
                    return false;
            return true;
        }
        private string isParasite(string fName)
        {
            for (int i = 0; i < parasite.Length; i++)
                if (fName == parasite[i])
                    return parasite[i];
            return null;
        }

        //Добавляет текст в Disks.textBox1
        private void WriteLn(string p)
        {
            tb.AppendText(p + "\n");
        }
    }
}
