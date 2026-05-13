CREATE DATABASE KlinikObatDB;
GO
USE KlinikObatDB;
GO

CREATE TABLE Users (
    id_user VARCHAR(10) PRIMARY KEY,
    nama_lengkap VARCHAR(100) NOT NULL,
    no_telp VARCHAR(15)
);

CREATE TABLE Akun (
    id_akun VARCHAR(10) PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,  -- tambah UNIQUE
    password VARCHAR(255) NOT NULL,         -- diperbesar
    role VARCHAR(20),
    id_user VARCHAR(10) FOREIGN KEY REFERENCES Users(id_user)
);

CREATE TABLE Obat (
    id_obat VARCHAR(10) PRIMARY KEY,
    nama_obat VARCHAR(100) NOT NULL,
    jenis_obat VARCHAR(50),                 -- tambah jenis
    satuan VARCHAR(20),
    stok_total INT DEFAULT 0
);

-- TAMBAHAN: Tabel Riwayat_Stok
CREATE TABLE Riwayat_Stok (
    id_riwayat VARCHAR(10) PRIMARY KEY,
    id_obat VARCHAR(10) FOREIGN KEY REFERENCES Obat(id_obat),
    id_akun VARCHAR(10) FOREIGN KEY REFERENCES Akun(id_akun),
    jenis_transaksi VARCHAR(10),
    jumlah INT NOT NULL,
    tanggal DATETIME DEFAULT GETDATE(),
    keterangan VARCHAR(200)
);

-- Data awal
INSERT INTO Users VALUES ('U01', 'Miranti', '081234567891');
INSERT INTO Users VALUES ('U02', 'Tiaa', '089876543219');
INSERT INTO Users VALUES ('U03', 'Riswanda', '083456789123');

INSERT INTO Akun VALUES ('A01', 'miranti', 'admin123', 'Petugas Farmasi', 'U01');
INSERT INTO Akun VALUES ('A02', 'bos', 'bos123', 'Pemilik', 'U02');
INSERT INTO Akun VALUES ('A03', 'riswanda', 'admin123', 'Petugas Farmasi', 'U03');

INSERT INTO Obat VALUES ('OB01', 'Paracetamol', 'Analgesik', 'Tablet', 100);
INSERT INTO Obat VALUES ('OB02', 'Sanmol', 'Analgesik', 'Tablet', 100);
INSERT INTO Obat VALUES ('OB03', 'Polysilane', 'Antasida', 'Sirup', 50);