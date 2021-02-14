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
    enum Screens // Перечисление всех экранов
    { 
        registrationLastName,
        registrationFirstName,
        registrationPhone,
        registrationRole,
        menu,
        sendRequest,
        editRequest,
        answerRequest
    }
    class Program
    {
        static TelegramBotClient bot;
        static Dictionary<RegistratedUser, Screens> userScreens;
        static Dictionary<long, RegistratedUser> users;
        static void Main(string[] args)
        {
            userScreens = new Dictionary<RegistratedUser, Screens>();
            users = new Dictionary<long, RegistratedUser>();
            bot = new TelegramBotClient("1541324472:AAGytI9-Dl0uPzjrTCCAl-xGA_gAZ-Rmoys");
            bot.OnMessage += Bot_OnMessage;
            bot.StartReceiving();
            Console.ReadLine();
        }

        private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            long id = e.Message.Chat.Id;
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
                ReplyKeyboardMarkup menuTeacherMarkup = new ReplyKeyboardMarkup(new[]
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
                ReplyKeyboardMarkup menuStudentMarkup = new ReplyKeyboardMarkup(new[]
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
                userScreens[user] = Screens.menu;
                return;
            }
            switch (userScreens[user]) // Проверяем, какой экран у пользователя
            {
                case Screens.registrationLastName:
                    user.lastName = e.Message.Text;
                    bot.SendTextMessageAsync(id, "Введите имя");
                    userScreens[user] = Screens.registrationFirstName;
                    break;
                case Screens.registrationFirstName:
                    user.firstName = e.Message.Text;
                    bot.SendTextMessageAsync(id, "Введите номер телефона");
                    userScreens[user] = Screens.registrationPhone;
                    break;
                case Screens.registrationPhone:
                    user.phone = e.Message.Text;
                    ReplyKeyboardMarkup roleMarkup = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton("Учитель"),
                        new KeyboardButton("Ученик"),
                    });
                    roleMarkup.OneTimeKeyboard = true;
                    bot.SendTextMessageAsync(id, "Вы учитель или ученик?", replyMarkup: roleMarkup);
                    userScreens[user] = Screens.registrationRole;
                    break;
                case Screens.registrationRole:
                    switch (e.Message.Text)
                    {
                        case "Учитель":
                            user.role = 0;
                            DbManager.CreateUser(user);
                            ReplyKeyboardMarkup menuTeacherMarkup = new ReplyKeyboardMarkup(new[]
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
                            userScreens[user] = Screens.menu;
                            break;
                        case "Ученик":
                            user.role = 1;
                            DbManager.CreateUser(user);
                            ReplyKeyboardMarkup menuStudentMarkup = new ReplyKeyboardMarkup(new[]
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
                            userScreens[user] = Screens.menu;
                            break;
                        default:
                            bot.SendTextMessageAsync(id, "Выберите вариант из предложенных");
                            break;
                    }
                    break;
                case Screens.menu:
                    switch (e.Message.Text)
                    {
                        case "Запросить помощь":
                            bot.SendTextMessageAsync(id, "Введите текст запроса");
                            userScreens[user] = Screens.sendRequest;
                            break;
                        case "Посмотреть отправленные запросы":
                            List<Request> requesterRequests = DbManager.GetUserRequests(user.telegramName);
                            List<KeyboardButton> requesterButtons = new List<KeyboardButton>();
                            ReplyKeyboardMarkup requestMarkup = new ReplyKeyboardMarkup(new List<KeyboardButton>().AsEnumerable());
                            foreach (Request request in requesterRequests)
                            {
                                bot.SendTextMessageAsync(id, request.Id +
                                    "\n" + request.Date +
                                    "\n" + request.Text +
                                    "\n" + request.Status);
                                KeyboardButton button = new KeyboardButton("Редактировать запрос " + request.Id);
                                requesterButtons.Add(button);
                            }
                            List<IEnumerable<KeyboardButton>> requesterList = new List<IEnumerable<KeyboardButton>>();
                            requesterList.Add(requesterButtons.AsEnumerable());
                            requestMarkup.Keyboard = requesterList.AsEnumerable();
                            requestMarkup.OneTimeKeyboard = true;
                            bot.SendTextMessageAsync(id, "Выберите запрос для редактирования", replyMarkup: requestMarkup);
                            userScreens[user] = Screens.editRequest;
                            break;
                        case "Оказать помощь":
                            List<Request> helperRequests = DbManager.GetAllRequests();
                            List<KeyboardButton> helperButtons = new List<KeyboardButton>();
                            ReplyKeyboardMarkup helperMarkup = new ReplyKeyboardMarkup(new List<KeyboardButton>().AsEnumerable());
                            foreach (Request request in helperRequests)
                            {
                                bot.SendTextMessageAsync(id, request.Id +
                                    "\n" + request.Date +
                                    "\n" + request.Text +
                                    "\n" + request.Status);
                                KeyboardButton button = new KeyboardButton("Редактировать запрос " + request.Id);
                                helperButtons.Add(button);
                            }
                            List<IEnumerable<KeyboardButton>> helperList = new List<IEnumerable<KeyboardButton>>();
                            helperList.Add(helperButtons.AsEnumerable());
                            helperMarkup.Keyboard = helperList.AsEnumerable();
                            helperMarkup.OneTimeKeyboard = true;
                            bot.SendTextMessageAsync(id, "Выберите запрос, на который хотите ответить", replyMarkup: helperMarkup);
                            userScreens[user] = Screens.answerRequest;
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
            userScreens[users[message.Chat.Id]] = Screens.registrationLastName;
        }
    }
}
