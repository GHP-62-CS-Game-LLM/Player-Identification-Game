using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OllamaSharp;
using OllamaSharp.Models.Chat;

public class Conversation
{
    public readonly string Context;
    
    private Func<string> _dynamicContext;
    
    private readonly Chat _chat;

    public Conversation(OllamaApiClient ollama, string context, Func<string> dynamicContext)
    {
        Context = context;
        _dynamicContext = dynamicContext;
        
        _chat = new Chat(ollama, Context);
    }

    public async Task<string> Message(string prompt)
    {
        return await Task.Run(async () =>
        {
            StringBuilder sb = new StringBuilder();
            _chat.SendAsAsync(ChatRole.System, _dynamicContext.Invoke());
            IAsyncEnumerable<string> response = _chat.SendAsync(prompt);
            await foreach (string token in response) sb.Append(token);

            return sb.ToString();
        });
    }

    public IEnumerable<string> GetAllMessagesStr() => _chat.Messages.Select(message => $"{message.Role}: {message.Content}");
    public IEnumerable<Message> GetImportantMessages() => _chat.Messages.Where(message => message.Role != ChatRole.System);

    public IEnumerable<Message> GetAllUserMessages() => _chat.Messages.Where(message => message.Role == ChatRole.User);
    public IEnumerable<Message> GetAllAIMessages() => _chat.Messages.Where(message => message.Role == ChatRole.Assistant);
    

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        foreach (Message message in GetImportantMessages())
        {
            sb.Append($"{message.Role}: {message.Content}");
            sb.Append("\n\n");
        }

        return sb.ToString();
    }
}
