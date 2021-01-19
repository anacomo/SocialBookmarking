namespace SocialBookmarking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bookmarks",
                c => new
                    {
                        BookmarkId = c.Int(nullable: false, identity: true),
                        BookmarkTitle = c.String(nullable: false, maxLength: 100),
                        BookmarkDesc = c.String(nullable: false),
                        BookmarkDate = c.DateTime(nullable: false),
                        BookmarkPopularity = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BookmarkId)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryId = c.Int(nullable: false, identity: true),
                        CategoryName = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.CategoryId);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        CommentId = c.Int(nullable: false, identity: true),
                        CommentContent = c.String(nullable: false, maxLength: 500),
                        CommentDate = c.DateTime(nullable: false),
                        BookmarkId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CommentId)
                .ForeignKey("dbo.Bookmarks", t => t.BookmarkId, cascadeDelete: true)
                .Index(t => t.BookmarkId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Comments", "BookmarkId", "dbo.Bookmarks");
            DropForeignKey("dbo.Bookmarks", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Comments", new[] { "BookmarkId" });
            DropIndex("dbo.Bookmarks", new[] { "CategoryId" });
            DropTable("dbo.Comments");
            DropTable("dbo.Categories");
            DropTable("dbo.Bookmarks");
        }
    }
}
