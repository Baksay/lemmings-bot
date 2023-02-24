using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

public class MyEvent
{
    public string user_cr, act_group, act_date, act_time;
    public int act_type, action_id;

    public MyEvent(int action_id_ins, string user_cr_ins, string act_group_ins, string act_date_ins, string act_time_ins, int act_type_ins)
    {
        action_id = action_id_ins;
        user_cr = user_cr_ins;
        act_group = act_group_ins;
        act_date = act_date_ins;
        act_time = act_time_ins;
        act_type = act_type_ins;
    }
}

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

            command.CommandText = "SELECT action_id, user_cr, act_group, act_date, act_time, act_type FROM Main WHERE action_id = @target_id;";
            command.Parameters.AddWithValue("@target_id", target_id);
            SqlDataReader a = command.ExecuteReader();
            a.Read();

            int action_id = a.GetInt32(a.GetOrdinal("action_id"));
            string user_cr = a.GetString(a.GetOrdinal("user_cr"));
            string act_group = a.GetString(a.GetOrdinal("act_group"));
            string act_date = a.GetDateTime(a.GetOrdinal("act_date")).ToString();
            string act_time = a.GetTimeSpan(a.GetOrdinal("act_time")).ToString();
            int act_type = a.GetInt32(a.GetOrdinal("act_type"));

            MyEvent result = new MyEvent(action_id, user_cr, act_group, act_date, act_time, act_type);
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

            command.CommandText = "SELECT action_id, user_cr, act_group, act_date, act_time, act_type FROM Main;";
            SqlDataReader a = command.ExecuteReader();

            while (a.Read() == true)
            {
                int action_id = a.GetInt32(a.GetOrdinal("action_id"));
                string user_cr = a.GetString(a.GetOrdinal("user_cr"));
                string act_group = a.GetString(a.GetOrdinal("act_group"));
                string act_date = a.GetDateTime(a.GetOrdinal("act_date")).ToString();
                string act_time = a.GetTimeSpan(a.GetOrdinal("act_time")).ToString();
                int act_type = a.GetInt32(a.GetOrdinal("act_type"));

                MyEvent e = new MyEvent(action_id, user_cr, act_group, act_date, act_time, act_type);
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

        if (TargetMessage.Substring(0, 5) == "!help")
        {
            await chan.SendMessageAsync("Сообщение для записи должно быть в формате !add @*группа пользователей* first day: *первый день упоминаний* time: *время упоминаний* type: *тип упоминаний* message: *сообщение при упоминании*");
            await chan.SendMessageAsync("*Требуется сохранить все пробелы, а курсивный текст заменить на свой*");
            await chan.SendMessageAsync("Формат даты: dd-mm-yyyy, Формат времени: hh:mm:ssss (Секунды можно не указывать)");
            await chan.SendMessageAsync("Пример: !add ***@Лемминги*** first day: ***10-10-2023*** time: ***10:10*** type: ***1*** message: ***Vsem ku!***");
        }
        else if (TargetMessage.Substring(0, 4) == "!add")
        {
            int ToFind = TargetMessage.IndexOf("@");
            int EndOf = TargetMessage.IndexOf(" ", ToFind);
            string UsersGroup = TargetMessage.Substring(ToFind, EndOf - ToFind); //Находит группу пользователей по собачке
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

            if(UsersGroup != null && TargetDate != null && TargetTime != null && ActType != null && UserMessage != null){
                await chan.SendMessageAsync("Успешно!");
                add_event("Server=localhost;Database=db1;Trusted_Connection=True;", message.Author.ToString(), UsersGroup, TargetDate, TargetTime, int.Parse(ActType), UserMessage);
            }
            else{
                await chan.SendMessageAsync("Что-то пошло не так!");
            }

            
        }
    }


    static void Main(string[] args)
    {

        create_db();
        //add_event("Server=localhost;Database=db1;Trusted_Connection=True;", "abobaNew", "3", "2010-10-10", "10:10:10", 10);


        //MyEvent result = get_eventdata("Server=localhost;Database=db1;Trusted_Connection=True;", 6);

        List<MyEvent> result1 = get_alldata("Server=localhost;Database=db1;Trusted_Connection=True;");


        Task.WaitAny(doWork());

    }
    //ODU5MTE5MzM0NzY4NzcxMTIy.GuSbAn.522ODH3wL2b9kiIX7PjJrlKKf3faIDlGl3iW5c
}

