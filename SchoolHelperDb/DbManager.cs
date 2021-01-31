using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolHelperBot;

namespace SchoolHelperDb
{
    public class DbManager
    {
        private static SchoolHelperDbEntities db = new SchoolHelperDbEntities();
        public static User GetUser(string telegramName)
        {
            db.User.Load();
            return db.User.FirstOrDefault(el => el.TelegramName == telegramName);
        }
        public static void CreateUser(RegistratedUser regUser)
        {
            User user = new User();
            user.FirstName = regUser.firstName;
            user.LastName = regUser.lastName;
            user.Phone = regUser.phone;
            user.Role = regUser.role;
            user.TelegramName = regUser.telegramName;
            db.User.Add(user);
            db.SaveChanges();
        }
    }
}
