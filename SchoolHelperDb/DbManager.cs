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
        public static User GetUser(string name)
        {
            db.User.Load();
            return db.User.FirstOrDefault(user => user.TelegramName == name);
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

        public static List<Request> GetUserRequests(string name)
        {
            db.User.Load();
            return db.Request.Where(request => 
                request.RequsterId == GetUser(name).Id).ToList();
        }

        public static List<Request> GetAllRequests()
        {
            db.Request.Load();
            return db.Request.ToList();
        }
    }
}
