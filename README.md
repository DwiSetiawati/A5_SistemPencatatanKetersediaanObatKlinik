<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/76e26a54-d504-4a3f-a269-3809b2b2c75f" /><img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/21005363-b6a8-4deb-90be-f4b4a4b3f347" />Form koneksi : <img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/d0ada260-bee0-4cbd-abe7-51ae58a632fd" />
              <img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/f076d6bb-a0a0-46be-8a87-9e8c7fffe456" />

Form input data : <img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/a22ff45a-9ee5-4c1a-a473-07779ba776ff" />

Form tampilan data : <img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/93f78919-a49a-42d1-8ee4-0e6b0d709df5" />

Bukti insert, update, delete, dan search :
insert : <img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/f18b0efa-12a4-4259-9470-fd18e174994d" />
update : <img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/4f4f1c43-f1bc-4fe7-8d41-2195ddcbfe28" />
delete : <img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/f18e020d-15d4-4368-847e-a8577ba02072" />
search : <img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/1d63ffd9-dc80-4efa-a24c-7f731b8d7e4c" />

# Sistem Pencatatan Ketersediaan Obat Klinik

## Teknologi
- C# Windows Forms (.NET Framework)
- SQL Server
- ADO.NET

## Fitur
- CRUD Obat menggunakan Stored Procedure
- Tampil data menggunakan VIEW v_DataObat
- Binding DataGridView dengan BindingSource
- BindingNavigator untuk navigasi data
- Backup data ke tabel Backup_Obat

## Cara Menjalankan
1. Buka SQL Server Management Studio (SSMS)
2. Jalankan file `database.sql`
3. Buka project di Visual Studio 2022
4. Sesuaikan connection string di `Form1.cs`
5. Jalankan aplikasi (F5)

## Struktur Database
- Tabel: Obat, Akun, Users, Riwayat_Stok, Backup_Obat
- View: v_DataObat, v_RiwayatStok
- Stored Procedure: sp_Login, sp_TambahObat, sp_UpdateObat,
  sp_HapusObat, sp_CariObat, sp_BackupDataObat

## Keamanan: SQL Injection — Skenario & Pencegahan

### Skenario Serangan
Query rentan (raw string):
```sql
SELECT * FROM Akun WHERE username = '' OR '1'='1'
```
Input `' OR '1'='1` membuat kondisi selalu TRUE → bypass login tanpa password.

### Pencegahan yang Diterapkan
Aplikasi menggunakan parameterized query di seluruh operasi database:
```csharp
SqlCommand cmd = new SqlCommand("sp_TambahObat", con);
cmd.CommandType = CommandType.StoredProcedure;
cmd.Parameters.AddWithValue("@nama_obat", txtNama.Text.Trim());
```
Input user diperlakukan sebagai data, bukan bagian perintah SQL.
