namespace TodoAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<NexuspayConfirmation> NexuspayConfirmation { get; set; }
        public DbSet<StkCallback> SktCallback { get; set; }
        public DbSet<TempSTKData> TempSTKData { get; set; }
    }
}
