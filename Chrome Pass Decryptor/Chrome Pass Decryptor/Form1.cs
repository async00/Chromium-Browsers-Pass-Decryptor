using GetPasswords;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chrome_Pass_Decryptor
{
    public partial class Form1 : Form
    {

        internal static string LOCAL_STATE = string.Empty;
        internal static string LOGIN_DATA  = string.Empty;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            string dosyaAdi = Guid.NewGuid().ToString() + ".txt";

            // Metin belgesi oluşturun ve "berkay" yazın
            using (StreamWriter yazici = File.CreateText(dosyaAdi))
            {
                if(textBox1.Text != string.Empty&&textBox2.Text != string.Empty) 
                    yazici.WriteLine(Chrome.GetPassword(textBox1.Text, textBox2.Text));
                if (textBox3.Text != string.Empty)
                    yazici.WriteLine(Chrome.ShowChromeHistory(textBox3.Text));
            }

            // Oluşturulan metin belgesini açın
            System.Diagnostics.Process.Start(dosyaAdi);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Tüm Dosyalar|*.*"; // İsteğe bağlı olarak dosya türü filtresini ayarlayabilirsiniz.

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;
                textBox1.Text = selectedFilePath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Tüm Dosyalar|*.*"; // İsteğe bağlı olarak dosya türü filtresini ayarlayabilirsiniz.

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;
                textBox2.Text = selectedFilePath;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Tüm Dosyalar|*.*"; // İsteğe bağlı olarak dosya türü filtresini ayarlayabilirsiniz.

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;
                textBox3.Text = selectedFilePath;
            }
        }
    }
}
