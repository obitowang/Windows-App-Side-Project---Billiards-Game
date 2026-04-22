using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace remake
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = DateTime.Now.ToString("'今天是' M月d日"); //視窗標題 出現 日期
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string account = accountobx.Text;
            string password = passwordbox.Text;
            if (account == "admin" && password == "1234")
            {
                Form2 f2 = new Form2();
                f2.Owner = this;
                this.Hide();
                f2.Show();
            }
            else
            {
                MessageBox.Show("帳號或密碼錯誤！");
                accountobx.Clear();
                passwordbox.Clear();
                
            }
        }
    }
}
