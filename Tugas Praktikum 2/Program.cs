using System;
using System.Collections.Generic;

public interface IKemampuan
{
    string Nama { get; }
    void Aktifkan(Robot pengguna, Robot target);
    bool SedangCooldown { get; }
    void PerbaruiCooldown();
}

public abstract class Robot
{
    public string Nama { get; protected set; }
    public int Energi { get; protected set; }
    public int Armor { get; protected set; }
    public int Serangan { get; protected set; }
    protected List<IKemampuan> Kemampuan;
    private bool SuperShieldAktif = false;

    protected Robot(string nama, int energi, int armor, int serangan)
    {
        Nama = nama;
        Energi = energi;
        Armor = armor;
        Serangan = serangan;
        Kemampuan = new List<IKemampuan>();
    }

    public virtual void Serang(Robot target)
    {
        int damage = Math.Max(0, Serangan - target.Armor);
        target.TerimaSerangan(damage);
        Console.WriteLine($"{Nama} menyerang {target.Nama} dan menyebabkan {damage} kerusakan!");
    }

    public virtual void TerimaSerangan(int damage)
    {
        if (SuperShieldAktif)
        {
            damage /= 2;
            Console.WriteLine($"Super Shield mengurangi damage menjadi {damage}!");
        }
        UbahEnergi(-damage);
        if (Energi <= 0)
        {
            Console.WriteLine($"{Nama} telah dikalahkan!");
        }
    }

    public virtual void GunakanKemampuan(Robot target)
    {
        Console.WriteLine($"Kemampuan yang tersedia untuk {Nama}:");
        for (int i = 0; i < Kemampuan.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {Kemampuan[i].Nama} {(Kemampuan[i].SedangCooldown ? "(Cooldown)" : "")}");
        }

        Console.Write("Pilih kemampuan (0 untuk batal): ");
        if (int.TryParse(Console.ReadLine(), out int pilihan) && pilihan > 0 && pilihan <= Kemampuan.Count)
        {
            IKemampuan kemampuanPilihan = Kemampuan[pilihan - 1];
            if (!kemampuanPilihan.SedangCooldown)
            {
                kemampuanPilihan.Aktifkan(this, target);
            }
            else
            {
                Console.WriteLine("Kemampuan sedang dalam cooldown!");
            }
        }
        else
        {
            Console.WriteLine("Membatalkan penggunaan kemampuan.");
        }
    }

    public void PerbaruiCooldown()
    {
        foreach (var kemampuan in Kemampuan)
        {
            kemampuan.PerbaruiCooldown();
        }
    }

    public virtual void CetakInformasi()
    {
        Console.WriteLine($"{Nama} - Energi: {Energi}, Armor: {Armor}, Serangan: {Serangan}");
    }

    public void AktifkanSuperShield()
    {
        SuperShieldAktif = true;
    }

    public void NonaktifkanSuperShield()
    {
        SuperShieldAktif = false;
    }

    public void UbahEnergi(int jumlah)
    {
        Energi += jumlah;
        if (Energi < 0) Energi = 0;
    }
}

// Robot Penyerang, Pertahanan, Pendukung, Penembak Jitu, Penyembuh dan Bos
public class RobotPenyerang : Robot
{
    public RobotPenyerang(string nama) : base(nama, 100, 5, 25)
    {
        Kemampuan.Add(new PlasmaCannon());
        Kemampuan.Add(new ElectricShock());
    }
}

public class RobotPertahanan : Robot
{
    public RobotPertahanan(string nama) : base(nama, 150, 15, 15)
    {
        Kemampuan.Add(new SuperShield());
        Kemampuan.Add(new Repair());
    }
}

public class RobotPendukung : Robot
{
    public RobotPendukung(string nama) : base(nama, 120, 8, 18)
    {
        Kemampuan.Add(new Repair());
        Kemampuan.Add(new ElectricShock());
    }
}
public class RobotPenembakJitu : Robot
{
    public RobotPenembakJitu(string nama) : base(nama, 90, 8, 40)
    {
        Kemampuan.Add(new SniperShot());
    }
}
public class RobotPenyembuh : Robot
{
    public RobotPenyembuh(string nama) : base(nama, 100, 10, 10)
    {
        Kemampuan.Add(new Heal());
    }
}

public class BosRobot : Robot
{
    private int TurnStun = 0;

    public BosRobot(string nama) : base(nama, 300, 20, 40)
    {
        Kemampuan.Add(new PlasmaCannon());
        Kemampuan.Add(new SuperShield());
    }

