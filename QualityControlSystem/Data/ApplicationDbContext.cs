using Microsoft.EntityFrameworkCore;
using QualityControlSystem.Models;

namespace QualityControlSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<BatchStatus> BatchStatuses { get; set; } = null!;
        public DbSet<FinalMarkType> FinalMarkTypes { get; set; } = null!;
        public DbSet<FrameModel> FrameModels { get; set; } = null!;

        public DbSet<NotificationType> NotificationTypes { get; set; } = null!;
        public DbSet<Sensor> Sensors { get; set; } = null!;

        public DbSet<ProductionBatch> ProductionBatches { get; set; } = null!;
        public DbSet<Frame> Frames { get; set; } = null!;

        public DbSet<ProdProcessedSensor> ProdProcessedSensors { get; set; } = null!;
        public DbSet<NotificationRule> NotificationRules { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("public");

            modelBuilder.Entity<Frame>()
                .Property(f => f.VisualAnalysParams)
                .HasColumnType("jsonb");

            modelBuilder.Entity<ProductionBatch>(entity =>
            {
                entity.ToTable("production_batch", "public");
                entity.Property(p => p.ProductionBatchId).HasColumnName("production_batch_id");
                entity.Property(p => p.StartDate).HasColumnName("start_date");
                entity.Property(p => p.EndDate).HasColumnName("end_date");
                entity.Property(p => p.BatchStatusId).HasColumnName("batch_status_id");
                entity.Property(p => p.Recom).HasColumnName("recom");

                entity.HasOne(p => p.BatchStatus)
                    .WithMany(bs => bs.ProductionBatches)
                    .HasForeignKey(p => p.BatchStatusId)
                    .HasConstraintName("production_batch_batch_status_id_fkey");
            });

            modelBuilder.Entity<BatchStatus>(entity =>
            {
                entity.ToTable("batch_status", "public");
                entity.Property(bs => bs.BatchStatusId).HasColumnName("batch_status_id");
                entity.Property(bs => bs.StatusName).HasColumnName("status_name");
            });

            modelBuilder.Entity<FrameModel>(entity =>
            {
                entity.ToTable("frame_model", "public");
                entity.Property(fm => fm.FrameModelId).HasColumnName("frame_model_id");
                entity.Property(fm => fm.FrameName).HasColumnName("frame_name");
            });

            modelBuilder.Entity<FinalMarkType>(entity =>
            {
                entity.ToTable("final_mark_type", "public");
                entity.Property(fmt => fmt.FinalMarkTypeId).HasColumnName("final_mark_type_id");
                entity.Property(fmt => fmt.FinalMarkName).HasColumnName("final_mark_name");
            });

            modelBuilder.Entity<Frame>(entity =>
            {
                entity.ToTable("frame", "public");
                entity.Property(f => f.FrameId).HasColumnName("frame_id");
                entity.Property(f => f.ProdBatchId).HasColumnName("prod_batch_id");
                entity.Property(f => f.SerialNumber).HasColumnName("serial_number");
                entity.Property(f => f.FrameModelId).HasColumnName("frame_model_id");
                entity.Property(f => f.SystemMarkId).HasColumnName("system_mark_id");
                entity.Property(f => f.ExpertMarkId).HasColumnName("expert_mark_id");
                entity.Property(f => f.FinalMarkId).HasColumnName("final_mark_id");
                entity.Property(f => f.VisualAnalysParams).HasColumnName("visual_analys_params").HasColumnType("jsonb");

                entity.HasOne(f => f.Batch)
                    .WithMany(p => p.Frames)
                    .HasForeignKey(f => f.ProdBatchId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("frame_prod_batch_id_fkey");

                entity.HasOne(f => f.FrameModel)
                    .WithMany(fm => fm.Frames)
                    .HasForeignKey(f => f.FrameModelId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("frame_frame_model_id_fkey");

                entity.HasOne(f => f.SystemMark)
                    .WithMany()
                    .HasForeignKey(f => f.SystemMarkId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("frame_system_mark_id_fkey");

                entity.HasOne(f => f.ExpertMark)
                    .WithMany()
                    .HasForeignKey(f => f.ExpertMarkId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("frame_expert_mark_id_fkey");

                entity.HasOne(f => f.FinalMark)
                    .WithMany()
                    .HasForeignKey(f => f.FinalMarkId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("frame_final_mark_id_fkey");
            });

            modelBuilder.Entity<Sensor>(entity =>
            {
                entity.ToTable("sensor", "public");
                entity.Property(s => s.SensorId).HasColumnName("sensor_id");
                entity.Property(s => s.SensorName).HasColumnName("sensor_name");
            });

            modelBuilder.Entity<ProdProcessedSensor>(entity =>
            {
                entity.ToTable("prod_processed_sensor", "public");

                entity.Property(p => p.ProdProcessedSensorId).HasColumnName("prod_processed_sensor_id");
                entity.Property(p => p.SensorId).HasColumnName("sensor_id");
                entity.Property(p => p.FrameId).HasColumnName("frame_id");
                entity.Property(p => p.ValueAfterProc).HasColumnName("value_after_proc");
                entity.Property(p => p.ProcessTime).HasColumnName("process_time");

                entity.HasOne(p => p.Sensor)
                    .WithMany()
                    .HasForeignKey(p => p.SensorId)
                    .HasConstraintName("prod_processed_sensor_sensor_id_fkey");

                entity.HasOne(p => p.Frame)
                    .WithMany(f => f.ProdProcessedSensors)
                    .HasForeignKey(p => p.FrameId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("prod_processed_sensor_frame_id_fkey");
            });

            modelBuilder.Entity<NotificationRule>(entity =>
            {
                entity.ToTable("notification_rule", "public");
                entity.Property(n => n.NotificationRuleId).HasColumnName("notification_rule_id");
                entity.Property(n => n.SensorId).HasColumnName("sensor_id");
                entity.Property(n => n.NormalValue).HasColumnName("normal_value");
                entity.Property(n => n.CriticalValue).HasColumnName("critical_value");
                entity.Property(n => n.NotificationTypeId).HasColumnName("notification_type_id");

                entity.HasOne(n => n.Sensor)
                    .WithMany()
                    .HasForeignKey(n => n.SensorId)
                    .HasConstraintName("notification_rule_sensor_id_fkey");

                entity.HasOne(n => n.NotificationType)
                    .WithMany()
                    .HasForeignKey(n => n.NotificationTypeId)
                    .HasConstraintName("notification_rule_notification_type_id_fkey");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notification", "public");
                entity.Property(n => n.NotificationId).HasColumnName("notification_id");
                entity.Property(n => n.SensorProdId).HasColumnName("sensor_prod_id");
                entity.Property(n => n.NotificationRuleId).HasColumnName("notification_rule_id");
                entity.Property(n => n.FrameId).HasColumnName("frame_id");
                entity.Property(n => n.NotificationTime).HasColumnName("notification_time");

                entity.HasOne(n => n.ProdProcessedSensor)
                    .WithMany()
                    .HasForeignKey(n => n.SensorProdId)
                    .HasConstraintName("notification_sensor_prod_id_fkey");

                entity.HasOne(n => n.NotificationRule)
                    .WithMany()
                    .HasForeignKey(n => n.NotificationRuleId)
                    .HasConstraintName("notification_notification_rule_id_fkey");

                entity.HasOne(n => n.Frame)
                    .WithMany(f => f.Notifications)
                    .HasForeignKey(n => n.FrameId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("notification_frame_id_fkey");
            });

            modelBuilder.Entity<NotificationType>(entity =>
            {
                entity.ToTable("notification_type", "public");
                entity.Property(nt => nt.NotificationTypeId).HasColumnName("notification_type_id");
                entity.Property(nt => nt.NotificationTypeName).HasColumnName("notification_name");
            });
        }
    }
}