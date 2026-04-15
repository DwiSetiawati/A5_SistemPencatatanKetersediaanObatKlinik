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

        // Tampilkan Data & Hitung Record (Bagian D & E)
        void TampilkanData()
        {
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Obat", con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dgvObat.DataSource = dt;
            con.Close();

            // ExecuteScalar untuk hitung total
            con.Open();
            SqlCommand cmdCount = new SqlCommand("SELECT COUNT(*) FROM Obat", con);
            lblTotal.Text = "Total Jenis Obat: " + cmdCount.ExecuteScalar().ToString();
            con.Close();
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (txtID.Text == "" || txtNama.Text == "")
            { // Validasi Bagian F
                MessageBox.Show("ID dan Nama wajib diisi!");
                return;
            }
            con.Open();
            SqlCommand cmd = new SqlCommand("INSERT INTO Obat VALUES (@id, @n, @s, @st)", con);
            cmd.Parameters.AddWithValue("@id", txtID.Text);
            cmd.Parameters.AddWithValue("@n", txtNama.Text);
            cmd.Parameters.AddWithValue("@s", txtSatuan.Text);
            cmd.Parameters.AddWithValue("@st", txtStok.Text);
            cmd.ExecuteNonQuery();
            con.Close();
            TampilkanData();
        }

       
    }
}
