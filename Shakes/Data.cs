namespace Shakes_3;

public static class Data
{
    public static int TempUserId;
    public static bool ClientIsUpToDate;
    public static bool QuestCompleted = true;
    public static bool QuestAssigned = false;
}

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public List<Character> Characters { get; set; }
    public User(string username, int userid)
    {
        Username = username;
        UserId = userid;
        Characters = new List<Character>();
    }
}

public class Leaderboard
{
    public string CharName { get; set; }
    public int Level { get; set; }
    public string OwnerName { get; set; }
}

public class Character
{
    public User Owner { get; set; }
    public int CharId { get; set; }
    public string Name { get; set; }
    public string CharClass { get; set; }
    public int CharDifficulty { get; set; }
    public int Level { get; set; }
    public int CharVit { get; set; }
    public int CharStr { get; set; }
    public int CharInt { get; set; }
    public int CharDex { get; set; }
    public int CharLuck { get; set; }
    public int CharMushrooms { get; set; }
    public int CharGold { get; set; }
    public long CharXp { get; set; }
    public List<Quest> CharQuests { get; set; }
    public double CharMana { get; set; }
    public List<int> CharQuestsId { get; set; }


    public Character
    (User owner,
        int characterId,
        string name,
        string characterClass,
        int characterDifficulty,
        int level,
        int vitality,
        int strength,
        int intelligence,
        int dexterity,
        int luck,
        int mushrooms,
        int gold,
        long experience,
        double mana,
        List<int> questsId
    )

    {
        Owner = owner;
        CharId = characterId;
        Name = name;
        CharClass = characterClass;
        CharDifficulty = characterDifficulty;
        Level = level;
        CharVit = vitality;
        CharStr = strength;
        CharInt = intelligence;
        CharDex = dexterity;
        CharLuck = luck;
        CharMushrooms = mushrooms;
        CharGold = gold;
        CharXp = experience;
        CharQuests = new List<Quest>();
        CharMana = mana;
        CharQuestsId = questsId;
    }
    public enum attributeType
    {
        Vitality,
        Strength,
        Intelligence,
        Dexterity,
        Luck
    }
}

public static class QuestData
{
    public static List<Quest> GameQuests { get; set; }

    static QuestData()
    {
        GameQuests = new List<Quest>();
    }
}

public class Quest
{
    public Character Owner { get; set; }
    public int QuestId { get; set; }
    public string? QuestLocation { get; set; }
    public int QuestTime { get; set; }
    public long QuestXp { get; set; }
    public int QuestGold { get; set; }
    public int QuestMushrooms { get; set; }


    public Quest
    (
        Character owner,
        int questId,
        string? questLocation,
        int questTime,
        long questXp,
        int questGold,
        int questMushrooms
    )

    {
        Owner = owner;
        QuestId = questId;
        QuestLocation = questLocation;
        QuestTime = questTime;
        QuestXp = questXp;
        QuestGold = questGold;
        QuestMushrooms = questMushrooms;
    }
}
