using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OllamaSharp;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public string host = "https://c3da-209-166-98-68.ngrok-free.app";
    public string local = "http://127.0.0.1:11434";
    public SceneContextManager scm;

    private readonly OllamaApiClient _ollama;
    private const string Model = "phi4-mini";

    public ChatManager()
    {
        _ollama = new OllamaApiClient(new Uri(host));
        _ollama.SelectedModel = Model;
    }

    public async Task<string> Message(string prompt)
    {
        Chat chat = new Chat(_ollama);

        StringBuilder sb = new StringBuilder();
        IAsyncEnumerable<string> response = chat.SendAsync(prompt);
        await foreach (string token in response) sb.Append(token);

        return sb.ToString();
    }

    public Conversation MakeConversation(string context) => new Conversation(_ollama, context, () => scm.GetDynamicContext());
}