    public override void Serang(Robot target)
    {
        if (TurnStun > 0)
        {
            Console.WriteLine($"{Nama} masih terkena stun dan tidak dapat menyerang!");
            TurnStun--;
            return;
        }
        base.Serang(target);
    }

    public void TerimaBantingan(int turnStun)
    {
        TurnStun = turnStun;
        Console.WriteLine($"{Nama} terbanting dan tidak bisa bergerak selama {turnStun} giliran!");
    }

    public override void CetakInformasi()
    {
        base.CetakInformasi();
        if (TurnStun > 0)
        {
            Console.WriteLine($"Terkena stun: {TurnStun} giliran tersisa");
        }
    }
}

// Kemampuan
public class Repair : IKemampuan
{
    public string Nama => "Repair";
    private int cooldown = 0;
    public bool SedangCooldown => cooldown > 0;

    public void Aktifkan(Robot pengguna, Robot target)
    {
        Console.WriteLine($"{pengguna.Nama} menggunakan Repair untuk memulihkan energi!");
        target.UbahEnergi(30);
        cooldown = 3;
    }

    public void PerbaruiCooldown()
    {
        if (cooldown > 0) cooldown--;
    }
}

public class ElectricShock : IKemampuan
{
    public string Nama => "Electric Shock";
    private int cooldown = 0;
    public bool SedangCooldown => cooldown > 0;

    public void Aktifkan(Robot pengguna, Robot target)
    {
        Console.WriteLine($"{pengguna.Nama} melancarkan Electric Shock!");
        int damage = pengguna.Serangan / 2;
        target.TerimaSerangan(damage);
        Console.WriteLine($"{target.Nama} terkena {damage} damage dan terganggu pergerakannya!");
        if (target is BosRobot bosRobot)
        {
            bosRobot.TerimaBantingan(1);
        }
        cooldown = 3;
    }

    public void PerbaruiCooldown()
    {
        if (cooldown > 0) cooldown--;
    }
}

public class PlasmaCannon : IKemampuan
{
    public string Nama => "Plasma Cannon";
    private int cooldown = 0;
    public bool SedangCooldown => cooldown > 0;

    public void Aktifkan(Robot pengguna, Robot target)
    {
        Console.WriteLine($"{pengguna.Nama} menembakkan Plasma Cannon!");
        int damage = pengguna.Serangan * 2;
        target.TerimaSerangan(damage);
        Console.WriteLine($"{target.Nama} terkena {damage} damage dari Plasma Cannon!");
        cooldown = 4;
    }

    public void PerbaruiCooldown()
    {
        if (cooldown > 0) cooldown--;
    }
}

public class SuperShield : IKemampuan
{
    public string Nama => "Super Shield";
    private int cooldown = 0;
    public bool SedangCooldown => cooldown > 0;
    private int durasiAktif = 0;

    public void Aktifkan(Robot pengguna, Robot target)
    {
        if (durasiAktif == 0)
        {
            Console.WriteLine($"{pengguna.Nama} mengaktifkan Super Shield!");
            pengguna.AktifkanSuperShield();
            durasiAktif = 2;
            cooldown = 4;
        }
        else
        {
            Console.WriteLine("Super Shield sudah aktif!");
        }
    }

    public void PerbaruiCooldown()
    {
        if (cooldown > 0) cooldown--;
        if (durasiAktif > 0)
        {
            durasiAktif--;
            if (durasiAktif == 0)
            {
                Console.WriteLine("Super Shield telah berakhir.");
            }
        }
    }
}

public class SniperShot : IKemampuan
{
    public string Nama => "Sniper Shot";
    private int cooldown = 0;
    public bool SedangCooldown => cooldown > 0;

    public void Aktifkan(Robot pengguna, Robot target)
    {
        Console.WriteLine($"{pengguna.Nama} menembakkan Sniper Shot!");
        int damage = pengguna.Serangan * 3; // Damage besar
        target.TerimaSerangan(damage);
        Console.WriteLine($"{target.Nama} terkena {damage} damage dari Sniper Shot!");
        cooldown = 5; // Cooldown yang lama
    }

    public void PerbaruiCooldown()
    {
        if (cooldown > 0) cooldown--;
    }
}

public class Heal : IKemampuan
{
    public string Nama => "Heal";
    private int cooldown = 0;
    public bool SedangCooldown => cooldown > 0;

