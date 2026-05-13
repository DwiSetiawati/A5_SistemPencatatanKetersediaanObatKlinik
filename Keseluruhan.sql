-- ============================================================
--  UCP 2 - KlinikObatDB  |  SQL Server Script 
--  Isi: VIEW, Stored Procedures (diperkaya), SP Login Vulnerable
-- ============================================================

USE KlinikObatDB;
GO

-- ==================================================
-- DROP SEMUA SP & VIEW LAMA (agar bisa dijalankan ulang)
-- ==================================================
IF OBJECT_ID('sp_Login_Vulnerable','P')      IS NOT NULL DROP PROCEDURE sp_Login_Vulnerable;
IF OBJECT_ID('sp_Login','P')                 IS NOT NULL DROP PROCEDURE sp_Login;
IF OBJECT_ID('sp_TambahObat','P')            IS NOT NULL DROP PROCEDURE sp_TambahObat;
IF OBJECT_ID('sp_UpdateObat','P')            IS NOT NULL DROP PROCEDURE sp_UpdateObat;
IF OBJECT_ID('sp_HapusObat','P')             IS NOT NULL DROP PROCEDURE sp_HapusObat;
IF OBJECT_ID('sp_CariObat','P')              IS NOT NULL DROP PROCEDURE sp_CariObat;
IF OBJECT_ID('sp_BackupDataObat','P')        IS NOT NULL DROP PROCEDURE sp_BackupDataObat;
IF OBJECT_ID('v_DataObat','V')               IS NOT NULL DROP VIEW v_DataObat;
IF OBJECT_ID('v_RiwayatStok','V')            IS NOT NULL DROP VIEW v_RiwayatStok;
IF OBJECT_ID('Backup_Obat','U')              IS NOT NULL DROP TABLE Backup_Obat;
GO

-- ==================================================
-- TAMBAH KOLOM jenis_obat jika belum ada
-- ==================================================
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME='Obat' AND COLUMN_NAME='jenis_obat'
)
BEGIN
    ALTER TABLE Obat ADD jenis_obat VARCHAR(50);
    UPDATE Obat SET jenis_obat = 'Umum' WHERE jenis_obat IS NULL;
END
GO

-- ==================================================
-- TABEL BACKUP OBAT (untuk SP Backup)
-- ==================================================
CREATE TABLE Backup_Obat (
    id_backup    INT IDENTITY(1,1) PRIMARY KEY,
    id_obat      VARCHAR(10),
    nama_obat    VARCHAR(100),
    jenis_obat   VARCHAR(50),
    satuan       VARCHAR(20),
    stok_total   INT,
    tanggal_backup DATETIME DEFAULT GETDATE(),
    backup_oleh  VARCHAR(50)
);
GO

-- ============================================================
--  VIEW 1 : v_DataObat  (digunakan di Form1 DataGridView)
-- ============================================================
CREATE VIEW v_DataObat AS
SELECT 
    o.id_obat,
    o.nama_obat,
    o.jenis_obat,
    o.satuan,
    o.stok_total,
    CASE 
        WHEN o.stok_total = 0      THEN 'Habis'
        WHEN o.stok_total < 20     THEN 'Hampir Habis'
        ELSE                            'Tersedia'
    END AS status_stok
FROM Obat o;
GO

-- ============================================================
--  VIEW 2 : v_RiwayatStok  (opsional, untuk laporan)
-- ============================================================
CREATE VIEW v_RiwayatStok AS
SELECT 
    rs.id_riwayat,
    rs.tanggal,
    o.nama_obat,
    a.username,
    rs.jenis_transaksi,
    rs.jumlah,
    rs.keterangan
FROM Riwayat_Stok rs
JOIN Obat  o ON rs.id_obat  = o.id_obat
JOIN Akun  a ON rs.id_akun  = a.id_akun;
GO

-- ============================================================
--  SP 1 : sp_Login  (AMAN - pakai parameterized)
-- ============================================================
CREATE PROCEDURE sp_Login
    @username VARCHAR(50),
    @password VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Cek apakah username ada dulu
    IF NOT EXISTS (SELECT 1 FROM Akun WHERE username = @username)
    BEGIN
        SELECT 'GAGAL' AS role;
        RETURN;
    END

    -- Cek password
    IF EXISTS (SELECT 1 FROM Akun WHERE username = @username AND password = @password)
    BEGIN
        -- Catat waktu login terakhir (opsional kolom last_login jika ada)
        SELECT a.role, u.nama_lengkap
        FROM Akun a
        JOIN Users u ON a.id_user = u.id_user
        WHERE a.username = @username AND a.password = @password;
    END
    ELSE
    BEGIN
        SELECT 'GAGAL' AS role;
    END
