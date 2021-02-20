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
        selectRequest,
        answerRequest,
        editRequest,
        resolveRequest
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
                    users[id] = new RegistratedUser();
                    users[id].telegramName = e.Message.From.Username;
                    bot.SendTextMessageAsync(id, "Вы не зарегистрированы. Введите фамилию");
                    userScreens[users[id]] = Screens.registrationLastName;
                    return;
                }
            }
            RegistratedUser user;
            if (!users.ContainsKey(id))
            {
                user = new RegistratedUser();
                users[id] = user;
                User dbUser = DbManager.GetUser(e.Message.From.Username);
                user.firstName = dbUser.FirstName;
                user.lastName = dbUser.LastName;
                user.phone = dbUser.Phone;
                user.role = dbUser.Role;
                user.telegramName = dbUser.TelegramName;
                Menu(id, user.role);
                userScreens[user] = Screens.menu;
                return;
            }
            user = users[id];
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
                            Menu(id, 0);
                            userScreens[user] = Screens.menu;
                            break;
                        case "Ученик":
                            user.role = 1;
                            DbManager.CreateUser(user);
                            Menu(id, 1);
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
                        case "Пометить запрос как решенный":
                            List<Request> requesterRequests = DbManager.GetUserRequests(user.telegramName);
                            List<KeyboardButton> requesterButtons = new List<KeyboardButton>();
                            ReplyKeyboardMarkup requestMarkup = new ReplyKeyboardMarkup(new List<KeyboardButton>().AsEnumerable());
                            if (requesterRequests.Count == 0)
                            {
                                bot.SendTextMessageAsync(id, "У вас нет запросов");
                                Menu(id, user.role);
                                userScreens[user] = Screens.menu;
                                break;
                            }
                            foreach (Request request in requesterRequests)
                            {
                                bot.SendTextMessageAsync(id, "Номер: " +  request.Id +
                                    "\nДата:" + request.Date +
                                    "\n" + request.Text +
                                    "\nСтатус: " + (request.Status == 0 ? "Отправлен" : "В работе"));
                                KeyboardButton button = new KeyboardButton("Запрос " + request.Id + " решен");
                                requesterButtons.Add(button);
                            }
                            List<IEnumerable<KeyboardButton>> requesterList = new List<IEnumerable<KeyboardButton>>();
                            requesterList.Add(requesterButtons.AsEnumerable());
                            requestMarkup.Keyboard = requesterList.AsEnumerable();
                            requestMarkup.OneTimeKeyboard = true;
                            bot.SendTextMessageAsync(id, "Выберите запрос", replyMarkup: requestMarkup);
                            userScreens[user] = Screens.resolveRequest;
                            break;
                        case "Оказать помощь":
                            List<Request> helperRequests = DbManager.GetAllRequests();
                            List<KeyboardButton> helperButtons = new List<KeyboardButton>();
                            ReplyKeyboardMarkup helperMarkup = new ReplyKeyboardMarkup(new List<KeyboardButton>().AsEnumerable());
                            if (helperRequests.Count == 0)
                            {
                                bot.SendTextMessageAsync(id, "В системе нет запросов");
                                Menu(id, user.role);
                                userScreens[user] = Screens.menu;
                                break;
                            }
                            foreach (Request request in helperRequests)
                            {
                                bot.SendTextMessageAsync(id, "Номер: " + request.Id +
                                    "\nДата: " + request.Date +
                                    "\n" + request.Text +
                                    "\nСтатус: " + request.Status);
                                KeyboardButton button = new KeyboardButton("Принять запрос " + request.Id);
                                helperButtons.Add(button);
                            }
                            List<IEnumerable<KeyboardButton>> helperList = new List<IEnumerable<KeyboardButton>>();
                            helperList.Add(helperButtons.AsEnumerable());
                            helperMarkup.Keyboard = helperList.AsEnumerable();
                            helperMarkup.OneTimeKeyboard = true;
                            bot.SendTextMessageAsync(id, "Выберите запрос, на который хотите ответить", replyMarkup: helperMarkup);
                            userScreens[user] = Screens.answerRequest;
                            break;
                        default:
                            bot.SendTextMessageAsync(id, "Выберите вариант из предложенных");
                            break;
                    }
                    break;
                case Screens.sendRequest:
                    DbManager.NewRequest(user.telegramName, e.Message.Text);
                    bot.SendTextMessageAsync(id, "Запрос отправлен!");
                    Menu(id, user.role);
                    userScreens[user] = Screens.menu;
                    break;
                case Screens.resolveRequest:
                    string resolvedRequestId = e.Message.Text.Split(' ')[1];
                    if (!int.TryParse(resolvedRequestId, out int resolved))
                    {
                        bot.SendTextMessageAsync(id, "Выберите вариант из предложенных");
                        return;
                    }
                    if (DbManager.GetRequest(resolved) == null)
                    {
                        bot.SendTextMessageAsync(id, "Выберите вариант из предложенных");
                        return;
                    }
                    DbManager.ResolveRequest(resolved);
                    bot.SendTextMessageAsync(id, "Запрос решен!");
                    Menu(id, user.role);
                    userScreens[user] = Screens.menu;
                    break;
                case Screens.answerRequest:
                    if (e.Message.Text.Split(' ').Length < 3)
                    {
                        bot.SendTextMessageAsync(id, "Выберите вариант из предложенных");
                        return;
                    }
                    string answeredRequestId = e.Message.Text.Split(' ')[2];
                    if (!int.TryParse(answeredRequestId, out int answered))
                    {
                        bot.SendTextMessageAsync(id, "Выберите вариант из предложенных");
                        return;
                    }
                    if (DbManager.GetRequest(answered) == null)
                    {
                        bot.SendTextMessageAsync(id, "Выберите вариант из предложенных");
                        return;
                    }
                    User requester = DbManager.AnswerRequest(answered, user.telegramName);
                    bot.SendTextMessageAsync(id, "Запрос принят! " +
                        "\nСвяжитесь с учителем для получения подробной информации." +
                        "\nТелефон: " + requester.Phone +
                        "\nИмя в Telegram: @" + requester.TelegramName);
                    Menu(id, user.role);
                    userScreens[user] = Screens.menu;
                    break;
                //case Screens.selectRequest:
                //    if (int.TryParse(e.Message.Text, out int result))
                //    {
                //        Request request = DbManager.GetRequest(result);
                //        if (request == null)
                //        {
                //            bot.SendTextMessageAsync(id, "Выберите вариант из предложенных");
                //            break;
                //        }
                //        ReplyKeyboardMarkup editRequestMarkup = new ReplyKeyboardMarkup(new[]
                //        {
                //                new KeyboardButton("Изменить запрос " + result),
                //                new KeyboardButton("Пометить запрос " + result + " как решенный")
                //            });
                //        bot.SendTextMessageAsync(id, "Что вы хотите сделать с этим запросом", replyMarkup: editRequestMarkup);
                //        userScreens[user] = Screens.editRequest;
                //    }
                //    else
                //    {
                //        bot.SendTextMessageAsync(id, "Выберите вариант из предложенных");
                //    }
                //    break;
                //case Screens.editRequest:
                //    string[] message = e.Message.Text.Split(' ');
                //    if (message[0] == "Изменить")
                //    {

                //    }
                //    break;

        }
            
            //bot.SendTextMessageAsync(chatId, e.Message.Text);
        }

        private static void Menu(long id, int role)
        {
            ReplyKeyboardMarkup menuTeacherMarkup = new ReplyKeyboardMarkup(new[]
                {
                    new[]
                    {
                        new KeyboardButton("Запросить помощь"),
                        new KeyboardButton("Пометить запрос как решенный"),
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
                (role == 0 ? "\n- Запросить помощь." : "") +
                (role == 0 ? "\n- Посмотреть отправленные запросы." : "") +
                "\n- Оказать помощь." +
                "\n- Изменить настройки аккаунта.",
                replyMarkup: role == 0 ? menuTeacherMarkup : menuStudentMarkup);
            return;
        }
    }
}
