using System;
using System.Linq;
using System.Text;
using Dba.DAL;
using Logic.Models;

namespace Logic
{
    
    public class HomeManager
    {
        private readonly string[] BodyToken = {"@&body&@"};
        private readonly string[] CategoryToken = {"@&items&@"};
        private const string ItemNameToken = "@&name&@";
        private const string ItemUrlToken = "@&url&@";
        private const string ItemImgToken = "@&img&@";

        public HomeModel ComposeHomepage(string userId)
        {
            return ComposeHomepage(new UserManager().GetStyleId(userId),new UserManager().GetUrlListId(userId));
        }

        public HomeModel ComposeHomepage(int styleId, int urlListId)
        {
            using (var db = new DbCtx())
            {
                var style = db.Styles.Find(styleId);
                var urlList = db.UrlLists.FirstOrDefault(d=>d.Id==urlListId);
            
            var html = new StringBuilder();
            var page = style.PageHTML.Split(BodyToken,StringSplitOptions.None);
            var pageBefore = page[0];
            var pageAfter = page[1];

            var categoryHtml = style.CategoryHTML.Split(CategoryToken, StringSplitOptions.None);
            var categoryBefore = categoryHtml[0];
            var categoryAfter = categoryHtml[1];
            html.Append(pageBefore);
            foreach (var category in urlList.Categories)
            {
                html.Append(categoryBefore.Replace(ItemNameToken, category.Name));
                foreach (var item in category.Items)
                {
                    if (item.CustomHTML != "")
                    {
                        html.Append(item.CustomHTML);
                    }
                    else
                    {
                        html.Append(
                            style.ItemHTML.Replace(ItemNameToken, item.Name)
                                .Replace(ItemImgToken, item.Image)
                                .Replace(ItemUrlToken, item.Url));
                    }
                }
                html.Append(categoryAfter.Replace(ItemNameToken, category.Name));
            }
            html.Append(pageAfter);
            return new HomeModel{Content = html.ToString(),Style = style.CSS};
            }
        }
    }
}