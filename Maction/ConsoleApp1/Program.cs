using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace Maction
{
    class Program
    {
        static string filePath = "players.txt";
        static Dictionary<string, Player> players = new Dictionary<string, Player>();
        static SoundPlayer playerMusic;
        static bool isMusicEnabled = true;

        static void Main(string[] args)
        {
            LoadPlayers();
            InitializeMusic();

            Console.Title = "Mağarada Macera";
            ShowBigTitle();

            while (true)
            {
                ShowMainMenu();
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    string email = Login();
                    Player player = players[email];

                    ShowStory();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\nHoş geldiniz, {player.Name}! Maceranız başlıyor...\n");
                    Console.ResetColor();

                    Console.Beep(400, 500);
                    if (isMusicEnabled) StartMusic();

                    Game game = new Game(player);
                    game.Start();

                    StopMusic();
                    Console.Beep(300, 700);

                    SavePlayers();
                }
                else if (choice == "2")
                {
                    ShowScoreboard();
                }
                else if (choice == "3")
                {
                    ShowSettings();
                }
                else if (choice == "4")
                {
                    InstallMusic();
                }
                else if (choice == "5")
                {
                    Console.WriteLine("Çıkış yapılıyor...");
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Geçersiz seçim. Lütfen tekrar deneyin.");
                    Console.ResetColor();
                }
            }
        }



        static void ShowMainMenu()
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("\n=== Mağarada Macera ===");
            Console.WriteLine("1. Oyuna Başla");
            Console.WriteLine("2. Skor Tablosu");
            Console.WriteLine("3. Ayarlar");
            Console.WriteLine("4. Müzik Kurulumu");
            Console.WriteLine("5. Çıkış");
            Console.Write("Seçim: ");
            Console.ResetColor();
        }

        static void DeleteAllPlayers()
        {
            if (File.Exists("players.txt"))
            {
                File.Delete("players.txt");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Tüm oyuncu bilgileri başarıyla silindi.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Oyuncu bilgileri dosyası bulunamadı.");
                Console.ResetColor();
            }
        }

        static void DeletePlayer(string email)
        {
            if (players.ContainsKey(email))
            {
                players.Remove(email);
                SavePlayers();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{email} adresine sahip oyuncu başarıyla silindi.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Bu email adresine sahip oyuncu bulunamadı: {email}");
                Console.ResetColor();
            }
        }

        static void ShowSettings()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== Ayarlar ===");
            Console.WriteLine("1. Skor Tablosunu Sıfırla");
            Console.WriteLine($"2. Müzik {(isMusicEnabled ? "Kapat" : "Aç")}");
            Console.WriteLine("3. Tüm Oyuncu Bilgilerini Sil");
            Console.WriteLine("4. Bir Oyuncu Bilgilerini Sil");
            Console.Write("Seçim: ");
            Console.ResetColor();

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                foreach (var player in players.Values)
                {
                    player.MaxProgress = 0;
                }
                SavePlayers();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Skor tablosu sıfırlandı!");
                Console.ResetColor();
            }
            else if (choice == "2")
            {
                isMusicEnabled = !isMusicEnabled;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Müzik {(isMusicEnabled ? "açıldı" : "kapatıldı")}!");
                Console.ResetColor();
            }
            else if (choice == "3")
            {
                DeleteAllPlayers();
            }
            else if (choice == "4")
            {
                Console.Write("Silmek istediğiniz oyuncunun email adresini girin: ");
                string email = Console.ReadLine();
                DeletePlayer(email);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Geçersiz seçim.");
                Console.ResetColor();
            }
        }

        static void InstallMusic()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n=== Müzik Kurulumu ===");
            Console.WriteLine("Lütfen .wav formatındaki müzik dosyanızı seçin.");
            Console.Write("Dosya yolunu girin: ");
            string sourcePath = Console.ReadLine();

            if (File.Exists(sourcePath))
            {
                string destinationPath = Path.Combine(Directory.GetCurrentDirectory(), "game_music.wav");
                File.Copy(sourcePath, destinationPath, true);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Müzik dosyası başarıyla kuruldu!");
                InitializeMusic();
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Dosya bulunamadı. Lütfen geçerli bir yol girin.");
                Console.ResetColor();
            }
        }

        static void InitializeMusic()
        {
            string musicPath = "game_music.wav";
            if (File.Exists(musicPath))
            {
                playerMusic = new SoundPlayer(musicPath);
            }
            else
            {
                playerMusic = null;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Arka plan müziği dosyası bulunamadı: game_music.wav");
                Console.ResetColor();
            }
        }

        static void StartMusic()
        {
            if (playerMusic != null)
            {
                playerMusic.PlayLooping();
            }
        }

        static void StopMusic()
        {
            if (playerMusic != null)
            {
                playerMusic.Stop();
            }
        }

        private static void LoadPlayers()
        {
            if (!File.Exists("players.txt"))
            {
                File.Create("players.txt").Dispose();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Oyuncular veritabanı dosyası oluşturuldu (players.txt).");
                Console.ResetColor();
            }
            else
            {
                foreach (var line in File.ReadAllLines("players.txt"))
                {
                    var data = line.Split('|');
                    if (data.Length == 4)
                    {
                        var player = new Player
                        {
                            Email = data[0],
                            Password = data[1],
                            Name = data[2],
                            MaxProgress = int.TryParse(data[3], out int maxProgress) ? maxProgress : 0
                        };
                        players[player.Email] = player;
                    }
                }
            }
        }

        static void SavePlayers()
        {
            var lines = new List<string>();
            foreach (var player in players.Values)
            {
                lines.Add($"{player.Email}|{player.Password}|{player.Name}|{player.MaxProgress}");
            }
            File.WriteAllLines(filePath, lines);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Oyuncular kaydedildi!");
            Console.ResetColor();
        }

        static string Login()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("\n1. Giriş Yap");
                Console.WriteLine("2. Kayıt Ol");
                Console.Write("Seçim: ");
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    Console.Write("Email: ");
                    string email = Console.ReadLine();
                    Console.Write("Şifre: ");
                    string password = Console.ReadLine();

                    if (players.ContainsKey(email) && players[email].Password == password)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Giriş başarılı!");
                        Console.ResetColor();
                        return email;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Geçersiz email veya şifre.");
                        Console.ResetColor();
                    }
                }
                else if (choice == "2")
                {
                    Console.Write("Email: ");
                    string email = Console.ReadLine();
                    if (players.ContainsKey(email))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Bu email zaten kayıtlı.");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write("Şifre: ");
                        string password = Console.ReadLine();
                        Console.Write("İsim: ");
                        string name = Console.ReadLine();

                        Player newPlayer = new Player { Name = name, Email = email, Password = password };
                        players[email] = newPlayer;

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Kayıt başarılı!");
                        Console.ResetColor();
                        return email;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Geçersiz seçim.");
                    Console.ResetColor();
                }
            }
        }

        static void ShowBigTitle()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(@"
                                                     ,----,                                                            
          ____                                     ,/   .`|              ,----..             ,--.
        ,'  , `.    ,---,          ,----..       ,`   .'  :    ,---,    /   /   \          ,--.'|
     ,-+-,.' _ |   '  .' \        /   /   \    ;    ;     / ,`--.' |   /   .     :     ,--,:  : |
  ,-+-. ;   , ||  /  ;    '.     |   :     : .'___,/    ,'  |   :  :  .   /   ;.  \ ,`--.'`|  ' :
 ,--.'|'   |  ;| :  :       \    .   |  ;. / |    :     |   :   |  ' .   ;   /  ` ; |   :  :  | |
|   |  ,', |  ': :  |   /\   \   .   ; /--`  ;    |.';  ;   |   :  | ;   |  ; \ ; | :   |   \ | :
|   | /  | |  || |  :  ' ;.   :  ;   | ;     `----'  |  |   '   '  ; |   :  | ; | ' |   : '  '; |
'   | :  | :  |, |  |  ;/  \   \ |   : |         '   :  ;   |   |  | .   |  ' ' ' : '   ' ;.    ;
;   . |  ; |--'  '  :  | \  \ ,' .   | '___      |   |  '   '   :  ; '   ;  \; /  | |   | | \   |
|   : |  | ,     |  |  '  '--'   '   ; : .'|     '   :  |   |   |  '  \   \  ',  /  '   : |  ; .'
|   : '  |/      |  :  :         '   | '/  :     ;   |.'    '   :  |   ;   :    /   |   | '`--'
;   | |`-'       |  | ,'         |   :    /      '---'      ;   |.'     \   \ .'    '   : |
|   ;/           `--''            \   \ .'                  '---'        `---`      ;   |.'
'---'                              `---`                                            '---'



 
");
            Console.ResetColor();
        }

        static void ShowStory()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("\n=== Kısa Hikaye ===");
            Console.WriteLine("Bir mağarada uyandınız. Hatırlayabildiğiniz tek şey, çıkışa ulaşmanız gerektiği...");
            Console.WriteLine("====================\n");
            Console.ResetColor();
            Thread.Sleep(3000);
        }

        static void ShowScoreboard()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n=== Skor Tablosu ===");
            foreach (var player in players.Values)
            {
                Console.WriteLine($"{player.Name}: {player.MaxProgress} olay");
            }
            Console.WriteLine("=====================");
            Console.ResetColor();
        }
    }

    class Player
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public int MaxProgress { get; set; }

        public int Health { get; set; } = 100;

        public virtual void TakeDamage(int damage)
        {
            MaxProgress -= damage;
            if (MaxProgress < 0) MaxProgress = 0;
        }
    }

    class Game
    {
        private Player _player;
        private Random _random;
        private int _currentProgress;

        public Game(Player player)
        {
            _player = player;
            _random = new Random();
            _currentProgress = 0;
        }

        private void TriggerEvent()
        {

            var events = new Dictionary<string, string>
    {
        { "Bir mağara çöküyor, kaçmanız gerekiyor!", "Escape" },
        { "Mağranın bu kısmını hızlıca su dolduruyor!", "Escape" },
        { "Yerde eski bir çanta buldun.", "Explore" },
        { "Bir iskelet asker önününe çıktı!", "Combat" },
        { "Bir yarasa sürüsü saldırıya geçti!", "Escape" },
        { "Bir goblin sürüsü saldırıya geçti!", "Escape" },
        { "Bir mağara canavarı önünüze çıktı!", "Combat" },
        { "Bir dev örümcek ağında sıkıştın.", "Combat" },
        { "Bir askerin cesedi ile karşılaştın.", "Explore" },
        { "Bir hazine sandığı buldunuz, ama bir tuzak olabilir!", "Explore" },
        { "Mağara duvarında eski bir yazıt buldun.", "Explore" },
        { "Bir ejderha önününe çıktı!", "Combat" },
        { "Mağara tabanı kaygan.", "Escape" },
        { "Bir kapı görüyorsunuz, açmak ister misin?", "Explore" },
        { "Bir ışık gördünüz, çıkış olduğunu düşünüyorsun.", "Explore" },
        { "Bir mağara ayısı size doğru geliyor.", "Combat" }
    };


            var randomEvent = events.ElementAt(_random.Next(events.Count));
            string eventText = randomEvent.Key;
            string category = randomEvent.Value;

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"\n=== Yeni Bir Olay! ===");
            Console.ResetColor();

            Console.WriteLine(eventText);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Canınız: {_player.Health}");
            Console.ResetColor();


            Console.WriteLine("\nSeçenekler:");
            bool hasEscape = category == "Escape" || category == "Combat";
            bool hasCombat = category == "Combat";
            bool hasExplore = category == "Explore" || category == "Combat";

            if (hasEscape)
                Console.WriteLine("1. Kaç");

            if (hasCombat)
                Console.WriteLine("2. Savaş");

            if (hasExplore)
                Console.WriteLine("3. İncele");


            Console.WriteLine("4. Savunmada kal");

            Console.Write("Seçim: ");
            string choice = Console.ReadLine();


            int outcome = _random.Next(0, 3);
            int effect = _random.Next(10, 25);

            switch (choice)
            {
                case "1":
                    if (hasEscape)
                    {
                        if (outcome == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"Kaçmaya çalışırken {effect} hasar aldınız!");
                            _player.Health -= effect;
                        }
                        else if (outcome == 2)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("Kaçmayı başarıyla başardınız ve güvenli bir alana ulaştınız.");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("Kaçışınız sırasında ufak bir taşla tökezlediniz, ama zarar görmediniz.");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Bu olayda kaçmak mantıklı bir seçenek değil.");
                        _player.TakeDamage(10);
                    }
                    break;

                case "2":
                    if (hasCombat)
                    {
                        if (outcome == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"Savaştınız ama {effect} hasar aldınız!");
                            _player.Health -= effect;
                        }
                        else if (outcome == 2)
                        {
                            int healthGain = _random.Next(1, 3);
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"Savaşı kazandınız ve {healthGain} can kazandınız!");
                            _player.Health += healthGain;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("Savaştınız ve durum dengede kaldı.");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Bu olayda savaşmak mantıklı bir seçenek değil.");
                        _player.TakeDamage(10);
                    }
                    break;

                case "3":
                    if (hasExplore)
                    {
                        if (outcome == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"İnceleme sırasında bir tuzağa yakalandınız ve {effect} hasar aldınız!");
                            _player.Health -= effect;
                        }
                        else if (outcome == 2)
                        {
                            int healthGain = _random.Next(1, 5);
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"İnceleme sırasında gizli bir iyileştirici buldunuz ve {healthGain} can kazandınız!");
                            _player.Health += healthGain;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("İnceleme sırasında herhangi bir şey bulamadınız, ama zarar da görmediniz.");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Bu olayda incelemek mantıklı bir seçenek değil.");
                        _player.TakeDamage(10);
                    }
                    break;

                case "4":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Savunmada kalmayı seçtiniz. Durumu dikkatlice izliyorsunuz...");
                    if (outcome == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"Beklenmedik bir saldırı sonucu {effect} hasar aldınız!");
                        _player.Health -= effect;
                    }
                    else if (outcome == 2)
                    {
                        int healthGain = _random.Next(1, 5);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"Savunmada kaldığınız için dinlendiniz ve {healthGain} can kazandınız!");
                        _player.Health += healthGain;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Herhangi bir olay yaşanmadı, ama tetikte kaldınız.");
                    }
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Geçersiz seçim yaptınız. Savunmasız kaldınız ve 10 hasar aldınız!");
                    _player.TakeDamage(10);
                    break;
            }

            if (_player.Health <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nCanınız sıfıra düştü, mağarada ölümle yüzleşiyorsunuz...");
                Console.ResetColor();
                


            }
        }
        public void Start()
        {
            while (true)
            {
                Console.Beep(600, 200);
                Console.WriteLine("\n=== Yeni Bir Olay! ===");
                TriggerEvent();
                _currentProgress++;

                if (_currentProgress > _player.MaxProgress)
                {
                    _player.MaxProgress = _currentProgress;
                }

                
               
                if(_player.Health <= 0)
                {
                    break;
                    
                }
            }
        }
    }
}