END
GO

-- ============================================================
--  SP 2 : sp_Login_Vulnerable  (SENGAJA VULNERABLE - Demo SQL Injection)
--  !! JANGAN DIPAKAI DI PRODUKSI !!
-- ============================================================
CREATE PROCEDURE sp_Login_Vulnerable
    @username VARCHAR(100),
    @password VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    -- QUERY BERBAHAYA: string langsung digabung tanpa sanitasi
    DECLARE @sql NVARCHAR(500);
    SET @sql = N'SELECT role FROM Akun WHERE username = ''' + @username 
             + N''' AND password = ''' + @password + N'''';
    EXEC sp_executesql @sql;
END
GO

-- ============================================================
--  SP 3 : sp_TambahObat  (diperkaya: validasi + catat riwayat)
-- ============================================================
CREATE PROCEDURE sp_TambahObat
    @id_obat    VARCHAR(10),
    @nama_obat  VARCHAR(100),
    @jenis_obat VARCHAR(50),
    @satuan     VARCHAR(20),
    @stok_total INT,
    @id_akun    VARCHAR(10) = NULL   -- untuk catat riwayat
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Validasi duplikat ID
        IF EXISTS (SELECT 1 FROM Obat WHERE id_obat = @id_obat)
            THROW 50001, 'ID Obat sudah terdaftar!', 1;

        -- Validasi duplikat nama
        IF EXISTS (SELECT 1 FROM Obat WHERE nama_obat = @nama_obat)
            THROW 50002, 'Nama Obat sudah terdaftar!', 1;

        -- Validasi stok
        IF @stok_total < 0
            THROW 50003, 'Stok tidak boleh negatif!', 1;

        -- Insert obat baru
        INSERT INTO Obat (id_obat, nama_obat, jenis_obat, satuan, stok_total)
        VALUES (@id_obat, @nama_obat, @jenis_obat, @satuan, @stok_total);

        -- Catat ke riwayat jika id_akun diberikan
        IF @id_akun IS NOT NULL AND @stok_total > 0
        BEGIN
            DECLARE @id_riwayat VARCHAR(10);
            SET @id_riwayat = 'RW' + RIGHT('000000' + CAST(
                (SELECT ISNULL(MAX(CAST(SUBSTRING(id_riwayat,3,10) AS INT)),0)+1
                 FROM Riwayat_Stok), 4), 4);

            INSERT INTO Riwayat_Stok (id_riwayat, id_obat, id_akun, jenis_transaksi, jumlah, keterangan)
            VALUES (@id_riwayat, @id_obat, @id_akun, 'MASUK', @stok_total, 'Stok awal saat penambahan obat baru');
        END

        COMMIT TRANSACTION;
        SELECT 'OK' AS hasil, 'Obat berhasil ditambahkan.' AS pesan;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================
--  SP 4 : sp_UpdateObat  (diperkaya: log perubahan stok)
-- ============================================================
CREATE PROCEDURE sp_UpdateObat
    @id_obat    VARCHAR(10),
    @nama_obat  VARCHAR(100),
    @jenis_obat VARCHAR(50),
    @satuan     VARCHAR(20),
    @stok_total INT,
    @id_akun    VARCHAR(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Obat WHERE id_obat = @id_obat)
            THROW 50004, 'ID Obat tidak ditemukan!', 1;

        IF @stok_total < 0
            THROW 50003, 'Stok tidak boleh negatif!', 1;

        -- Hitung selisih stok untuk riwayat
        DECLARE @stok_lama INT;
        SELECT @stok_lama = stok_total FROM Obat WHERE id_obat = @id_obat;

        UPDATE Obat 
        SET nama_obat  = @nama_obat,
            jenis_obat = @jenis_obat,
            satuan     = @satuan,
            stok_total = @stok_total
        WHERE id_obat = @id_obat;

        -- Catat riwayat jika stok berubah
        IF @id_akun IS NOT NULL AND @stok_total <> @stok_lama
        BEGIN
            DECLARE @selisih INT = @stok_total - @stok_lama;
            DECLARE @jenis   VARCHAR(10) = CASE WHEN @selisih > 0 THEN 'MASUK' ELSE 'KELUAR' END;
            DECLARE @id_rw   VARCHAR(10);
            SET @id_rw = 'RW' + RIGHT('0000' + CAST(
                (SELECT ISNULL(MAX(CAST(SUBSTRING(id_riwayat,3,10) AS INT)),0)+1
                 FROM Riwayat_Stok), 4), 4);

            INSERT INTO Riwayat_Stok (id_riwayat, id_obat, id_akun, jenis_transaksi, jumlah, keterangan)
            VALUES (@id_rw, @id_obat, @id_akun, @jenis, ABS(@selisih),
                    'Update stok dari ' + CAST(@stok_lama AS VARCHAR) + ' ke ' + CAST(@stok_total AS VARCHAR));
        END

        COMMIT TRANSACTION;
        SELECT 'OK' AS hasil, 'Data obat berhasil diperbarui.' AS pesan;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================
--  SP 5 : sp_HapusObat  (diperkaya: cek riwayat transaksi)
-- ============================================================
CREATE PROCEDURE sp_HapusObat
    @id_obat VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Obat WHERE id_obat = @id_obat)
            THROW 50004, 'ID Obat tidak ditemukan!', 1;

        -- Cek stok
        DECLARE @stok INT;
        SELECT @stok = stok_total FROM Obat WHERE id_obat = @id_obat;
        IF @stok > 0
            THROW 50005, 'Obat masih memiliki stok, tidak bisa dihapus!', 1;

        -- Hapus riwayat dulu (jika ada), baru hapus obat
        DELETE FROM Riwayat_Stok WHERE id_obat = @id_obat;
        DELETE FROM Obat WHERE id_obat = @id_obat;

        COMMIT TRANSACTION;
        SELECT 'OK' AS hasil, 'Obat berhasil dihapus.' AS pesan;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================
--  SP 6 : sp_CariObat  (diperkaya: bisa filter by jenis & status)
-- ============================================================
CREATE PROCEDURE sp_CariObat
    @keyword    VARCHAR(100) = '',
    @jenis_obat VARCHAR(50)  = '',
    @status     VARCHAR(20)  = ''
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM v_DataObat
    WHERE 
        -- Filter keyword (nama atau satuan)
        (
            @keyword = '' OR @keyword IS NULL OR
            nama_obat LIKE '%' + @keyword + '%' OR
            satuan    LIKE '%' + @keyword + '%' OR
            id_obat   LIKE '%' + @keyword + '%'
        )
        AND
        -- Filter jenis obat
        (
            @jenis_obat = '' OR @jenis_obat IS NULL OR
            jenis_obat = @jenis_obat
        )
        AND
        -- Filter status stok
        (
            @status = '' OR @status IS NULL OR
            CASE 
                WHEN stok_total = 0  THEN 'Habis'
                WHEN stok_total < 20 THEN 'Hampir Habis'
                ELSE 'Tersedia'
            END = @status
        )
    ORDER BY nama_obat;
END
GO

-- ============================================================
--  SP 7 : sp_BackupDataObat  (SP baru - fitur backup ke tabel)
-- ============================================================
CREATE PROCEDURE sp_BackupDataObat
    @backup_oleh VARCHAR(50) = 'System'
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO Backup_Obat (id_obat, nama_obat, jenis_obat, satuan, stok_total, backup_oleh)
        SELECT id_obat, nama_obat, jenis_obat, satuan, stok_total, @backup_oleh
        FROM Obat;

        DECLARE @jumlah INT = @@ROWCOUNT;
        COMMIT TRANSACTION;
        SELECT 'OK' AS hasil, 
               CAST(@jumlah AS VARCHAR) + ' data obat berhasil dibackup pada ' 
               + CONVERT(VARCHAR, GETDATE(), 120) AS pesan;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ==================================================
-- VERIFIKASI semua objek
	-- ==================================================
	SELECT name, type_desc 
	FROM sys.objects 
	WHERE type IN ('P','V')
	  AND (name LIKE 'sp_%' OR name LIKE 'v_%')
	ORDER BY type_desc, name;
	GO