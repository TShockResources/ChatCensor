using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace ChatCensor
{
    [ApiVersion(1,17)]
    public class ChatCensor : TerrariaPlugin
    {
        public override string Name
        {
            get { return "ChatCensor"; }
        }

        public override Version Version
        {
            get { return new Version(1,0,0); }
        }

        public override string Author
        {
            get { return "Ijwu"; }
        }

        public override string Description
        {
            get { return "Censors words in chat."; }
        }

        public List<string> CensoredWords;
        private string _censorFilePath = Path.Combine(TShock.SavePath, "censored.txt");

        public ChatCensor(Main game) : base(game)
        {
            
        }

        public override void Initialize()
        {
            //Try to load up censored words from a file.
            GetCensoredWordsFromFile(_censorFilePath);

            //Hook into the chat. This specific hook catches the chat before it is sent out to other clients.
            //This allows us to edit the chat message before others get it.
            ServerApi.Hooks.ClientChat.Register(this, OnChat);

            //This hook is a part of TShock and not a part of TS-API. There is a strict distinction between those two assemblies.
            //This event is provided through the C# ``event`` keyword, which is a feature of the language itself.
            GeneralHooks.ReloadEvent += OnReloadEvent;
        }

        private void OnReloadEvent(ReloadEventArgs reloadEventArgs)
        {
            GetCensoredWordsFromFile(_censorFilePath);
        }

        private void GetCensoredWordsFromFile(string path)
        {
            if (Directory.Exists(path))
                CensoredWords = File.ReadAllLines(path).ToList();
            else
            {
                CensoredWords = new List<string>();
                TShock.Log.ConsoleError("Censored words file not found. It has been made for you. " +
                                        "Please edit the file with words you wish to be censored (one word per line). " +
                                        "Then use the /reload command.");
            }
        }

        private void OnChat(ChatEventArgs args)
        {
            //Take each censored word and replace it with a starred out equivalent.
            //By overwriting ``args.Message`` you're able to alter the chat message that's sent out.
            foreach (var word in CensoredWords)
            {
                args.Message = args.Message.Replace(word, new String('*', word.Length));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ClientChat.Deregister(this, OnChat);
            }
            base.Dispose(disposing);
        }
    }
}
