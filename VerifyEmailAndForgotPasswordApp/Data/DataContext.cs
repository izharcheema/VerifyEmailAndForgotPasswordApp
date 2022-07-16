namespace VerifyEmailAndForgotPasswordApp.Data
{
    public class DataContext:DbContext
    {
        public DataContext(DbContextOptions<DataContext> options):base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer
                ("Data Source=.\\SQLExpress; initial Catalog=VEAFPA; Integrated Security=true;Encrypt = false; User ID =sqluser;Password =Pakistan@1234;");
        }
        public DbSet<User> Users=> Set<User>();
    }
}
