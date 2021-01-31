using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using SchoolHelperDb;
using Telegram.Bot.Types.ReplyMarkups;

namespace SchoolHelperBot
{
    enum screens 
    { 
        registrationLastName,
        registrationFirstName,
        registrationPhone,
        registrationRole,
        menu
    }
    class Program
    {
        static TelegramBotClient bot;
        static Dictionary<long, screens> users;
        static Dictionary<long, RegistratedUser> registratedUsers;
        static void Main(string[] args)
        {
            users = new Dictionary<long, screens>();
            registratedUsers = new Dictionary<long, RegistratedUser>();
            bot = new TelegramBotClient("1541324472:AAGytI9-Dl0uPzjrTCCAl-xGA_gAZ-Rmoys");
            bot.OnMessage += Bot_OnMessage;
            bot.StartReceiving();
            Console.ReadLine();
        }

        private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var id = e.Message.Chat.Id;
            if (DbManager.GetUser(e.Message.From.Username) == null)
            {
                if (!registratedUsers.ContainsKey(e.Message.Chat.Id))
                {
                    RegistrateUser(e.Message);
                    return;
                }
            }
            else if(!registratedUsers.ContainsKey(e.Message.Chat.Id))
            {
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Меню");
                return;
            }
            switch (users[id])
            {
                case screens.registrationLastName:
                    registratedUsers[id].lastName = e.Message.Text;
                    bot.SendTextMessageAsync(e.Message.Chat.Id, "Введите имя");
                    users[id] = screens.registrationFirstName;
                    break;
                case screens.registrationFirstName:
                    registratedUsers[id].firstName = e.Message.Text;
                    bot.SendTextMessageAsync(e.Message.Chat.Id, "Введите номер телефона");
                    users[id] = screens.registrationPhone;
                    break;
                case screens.registrationPhone:
                    registratedUsers[id].phone = e.Message.Text;
                    var markup = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton("Учитель"),
                        new KeyboardButton("Ученик")
                    });
                    markup.OneTimeKeyboard = true;
                    bot.SendTextMessageAsync(id, "Вы учитель или ученик?", replyMarkup: markup);
                    users[id] = screens.registrationRole;
                    break;
                case screens.registrationRole:
                    switch (e.Message.Text)
                    {
                        case "Учитель":
                            registratedUsers[id].role = 0;
                            DbManager.CreateUser(registratedUsers[e.Message.Chat.Id]);
                            bot.SendTextMessageAsync(e.Message.Chat.Id, "Вы зарегистрированы");
                            break;
                        case "Ученик":
                            registratedUsers[id].role = 1;
                            DbManager.CreateUser(registratedUsers[e.Message.Chat.Id]);
                            bot.SendTextMessageAsync(e.Message.Chat.Id, "Вы зарегистрированы");
                            users[id] = screens.menu;
                            break;
                        default:
                            bot.SendTextMessageAsync(e.Message.Chat.Id, "Выберите вариант из предложенных");
                            break;
                    }
                    break;
            }

            //bot.SendTextMessageAsync(chatId, e.Message.Text);
        }

        private static void RegistrateUser(Telegram.Bot.Types.Message message)
        {
            registratedUsers[message.Chat.Id] = new RegistratedUser();
            registratedUsers[message.Chat.Id].telegramName = message.From.Username;
            bot.SendTextMessageAsync(message.Chat.Id, "Вы не зарегистрированы. Введите фамилию");
            users[message.Chat.Id] = screens.registrationLastName;
        }
    }
}
