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

        public static User GetUser(int id)
        {
            db.User.Load();
            return db.User.FirstOrDefault(user => user.Id == id);
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
            User user = GetUser(name);
            return db.Request.Where(request => 
                request.RequsterId == user.Id
                && request.Status != 2).ToList();
        }

        public static List<Request> GetAllRequests()
        {
            db.Request.Load();
            return db.Request.Where(request => request.Status != 2).ToList();
        }

        public static void NewRequest(string name, string text)
        {
            Request request = new Request();
            request.RequsterId = GetUser(name).Id;
            request.Text = text;
            request.Date = DateTime.Now;
            request.Status = 0;
            db.Request.Add(request);
            db.SaveChanges();
        }

        public static Request GetRequest(int id)
        {
            db.Request.Load();
            return db.Request.FirstOrDefault(request => request.Id == id);
        }

        public static void ResolveRequest(int id)
        {
            db.Request.Load();
            GetRequest(id).Status = 2;
            db.SaveChanges();
        }

        public static User AnswerRequest(int id, string helperName)
        {
            db.Request.Load();
            Request request = GetRequest(id);
            HelperRequest hr = new HelperRequest();
            hr.RequestId = id;
            hr.HelperId = GetUser(helperName).Id;
            db.HelperRequest.Add(hr);
            db.SaveChanges();
            return GetUser(request.RequsterId);
        }
    }
}
