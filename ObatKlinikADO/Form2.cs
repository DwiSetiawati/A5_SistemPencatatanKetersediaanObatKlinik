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
    public partial class Form2 : Form
    {
        string connectionString = "Data Source=LAPTOP-PQSI1Q9H\\TIAA;Initial Catalog=KlinikObatDB;Integrated Security=True";
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                // Mengambil role untuk dikirim ke form utama
                SqlCommand cmd = new SqlCommand("SELECT role FROM Akun WHERE username=@u AND password=@p", con);
                cmd.Parameters.AddWithValue("@u", txtUsername.Text);
                cmd.Parameters.AddWithValue("@p", txtPassword.Text);

                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    string role = dr["role"].ToString();
                    MessageBox.Show("Selamat Datang, " + role);

                    // Buka Form Utama dan kirim Role-nya
                    Form1 utama = new Form1(role);
                    utama.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Username/Password Salah!");
                }
            }
        }
    }
}