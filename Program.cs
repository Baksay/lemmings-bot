using System;
using System.Collections.Generic;
using System.Timers;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Discord;
using Discord.WebSocket;

public class MyEvent
{
    public string user_cr, act_group, act_date, act_time, message;
    public int act_type, action_id;
    public bool isActive;

    public MyEvent(
        int action_id_ins,
        string user_cr_ins,
        string act_group_ins,
        string act_date_ins,
        string act_time_ins,
        int act_type_ins,
        string message_ins,
        bool isActive_ins
        )
    {
        action_id = action_id_ins;
        user_cr = user_cr_ins;
        act_group = act_group_ins;
        act_date = act_date_ins;
        act_time = act_time_ins;
        message = message_ins;
        act_type = act_type_ins;
        isActive = isActive_ins;
    }
}

public struct MessageInstance { public static SocketMessage mes { get; set; } }
class Program
{
    static void create_db()
    {
        string connectionString = "Server=localhost;Database=master;Trusted_Connection=True;";//Блок создания таблиц и БД. Прогоняется при первом запуске, а после игнорируется
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();   // открываем подключение 
            SqlCommand command = new SqlCommand();
            command.Connection = connection;
            // определяем выполняемую команду
            try
            {
                command.CommandText = "CREATE DATABASE db1";
                command.ExecuteNonQuery();
                Console.WriteLine("База данных создана");
            }
            catch
            {
                Console.WriteLine("БД уже была создана");
            }
        }
        connectionString = "Server=localhost;Database=db1;Trusted_Connection=True;";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand command = new SqlCommand();
            command.Connection = connection;
            try
            {
                command.CommandText = "CREATE TABLE Main (action_id INT PRIMARY KEY IDENTITY, user_cr NVARCHAR(20) NOT NULL, act_group NVARCHAR(100) NOT NULL, act_date DATE NOT NULL, act_time TIME NOT NULL, act_type INT NOT NULL, message NVARCHAR(100) NOT NULL, isActive BIT NOT NULL)";
                command.ExecuteNonQuery();
                Console.WriteLine("Таблица БД создана");
            }
            catch
            {
                Console.WriteLine("Таблица БД уже была создана");
            }
        }
    }

    static void add_event(string connectionString, string user_cr, string act_group, string act_date, string act_time, int act_type, string message)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand command = new SqlCommand();
            command.Connection = connection;


            String query = "INSERT INTO Main (user_cr, act_group, act_date, act_time, act_type, message, isActive) VALUES (@user_cr, @act_group, @act_date, @act_time, @act_type, @message, 1);";

            command.CommandText = query;
            command.Parameters.AddWithValue("@user_cr", user_cr);
            command.Parameters.AddWithValue("@act_group", act_group);
            command.Parameters.AddWithValue("@act_date", act_date);
            command.Parameters.AddWithValue("@act_time", act_time);
            command.Parameters.AddWithValue("@act_type", act_type);
            command.Parameters.AddWithValue("@message", message);

            command.ExecuteNonQuery();

        }
    }



    static MyEvent get_eventdata(string connectionString, int target_id)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand command = new SqlCommand();
            command.Connection = connection;

            command.CommandText = "SELECT * FROM Main WHERE action_id = @target_id;";
            command.Parameters.AddWithValue("@target_id", target_id);
            SqlDataReader a = command.ExecuteReader();
            a.Read();

            MyEvent result = new MyEvent(
                a.GetInt32(a.GetOrdinal("action_id")),
                a.GetString(a.GetOrdinal("user_cr")),
                a.GetString(a.GetOrdinal("act_group")),
                a.GetDateTime(a.GetOrdinal("act_date")).ToString(),
                a.GetTimeSpan(a.GetOrdinal("act_time")).ToString(),
                a.GetInt32(a.GetOrdinal("act_type")),
                a.GetString(a.GetOrdinal("message")),
                a.GetBoolean(a.GetOrdinal("isActive"))
                );
            return result;

        }
    }

    static List<MyEvent> GetTodayActiveData(string connectionString)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {

            List<MyEvent> result = new List<MyEvent>();

            connection.Open();
            SqlCommand command = new SqlCommand();
            command.Connection = connection;
            DateTime today = DateTime.Today;

            command.CommandText = "SELECT * FROM Main WHERE isActive = 1 AND act_date = @date OR (act_type = 1 AND act_date < @date);";
            command.Parameters.AddWithValue("@date", today);
            SqlDataReader a = command.ExecuteReader();

            while (a.Read() == true)
            {
                int action_id = a.GetInt32(a.GetOrdinal("action_id"));
                string user_cr = a.GetString(a.GetOrdinal("user_cr"));
                string act_group = a.GetString(a.GetOrdinal("act_group"));
                string act_date = a.GetDateTime(a.GetOrdinal("act_date")).ToString();
                string act_time = a.GetTimeSpan(a.GetOrdinal("act_time")).ToString();
                int act_type = a.GetInt32(a.GetOrdinal("act_type"));
                string message = a.GetString(a.GetOrdinal("message"));
                bool isActive = a.GetBoolean(a.GetOrdinal("isActive"));

                act_time = act_time.Remove(5, 3);     //уборка из вывода секунд, но по умолчанию время в час ночи он пишет как 1, а не 01
                //else if (act_time.Length == 7) { act_time = act_time.Remove(4, 3); }

                MyEvent e = new MyEvent(action_id, user_cr, act_group, act_date, act_time, act_type, message, isActive);
                result.Add(e);
            }

            return result;
        }
    }

    static List<MyEvent> get_alldata(string connectionString)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {

            List<MyEvent> result = new List<MyEvent>();

            connection.Open();
            SqlCommand command = new SqlCommand();
            command.Connection = connection;

            command.CommandText = "SELECT * FROM Main;";
            SqlDataReader a = command.ExecuteReader();

            while (a.Read() == true)
            {
                int action_id = a.GetInt32(a.GetOrdinal("action_id"));
                string user_cr = a.GetString(a.GetOrdinal("user_cr"));
                string act_group = a.GetString(a.GetOrdinal("act_group"));
                string act_date = a.GetDateTime(a.GetOrdinal("act_date")).ToString();
                string act_time = a.GetTimeSpan(a.GetOrdinal("act_time")).ToString();
                int act_type = a.GetInt32(a.GetOrdinal("act_type"));
                string message = a.GetString(a.GetOrdinal("message"));
                bool isActive = a.GetBoolean(a.GetOrdinal("isActive"));

                MyEvent e = new MyEvent(action_id, user_cr, act_group, act_date, act_time, act_type, message, isActive);
                result.Add(e);
            }
            return result;
        }
    }


    static async Task doWork()
    {
        var config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged

        };
        DiscordSocketClient discord = new DiscordSocketClient(config);

        string token = "ODU5MTE5MzM0NzY4NzcxMTIy.GuSbAn.522ODH3wL2b9kiIX7PjJrlKKf3faIDlGl3iW5c";
        await discord.LoginAsync(TokenType.Bot, token);
        await discord.StartAsync();

        discord.MessageUpdated += MessageUpdated;
        discord.MessageReceived += MessageReceived;

        EventStarter(60000);

        await Task.Delay(-1);
    }
    public static async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
    {

        var message = after;
        Console.WriteLine(message);

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
            else if(TargetMessage.Substring(0, 11) == "!event help"){
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
                else{
                    await chan.SendMessageAsync("Недостаточно прав");
                }
            }
        }
    }

    public static async Task HelpMessage(SocketMessage message)
    {
        var chan = message.Channel;
        await chan.SendMessageAsync("```Сообщение для записи должно быть в формате !add *группа пользователей*(Без собачки) first day: *первый день упоминаний* time: *время упоминаний* type: *тип упоминаний* message: *сообщение при упоминании*```");
        await chan.SendMessageAsync("```*Требуется сохранить все пробелы, а курсивный текст заменить на свой*```");
        await chan.SendMessageAsync("```Формат даты: dd-mm-yyyy, Формат времени: hh:mm:ssss (Секунды можно не указывать), Тип уведомлений 1 - уведомления будут каждый день ПОСЛЕ указанной даты в нужное время, люой другой тип - единоразово.```");
        await chan.SendMessageAsync("```Пример: !add *Лемминги* first day: *10-10-2023* time: *10:10* type: *1* message: *Vsem ku!*```");
        await chan.SendMessageAsync("```Команда !set info channel задаст активный канал как канал для уведомлений```");
    }

    public static async Task SubstringingMessage(SocketMessage message)
    {
        string TargetMessage = message.ToString();
        var chan = message.Channel;
        TargetMessage += " endofstring";
        int ToFind = TargetMessage.IndexOf("!add ");
        ToFind = ToFind+5;
        int EndOf = TargetMessage.IndexOf("first day:", ToFind);
        
        string UsersGroup = TargetMessage.Substring(ToFind, EndOf-1-ToFind); //Находит группу пользователей
        

        TargetMessage = TargetMessage.Remove(0, EndOf++); //Убирает все связанное с группой

        ToFind = TargetMessage.IndexOf("first day: ");
        ToFind = ToFind + 11;
        EndOf = TargetMessage.IndexOf(" ", ToFind);
        string TargetDate = TargetMessage.Substring(ToFind, EndOf - ToFind); //Находит целевой день пользователей
        TargetMessage = TargetMessage.Remove(0, EndOf++);

        ToFind = TargetMessage.IndexOf("time: ");
        ToFind = ToFind + 6;
        EndOf = TargetMessage.IndexOf(" ", ToFind);
        string TargetTime = TargetMessage.Substring(ToFind, EndOf - ToFind); //Находит целевое время пользователей
        TargetMessage = TargetMessage.Remove(0, EndOf++);

        ToFind = TargetMessage.IndexOf("type: ");
        ToFind = ToFind + 6;
        EndOf = TargetMessage.IndexOf(" ", ToFind);
        string ActType = TargetMessage.Substring(ToFind, EndOf - ToFind); //Находит тип упоминания пользователей
        TargetMessage = TargetMessage.Remove(0, EndOf++);

        ToFind = TargetMessage.IndexOf("message: ");
        ToFind = ToFind + 9;
        EndOf = TargetMessage.IndexOf(" endofstring", ToFind);
        string UserMessage = TargetMessage.Substring(ToFind, EndOf - ToFind); //Находит сообщение для пользователей
        TargetMessage = TargetMessage.Remove(0, EndOf++);

        if (UsersGroup != null && TargetDate != null && TargetTime != null && ActType != null && UserMessage != null)
        {
            await chan.SendMessageAsync("Успешно!");
            add_event("Server=localhost;Database=db1;Trusted_Connection=True;", message.Author.ToString(), UsersGroup, TargetDate, TargetTime, int.Parse(ActType), UserMessage);
            Console.WriteLine("Ивент добавлен");
        }
        else
        {
            await chan.SendMessageAsync("Что-то пошло не так!");
            Console.WriteLine("Ошибка добавления ивента");
        }
    }

    public static async Task EventReturner(string TargetMessage, ISocketMessageChannel chan)
    {
        int EndOf = TargetMessage.IndexOf(" ", 6);
        string task = TargetMessage.Substring(6, EndOf - 6);
        if (task == "all")
        {
            List<MyEvent> Events = get_alldata("Server=localhost;Database=db1;Trusted_Connection=True;");
            for (int i = 0; i < Events.Count(); i++)
            {
                string result = "```";
                result += "Id: ";
                result += Events[i].action_id.ToString();
                result += "\n";

                result += "Creator: ";
                result += Events[i].user_cr.ToString();
                result += "\n";

                string group = Events[i].act_group.ToString();
                result += "Group: ";
                result += Events[i].act_group.ToString();
                result += "\n";

                result += "Message: ";
                result += Events[i].message.ToString();
                result += "\n";

                result += "Target time: ";
                result += Events[i].act_date.ToString().Remove(11);
                result += Events[i].act_time.ToString();
                result += "\n";

                result += "Is active: ";
                if (Events[i].isActive == true)
                {
                    result += "active";
                }
                else
                {
                    result += "false";
                }
                result += "\n";

                result += "```";

                await chan.SendMessageAsync(result);
            }
            await chan.SendMessageAsync();
        }
    }

    public static void EventStarter(int timer)
    {
        System.Timers.Timer aTimer = new System.Timers.Timer(timer);
        aTimer.AutoReset = true;
        aTimer.Elapsed += OnTimedEvent;
        aTimer.Enabled = true;
        ElapsedEventArgs e;
    }

    public static async void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        List<MyEvent> Events = GetTodayActiveData("Server=localhost;Database=db1;Trusted_Connection=True;");
        DateTime ControlTime = DateTime.Now;
        string nowsTime = "0";
        string nowsTime1 = ControlTime.ToString().Remove(0, 11);//11 - date
        if (nowsTime1.Length == 8)
        {
            nowsTime = nowsTime1;
            nowsTime = nowsTime.Remove(5, 3);
        }      //уборка из вывода секунд, но по умолчанию время в час ночи он пишет как 1, а не 01
        else if (nowsTime1.Length == 7)
        {
            nowsTime += nowsTime1;
            nowsTime = nowsTime.Remove(5, 3);
        }
        Console.WriteLine("Старт проверки " + Events.Count() + " ивентов в " + nowsTime);
        for (int i = 0; i < Events.Count(); i++)
        {
            if (Events[i].act_time == nowsTime)
            {
                Console.WriteLine("Успешная проверка");
                MessageSender(Events[i].action_id);
            }
            else{
                Console.WriteLine("Неуспешная проверка");
            }
            
        }
    }



    public static async Task MessageSender(int ID)
    {
        //id канала по умолчанию: 859120596147240993
        ISocketMessageChannel chan = MessageInstance.mes.Channel;
        MyEvent targetEvent = get_eventdata("Server=localhost;Database=db1;Trusted_Connection=True;", ID);
        await chan.SendMessageAsync(targetEvent.act_group);
        await chan.SendMessageAsync(targetEvent.message);
    }

    static void Main(string[] args)
    {

        create_db();
        Console.WriteLine("Бд создана");

        //add_event("Server=localhost;Database=db1;Trusted_Connection=True;", "abobaNew", "3", "2010-10-10", "10:10:10", 10);


        //MyEvent result = get_eventdata("Server=localhost;Database=db1;Trusted_Connection=True;", 6);

        //List<MyEvent> result1 = get_alldata("Server=localhost;Database=db1;Trusted_Connection=True;");

        Task.WaitAny(doWork());

    }
    //ODU5MTE5MzM0NzY4NzcxMTIy.GuSbAn.522ODH3wL2b9kiIX7PjJrlKKf3faIDlGl3iW5c
}