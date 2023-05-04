using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace API.Data
{
    public class DataContext : IdentityDbContext<AppUser,AppRole,int,
        IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, 
        IdentityRoleClaim<int>,IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        //public DbSet<AppUser> Users {get; set;} identity already have it
        public DbSet<UserLike> Likes {get; set;}
        public DbSet<Message>  Messages{ get; set; }
        public DbSet<Group> Groups{ get; set; }
        public DbSet<Connection> Connections{ get; set; }
        public DbSet<Photo> Photos {get; set;}
        protected override void OnModelCreating (ModelBuilder builder){
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                .HasMany(ur=>ur.UserRoles)
                .WithOne(u => u.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired(); //foreign cannot be null
            
            builder.Entity<AppRole>()
                .HasMany(ur=>ur.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
    
            

            builder.Entity<UserLike>()//<UserLike> entity we target here relationship
                .HasKey(k =>new {k.SourceUserId, k.TargetUserId}); //primary key
            builder.Entity<UserLike>()
                .HasOne(s => s.SourceUser) //AppUser table
                .WithMany(l =>l.LikedUsers) //Likes table
                .HasForeignKey(s=>s.SourceUserId) //how to connect 
                .OnDelete(DeleteBehavior.Cascade); //delete a appuser will delete all its liked users
            builder.Entity<UserLike>()
                .HasOne(s => s.TargetUser)
                .WithMany(l =>l.LikedByUsers)
                .HasForeignKey(s=>s.TargetUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Message>()
                .HasOne(u => u.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(m => m.MessagesSent)
                .HasForeignKey(s=>s.SenderId) //can be omitted
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Photo>().HasQueryFilter(p => p.IsApproved);
            
        }
    }
}



// There are 2 ways of applying migrations and they both do the same thing - create the DB if it does not exist and apply any migrations that have not been already applied to the DB:

// 1.  Using the dotnet ef database update command

// 2.  Using the context.Database.Migrate() in the code.

// We are using option 2 in the Program.cs class so we do not need to update the DB using the command line. 

