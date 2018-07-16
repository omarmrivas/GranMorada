module DB

open System
open System.IO
open System.Data.SQLite

let DB_NAME = "privadalaurel.sqlite"

let connectionString = sprintf "Data Source=%s;Version=3;" DB_NAME  

let createHistoryCmd =
    "CREATE TABLE History (" +
    "phone char(10), " +
    "timestamp datetime)"

let createUsersCmd =
    "CREATE TABLE Users (" +
    "phone char(10) PRIMARY KEY, " +
    "name varchar(200), " +
    "house varchar(3), " + 
    "status varchar(1))"

let selectUsersCmd = function
    | Some phone -> "SELECT * FROM Users WHERE phone='" + phone + "'"
    | None -> "SELECT * FROM Users"

let selectHistoryCmd =
    "SELECT * FROM History"

let insertHistoryCmd = 
    "INSERT INTO History(phone, timestamp) " + 
    "VALUES (@Phone, @Timestamp)"

let insertUserCmd = 
    "INSERT INTO Users(phone, name, house, status) " + 
    "VALUES (@Phone, @Name, @House, @Status)"

let updateUserCmd = 
    "UPDATE Users " +
    "SET name=@Name, house=@House, status=@Status " + 
    "WHERE phone=@Phone"

type historyEntry =
    {
        phone     : string
        timestamp : DateTime
    }

type userEntry =
    {
        phone  : string
        name   : string
        house  : string
        status : string
    }

let selectUser sphone =
    use connection = new SQLiteConnection(connectionString)
    connection.Open()
    use command = new SQLiteCommand(selectUsersCmd sphone, connection)
    let reader = command.ExecuteReader()
    let res = [while reader.Read() do
                yield {
                    phone  = reader.["phone"].ToString()
                    name   = reader.["name"].ToString()
                    house  = reader.["house"].ToString()
                    status = reader.["status"].ToString()
                }]
    res

let selectHistory () =
    use connection = new SQLiteConnection(connectionString)
    connection.Open()
    use command = new SQLiteCommand(selectHistoryCmd, connection)
    let reader = command.ExecuteReader()
    let res = [while reader.Read() do
                yield {
                phone  = reader.["phone"].ToString()
                timestamp = System.Convert.ToDateTime(reader.["timestamp"])
                }]
    connection.Close()
    res

let addUser x =
    use connection = new SQLiteConnection(connectionString)
    connection.Open()
    use command = new SQLiteCommand(insertUserCmd, connection)
    command.Parameters.AddWithValue("@phone", x.phone) |> ignore
    command.Parameters.AddWithValue("@name", x.name) |> ignore
    command.Parameters.AddWithValue("@house", x.house) |> ignore
    command.Parameters.AddWithValue("@status", x.status) |> ignore
    let res = command.ExecuteNonQuery()
    connection.Close()
    res

let updateUser x =
    use connection = new SQLiteConnection(connectionString)
    connection.Open()
    use command = new SQLiteCommand(updateUserCmd, connection)
    command.Parameters.AddWithValue("@phone", x.phone) |> ignore
    command.Parameters.AddWithValue("@name", x.name) |> ignore
    command.Parameters.AddWithValue("@house", x.house) |> ignore
    command.Parameters.AddWithValue("@status", x.status) |> ignore
    let res = command.ExecuteNonQuery()
    connection.Close()
    res

let addAccess (x:historyEntry) =
    use connection = new SQLiteConnection(connectionString)
    connection.Open()
    use command = new SQLiteCommand(insertHistoryCmd, connection)
    command.Parameters.AddWithValue("@phone", x.phone) |> ignore
    command.Parameters.AddWithValue("@timestamp", x.timestamp) |> ignore
    let res = command.ExecuteNonQuery()
    connection.Close()
    res

let createDB () =
    if File.Exists DB_NAME
    then ()
    else SQLiteConnection.CreateFile(DB_NAME)
         let connection = new SQLiteConnection(connectionString)
         connection.Open()
         let historyCommand = new SQLiteCommand(createHistoryCmd, connection)
         historyCommand.ExecuteNonQuery() |> ignore
         let usersCommand = new SQLiteCommand(createUsersCmd, connection)
         usersCommand.ExecuteNonQuery() |> ignore
         let user = {
             phone = "4444198896"
             name = "Omar Montaño Rivas"
             house = "117"
             status = "1"
         }

         addUser user |> ignore
