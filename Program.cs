using System.Collections;
using System.Configuration;
using MyTestProject;
using Npgsql;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

var playerDao = new PlayerDao();


var players = playerDao.getPlayers();

foreach (Player player in players)
{
        Console.WriteLine(player.Id + "/" + player.Name);
}



string token = File.ReadAllText($"{Environment.CurrentDirectory}/token.txt");
// replace YOUR_BOT_TOKEN below, or set your TOKEN in Project Properties > Debug > Launch profiles UI > Environment variables
//var token = Environment.GetEnvironmentVariable("TOKEN") ?? "";

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(token, cancellationToken: cts.Token);
var me = await bot.GetMeAsync();
await bot.DropPendingUpdatesAsync();
bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel(); // stop the bot

async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception);
    await Task.Delay(2000, cts.Token);
}

async Task OnMessage(Message msg, UpdateType type)
{
    var message = await bot.SendTextMessageAsync(msg.Chat, "Trying <b>all the parameters</b> of <code>sendMessage</code> method",
        parseMode: ParseMode.Html,
        protectContent: true,
        replyParameters: msg.MessageId,
        replyMarkup: new InlineKeyboardMarkup(
            InlineKeyboardButton.WithUrl("Check sendMessage method", "https://core.telegram.org/bots/api#sendmessage")));
    
    await bot.SendTextMessageAsync(msg.Chat, "Hello, World!");
    
    Console.WriteLine(msg.Text);
    
    Console.WriteLine(message.Chat.Username);
    Console.WriteLine(message.Chat.FirstName);
    Console.WriteLine(message.Chat.LastName);
    Console.WriteLine(message.Chat.Title);
    
   // var player = new Player(5, message.Chat.Username);
   // playerDao.setPlayer(player);
    
    Console.WriteLine(
        $"{message.From.FirstName} sent message {message.MessageId} " +
        $"to chat {message.Chat.Id} at {message.Date.ToLocalTime()}. " +
        $"It is a reply to message {message.ReplyToMessage.MessageId} " +
        $"and has {message.Entities.Length} message entities.");

   // await bot.SendTextMessageAsync(msg.Chat, "Hello, World!");
  //await bot.SendStickerAsync(msg.Chat, "https://telegrambots.github.io/book/docs/sticker-dali.webp");
  //  await bot.SendVideoAsync(msg.Chat, "https://telegrambots.github.io/book/docs/video-hawk.mp4");


    
    if (msg.Text is not { } text)
        Console.WriteLine($"Received a message of type {msg.Type}");
    else if (text.StartsWith('/'))
    {
        var space = text.IndexOf(' ');
        if (space < 0) space = text.Length;
        var command = text[..space].ToLower();
        if (command.LastIndexOf('@') is > 0 and int at) // it's a targeted command
            if (command[(at + 1)..].Equals(me.Username, StringComparison.OrdinalIgnoreCase))
                command = command[..at];
            else
                return; // command was not targeted at me
        await OnCommand(command, text[space..].TrimStart(), msg);
    }
    else
        await OnTextMessage(msg);
        
}

async Task OnTextMessage(Message msg) // received a text message that is not a command
{
    Console.WriteLine($"Received text '{msg.Text}' in {msg.Chat}");
    await OnCommand("/start", "", msg); // for now we redirect to command /start
}
async Task OnCommand(string command, string args, Message msg)
{
    Console.WriteLine($"Received command: {command} {args}");
    switch (command)
    {
        case "/start":
            await bot.SendTextMessageAsync(msg.Chat, """
                <b><u>Bot menu</u></b>:
                /photo [url]    - send a photo <i>(optionally from an <a href="https://picsum.photos/310/200.jpg">url</a>)</i>
                /inline_buttons - send inline buttons
                /keyboard       - send keyboard buttons
                /remove         - remove keyboard buttons
                /poll           - send a poll
                /reaction       - send a reaction
                """, parseMode: ParseMode.Html, linkPreviewOptions: true,
                replyMarkup: new ReplyKeyboardRemove()); // also remove keyboard to clean-up things
            break;
        case "/photo":
            if (args.StartsWith("http"))
                await bot.SendPhotoAsync(msg.Chat, args, caption: "Source: " + args);
            else
            {
                await bot.SendChatActionAsync(msg.Chat, ChatAction.UploadPhoto);
                await Task.Delay(2000); // simulate a long task
                await using var fileStream = new FileStream("bot.gif", FileMode.Open, FileAccess.Read);
                await bot.SendPhotoAsync(msg.Chat, fileStream, caption: "Read https://telegrambots.github.io/book/");
            }
            break;
        case "/inline_buttons":
            var inlineMarkup = new InlineKeyboardMarkup()
                .AddNewRow("1.1", "1.2", "1.3")
                .AddNewRow()
                    .AddButton("WithCallbackData", "CallbackData")
                    .AddButton(InlineKeyboardButton.WithUrl("WithUrl", "https://github.com/TelegramBots/Telegram.Bot"));
            await bot.SendTextMessageAsync(msg.Chat, "Inline buttons:", replyMarkup: inlineMarkup);
            break;
        case "/keyboard":
            var replyMarkup = new ReplyKeyboardMarkup(true)
                .AddNewRow("1.1", "1.2", "1.3")
                .AddNewRow().AddButton("2.1").AddButton("2.2");
            await bot.SendTextMessageAsync(msg.Chat, "Keyboard buttons:", replyMarkup: replyMarkup);
            break;
        case "/remove":
            await bot.SendTextMessageAsync(msg.Chat, "Removing keyboard", replyMarkup: new ReplyKeyboardRemove());
            break;
        case "/poll":
            await bot.SendPollAsync(msg.Chat, "Question", ["Option 0", "Option 1", "Option 2"], isAnonymous: false, allowsMultipleAnswers: true);
            break;
        case "/reaction":
            await bot.SetMessageReactionAsync(msg.Chat, msg.MessageId, ["❤"], false);
            break;
    }
}

async Task OnUpdate(Update update)
{
    switch (update)
    {
        case { CallbackQuery: { } callbackQuery }: await OnCallbackQuery(callbackQuery); break;
        case { PollAnswer: { } pollAnswer }: await OnPollAnswer(pollAnswer); break;
        default: Console.WriteLine($"Received unhandled update {update.Type}"); break;
    };
}

async Task OnCallbackQuery(CallbackQuery callbackQuery)
{
    await bot.AnswerCallbackQueryAsync(callbackQuery.Id, $"You selected {callbackQuery.Data}");
    await bot.SendTextMessageAsync(callbackQuery.Message!.Chat, $"Received callback from inline button {callbackQuery.Data}");
}

async Task OnPollAnswer(PollAnswer pollAnswer)
{
    if (pollAnswer.User != null)
        await bot.SendTextMessageAsync(pollAnswer.User.Id, $"You voted for option(s) id [{string.Join(',', pollAnswer.OptionIds)}]");
}
