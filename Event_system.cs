using System;
using System.IO;
using System.Collections.Generic;
using System.Timers;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Discord;
using Discord.WebSocket;

public partial class MyEvent
{
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
partial class Program
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
                Console.WriteLine("DB Main created");
            }
            catch
            {
                Console.WriteLine("DB Main already exists");
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
                Console.WriteLine("DB Table created");
            }
            catch
            {
                Console.WriteLine("DB Table already exists");
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

    public static async Task SubstringingMessage(SocketMessage message)
    {
        string TargetMessage = message.ToString();
        var chan = message.Channel;
        TargetMessage += " endofstring";
        int ToFind = TargetMessage.IndexOf("!add ");
        ToFind = ToFind + 5;
        int EndOf = TargetMessage.IndexOf("first day:", ToFind);

        string UsersGroup = TargetMessage.Substring(ToFind, EndOf - 1 - ToFind); //Находит группу пользователей


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
                MessageSenderForEventSystem(Events[i].action_id);
            }
            else
            {
                Console.WriteLine("Неуспешная проверка");
            }

        }
    }

    public static async Task MessageSenderForEventSystem(int ID)
    {
        //id канала по умолчанию: 859120596147240993
        ISocketMessageChannel chan = MessageInstance.mes.Channel;
        MyEvent targetEvent = get_eventdata("Server=localhost;Database=db1;Trusted_Connection=True;", ID);
        await chan.SendMessageAsync(targetEvent.act_group);
        await chan.SendMessageAsync(targetEvent.message);
    }
}