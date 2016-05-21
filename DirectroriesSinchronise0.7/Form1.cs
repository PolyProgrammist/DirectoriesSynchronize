using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DirectroriesSinchronise0._6
{
    public partial class Disks : Form
    {
        private string dir1, dir2;
        Sync sync;
        public Disks()
        {
            InitializeComponent();
        }

        //Добавление результата сравнения в TreeView
        private void Printing()
        {
            for (int i = 0; i < 2; i++)
            {
                sync.CmpTn[i].Checked = false;
                sync.CmpTn[i].Toggle();
                CheckFamily(sync.CmpTn[i]);
            }
            treeView1.Nodes.Add(sync.CmpTn[0]);
            treeView2.Nodes.Add(sync.CmpTn[1]);
        }

        //Пометка всех нужных нодов при пометке пользователем одного из нодов
        private void CheckFamily(TreeNode tn)
        {
            CheckChilds(tn);
            CheckParents(tn);
        }
        //Пометка всех нужных родительских нодов
        private void CheckParents(TreeNode tn)
        {
            if (tn.Parent == null)
                return;
            for (int i = 0; i < tn.Parent.Nodes.Count; i++)
                if (i != tn.Index && tn.Parent.Nodes[i].Checked)
                    return;
            tn.Parent.Checked = tn.Checked;
            CheckParents(tn.Parent);
        }
        //Пометка всех нужных дочерних нодов
        private void CheckChilds(TreeNode tn)
        {
            for (int i = 0; i < tn.Nodes.Count; i++)
            {
                tn.Nodes[i].Checked = tn.Checked;
                CheckChilds(tn.Nodes[i]);
            }
        }
        // выбор каталога 1
        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.SelectedPath = String.IsNullOrEmpty(dir1) ? null : dir1;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                if (dir1 != folderBrowserDialog1.SelectedPath)
                {
                    treeView1.Nodes.Clear();
                    treeView2.Nodes.Clear();
                    sync = null;
                }
                dir1 = folderBrowserDialog1.SelectedPath;
                WriteLn(String.Format("dir1 = {0}", dir1));
                button1.Text = folderBrowserDialog1.SelectedPath;
                return;
            }
        }
        // выбор каталога 2
        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.SelectedPath = String.IsNullOrEmpty(dir2) ? null : dir2;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                if (dir2 != folderBrowserDialog1.SelectedPath)
                {
                    treeView1.Nodes.Clear();
                    treeView2.Nodes.Clear();
                    sync = null;
                }
                dir2 = folderBrowserDialog1.SelectedPath;
                WriteLn(String.Format("dir2 = {0}", dir2));
                button2.Text = folderBrowserDialog1.SelectedPath;
                return;
            }
        }

        /**
         * сравнить каталоги
         */ 
        private void button3_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(dir1) || String.IsNullOrEmpty(dir2))
            {
                WriteLn("warn: Two folders are not selected");
                return;
            }
            if (dir1 == dir2)
            {
                WriteLn("Names of Disks are same");
                return;
            }
            treeView1.Nodes.Clear();
            treeView2.Nodes.Clear();

            WriteLn("Compare start...");

            sync = new Sync(dir1, dir2, textBox1, checkBox1.Checked);
            sync.Comparing();
            Printing();

            WriteLn("Compare Finished...");
        }

        /**
        * синхронизация 2х выбранных каталогов
        */
        private void button4_Click(object sender, EventArgs e)
        {
            if (sync == null) {
                WriteLn("warn: Compare folders first!");
                return;
            }
            if (treeView1.Nodes[0].Nodes.Count == 0 && treeView2.Nodes[0].Nodes.Count == 0)
            {
                WriteLn("Folders are already sync'ed");
                return;
            }
            WriteLn("Synchronize start...");

            sync.Synchronize();

            WriteLn("Synchronize finished...");
        }
        
        //Поставка галочки в первом TreeView
        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByMouse || e.Action == TreeViewAction.ByKeyboard)
                CheckFamily(e.Node);
        }
        //Поставка галочки во втором TreeView
        private void treeView2_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByMouse || e.Action == TreeViewAction.ByKeyboard)
                CheckFamily(e.Node);
        }

        //Вывод в текстбокс с переносом строки
        public void WriteLn(string p)
        {
            textBox1.AppendText(p + "\n");
        }
    }
}
