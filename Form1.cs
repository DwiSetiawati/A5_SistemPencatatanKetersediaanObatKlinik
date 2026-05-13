using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ObatKlinikADO
{
    public partial class Form1 : Form
    {
        string connectionString = "Data Source=RISWANDA\\WANDA_PUTRA;Initial Catalog=KlinikObatDB;Integrated Security=True";
        SqlConnection con;
        string roleLogin;
        string usernameLogin;

        // ── DATA BINDING ──────────────────────────────────────────────
        // BindingSource menjadi "jembatan" antara DataTable dan DataGridView
        // serta BindingNavigator untuk navigasi baris
        private BindingSource bindingSource = new BindingSource();
        private DataTable dtObat = new DataTable();

        public Form1(string role, string username = "")
        {
            InitializeComponent();
            this.roleLogin = role;
            this.usernameLogin = username;
            con = new SqlConnection(connectionString);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Hubungkan BindingNavigator ke BindingSource yang sama
            bindingNavigator1.BindingSource = bindingSource;

            // Hubungkan DataGridView ke BindingSource (bukan langsung ke DataTable)
            dgvObat.DataSource = bindingSource;

            // Daftarkan event PositionChanged agar form input ikut terisi saat navigasi
            bindingSource.PositionChanged += new EventHandler(bindingSource_PositionChanged);

            TampilkanData();

            // Hak akses role
            if (roleLogin == "Pemilik")
            {
                btnTambah.Enabled = false;
                btnHapus.Enabled = false;
                btnUpdate.Enabled = false;
                btnBackup.Enabled = false;
                this.Text = "Sistem Pencatatan Obat Klinik — Mode Monitoring (Pemilik)";
            }
            else
            {
                this.Text = "Sistem Pencatatan Obat Klinik — " + roleLogin;
            }
        }

        // ── TAMPILKAN DATA via VIEW v_DataObat ────────────────────────
        void TampilkanData()
        {
            try
            {
                if (con.State == ConnectionState.Open) con.Close();
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT * FROM v_DataObat", con);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                dtObat.Clear();
                adapter.Fill(dtObat);

                // Set BindingSource dari DataTable
                // BindingNavigator otomatis update karena sudah terhubung ke bindingSource
                bindingSource.DataSource = dtObat;

                con.Close();

                // Hitung total
                con.Open();
                SqlCommand cmdCount = new SqlCommand("SELECT COUNT(*) FROM Obat", con);
                lblTotal.Text = "Total Jenis Obat: " + cmdCount.ExecuteScalar().ToString();
                con.Close();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open) con.Close();
                MessageBox.Show("Gagal memuat data:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── KLIK BARIS DGV → isi form input ──────────────────────────
        private void dgvObat_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvObat.Rows[e.RowIndex];
                txtID.Text = row.Cells["id_obat"].Value?.ToString() ?? "";
                txtNama.Text = row.Cells["nama_obat"].Value?.ToString() ?? "";
                txtJenis.Text = row.Cells["jenis_obat"].Value?.ToString() ?? "";
                txtSatuan.Text = row.Cells["satuan"].Value?.ToString() ?? "";
                txtStok.Text = row.Cells["stok_total"].Value?.ToString() ?? "";
            }
        }

        // ── SAAT BINDING NAVIGATOR PINDAH POSISI → isi form input ────
        private void bindingSource_PositionChanged(object sender, EventArgs e)
        {
            if (bindingSource.Current is DataRowView drv)
            {
                txtID.Text = drv["id_obat"]?.ToString() ?? "";
                txtNama.Text = drv["nama_obat"]?.ToString() ?? "";
                txtJenis.Text = drv["jenis_obat"]?.ToString() ?? "";
                txtSatuan.Text = drv["satuan"]?.ToString() ?? "";
                txtStok.Text = drv["stok_total"]?.ToString() ?? "";
            }
        }

        // ── TAMBAH ────────────────────────────────────────────────────
        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (txtID.Text.Trim() == "" || txtNama.Text.Trim() == "")
            {
                MessageBox.Show("ID dan Nama Obat wajib diisi!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtStok.Text, out int stok) || stok < 0)
            {
                MessageBox.Show("Stok harus berupa angka dan tidak boleh negatif!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (con.State == ConnectionState.Open) con.Close();
                con.Open();

                SqlCommand cmd = new SqlCommand("sp_TambahObat", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_obat", txtID.Text.Trim());
                cmd.Parameters.AddWithValue("@nama_obat", txtNama.Text.Trim());
                cmd.Parameters.AddWithValue("@jenis_obat", txtJenis.Text.Trim());
                cmd.Parameters.AddWithValue("@satuan", txtSatuan.Text.Trim());
                cmd.Parameters.AddWithValue("@stok_total", stok);
                cmd.Parameters.AddWithValue("@id_akun", GetIdAkun());
                cmd.ExecuteNonQuery();
                con.Close();

                MessageBox.Show("Data obat berhasil ditambahkan!", "Sukses",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                TampilkanData();
                BersihkanInput();
            }
            catch (SqlException ex)
            {
                if (con.State == ConnectionState.Open) con.Close();
                MessageBox.Show(ex.Message, "Gagal Tambah",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── UPDATE ────────────────────────────────────────────────────
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (txtID.Text.Trim() == "")
            {
                MessageBox.Show("Pilih data dari tabel terlebih dahulu!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtStok.Text, out int stok) || stok < 0)
            {
                MessageBox.Show("Stok harus berupa angka dan tidak boleh negatif!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Yakin ingin mengubah data obat ini?", "Konfirmasi",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (con.State == ConnectionState.Open) con.Close();
                    con.Open();

                    SqlCommand cmd = new SqlCommand("sp_UpdateObat", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_obat", txtID.Text.Trim());
                    cmd.Parameters.AddWithValue("@nama_obat", txtNama.Text.Trim());
                    cmd.Parameters.AddWithValue("@jenis_obat", txtJenis.Text.Trim());
                    cmd.Parameters.AddWithValue("@satuan", txtSatuan.Text.Trim());
                    cmd.Parameters.AddWithValue("@stok_total", stok);
                    cmd.Parameters.AddWithValue("@id_akun", GetIdAkun());
                    cmd.ExecuteNonQuery();
                    con.Close();

                    MessageBox.Show("Data berhasil diperbarui!", "Sukses",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    TampilkanData();
                    BersihkanInput();
                }
                catch (SqlException ex)
                {
                    if (con.State == ConnectionState.Open) con.Close();
                    MessageBox.Show(ex.Message, "Gagal Update",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── HAPUS ─────────────────────────────────────────────────────
        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (txtID.Text.Trim() == "")
            {
                MessageBox.Show("Pilih data dari tabel terlebih dahulu!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Yakin hapus obat ini?", "Konfirmasi",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (con.State == ConnectionState.Open) con.Close();
                    con.Open();

                    SqlCommand cmd = new SqlCommand("sp_HapusObat", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_obat", txtID.Text.Trim());
                    cmd.ExecuteNonQuery();
                    con.Close();

                    MessageBox.Show("Data berhasil dihapus!", "Sukses",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    TampilkanData();
                    BersihkanInput();
                }
                catch (SqlException ex)
                {
                    if (con.State == ConnectionState.Open) con.Close();
                    MessageBox.Show(ex.Message, "Gagal Hapus",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── CARI ──────────────────────────────────────────────────────
        private void btnCari_Click(object sender, EventArgs e)
        {
            try
            {
                if (con.State == ConnectionState.Open) con.Close();
                con.Open();

                SqlCommand cmd = new SqlCommand("sp_CariObat", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@keyword", txtCari.Text.Trim());
                cmd.Parameters.AddWithValue("@jenis_obat", "");
                cmd.Parameters.AddWithValue("@status", "");

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                dtObat.Clear();
                adapter.Fill(dtObat);
                bindingSource.DataSource = dtObat;
                lblTotal.Text = "Hasil Pencarian: " + dtObat.Rows.Count + " data";
                con.Close();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open) con.Close();
                MessageBox.Show("Gagal mencari data:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── TAMPILKAN (refresh) ───────────────────────────────────────
        private void btnTampilkan_Click(object sender, EventArgs e)
        {
            TampilkanData();
            BersihkanInput();
        }

        // ── BACKUP DATA ───────────────────────────────────────────────
        private void btnBackup_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Backup semua data obat sekarang?", "Konfirmasi Backup",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (con.State == ConnectionState.Open) con.Close();
                    con.Open();

                    SqlCommand cmd = new SqlCommand("sp_BackupDataObat", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@backup_oleh", usernameLogin);

                    SqlDataReader reader = cmd.ExecuteReader();
                    string pesan = "";
                    if (reader.Read()) pesan = reader["pesan"].ToString();
                    reader.Close();
                    con.Close();

                    MessageBox.Show(pesan, "Backup Berhasil",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    if (con.State == ConnectionState.Open) con.Close();
                    MessageBox.Show("Gagal backup:\n" + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── DEMO SQL INJECTION ────────────────────────────────────────
        // Menampilkan penjelasan skenario SQL Injection
        // (FormSQLInjection terpisah — lihat file FormSQLInjection.cs)
        private void btnDemoInjection_Click(object sender, EventArgs e)
        {
            string pesan =
                "=== DEMO SQL INJECTION ===\n\n" +
                "Query RENTAN (raw string):\n" +
                "  SELECT * FROM Akun WHERE username = '" + "' OR '1'='1" + "'\n\n" +
                "Hasil: semua data akun terbaca → bypass login!\n\n" +
                "Query AMAN (parameterized):\n" +
                "  cmd.Parameters.AddWithValue(\"@u\", txtUser.Text)\n\n" +
                "Input user TIDAK dieksekusi sebagai SQL.\n" +
                "Lihat detail skenario di README GitHub.";

            MessageBox.Show(pesan, "Demo SQL Injection",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── LOGOUT ────────────────────────────────────────────────────
        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Yakin ingin logout?", "Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Form2 login = new Form2();
                login.Show();
                this.Close();
            }
        }

        // ── HELPERS ───────────────────────────────────────────────────
        void BersihkanInput()
        {
            txtID.Clear();
            txtNama.Clear();
            txtJenis.Clear();
            txtSatuan.Clear();
            txtStok.Clear();
        }

        string GetIdAkun()
        {
            try
            {
                if (con.State == ConnectionState.Open) con.Close();
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT id_akun FROM Akun WHERE username = @u", con);
                cmd.Parameters.AddWithValue("@u", usernameLogin);
                object result = cmd.ExecuteScalar();
                con.Close();
                return result?.ToString() ?? "";
            }
            catch { if (con.State == ConnectionState.Open) con.Close(); return ""; }
        }

        private void txtID_TextChanged(object sender, EventArgs e) { }
    }
}