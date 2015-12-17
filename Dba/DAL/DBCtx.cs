using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Dba.DTO;
using Dba.DTO.BotDTO;


namespace Dba.DAL
{
    public partial class DbCtx : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Style> Styles { get; set; }
        public DbSet<UrlList> UrlLists { get; set; }
        public DbSet<RssTorrents> RssTorrents { get; set; }
        public DbSet<SoundEntry> SoundEntries { get; set; }
        public DbSet<SoundTag> SoundTags { get; set; }
        public DbSet<WebHome> WebHomes { get; set; }
        public DbSet<WebImage> WebImages { get; set; }
        //BOT
        public DbSet<UserIgnore> UserIgnores { get; set; }
        public DbSet<BotPlugin> BotPlugins { get; set; }
        public DbSet<ReactionMacro> ReactionMacros { get; set; }
        public DbSet<ReactionMacroType> ReactionMacroTypes { get; set; }
        public DbSet<Fapcount> Fapcounts { get; set; }
        public DbSet<EasterEgg> EasterEggs { get; set; }
        public DbSet<AlertsOnTrigger> AlertsOnTriggers { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<CheckTimer> CheckTimers { get; set; }
        public DbSet<JoinSbTrigger> JoinSbTriggers { get; set; }
        public DbSet<KeyValueStore> KeyValueStore { get; set; }

        //ENDBOT
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingEntitySetNameConvention>();


            modelBuilder.Entity<SoundEntry>()
                   .HasMany<SoundTag>(s => s.SoundTags)
                   .WithMany(c => c.SoundEntries)
                   .Map(cs =>
                   {
                       cs.MapLeftKey("SoundEntryId");
                       cs.MapRightKey("Tag");
                       cs.ToTable("SoundEntryTags");
                   });

            base.OnModelCreating(modelBuilder);
        }

        public DbCtx() : base(Settings.Instance.Connection)
        {
        
        }
    }
}