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

        
    }
}
