using Data.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Data
{
	public class ApplicationDbContext : DbContext
	{
		private readonly SqlConnection _sqlConnection;
		public ApplicationDbContext(
			DbContextOptions<ApplicationDbContext> options,
			IConfiguration configuration
			) : base(options)
		{
			_sqlConnection = new SqlConnection(configuration.GetConnectionString("CMS"));
		}
		public DbSet<User> Users { get; set; }
	}
}
