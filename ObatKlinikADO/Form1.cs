using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ObatKlinikADO
{
    public partial class Form1 : Form
    {
        string connectionString = "Data Source=LAPTOP-PQSI1Q9H\\TIAA;Initial Catalog=KlinikObatDB;Integrated Security=True";
        SqlConnection con;
        SqlCommand cmd;
        string roleLogin; // Untuk menampung kiriman dari FormLogin

        public Form1(string role)
        {
            InitializeComponent();
            this.roleLogin = role;
            con = new SqlConnection(connectionString);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TampilkanData();
            // Cek Hak Akses (Pemilik = Read Only)
            if (roleLogin == "Pemilik")
            {
                btnTambah.Enabled = false;
                btnHapus.Enabled = false;
                btnUpdate.Enabled = false;
                this.Text = "Mode Monitoring (Pemilik)";
            }
        }

       
    }
}
