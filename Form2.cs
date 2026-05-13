using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ObatKlinikADO
{
    public partial class Form2 : Form
    {
        string connectionString = "Data Source=RISWANDA\\WANDA_PUTRA;Initial Catalog=KlinikObatDB;Integrated Security=True";

        public Form2()
        {
            InitializeComponent();
            // [FIX 1] Sembunyikan karakter password
            txtPassword.PasswordChar = '*';
        }

        private void Form2_Load(object sender, EventArgs e)
        {
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // [FIX 2] Validasi input kosong
            if (txtUsername.Text.Trim() == "" || txtPassword.Text.Trim() == "")
            {
                MessageBox.Show("Username dan Password wajib diisi!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // [FIX 3] Pakai Stored Procedure sp_Login
                    SqlCommand cmd = new SqlCommand("sp_Login", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@password", txtPassword.Text.Trim());

                    string role = cmd.ExecuteScalar()?.ToString();

                    if (role != null && role != "GAGAL")
                    {
                        MessageBox.Show("Selamat Datang, " + role + "!", "Login Berhasil",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // [FIX 4] Buka Form1, tutup Form2 dengan benar agar app tidak menggantung
                        Form1 utama = new Form1(role);
                        utama.FormClosed += (s, args) => this.Close(); // Tutup app kalau Form1 ditutup
                        utama.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Username atau Password salah!", "Login Gagal",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtPassword.Clear();
                        txtPassword.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koneksi database gagal:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}