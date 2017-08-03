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
    [ApiVersion(2, 1)]
    public class ChatCensor : TerrariaPlugin
    {
        public override string Name => "ChatCensor";

        public override Version Version => new Version(1,0,0);

        public override string Author => "Ijwu";

        public override string Description => "Kicks bad words (in the face).";

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
            ServerApi.Hooks.ServerChat.Register(this, OnChat);

            //This hook is a part of TShock and not a part of TS-API. There is a strict distinction between those two assemblies.
            //This event is provided through the C# ``event`` keyword, which is a feature of the language itself.
            GeneralHooks.ReloadEvent += OnReload;
        }

        private void OnReload(ReloadEventArgs reloadEventArgs)
        {
            GetCensoredWordsFromFile(_censorFilePath);
        }

        private void GetCensoredWordsFromFile(string path)
        {
            if (File.Exists(path))
            {
                CensoredWords = File.ReadAllLines(path).ToList();
            }
            else
            {
                CensoredWords = new List<string>();
                TShock.Log.ConsoleError("Censored words file not found. It has been made for you. " +
                                        "Please edit the file with words you wish to be censored (one word per line). " +
                                        "Then use the /reload command.");
                File.WriteAllText(path, "");
            }
        }

        private void OnChat(ServerChatEventArgs args)
        {
            //Kick anybody who uses bad words.
            foreach (var word in CensoredWords)
            {
                if (args.Text.Contains(word))
                    TShock.Utils.Kick(TShock.Players[args.Who], "You said a bad word.");

            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }
    }
}
