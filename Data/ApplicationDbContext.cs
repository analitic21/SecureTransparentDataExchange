using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Billing;
using SecureTransparentDataExchange.Models.Feedback;
using SecureTransparentDataExchange.Models.identity;
using SecureTransparentDataExchange.Models.Location;
using SecureTransparentDataExchange.Models.Orders;
using SecureTransparentDataExchange.Models.Shipping;


namespace SecureTransparentDataExchange.Data
{
    public class ApplicationDbContext : IdentityDbContext<Login, Role, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ================= DBSETS =================

        public DbSet<AppUser> AppUsers { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<Feedback> Feedbacks { get; set; } = null!;
        public DbSet<SystemLog> SystemLogs { get; set; } = null!;
        public DbSet<BlockchainLog> BlockchainLogs { get; set; } = null!;

        public DbSet<LoginAgreement> LoginAgreements { get; set; } = null!;
        public DbSet<LoginAgreementConsent> LoginAgreementConsents { get; set; } = null!;
        public DbSet<LoginAgreementArticle> LoginAgreementArticles { get; set; } = null!;

        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<LegalEntity> LegalEntities { get; set; } = null!;

        public DbSet<Shipment> Shipments { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Parcel> Parcels { get; set; } = null!;
        public DbSet<Package> Packages { get; set; } = null!;
        public DbSet<TrackingNumberEntity> TrackingNumbers { get; set; } = null!;

        public DbSet<CargoType> CargoTypes { get; set; } = null!;
        public DbSet<IoTDevice> IoTDevices { get; set; } = null!;

        public DbSet<DeliveryRoute> DeliveryRoutes { get; set; } = null!;
        public DbSet<DeliveryHistory> DeliveryHistories { get; set; } = null!;

        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<PaymentRefund> PaymentRefunds { get; set; } = null!;
        public DbSet<CardTransaction> CardTransactions { get; set; } = null!;

        public DbSet<City> Cities { get; set; } = null!;
        public DbSet<Country> Countries { get; set; } = null!;
        public DbSet<PostalCode> PostalCodes { get; set; } = null!;
        public DbSet<Address> Addresses { get; set; } = null!;

        public DbSet<EncryptionKey> EncryptionKeys { get; set; } = null!;
        public DbSet<JwtSetting> JwtSettings { get; set; } = null!;
        public DbSet<GeoFenceZone> GeoFenceZones { get; set; } = null!;

        public DbSet<TwoFactorAuthModel> TwoFactorAuthModels { get; set; } = null!;
        public DbSet<QRCodeModel> QRCodeModels { get; set; } = null!;

        // ================= MODEL CONFIG =================

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // ===== TABLE NAMES fixed) =====
            b.Entity<Login>().ToTable("Login");
            b.Entity<Role>().ToTable("Role");
            b.Entity<Order>().ToTable("Order");
            b.Entity<Shipment>().ToTable("Shipment");
            b.Entity<Parcel>().ToTable("Parcel");
            b.Entity<City>().ToTable("City");
            b.Entity<Country>().ToTable("Country");
            b.Entity<PostalCode>().ToTable("PostalCode");
            b.Entity<Address>().ToTable("Address");
            b.Entity<DeliveryRoute>().ToTable("DeliveryRoute");
            b.Entity<EncryptionKey>().ToTable("EncryptionKeys");
            b.Entity<JwtSetting>().ToTable("JwtSettings");
            b.Entity<GeoFenceZone>().ToTable("GeoFenceZones");

            // ================= LOGIN =================
            b.Entity<Login>(e =>
            {
                e.Property(x => x.Email).IsRequired().HasMaxLength(256);
                e.Property(x => x.UserName).IsRequired().HasMaxLength(256);

                // ✅ 3NF: только Address
                e.HasOne(x => x.Address)
                    .WithMany()
                    .HasForeignKey(x => x.AddressId)
                    .OnDelete(DeleteBehavior.Restrict);

                
            });

            // ================= CLIENT =================
            b.Entity<Client>(e =>
            {
                e.HasOne(x => x.Address)
                    .WithMany()
                    .HasForeignKey(x => x.AddressId)
                    .OnDelete(DeleteBehavior.Restrict);

        
            });

            // ================= COMPANY =================
            b.Entity<Company>(e =>
            {
                e.ToTable("Companies");

                e.HasOne(x => x.CompanyAddress)
                    .WithMany()
                    .HasForeignKey(x => x.AddressId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.LegalEntity)
                    .WithOne(x => x.Company)
                    .HasForeignKey<Company>(x => x.LegalEntityId)
                    .OnDelete(DeleteBehavior.Restrict);

                

                
            });

            // ================= LEGAL ENTITY =================
            b.Entity<LegalEntity>(e =>
            {
                e.ToTable("LegalEntities");

                e.HasOne(x => x.Login)
                    .WithMany()
                    .HasForeignKey(x => x.LoginId)
                    .OnDelete(DeleteBehavior.Restrict);

        

                e.Property(x => x.CompanyName)
                    .IsRequired()
                    .HasMaxLength(200);

                e.Property(x => x.ManualCity).HasMaxLength(100);
                e.Property(x => x.ManualCountry).HasMaxLength(100);
                e.Property(x => x.ManualPostalCode).HasMaxLength(20);
                e.Property(x => x.ManualAddress).HasMaxLength(200);
            });


            // ================= PAYMENTS =================
            b.Entity<Payment>(e =>
            {
                e.Property(p => p.Amount).HasPrecision(18, 2);

                e.HasOne(p => p.Shipment)
                    .WithMany(s => s.Payments)
                    .HasForeignKey(p => p.ShipmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.Invoice)
                    .WithMany(i => i.Payments)
                    .HasForeignKey(p => p.InvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            b.Entity<PaymentRefund>(e =>
            {
                e.HasOne(x => x.Payment)
                    .WithMany(p => p.Refunds)
                    .HasForeignKey(x => x.PaymentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================= SHIPMENT =================
            b.Entity<Shipment>()
                .HasOne(s => s.DeliveryRoute)
                .WithMany()
                .HasForeignKey(s => s.DeliveryRouteId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= PARCEL =================
            b.Entity<Parcel>()
                .HasOne(p => p.Shipment)
                .WithMany(s => s.Parcels)
                .HasForeignKey(p => p.ShipmentId)
                .OnDelete(DeleteBehavior.Restrict);
              b.Entity<Parcel>()
    .HasOne(p => p.CreatedByUser)
    .WithMany()
    .HasForeignKey(p => p.CreatedByUserId)
    .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Parcel>()
                .HasOne(p => p.UpdatedByUser)
                .WithMany()
                .HasForeignKey(p => p.UpdatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= IoT =================
            b.Entity<IoTDevice>()
                .HasOne(d => d.Parcel)
                .WithOne(p => p.IoTDevice)
                .HasForeignKey<IoTDevice>(d => d.ParcelId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= LOCATION =================
            b.Entity<City>()
                .HasOne(c => c.Country)
                .WithMany(cn => cn.Cities)
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<PostalCode>()
                .HasOne(pc => pc.City)
                .WithMany(c => c.PostalCodes)
                .HasForeignKey(pc => pc.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Address>(e =>
            {
                e.ToTable("Address");

                e.HasOne(a => a.PostalCode)
                    .WithMany(p => p.Address)
                    .HasForeignKey(a => a.PostalCodeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================= GEOFENCE =================
            b.Entity<GeoFenceZone>(e =>
            {
                e.HasOne(x => x.EncryptionKey)
                    .WithMany(x => x.GeoFenceZones)
                    .HasForeignKey(x => x.EncryptionKeyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================= JWT =================
            b.Entity<JwtSetting>(e =>
            {
                e.HasOne(x => x.EncryptionKey)
                    .WithMany(x => x.JwtSettings)
                    .HasForeignKey(x => x.EncryptionKeyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================= PRECISION =================
            b.Entity<Invoice>().Property(i => i.Amount).HasPrecision(18, 2);
            b.Entity<Invoice>().Property(i => i.PaidAmount).HasPrecision(18, 2);
        }
    }
}