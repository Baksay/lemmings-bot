using System;
using System.IO;
using System.Collections.Generic;
using System.Timers;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Discord;
using Discord.WebSocket;
using System.Diagnostics;


public partial class MyEvent
{
    public string user_cr, act_group, act_date, act_time, message;
    public int act_type, action_id;
    public bool isActive;

}

partial class Program
{
    static string ReadToken()
    {
        string result = "";
        string path = "C:\\Users\\Ilya\\Documents\\token.txt";//путь к файлу с токеном

        result = File.ReadAllText(path);

        if (result != null)
        {
            Console.WriteLine("Token read successfully");
            return result;
        }
        else
        {
            Console.WriteLine("Token reading error!");
            return result = "";
        }
    }

    static async Task doWork(int bottype)
    {
        var config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged

        };
        DiscordSocketClient discord = new DiscordSocketClient(config);

        string token = ReadToken();
        await discord.LoginAsync(TokenType.Bot, token);
        await discord.StartAsync();

        discord.MessageUpdated += MessageUpdated;
        discord.MessageReceived += MessageReceived;
        if (bottype == 0)
        {       //запускает бота в режиме эвентов(потом переделать иначе, сейчас лень)
            create_db();
            Console.WriteLine("DB ready");
            EventStarter(60000);
        }
        if (bottype == 1)
        {       //запускает бота в режиме управления
            ControlStarter();
        }

        await Task.Delay(-1);
    }
    public static async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
    {
        //Обработчик измененных сообщений
        //var message = after;
        //Console.WriteLine(message);

    }

    public static async Task MessageReceived(SocketMessage message)
    {
        //!add groupofusers time day type
        string TargetMessage = message.ToString();
        var chan = message.Channel;
        TargetMessage += " endofstring";



        if (message.Author.Id != 859119334768771122)
        {
            if (TargetMessage.Substring(0, 5) == "!help")
            {
                await HelpMessage(message);
            }
            else if (TargetMessage.Substring(0, 4) == "!add")
            {
                await SubstringingMessage(message);
            }
            else if (TargetMessage.Substring(0, 6) == "!show ")
            {
                await EventReturner(TargetMessage, chan);
            }
            else if (TargetMessage.Substring(0, 11) == "!event help")
            {
                await chan.SendMessageAsync("!add **@роль** first day: **день начала** time: **нужное время** type: **тип уведомлений(1 для каждодневных, 2 для единоразовых)** message: **текст уведомления**");
            }
            else if (TargetMessage.Substring(0, 17) == "!set info channel")
            {//задает как канал для уведомлений канал в котором написанно это сообщение
                if (message.Author.Id == 292707993596985366)
                {
                    MessageInstance.mes = message;
                    await chan.SendMessageAsync("Успешно!");
                    Console.WriteLine("Канал выдачи добавлен");
                }
                else
                {
                    await chan.SendMessageAsync("Недостаточно прав");
                }
            }
        }
    }

    public static async Task HelpMessage(SocketMessage message)
    {
        var chan = message.Channel;
        await chan.SendMessageAsync("```Заплашка для FAQ бота для системы управления```");
        /*await chan.SendMessageAsync("```Сообщение для записи должно быть в формате !add группа пользователей first day: первый день упоминаний time: время упоминаний type: тип упоминаний message: сообщение при упоминании```");
        await chan.SendMessageAsync("```Формат даты: dd-mm-yyyy, Формат времени: hh:mm:ssss (Секунды можно не указывать), Тип уведомлений 1 - уведомления будут каждый день ПОСЛЕ указанной даты в нужное время, люой другой тип - единоразово.```");
        await chan.SendMessageAsync("```Пример: !add Лемминги first day: 10-10-2023 time: 10:10 type: 1 message: Vsem ku!```");
        await chan.SendMessageAsync("```Команда !set info channel задаст активный канал как канал для уведомлений```");
        await chan.SendMessageAsync("```Заглушка для нового хелпа```");*/
    }




    static int Main(string[] args)
    {
        int bottype = 1;

        //Task.WaitAny(doWork(bottype));

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "ping 74.56.228.180",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return 0;
    }
}
