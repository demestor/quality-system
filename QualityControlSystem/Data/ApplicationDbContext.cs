using Microsoft.EntityFrameworkCore;
using QualityControlSystem.Models;

namespace QualityControlSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Справочники
        public DbSet<BatchStatus> BatchStatuses { get; set; } = null!;
        public DbSet<FinalMarkType> FinalMarkTypes { get; set; } = null!;
        public DbSet<FrameModel> FrameModels { get; set; } = null!;

        // Основные сущности
        public DbSet<ProductionBatch> ProductionBatches { get; set; } = null!;
        public DbSet<Frame> Frames { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("public");

            // JSON-поле в Frame
            modelBuilder.Entity<Frame>()
                .Property(f => f.VisualAnalysParams)
                .HasColumnType("jsonb");

            // ProductionBatch
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
                    .HasConstraintName("production_batch_batch_status_id_fkey"); // из скрипта БД
            });

            // BatchStatus
            modelBuilder.Entity<BatchStatus>(entity =>
            {
                entity.ToTable("batch_status", "public");

                entity.Property(bs => bs.BatchStatusId).HasColumnName("batch_status_id");
                entity.Property(bs => bs.StatusName).HasColumnName("status_name");
            });

            // FrameModel
            modelBuilder.Entity<FrameModel>(entity =>
            {
                entity.ToTable("frame_model", "public");

                entity.Property(fm => fm.FrameModelId).HasColumnName("frame_model_id");
                entity.Property(fm => fm.FrameName).HasColumnName("frame_name");
            });

            // FinalMarkType
            modelBuilder.Entity<FinalMarkType>(entity =>
            {
                entity.ToTable("final_mark_type", "public");

                entity.Property(fmt => fmt.FinalMarkTypeId).HasColumnName("final_mark_type_id");
                entity.Property(fmt => fmt.FinalMarkName).HasColumnName("final_mark_name");
            });

            // Frame — полная конфигурация для исправления ошибки
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

                // Явные отношения — исправляет неправильные имена FK в запросах
                entity.HasOne(f => f.Batch)
                    .WithMany(p => p.Frames)
                    .HasForeignKey(f => f.ProdBatchId)
                    .OnDelete(DeleteBehavior.SetNull) // по желанию, если нужно
                    .HasConstraintName("frame_prod_batch_id_fkey");

                entity.HasOne(f => f.FrameModel)
                    .WithMany(fm => fm.Frames)
                    .HasForeignKey(f => f.FrameModelId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("frame_frame_model_id_fkey");

                entity.HasOne(f => f.SystemMark)
                    .WithMany() // без обратной коллекции
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
        }
    }
}