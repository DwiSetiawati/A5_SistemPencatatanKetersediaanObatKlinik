CREATE DATABASE KlinikObatDB;
GO
USE KlinikObatDB;
GO

-- 1. Tabel Profil User
CREATE TABLE Users (
    id_user VARCHAR(10) PRIMARY KEY,
    nama_lengkap VARCHAR(100) NOT NULL,
    no_telp VARCHAR(15)
);

-- 2. Tabel Akun (Login)
CREATE TABLE Akun (
    id_akun VARCHAR(10) PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    password VARCHAR(50) NOT NULL,
    role VARCHAR(20), -- 'Petugas Farmasi' atau 'Pemilik'
    id_user VARCHAR(10) FOREIGN KEY REFERENCES Users(id_user)
);

-- 3. Tabel Obat (Data Master)
CREATE TABLE Obat (
    id_obat VARCHAR(10) PRIMARY KEY,
    nama_obat VARCHAR(100) NOT NULL,
    satuan VARCHAR(20),
    stok_total INT DEFAULT 0
);

-- ISI DATA AWAL (PENTING!)
INSERT INTO Users VALUES ('U01', 'Miranti', '081234567891');
INSERT INTO Users VALUES ('U02', 'Tiaa', '089876543219');
INSERT INTO Users VALUES ('U03', 'Riswanda', '083456789123');
INSERT INTO Akun VALUES ('A01', 'admin', 'admin123', 'Petugas Farmasi', 'U01');
INSERT INTO Akun VALUES ('A02', 'bos', 'bos123', 'Pemilik', 'U02');
INSERT INTO Akun VALUES ('A03', 'admin', 'admin123', 'Petugas Farmasi', 'U03');
INSERT INTO Obat VALUES ('OB01', 'Paracetamol', 'Tablet', 100);
INSERT INTO Obat VALUES ('OB02', 'Sanmol', 'Tablet', 100);
INSERT INTO Obat VALUES ('OB03', 'Polysilane', 'Sirup', 50);