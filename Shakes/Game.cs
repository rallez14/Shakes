using MySql.Data.MySqlClient;

namespace Shakes_3;

public class Game
{
    private readonly Database _db = new();
    private User user;

    public async Task MainMenu()
    {
        Console.Clear();
        _db.Connect();
        GetQuest(GetQuestIdList());
        // _db.Disconnect();
        incorrectChoice:
        Console.Clear();
        Console.WriteLine("SHAKES 3");
        Console.WriteLine();
        Console.WriteLine("1. Logowanie");
        Console.WriteLine("2. Rejestracja");
        Console.WriteLine("3. Ranking bohaterów");
        Console.WriteLine("4. Wyjdź z gry");
        var option = Console.ReadKey().KeyChar;
        switch (option)
        {
            case '1':
                await Login();
                break;
            case '2':
                await Register();
                break;
            case '3':
                await Leaderboard();
                break;
            case '4':
                Environment.Exit(0);
                break;
            default:
                Console.Clear();
                Console.WriteLine("Nie ma takiej opcji, wracasz do menu...");
                Thread.Sleep(2000);
                goto incorrectChoice;
        }
    }

    private static string ValidatePassword(string password)
    {
        var hiddenPassword = password;
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);
            if (char.IsLetterOrDigit(key.KeyChar) && !char.IsControl(key.KeyChar))
            {
                hiddenPassword += key.KeyChar;
                Console.Write("*");
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace && hiddenPassword.Length > 0)
                {
                    hiddenPassword = hiddenPassword.Substring(0, hiddenPassword.Length - 1);
                    Console.Write("\b \b");
                }
            }
        } while (key.Key != ConsoleKey.Enter);

        return hiddenPassword;
    }

    private static string ValidateName(string name)
    {
        string validatedName = name;
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);
            if (char.IsLetterOrDigit(key.KeyChar) && !char.IsControl(key.KeyChar) && !char.IsWhiteSpace(key.KeyChar))
            {
                validatedName += key.KeyChar;
                Console.Write(key.KeyChar);
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace && validatedName.Length > 0)
                {
                    validatedName = validatedName.Substring(0, validatedName.Length - 1);
                    Console.Write("\b \b");
                }
            }
        } while (key.Key != ConsoleKey.Enter);

        return validatedName;
    }

    private async Task Login()
    {
        incorrectAccountDetails:
        var providedUsername = "";
        var providedPassword = "";

        Console.Clear();
        Console.WriteLine("LOGOWANIE");
        Console.WriteLine("");
        Console.Write("Podaj login: ");
        providedUsername = ValidateName(providedUsername).ToLower();
        Console.WriteLine("");
        Console.Write("Podaj hasło: ");

        providedPassword = ValidatePassword(providedPassword);
        // _db.Connect();
        Console.WriteLine();
        Console.WriteLine("Logowanie...");
        if (providedUsername.Length < 1 || providedPassword.Length < 1)
        {
            Console.WriteLine();
            Console.WriteLine("Nieprawidłowe dane! Spróbuj ponowanie...");
            Thread.Sleep(2000);
            // _db.Disconnect();
            goto incorrectAccountDetails;
        }
        else
        {
            using (var command = new MySqlCommand("SELECT password, user_id FROM player_data WHERE username = @providedUsername", _db.Cnn))
            {
                command.Parameters.AddWithValue("@providedUsername", providedUsername);

                await using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string hashedPassword = reader.GetString(0);
                        int userId = reader.GetInt32(1);

                        if (hashedPassword != null && BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword))
                        {
                            reader.Close();
                            Data.TempUserId = userId;
                            user = new User(providedUsername, Data.TempUserId);
                            Console.WriteLine("test1");
                            Console.WriteLine("test");
                            GetCharacter(GetCharacterIdList(user), user);
                            await CharacterSelection(user);
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("Nieprawidłowe dane! Spróbuj ponowanie...");
                            Thread.Sleep(2000);
                            // _db.Disconnect();
                            goto incorrectAccountDetails;
                        }
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("Nieprawidłowe dane! Spróbuj ponowanie...");
                        Thread.Sleep(2000);
                        // _db.Disconnect();
                        goto incorrectAccountDetails;
                    }
                }
            }
        }
    }
    
    private async Task Logout(User user, Character character)
    {
        user.Characters.Clear();
        user = null;
        character = null;
        _db.Disconnect();
        await MainMenu();
    }

    private async Task Register()
    {
        register:
        var providedUsername = "";
        var providedPassword = "";
        var providedConfirmPassword = "";

        Console.Clear();
        Console.WriteLine("REJESTRACJA");
        Console.WriteLine("");
        Console.Write("Podaj login: ");
        providedUsername = ValidateName(providedUsername);

        if ((providedUsername.Length > 30) | (providedUsername.Length < 6))
        {
            Console.WriteLine("");
            Console.WriteLine("Login musi mieć od 6 do 30 znaków!");
            Thread.Sleep(2000);
            goto register;
        }

        Console.WriteLine("");
        Console.Write("Podaj hasło: ");
        providedPassword = ValidatePassword(providedPassword);
        Console.WriteLine("");
        Console.Write("Podaj hasło ponownie: ");
        providedConfirmPassword = ValidatePassword(providedConfirmPassword);
        Console.WriteLine("");

        if (providedPassword != providedConfirmPassword)
        {
            Console.WriteLine("Hasła nie są takie same!");
            Thread.Sleep(2000);
            goto register;
        }

        if ((providedPassword.Length > 30) | (providedPassword.Length < 8))
        {
            Console.WriteLine("Hasło musi mieć od 8 do 30 znaków");
            Thread.Sleep(2000);
            goto register;
        }

        string salt = BCrypt.Net.BCrypt.GenerateSalt(16);
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(providedPassword, salt);

        // _db.Connect();
        using (var command = new MySqlCommand("SELECT * FROM player_data WHERE username = @providedUsername", _db.Cnn))
        {
            command.Parameters.AddWithValue("@providedUsername", providedUsername);

            await using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    Console.Clear();
                    Console.WriteLine("Podany login jest zajęty!"); // DEV
                    Thread.Sleep(2000);
                    reader.Close();
                    // _db.Disconnect();
                    await Register();
                }
                else
                {
                    reader.Close();
                    await using var command2 =
                        new MySqlCommand(
                            "INSERT INTO player_data (username, password) VALUES (@providedUsername, @hashedPassword)",
                            _db.Cnn);
                    command2.Parameters.AddWithValue("@providedUsername", providedUsername);
                    command2.Parameters.AddWithValue("@hashedPassword", hashedPassword);

                    var rowsAffected = command2.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Rejestracja przebiegła pomyślnie!");
                        Thread.Sleep(2000);
                        Console.Clear();
                        await Login();
                    }
                    else
                    {
                        Console.WriteLine("Rejestracja nie powiodła się!");
                        Thread.Sleep(2000);
                        Console.Clear();
                        await MainMenu();
                    }
                }
            }
        }
        // _db.Disconnect();
    }

    private void GetCharacter(List<int> characterIdList, User user)
    {
        foreach (var charId in characterIdList)
        {
            Character character = null;
            using (var command = new MySqlCommand("SELECT * FROM character_data WHERE char_id = @charId", _db.Cnn))
            {
                command.Parameters.AddWithValue("@charId", charId);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        List<int> questIds = new List<int>();
                        for (int i = 1; i <= 4; i++)
                        {
                            if (reader[$"char_quest{i}"] != DBNull.Value)
                            {
                                int questId = Convert.ToInt32(reader[$"char_quest{i}"]);
                                questIds.Add(questId);
                            }
                        }
                        
                        character = new Character(
                            user,
                            Convert.ToInt32(reader["char_id"]),
                            reader["char_name"].ToString(),
                            reader["char_class"].ToString(),
                            Convert.ToInt32(reader["char_difficulty"]),
                            Convert.ToInt32(reader["char_level"]),
                            Convert.ToInt32(reader["char_vit"]),
                            Convert.ToInt32(reader["char_str"]),
                            Convert.ToInt32(reader["char_int"]),
                            Convert.ToInt32(reader["char_dex"]),
                            Convert.ToInt32(reader["char_luck"]),
                            Convert.ToInt32(reader["char_mushrooms"]),
                            Convert.ToInt32(reader["char_gold"]),
                            Convert.ToInt64(reader["char_xp"]),
                            Convert.ToInt32(reader["char_mana"]),
                            questIds
                        );
                        user.Characters.Add(character);
                    }
                }
            }
        }
    }
    
    private List<int> GetCharacterIdList(User user)
    {
        var charactersIdList = new List<int>();

        using var command = new MySqlCommand("SELECT char_id FROM player_char_data WHERE user_id = @UserId", _db.Cnn);
        command.Parameters.AddWithValue("@UserId", user.UserId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            charactersIdList.Add(Convert.ToInt32(reader["char_id"]));
        }

        return charactersIdList;
    }

    private async Task CharacterSelection(User user)
    {
        Data.QuestAssigned = false;
        Character activeCharacter = null;

        Console.Clear();
        Console.WriteLine("SHAKES MENU || konto: " + user.Username); // DEV
        Console.WriteLine("");
        Console.WriteLine("Wybierz postać:");
        Console.WriteLine("===============");

        if (user.Characters.Count > 0)
        {
            var character1 = user.Characters[0];
            Console.WriteLine("1. " + user.Characters[0].Name);
        }
        else
        {
            Console.WriteLine("1. Wolny slot");
        }

        if (user.Characters.Count > 1)
        {
            var character2 = user.Characters[1];
            Console.WriteLine("2. " + user.Characters[1].Name);
        }
        else
        {
            Console.WriteLine("2. Wolny slot");
        }

        if (user.Characters.Count > 2)
        {
            var character3 = user.Characters[2];
            Console.WriteLine("3. " + user.Characters[2].Name);
        }
        else
        {
            Console.WriteLine("3. Wolny slot");
        }

        if (user.Characters.Count > 3)
        {
            var character4 = user.Characters[3];
            Console.WriteLine("4. " + user.Characters[3].Name);
        }
        else if (user.Characters.Count < 4)
        {
            Console.WriteLine("4. Wolny slot");
        }

        if (user.Characters.Count < 4)
        {
            Console.WriteLine("===============");
            Console.WriteLine("5. Nowa postać");
        }
        else
        {
            Console.WriteLine("===============");
        }

        Console.WriteLine("6. Wyloguj");

        here:
        var option = Console.ReadKey().KeyChar;
        switch (option)
        {
            case '1':
                if (user.Characters.Any())
                {
                    activeCharacter = user.Characters[0];
                    await GameMenu(activeCharacter);
                    break;
                }
                else
                {
                    goto here;
                }
            case '2':
                if (user.Characters.Count() > 1)
                {
                    activeCharacter = user.Characters[1];
                    await GameMenu(activeCharacter);
                    break;
                }
                else
                {
                    goto here;
                }
            case '3':
                if (user.Characters.Count() > 2)
                {
                    activeCharacter = user.Characters[2];
                    await GameMenu(activeCharacter);
                    break;
                }
                else
                {
                    goto here;
                }
            case '4':
                if (user.Characters.Count() > 3)
                {
                    activeCharacter = user.Characters[3];
                    await GameMenu(activeCharacter);
                    break;
                }
                else
                {
                    goto here;
                }
            case '5':
                if (user.Characters.Count() < 4)
                {
                    await CreateCharacter(user);
                    break;
                }
                else
                {
                    goto here;
                }
            case '6':
                await Logout(user, activeCharacter);
                break;
            default:
                goto here;
        }
    }

    private static int AttributePointPrice(double attributeValue)
    {
        const double targetGold = 10000000;
        const double targetLevel = 300;
        var scaleFactor = Math.Log10(targetGold) / targetLevel;
        var price = Math.Round(Math.Pow(10, attributeValue * scaleFactor));
        return (int)price;
    }

    private static void PurchaseAttributePoint(Character character, int price, Enum attributeType)
    {
        if (price <= character.CharGold)
        {
            character.CharGold -= price;
            switch (attributeType)
            {
                case Character.attributeType.Vitality:
                    character.CharVit++;
                    break;
                case Character.attributeType.Strength:
                    character.CharStr++;
                    break;
                case Character.attributeType.Intelligence:
                    character.CharInt++;
                    break;
                case Character.attributeType.Dexterity:
                    character.CharDex++;
                    break;
                case Character.attributeType.Luck:
                    character.CharLuck++;
                    break;
            }
        }
    }

    private void UploadAttributeToDb(Character character)
    {
        using (var command = new MySqlCommand("UPDATE character_data SET char_vit = @CharVit, char_str = @CharStr, char_int = @CharInt, char_dex = @CharDex, char_luck = @CharLuck, char_gold = @CharGold WHERE char_id = @charId", _db.Cnn))
        {
            command.Parameters.AddWithValue("@charId", character.CharId);
            command.Parameters.AddWithValue("@CharVit", character.CharVit);
            command.Parameters.AddWithValue("@CharStr", character.CharStr);
            command.Parameters.AddWithValue("@CharInt", character.CharInt);
            command.Parameters.AddWithValue("@CharDex", character.CharDex);
            command.Parameters.AddWithValue("@CharLuck", character.CharLuck);
            command.Parameters.AddWithValue("@CharGold", character.CharGold);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Console.WriteLine("Wystąpił problem z przydzielaniem punktów!");
            }
        }
    }
    
    private async Task CharacterPreview(Character character)
    {
        int vitCount = 0, strCount = 0, intCount = 0, dexCount = 0, luckCount = 0;
        
        refresh:
        Console.Clear();
        Console.WriteLine("SHAKES PRZEGLĄD POSTACI || Postać: " + character.Name); // DEV
        Console.WriteLine();
        Console.WriteLine("Poziom: " + character.Level);
        Console.WriteLine("Złoto: " + character.CharGold);
        Console.WriteLine("Witalność: " + character.CharVit);
        Console.WriteLine("Siła: " + character.CharStr);
        Console.WriteLine("Inteligencja: " + character.CharInt);
        Console.WriteLine("Zręczność: " + character.CharDex);
        Console.WriteLine("Szczęście: " + character.CharLuck);
        Console.WriteLine();
        Console.WriteLine($"1. Dodaj WIT za {AttributePointPrice(character.CharVit)}");
        Console.WriteLine($"2. Dodaj SIŁ za {AttributePointPrice(character.CharStr)}");
        Console.WriteLine($"3. Dodaj INT za {AttributePointPrice(character.CharInt)}");
        Console.WriteLine($"4. Dodaj ZRE za {AttributePointPrice(character.CharDex)}");
        Console.WriteLine($"5. Dodaj SZCZ za {AttributePointPrice(character.CharLuck)}");
        Console.WriteLine("=====================");
        Console.WriteLine("6. Wyjdź");


        here:
        var option = Console.ReadKey().KeyChar;
        switch (option)
        {
            case '1':
                PurchaseAttributePoint(character, AttributePointPrice(character.CharVit),
                    Character.attributeType.Vitality);
                vitCount++;
                goto refresh;
            case '2':
                PurchaseAttributePoint(character, AttributePointPrice(character.CharStr),
                    Character.attributeType.Strength);
                strCount++;
                goto refresh;
            case '3':
                PurchaseAttributePoint(character, AttributePointPrice(character.CharInt),
                    Character.attributeType.Intelligence);
                intCount++;
                goto refresh;
            case '4':
                PurchaseAttributePoint(character, AttributePointPrice(character.CharDex),
                    Character.attributeType.Dexterity);
                dexCount++;
                goto refresh;
            case '5':
                PurchaseAttributePoint(character, AttributePointPrice(character.CharLuck),
                    Character.attributeType.Luck);
                luckCount++;
                goto refresh;
            case '6':
                UploadAttributeToDb(character);
                await GameMenu(character);
                break;
            default:
                goto here;
        }
    }

    private async Task GameMenu(Character character)
    {
        Console.Clear();
        Console.WriteLine("SHAKES MENU || Postać: " + character.Name); // DEV
        Console.WriteLine();
        Console.WriteLine("===============");
        Console.WriteLine("1. Karczma");
        Console.WriteLine("2. Warta");
        Console.WriteLine("3. Lochy");
        Console.WriteLine("4. Twoja postać");
        Console.WriteLine("===============");
        Console.WriteLine("5. Wyjdź");
        here:
        var option = Console.ReadKey().KeyChar;
        switch (option)
        {
            case '1':
                await Tavern(character);
                break;
            case '2':
                Console.WriteLine("WARTA");
                break;
            case '3':
                Console.WriteLine("LOCHY");
                break;
            case '4':
                await CharacterPreview(character);
                break;
            case '5':
                await CharacterSelection(user);
                break;
            default:
                goto here;
        }
    }

    private async Task CreateCharacter(User user)
    {
        characterName:
        string providedCharacterName = "";
        string providedCharacterClass = "";
        int providedCharacterDifficulty;
        Character newCharacter = null;
        Console.Clear();
        Console.WriteLine("SHAKES MENU || konto: " + user.Username); // DEV
        Console.WriteLine();
        Console.Write("Nazwa postaci: ");
        providedCharacterName = ValidateName(providedCharacterName);
        if (providedCharacterName.Length > 16 | providedCharacterName.Length < 5)
        {
            Console.WriteLine("");
            Console.WriteLine("Nazwa musi mieć od 5 do 16 znaków!");
            Thread.Sleep(2000);
            Console.Clear();
            goto characterName;
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Klasa postaci:");
        Console.WriteLine("1. Wojownik:");
        Console.WriteLine("2. Czarodziej:");
        Console.WriteLine("3. Zwiadowca:");
        Console.Write("Wybór: ");
        provideClass:
        var option = Console.ReadKey().KeyChar;
        switch (option)
        {
            case '1':
                providedCharacterClass = "Wojownik";
                break;
            case '2':
                providedCharacterClass = "Czarodziej";
                break;
            case '3':
                providedCharacterClass = "Zwiadowca";
                break;
            default:
                goto provideClass;
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Poziom trudności:");
        Console.WriteLine("1. Łatwy:");
        Console.WriteLine("2. Średni:");
        Console.WriteLine("3. Trudny:");
        Console.Write("Wybór: ");
        provideDifficulty:
        var option2 = Console.ReadKey().KeyChar;
        switch (option2)
        {
            case '1':
                providedCharacterDifficulty = 1;
                break;
            case '2':
                providedCharacterDifficulty = 2;
                break;
            case '3':
                providedCharacterDifficulty = 3;
                break;
            default:
                goto provideDifficulty;
        }

        int lastCharId = 11111;
        await using (var command =
                     new MySqlCommand("SELECT char_id FROM character_data ORDER BY char_id DESC LIMIT 1", _db.Cnn))
        {
            await using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    lastCharId = Convert.ToInt32(reader["char_id"]);
                }
            }
        }

        var nullQuestIdList = new List<int>() { 0, 0, 0, 0 };
        newCharacter = new Character(user, (lastCharId + 1), providedCharacterName, providedCharacterClass,
            providedCharacterDifficulty, 1, 1, 1, 1, 1, 1, 15, 10, 0, 100, nullQuestIdList) ;
        user.Characters.Add(newCharacter);
        try
        {
            await using (var command =
                         new MySqlCommand("INSERT INTO character_data (char_name, char_class, char_difficulty, char_owner) VALUES (@CharName, @CharClass, @CharDifficulty, @CharOwner)", _db.Cnn))
            {
                command.Parameters.AddWithValue("@CharName", providedCharacterName);
                command.Parameters.AddWithValue("@CharClass", providedCharacterClass);
                command.Parameters.AddWithValue("@CharDifficulty", providedCharacterDifficulty);
                command.Parameters.AddWithValue("@CharOwner", user.Username);
                var rowsAffected = command.ExecuteNonQuery();
            }

            await using (var command =
                         new MySqlCommand("INSERT INTO player_char_data (char_id, user_id) VALUES (@CharId, @UserId)",
                             _db.Cnn))
            {
                command.Parameters.AddWithValue("@CharId", (lastCharId + 1));
                command.Parameters.AddWithValue("@UserId", user.UserId);
                var rowsAffected = command.ExecuteNonQuery();
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine(": " + ex.Message); // DEV
        }

        Console.Clear();
        await CharacterSelection(user);
    }

    private async Task Leaderboard()
    {
        refresh:
        _db.Disconnect();
        Console.Clear();
        _db.Connect();
        List<Leaderboard> leaderboard = new List<Leaderboard>();
        await using (var command =
                     new MySqlCommand(
                         "SELECT char_name, char_level, char_owner FROM character_data ORDER BY char_level DESC LIMIT 15",
                         _db.Cnn))
        {
            await using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    leaderboard.Add(new Leaderboard
                    {
                        CharName = reader.GetString(0),
                        Level = reader.GetInt32(1),
                        OwnerName = reader.GetString(2)
                    });
                }
            }
        }

        Console.WriteLine("Ranking postaci: ");
        Console.WriteLine();
        int rank = 1;
        foreach (var element in leaderboard)
        {
            Console.WriteLine("Top {0} {1} - Level {2} - {3}", rank, element.CharName, element.Level,
                element.OwnerName);
            rank++;
        }

        _db.Disconnect();
        Console.WriteLine();
        Console.WriteLine("1. Odśwież");
        Console.WriteLine("2. Wyjdź");
        invlaidOption:
        var option = Console.ReadKey().KeyChar;
        switch (option)
        {
            case '1':
                goto refresh;
            case '2':
                await MainMenu();
                break;
            default:
                goto invlaidOption;
        }
    }

    private List<int> GetQuestIdList()
    {
        var questIdList = new List<int>();

        using var command = new MySqlCommand("SELECT quest_id FROM quest_data", _db.Cnn);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            questIdList.Add(Convert.ToInt32(reader["quest_id"]));
        }

        return questIdList;
    }

    private void GetQuest(List<int> questIdList)
    {
        foreach (var questId in questIdList)
        {
            using (var command = new MySqlCommand("SELECT * FROM quest_data WHERE quest_id = @questId", _db.Cnn))
            {
                command.Parameters.AddWithValue("@questId", questId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var quest = new Quest(
                            null,
                            Convert.ToInt32(reader["quest_id"]),
                            reader["quest_location"].ToString(),
                            Convert.ToInt32(reader["quest_time"]),
                            Convert.ToInt64(reader["quest_xp"]),
                            Convert.ToInt32(reader["quest_gold"]),
                            Convert.ToInt32(reader["quest_mushrooms"])
                        );
                        QuestData.GameQuests.Add(quest);
                    }
                }
            }
        }
    }

    private static List<Quest> GenerateRandomQuestList()
    {
        var randomNumbers = new List<int>();
        var random = new Random((int)DateTime.Now.Ticks);
        while (randomNumbers.Count < 4)
        {
            var num = random.Next(QuestData.GameQuests.Count);
            if (!randomNumbers.Contains(num))
            {
                randomNumbers.Add(num);
            }
        }

        return randomNumbers.Select(num => QuestData.GameQuests[num]).ToList();
    }

    private static void BalanceQuest(List<Quest> questsToBalance, Character character)
    {
        foreach (var quest in questsToBalance)
        {
            quest.QuestXp = character.CharDifficulty * quest.QuestXp * character.Level;
            quest.QuestGold = character.CharDifficulty * quest.QuestGold * character.Level;
            quest.QuestMushrooms = character.CharDifficulty * quest.QuestMushrooms * character.Level;
        }
    }
    
    private void AssignQuests(Character character, List<Quest> randomQuests)
    {
        if (character.CharQuestsId[0] != 0) // download already assigned quest from db
        {
            for (int i = 0; i <= 3; i++)
            {
                foreach (var quest in QuestData.GameQuests)
                {
                    if (character.CharQuestsId[i] == quest.QuestId)
                    {
                        character.CharQuests.Add(quest);
                    }
                }
            }
            
            BalanceQuest(character.CharQuests, character);
        }
        else // generate new quests
        {
            BalanceQuest(randomQuests, character);

            foreach (var quest in randomQuests)
            {
                character.CharQuests.Add(quest);
            }
            
            using (var command = new MySqlCommand("UPDATE character_data SET char_quest1 = @questId1, char_quest2 = @questId2, char_quest3 = @questId3, char_quest4 = @questId4 WHERE char_id = @charId", _db.Cnn))
            {
                command.Parameters.AddWithValue("@charId", character.CharId);
                command.Parameters.AddWithValue("@questId1", (character.CharQuests.Count > 0) ? character.CharQuests[0].QuestId : 0);
                command.Parameters.AddWithValue("@questId2", (character.CharQuests.Count > 1) ? character.CharQuests[1].QuestId : 0);
                command.Parameters.AddWithValue("@questId3", (character.CharQuests.Count > 2) ? character.CharQuests[2].QuestId : 0);
                command.Parameters.AddWithValue("@questId4", (character.CharQuests.Count > 3) ? character.CharQuests[3].QuestId : 0);

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    Console.WriteLine("Wystąpił problem z przydzielaniem misji!");
                }
            }
        }
    }
    
    private async Task Tavern(Character character)
    {
        Console.Clear();
        Console.WriteLine("SHAKES KARCZMA || Postać: " + character.Name);
        Console.WriteLine();
        if (Data.QuestCompleted)
        {
            if (Data.QuestAssigned == false)
            {
                AssignQuests(character, GenerateRandomQuestList());
                Data.QuestAssigned = true;
            }
            var i = 1;
            foreach (var quest in character.CharQuests)
            {
                Console.WriteLine("Zadanie {0}.", i);
                Console.WriteLine("=============================");
                Console.WriteLine("Lokacja: " + quest.QuestLocation);
                Console.WriteLine("Złoto: " + quest.QuestGold);
                Console.WriteLine("Doświadczenie: " + quest.QuestXp);
                Console.WriteLine($"Czas wykonywania: {(double)quest.QuestTime / 60}m");
                Console.WriteLine("=============================");
                Console.WriteLine();
                i++;
            }

            Console.WriteLine("5. Wyjdź");
            
            here:
            var option = Console.ReadKey().KeyChar;
            switch (option)
            {
                case '1':
                    // Console.WriteLine("Wybrano misje 1");
                    var time = await GetServerTime();
                    Console.WriteLine($"Czas: {time}");
                    break;
                case '2':
                    Console.WriteLine("Wybrano misje 2");
                    break;
                case '3':
                    Console.WriteLine("Wybrano misje 3");
                    break;
                case '4':
                    Console.WriteLine("Wybrano misje 4");
                    break;
                case '5':
                    await GameMenu(character);
                    break;
                default:
                    goto here;
            }
        }
    }
    
    private static async Task<int> GetServerTime()
    {
        using (var client = new HttpClient())
        {
            try
            {
                var response = await client.GetAsync("http://51.83.134.230/");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                var serverTime = int.Parse(result);
                return serverTime;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Błąd: " + ex.Message);
                throw;
            }
        }
    }
    
    
}