    public void Aktifkan(Robot pengguna, Robot target)
    {
        Console.WriteLine($"{pengguna.Nama} menggunakan Heal untuk menyembuhkan {target.Nama}!");
        target.UbahEnergi(40); // Memulihkan energi target
        cooldown = 3; // Cooldown sedang
    }

    public void PerbaruiCooldown()
    {
        if (cooldown > 0) cooldown--;
    }
}

// Permainan
public class Permainan
{
    private List<Robot> TimRobot;
    private BosRobot Bos;

    public Permainan()
    {
        TimRobot = new List<Robot>();
        Bos = new BosRobot("Mega Boss");
    }

    public void MulaiPermainan()
    {
        Console.WriteLine("Selamat datang di Simulator Pertarungan Robot!");
        BuatTim();

        while (TimRobot.Count > 0 && Bos.Energi > 0)
        {
            GiliranTim();
            if (Bos.Energi <= 0) break;
            GiliranBos();
            TimRobot.RemoveAll(robot => robot.Energi <= 0);

            // Pemulihan energi di akhir setiap giliran
            PulihkanEnergi();
        }

        if (TimRobot.Count > 0)
        {
            Console.WriteLine("Selamat! Tim Robot berhasil mengalahkan Boss!");
        }
        else
        {
            Console.WriteLine("Game Over. Boss mengalahkan semua robot.");
        }
    }

    private void BuatTim()
    {
        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine($"\nPilih Robot ke-{i + 1}:");
            Console.WriteLine("1. Robot Penyerang");
            Console.WriteLine("2. Robot Pertahanan");
            Console.WriteLine("3. Robot Pendukung");
            Console.WriteLine("4. Robot Penembak Jitu");
            Console.WriteLine("5. Robot Penyembuh");

            int pilihan;
            while (int.TryParse(Console.ReadLine(), out pilihan) || pilihan < 1 || pilihan > 5)
            {
                Console.WriteLine("Pilihan tidak valid. Silakan coba lagi.");
            }

            Robot robot;
            switch (pilihan)
            {
                case 1:
                    robot = new RobotPenyerang($"Penyerang-{i + 1}");
                    break;
                case 2:
                    robot = new RobotPertahanan($"Pertahanan-{i + 1}");
                    break;
                case 3:
                    robot = new RobotPendukung($"Pendukung-{i + 1}");
                    break;
                case 4:
                    robot = new RobotPenembakJitu($"PenembakJitu-{i + 1}");
                    break;
                case 5:
                    robot = new RobotPenyembuh($"Penyembuh-{i + 1}");
                    break;
                default:
                    robot = new RobotPenyerang($"Penyerang-{i + 1}");
                    break;
            }
            TimRobot.Add(robot);
        }
    }

    private void GiliranTim()
    {
        foreach (var robot in TimRobot)
        {
            Console.WriteLine($"\nGiliran {robot.Nama}");
            robot.CetakInformasi();
            Bos.CetakInformasi();

            Console.WriteLine("Pilih tindakan:");
            Console.WriteLine("1. Serang Boss");
            Console.WriteLine("2. Gunakan Kemampuan");

            int pilihan;
            while (!int.TryParse(Console.ReadLine(), out pilihan) || pilihan < 1 || pilihan > 2)
            {
                Console.WriteLine("Pilihan tidak valid. Silakan coba lagi.");
            }

            switch (pilihan)
            {
                case 1:
                    robot.Serang(Bos);
                    break;
                case 2:
                    robot.GunakanKemampuan(Bos);
                    break;
            }

            robot.PerbaruiCooldown();
        }
    }

    private void GiliranBos()
    {
        Console.WriteLine("\nGiliran Boss");
        Bos.CetakInformasi();

        if (TimRobot.Count > 0)
        {
            Random rnd = new Random();
            Robot target = TimRobot[rnd.Next(TimRobot.Count)];
            Bos.Serang(target);
        }

        Bos.PerbaruiCooldown();
    }

    // Pemulihan energi setelah setiap giliran
    protected void PulihkanEnergi()
    {
        Console.WriteLine("\nMemulihkan energi untuk semua robot...");
        foreach (var robot in TimRobot)
        {
            robot.UbahEnergi(10);
        }
        Bos.UbahEnergi(10);
    }
}

// Main Program
public class Program
{
    public static void Main(string[] args)
    {
        Permainan permainan = new Permainan();
        permainan.MulaiPermainan();
    }
}
