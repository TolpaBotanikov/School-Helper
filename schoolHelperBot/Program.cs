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
        static Dictionary<RegistratedUser, screens> userScreens;
        static Dictionary<long, RegistratedUser> users;
        static void Main(string[] args)
        {
            userScreens = new Dictionary<RegistratedUser, screens>();
            users = new Dictionary<long, RegistratedUser>();
            bot = new TelegramBotClient("1541324472:AAGytI9-Dl0uPzjrTCCAl-xGA_gAZ-Rmoys");
            bot.OnMessage += Bot_OnMessage;
            bot.StartReceiving();
            Console.ReadLine();
        }

        private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var id = e.Message.Chat.Id;
            if (e.Message.Type != Telegram.Bot.Types.Enums.MessageType.Text)
            {
                bot.SendTextMessageAsync(id, "Пожалуйста, отправьте текстовое сообщение");
                return;
            }
            if (DbManager.GetUser(e.Message.From.Username) == null)
            {
                if (!users.ContainsKey(id))
                {
                    RegistrateUser(e.Message);
                    return;
                }
            }
            RegistratedUser user = users[id];
            if (!users.ContainsKey(id))
            {
                bot.SendTextMessageAsync(id, "Меню");
                var menuTeacherMarkup = new ReplyKeyboardMarkup(new[]
                {
                    new[]
                    {
                        new KeyboardButton("Запросить помощь"),
                        new KeyboardButton("Посмотреть отправленные запросы"),
                    },
                    new[]
                    {
                        new KeyboardButton("Оказать помощь"),
                        new KeyboardButton("Настройки")
                     },
                });
                var menuStudentMarkup = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton("Оказать помощь"),
                    new KeyboardButton("Настройки")
                });
                menuTeacherMarkup.OneTimeKeyboard = true;
                menuStudentMarkup.OneTimeKeyboard = true;
                bot.SendTextMessageAsync(id, "**Меню**" +
                    "\nЗдесь вы можете:" +
                    (user.role == 0 ? "\n- Запросить помощь." : "")+
                    (user.role == 0 ? "\n- Посмотреть отправленные запросы." : "") +
                    "\n- Оказать помощь." +
                    "\n- Изменить настройки аккаунта.",
                    replyMarkup: user.role == 0 ? menuTeacherMarkup : menuStudentMarkup);
                userScreens[user] = screens.menu;
                return;
            }
            switch (userScreens[user])
            {
                case screens.registrationLastName:
                    user.lastName = e.Message.Text;
                    bot.SendTextMessageAsync(id, "Введите имя");
                    userScreens[user] = screens.registrationFirstName;
                    break;
                case screens.registrationFirstName:
                    user.firstName = e.Message.Text;
                    bot.SendTextMessageAsync(id, "Введите номер телефона");
                    userScreens[user] = screens.registrationPhone;
                    break;
                case screens.registrationPhone:
                    user.phone = e.Message.Text;
                    var roleMarkup = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton("Учитель"),
                        new KeyboardButton("Ученик")
                    });
                    roleMarkup.OneTimeKeyboard = true;
                    bot.SendTextMessageAsync(id, "Вы учитель или ученик?", replyMarkup: roleMarkup);
                    userScreens[user] = screens.registrationRole;
                    break;
                case screens.registrationRole:
                    switch (e.Message.Text)
                    {
                        case "Учитель":
                            user.role = 0;
                            DbManager.CreateUser(user);
                            var menuTeacherMarkup = new ReplyKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    new KeyboardButton("Запросить помощь"),
                                    new KeyboardButton("Посмотреть отправленные запросы"),
                                },
                                new[]
                                {
                                    new KeyboardButton("Оказать помощь"),
                                    new KeyboardButton("Настройки")
                                },
                            });
                            bot.SendTextMessageAsync(id, "**Меню**" +
                                "\nЗдесь вы можете:" +
                                "\n- Запросить помощь."  +
                                "\n- Посмотреть отправленные запросы." +
                                "\n- Оказать помощь." +
                                "\n- Изменить настройки аккаунта.",
                                replyMarkup: menuTeacherMarkup);
                            menuTeacherMarkup.OneTimeKeyboard = true;
                            userScreens[user] = screens.menu;
                            break;
                        case "Ученик":
                            user.role = 1;
                            DbManager.CreateUser(user);
                            var menuStudentMarkup = new ReplyKeyboardMarkup(new[]
                            {
                                    new KeyboardButton("Оказать помощь"),
                                    new KeyboardButton("Настройки")
                            });
                            bot.SendTextMessageAsync(id, "**Меню**" +
                                "\nЗдесь вы можете:" +
                                "\n- Оказать помощь." +
                                "\n- Изменить настройки аккаунта.",
                                replyMarkup: menuStudentMarkup);
                            menuStudentMarkup.OneTimeKeyboard = true;
                            userScreens[user] = screens.menu;
                            break;
                        default:
                            bot.SendTextMessageAsync(id, "Выберите вариант из предложенных");
                            break;
                    }
                    break;
            }

            //bot.SendTextMessageAsync(chatId, e.Message.Text);
        }

        private static void RegistrateUser(Telegram.Bot.Types.Message message)
        {
            users[message.Chat.Id] = new RegistratedUser();
            users[message.Chat.Id].telegramName = message.From.Username;
            bot.SendTextMessageAsync(message.Chat.Id, "Вы не зарегистрированы. Введите фамилию");
            userScreens[users[message.Chat.Id]] = screens.registrationLastName;
        }
    }
}
