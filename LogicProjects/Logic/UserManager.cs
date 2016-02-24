
using Dba.DAL;

namespace Logic
{
    public class UserManager
    {
        public int GetUrlListId(string userId)
        {
            using (var db = new DbCtx())
            {
                return 1;
            }
        }
        public int GetStyleId(string userId)
        {
            using (var db = new DbCtx())
            {
                return 1;
            }
        }
    }
}