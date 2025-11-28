using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace APP_QLHocPhi.Models;

public partial class QuanLiHocPhiContext : DbContext
{
    // Constructor mặc định đơn giản, không cấu hình cứng ở đây nữa
    public QuanLiHocPhiContext()
    {
    }

    // Constructor cho phép truyền options từ bên ngoài (ví dụ từ Dependency Injection)
    public QuanLiHocPhiContext(DbContextOptions<QuanLiHocPhiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Invoice> Invoices { get; set; }
    public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }
    public virtual DbSet<Student> Students { get; set; }
    public virtual DbSet<StudentRegistration> StudentRegistrations { get; set; }
    public virtual DbSet<Subject> Subjects { get; set; }
    public virtual DbSet<TutitionConfig> TutitionConfigs { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Kiểm tra xem đã có cấu hình chưa, nếu chưa thì mới thiết lập
        if (!optionsBuilder.IsConfigured)
        {
            // Thiết lập đường dẫn tới file database .mdf
            // |DataDirectory| sẽ trỏ tới thư mục bin/Debug/net8.0-windows/Database khi chạy
            string connectionString = @"Server=(localdb)\mssqllocaldb;AttachDbFileName=|DataDirectory|\Database\QuanLiHocPhiDB.mdf;Trusted_Connection=True;MultipleActiveResultSets=true";

            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Invoice__3214EC077FF63E7D");

            entity.Property(e => e.NgayThu).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.HocKyNavigation).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoice__HocKy__47DBAE45");

            entity.HasOne(d => d.Student).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoice__Student__44FF419A");

            entity.HasOne(d => d.User).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoice__UserId__45F365D3");
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__InvoiceD__3214EC077F3AAEAE");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceDetails).HasConstraintName("FK__InvoiceDe__Invoi__4AB81AF0");

            entity.HasOne(d => d.Registration).WithMany(p => p.InvoiceDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvoiceDe__Regis__4BAC3F29");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Student__3214EC070A717F0B");
        });

        modelBuilder.Entity<StudentRegistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudentR__3214EC078B24E70C");

            entity.HasOne(d => d.HocKyNavigation).WithMany(p => p.StudentRegistrations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentRe__HocKy__412EB0B6");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentRegistrations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentRe__Stude__3F466844");

            entity.HasOne(d => d.Subject).WithMany(p => p.StudentRegistrations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentRe__Subje__403A8C7D");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subject__3214EC0729A6F555");
        });

        modelBuilder.Entity<TutitionConfig>(entity =>
        {
            entity.HasKey(e => e.HocKy).HasName("PK__Tutition__2BB032E4EA9F1DBB");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC0780291E43");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}