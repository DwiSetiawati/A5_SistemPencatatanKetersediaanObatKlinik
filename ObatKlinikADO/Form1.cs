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

        private void btnHapus_Click(object sender, EventArgs e)
        {
            DialogResult kf = MessageBox.Show("Yakin hapus?", "Konfirmasi", MessageBoxButtons.YesNo);
            if (kf == DialogResult.Yes)
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM Obat WHERE id_obat=@id", con);
                cmd.Parameters.AddWithValue("@id", txtID.Text);
                cmd.ExecuteNonQuery();
                con.Close();
                TampilkanData();
            }
        }

        private void btnCari_Click(object sender, EventArgs e)
        {
            con.Open();
            // Mencari data berdasarkan nama (Bagian E)
            string query = "SELECT * FROM Obat WHERE nama_obat LIKE @cari";
            cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@cari", "%" + txtCari.Text + "%");

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            dgvObat.DataSource = dt;
            con.Close();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Validasi input (Bagian F)
            if (txtID.Text == "" || txtNama.Text == "")
            {
                MessageBox.Show("Pilih data yang ingin diubah terlebih dahulu!");
                return;
            }

            DialogResult result = MessageBox.Show("Apakah Anda yakin ingin mengubah data ini?", "Konfirmasi", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                con.Open();
                // Query UPDATE (Bagian D)
                string query = "UPDATE Obat SET nama_obat=@nama, satuan=@sat, stok_total=@stok WHERE id_obat=@id";
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", txtID.Text);
                cmd.Parameters.AddWithValue("@nama", txtNama.Text);
                cmd.Parameters.AddWithValue("@sat", txtSatuan.Text);
                cmd.Parameters.AddWithValue("@stok", txtStok.Text);

                cmd.ExecuteNonQuery();
                con.Close();

                MessageBox.Show("Data Berhasil Diperbarui!");
                TampilkanData(); // Refresh tabel
            }
        }

        private void dgvObat_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvObat.Rows[e.RowIndex];
                // Pindahkan data dari tabel ke TextBox (Bagian E)
                txtID.Text = row.Cells["id_obat"].Value.ToString();
                txtNama.Text = row.Cells["nama_obat"].Value.ToString();
                txtSatuan.Text = row.Cells["satuan"].Value.ToString();
                txtStok.Text = row.Cells["stok_total"].Value.ToString();
            }
        }
    }
}
