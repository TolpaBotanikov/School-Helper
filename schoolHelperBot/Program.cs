using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace schoolHelperBot
{
    class Program
    {
        static TelegramBotClient bot;
        static void Main(string[] args)
        {
            string token = "1541324472:AAGytI9-Dl0uPzjrTCCAl-xGA_gAZ-Rmoys";
            bot = new TelegramBotClient(token);
            bot.OnMessage += Bot_OnMessage;
            bot.StartReceiving();
            Console.ReadLine();
        }

        private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var chatId = e.Message.Chat.Id;
            bot.SendTextMessageAsync(chatId, e.Message.Text);
        }

        //async static void Bw_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    var worker = sender as BackgroundWorker;
        //    var key = e.Argument as String;
        //    try
        //    {
        //        await bot.SetWebhookAsync("");
        //        //Bot.SetWebhook(""); // Обязательно! убираем старую привязку к вебхуку для бота
        //        int offset = 0; // отступ по сообщениям
        //        while (true)
        //        {
        //            var updates = await bot.GetUpdatesAsync(offset); // получаем массив обновлений

        //            foreach (var update in updates) // Перебираем все обновления
        //            {
        //                offset = update.Id + 1;
        //                var message = update.Message;
        //                Console.WriteLine(message.Text);
        //                //if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
        //                //{
        //                //    if (message.Text == "/saysomething")
        //                //    {
        //                //        // в ответ на команду /saysomething выводим сообщение
        //                //        await bot.SendTextMessageAsync(message.Chat.Id, "тест", replyToMessageId: message.MessageId);
        //                //    }
        //                //}

        //                await bot.SendTextMessageAsync(message.Chat.Id, message.Text);
        //            }

        //        }
        //    }
        //    catch (Telegram.Bot.Exceptions.ApiRequestException ex)
        //    {
        //        Console.WriteLine(ex.Message); // если ключ не подошел - пишем об этом в консоль отладки
        //    }
        //}
    }
}
