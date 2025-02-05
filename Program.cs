using System;
using System.Drawing;
using System.Runtime.InteropServices;
using static System.Console;
//trying to make a nice user interface in a console program makes a mess
//if you want to do the interface properly in C#, use Winforms or WPF
internal class Program
{
    public static void Main()
    {
        ConsoleWindowSettings.DisableControls(false, true, true, true); //see static class
        Title = "Roulette";
        CursorVisible = false;
        if (OperatingSystem.IsWindows()) //just to shut-up the error message
        {
            SetWindowSize(72, 25);
            SetBufferSize(72, 25);
        }
        RouletteTable game = new RouletteTable(); //instantiate!
        game.Play();
    }
    public static class ConsoleWindowSettings //messing around with console window
    {
        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        public static void DisableControls(bool close, bool minimise, bool maximise, bool resize)
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);
            if (handle != IntPtr.Zero)
            {
                if (close) DeleteMenu(sysMenu, 0xF060, 0); //disable console window close button
                if (minimise) DeleteMenu(sysMenu, 0xF020, 0); //prevent use of console window minimise button
                if (maximise) DeleteMenu(sysMenu, 0xF030, 0); //prevent use of console window maximise button
                if (resize) DeleteMenu(sysMenu, 0xF000, 0); //prevent resizing of console window
            }
        }
    }
    public static class UserInterface //static values
    {
        public static Point WelcomeMessagePosition = new Point(2, 0);
        public static Point TablePosition = new Point(2, 1);
        public static Point WheelPosition = new Point(49, 1);
        public static Point PlayerPosition1 = new Point(2, 13);
        public static int PlayerInterfaceWidth = 17;
    }
    public class RouletteTable
    {
        private ConsoleKeyInfo _input;
        private Dictionary<int, ConsoleColor> _numberColours;
        private List<BetType> _betTypes;
        private List<Player> _players;
        private string[] _tableLayout;
        private int _winningNumber;
        private int _selectedBetTypeIndex;
        private int _maxBetsPerSpin;
        private int _defaultStartingMoney;
        private int _wheelSpinTime;
        public RouletteTable()
        {

            _tableLayout = new string[]             //unicode box-drawing characters
            {
            "┌───┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬───┐",
            "│   │03│06│09│12│15│18│21│24│27│30│33│36│Top│",
            "│   ├──┼──┼──┼──┼──┼──┼──┼──┼──┼──┼──┼──┼───┤",
            "│ 0 │02│05│08│11│14│15│20│23│26│29│32│35│Mid│",
            "│   ├──┼──┼──┼──┼──┼──┼──┼──┼──┼──┼──┼──┼───┤",
            "│   │01│04│07│10│13│16│19│22│25│28│31│34│Btm│",
            "└───┼──┴──┴──┴──┼──┴──┴──┴──┼──┴──┴──┴──┼───┘",
            "    │  1st 12   │  2nd 12   │  3rd 12   │    ",
            "    ├─────┬─────┼─────┬─────┼─────┬─────┤    ",
            "    │ 1─18│ EVEN│ RED │BLACK│ ODD │19─36│    ",
            "    └─────┴─────┴─────┴─────┴─────┴─────┘    "
            };
            _defaultStartingMoney = 100;
            _maxBetsPerSpin = 10;
            _selectedBetTypeIndex = 0;
            _wheelSpinTime = 80;
            _players = new List<Player>();
            LoadPlayers();
            _betTypes = new List<BetType>();
            LoadBetTypes();
            _numberColours = new Dictionary<int, ConsoleColor>();
            LoadNumberColours();
        }
        private void LoadPlayers()
        {
            //interface breaks with more than 4 players.
            //would be better with manual setup of players
            _players.Add(new Player("Alice", _defaultStartingMoney));
            _players.Add(new Player("Bob", _defaultStartingMoney));
            _players.Add(new Player("Yvonne", _defaultStartingMoney));
            _players.Add(new Player("Zoya", _defaultStartingMoney));
        }
        private void LoadBetTypes()
        {
            //see constructor: name, numbers,payout, interface position, interface display text
            _betTypes.Add(new BetType("Zero", new int[] { 0 }, 36, new Rectangle(1, 3, 3, 5), " 0 "));
            _betTypes.Add(new BetType("1", new int[] {1 }, 36, new Rectangle(5, 5, 2, 1), "01"));
            _betTypes.Add(new BetType("2", new int[] {2 }, 36, new Rectangle(5, 3, 2, 1), "02"));
            _betTypes.Add(new BetType("3", new int[] {3 }, 36, new Rectangle(5, 1, 2, 1), "03"));
            _betTypes.Add(new BetType("4", new int[] {4 }, 36, new Rectangle(8, 5, 2, 1), "04"));
            _betTypes.Add(new BetType("5", new int[] {5 }, 36, new Rectangle(8, 3, 2, 1), "05"));
            _betTypes.Add(new BetType("6", new int[] {5 }, 36, new Rectangle(8, 1, 2, 1), "06"));
            _betTypes.Add(new BetType("7", new int[] {7 }, 36, new Rectangle(11, 5, 2, 1), "07"));
            _betTypes.Add(new BetType("8", new int[] {8 }, 36, new Rectangle(11, 3, 2, 1), "08"));
            _betTypes.Add(new BetType("9", new int[] {9 }, 36, new Rectangle(11, 1, 2, 1), "09"));
            _betTypes.Add(new BetType("10", new int[] {10 }, 36, new Rectangle(14, 5, 2, 1), "10"));
            _betTypes.Add(new BetType("11", new int[] { 11}, 36, new Rectangle(14, 3, 2, 1), "11"));
            _betTypes.Add(new BetType("12", new int[] {12 }, 36, new Rectangle(14, 1, 2, 1), "12"));
            _betTypes.Add(new BetType("13", new int[] {13 }, 36, new Rectangle(17, 5, 2, 1), "13"));
            _betTypes.Add(new BetType("14", new int[] {14 }, 36, new Rectangle(17, 3, 2, 1), "14"));
            _betTypes.Add(new BetType("15", new int[] {15 }, 36, new Rectangle(17, 1, 2, 1), "15"));
            _betTypes.Add(new BetType("16", new int[] {16 }, 36, new Rectangle(20, 5, 2, 1), "16"));
            _betTypes.Add(new BetType("17", new int[] {17 }, 36, new Rectangle(20, 3, 2, 1), "17"));
            _betTypes.Add(new BetType("18", new int[] {18 }, 36, new Rectangle(20, 1, 2, 1), "18"));
            _betTypes.Add(new BetType("19", new int[] {19 }, 36, new Rectangle(23, 5, 2, 1), "19"));
            _betTypes.Add(new BetType("20", new int[] {20 }, 36, new Rectangle(23, 3, 2, 1), "20"));
            _betTypes.Add(new BetType("21", new int[] {21 }, 36, new Rectangle(23, 1, 2, 1), "21"));
            _betTypes.Add(new BetType("22", new int[] {22 }, 36, new Rectangle(26, 5, 2, 1), "22"));
            _betTypes.Add(new BetType("23", new int[] {23 }, 36, new Rectangle(26, 3, 2, 1), "23"));
            _betTypes.Add(new BetType("24", new int[] {24 }, 36, new Rectangle(26, 1, 2, 1), "24"));
            _betTypes.Add(new BetType("25", new int[] {25 }, 36, new Rectangle(29, 5, 2, 1), "25"));
            _betTypes.Add(new BetType("26", new int[] {26 }, 36, new Rectangle(29, 3, 2, 1), "26"));
            _betTypes.Add(new BetType("27", new int[] {27 }, 36, new Rectangle(29, 1, 2, 1), "27"));
            _betTypes.Add(new BetType("28", new int[] {28 }, 36, new Rectangle(32, 5, 2, 1), "28"));
            _betTypes.Add(new BetType("29", new int[] {29 }, 36, new Rectangle(32, 3, 2, 1), "29"));
            _betTypes.Add(new BetType("30", new int[] {30 }, 36, new Rectangle(32, 1, 2, 1), "30"));
            _betTypes.Add(new BetType("31", new int[] {31 }, 36, new Rectangle(35, 5, 2, 1), "31"));
            _betTypes.Add(new BetType("32", new int[] {32 }, 36, new Rectangle(35, 3, 2, 1), "32"));
            _betTypes.Add(new BetType("33", new int[] {33 }, 36, new Rectangle(35, 1, 2, 1), "33"));
            _betTypes.Add(new BetType("34", new int[] {34 }, 36, new Rectangle(38, 5, 2, 1), "34"));
            _betTypes.Add(new BetType("35", new int[] {35 }, 36, new Rectangle(38, 3, 2, 1), "35"));
            _betTypes.Add(new BetType("36", new int[] {36 }, 36, new Rectangle(38, 1, 2, 1), "36"));
            _betTypes.Add(new BetType("Tops", new int[] { 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36 }, 3, new Rectangle(41, 1, 3, 1), "Top"));
            _betTypes.Add(new BetType("Middles", new int[] { 2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35 }, 3, new Rectangle(41, 3, 3, 1), "Mid"));
            _betTypes.Add(new BetType("Bottoms", new int[] { 1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34 }, 3, new Rectangle(41, 5, 3, 1), "Btm"));
            _betTypes.Add(new BetType("1st 12", new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 3, new Rectangle(5, 7, 11, 1), "  1st 12   "));
            _betTypes.Add(new BetType("2nd 12", new int[] { 13, 14, 15, 61, 17, 18, 19, 20, 21, 22, 23, 24 }, 3, new Rectangle(17, 7, 11, 1), "  2nd 12   "));
            _betTypes.Add(new BetType("3rd 12", new int[] { 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 }, 3, new Rectangle(29, 7, 11, 1), "  3rd 12   "));
            _betTypes.Add(new BetType("1 to 18", new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, 2, new Rectangle(5, 9, 5, 1), " 1-18"));
            _betTypes.Add(new BetType("Even", new int[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36 }, 2, new Rectangle(11, 9, 5, 1), " EVEN"));
            _betTypes.Add(new BetType("Red", new int[] { 32, 19, 21, 25, 34, 27, 36, 30, 23, 5, 16, 1, 14, 9, 18, 7, 12, 3 }, 2, new Rectangle(17, 9, 5, 1), " RED "));
            _betTypes.Add(new BetType("Black", new int[] { 2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35 }, 2, new Rectangle(23, 9, 5, 1), "BLACK"));
            _betTypes.Add(new BetType("Odd", new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35 }, 2, new Rectangle(29, 9, 5, 1), " ODD"));
            _betTypes.Add(new BetType("19 to 36", new int[] { 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 }, 2, new Rectangle(35, 9, 5, 1), "19-36"));
        }
        private void LoadNumberColours()
        {
            //used in display of winning number on wheel
            _numberColours.Add(0, ConsoleColor.Green);
            int[] red = { 32, 19, 21, 25, 34, 27, 36, 30, 23, 5, 16, 1, 14, 9, 18, 7, 12, 3 };
            int[] black = { 2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35 };
            foreach (int i in red)
            {
                _numberColours.Add(i, ConsoleColor.Red);
            }
            foreach (int i in black)
            {
                _numberColours.Add(i, ConsoleColor.Black);
            }
        }
        public void Play()
        {
            bool gameOver = false;
            DisplayWelcomeMessage(UserInterface.WelcomeMessagePosition);
            do
            {
                Point currentPlayerInterfacePosition;
                DisplayWheel(UserInterface.WheelPosition, "?", false);                
                for (int i = 0; i < _players.Count; i++) //initial display on users (name, money, no bets)
                {
                    currentPlayerInterfacePosition = new Point(i * UserInterface.PlayerInterfaceWidth + UserInterface.PlayerPosition1.X + i, UserInterface.PlayerPosition1.Y);
                    DisplayPlayerInfo(_players[i], false, currentPlayerInterfacePosition);
                }               
                for (int i = 0; i < _players.Count; i++) //player turns:
                {
                    currentPlayerInterfacePosition = new Point(i * UserInterface.PlayerInterfaceWidth + UserInterface.PlayerPosition1.X + i, UserInterface.PlayerPosition1.Y);
                    do
                    {
                        DisplayPlayerInfo(_players[i], true, currentPlayerInterfacePosition);
                        do //placing each bet
                        {
                            DisplayTable(UserInterface.TablePosition);
                            if (_players[i].Money > 0) //no interaction if no money
                            {
                                _input = ReadKey(true);
                                SelectBet(); //
                            }
                        }
                        while (!PlaceBet() && _input.Key != ConsoleKey.Spacebar && _players[i].Money > 0); 
                        if (_input.Key == ConsoleKey.Enter && _players[i].Money > 0) //place valid bets
                        {
                            _players[i].PlaceBet(_betTypes[_selectedBetTypeIndex], currentPlayerInterfacePosition);
                        }
                    }
                    while (_input.Key != ConsoleKey.Spacebar && _players[i].Bets.Count < _maxBetsPerSpin && _players[i].Money > 0); //another bet
                    DisplayPlayerInfo(_players[i], false, currentPlayerInterfacePosition);
                }
                DisplayWheel(UserInterface.WheelPosition, "Spinning...", true); //wheel animation
                _winningNumber = Random.Shared.Next(0, 37);
                DisplayWheel(UserInterface.WheelPosition, Convert.ToString(_winningNumber), false);
                gameOver = true;
                for (int i = 0; i < _players.Count; i++) //loop through players to check bets
                {
                    currentPlayerInterfacePosition = new Point(i * UserInterface.PlayerInterfaceWidth + UserInterface.PlayerPosition1.X + i, UserInterface.PlayerPosition1.Y);
                    UpdateBets(_players[i]); //win or lose 
                    DisplayPlayerInfo(_players[i], false, currentPlayerInterfacePosition);
                    if (_players[i].Money > 0)
                    {
                        gameOver = false; //game not over if anyone has money left
                    }
                    _players[i].ClearBets(); //remove all bets for next round
                }
                ReadKey(true);
            }
            while (!gameOver);
            Console.SetCursorPosition(0, 0);
        }
        private void SelectBet() //this is ugly and hacky
        {
            bool move = false;
            int xOffset = 0;
            int yOffset = 0;
            if (_input.Key == ConsoleKey.UpArrow)
            {
                move = true;
                xOffset = 0;
                yOffset = -2;
            }
            if (_input.Key == ConsoleKey.DownArrow)
            {
                move = true;
                xOffset = 0;
                yOffset = 2;
            }
            if (_input.Key == ConsoleKey.LeftArrow)
            {
                move = true;
                xOffset = -2;
                yOffset = 0;
            }
            if (_input.Key == ConsoleKey.RightArrow)
            {
                move = true;
                xOffset = _betTypes[_selectedBetTypeIndex].InterfacePosition.Width + 1;
                yOffset = 0;
            }
            if (move)
            {
                Rectangle current = _betTypes[_selectedBetTypeIndex].InterfacePosition; //get area of selected bet
                for (int i = 0; i < _betTypes.Count; i++)
                {
                    if (_betTypes[i].InterfacePosition.Contains(current.X + xOffset, current.Y + yOffset)) //cursor in this bet's rectangle?
                    {
                        _selectedBetTypeIndex = i;
                        return;
                    }
                }
            }
        }
        private bool PlaceBet()
        {
            if (_input.Key == ConsoleKey.Enter)
            {
                return true;
            }
            return false;
        }
        private void UpdateBets(Player player) //give prize money for each bet if it wins
        {
            if (player.Bets.Count > 0)
            {
                foreach (PlayerBet bet in player.Bets)
                {
                    bet.Process(_winningNumber);
                    if (bet.BetStatus == BetStatus.Win)
                    {
                        player.AwardMoney(bet.Winnings());
                    }
                }
            }
        }
        private void DisplayWelcomeMessage(Point position)
        {
            SetCursorPosition(position.X, position.Y);
            Write("Welcome to the Roulette table! (ENTER to bet, SPACE to end turn)");
        }
        private void DisplayPlayerInfo(Player player, bool current, Point offset)
        {
            ResetColor();
            BackgroundColor = ConsoleColor.DarkGreen;
            string text = player.Name + ": £" + player.Money;
            text = text.Substring(0, Math.Min(text.Length, UserInterface.PlayerInterfaceWidth - 1)).PadRight(UserInterface.PlayerInterfaceWidth - 1, ' ');
            SetCursorPosition(offset.X, offset.Y);
            if (current) //current player's name in green
            {
                ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                ForegroundColor = ConsoleColor.White;
            }
            Write(text);
            if (player.Bets.Count > 0) //list all bets with amounts
            {
                for (int i = 0; i < player.Bets.Count; i++)
                {
                    text = player.Bets[i].Description();
                    text = text.Substring(0, Math.Min(text.Length, UserInterface.PlayerInterfaceWidth - 1)); //truncate
                    text = text.PadRight(UserInterface.PlayerInterfaceWidth - 1, ' '); //pad to width
                    if (player.Bets[i].BetStatus == BetStatus.Win) //wins in green
                    {
                        ForegroundColor = ConsoleColor.Green;
                    }
                    else if (player.Bets[i].BetStatus == BetStatus.Loss) //lesses in red
                    {
                        ForegroundColor = ConsoleColor.Red;
                    }
                    else
                    {
                        ForegroundColor = ConsoleColor.Gray;
                    }
                    SetCursorPosition(offset.X, offset.Y + i + 1);
                    Write(text);
                }
            }
            else if (current) //if not bets, list current player as betting
            {
                text = "betting...".PadRight(UserInterface.PlayerInterfaceWidth - 1, ' ');
                SetCursorPosition(offset.X, offset.Y + 1);
                Write(text);
            }
            else //list non-betters as empty
            {
                text = "".PadRight(UserInterface.PlayerInterfaceWidth - 1, ' ');
                SetCursorPosition(offset.X, offset.Y + 1);
                Write(text);
            }
            SetCursorPosition(offset.X, player.Bets.Count + offset.Y + 1);
            Write(" ".PadLeft(UserInterface.PlayerInterfaceWidth - 1, ' '));
            SetCursorPosition(offset.X, player.Bets.Count + offset.Y + 1);
            for (int y = offset.Y + player.Bets.Count+2; y < WindowHeight; y++)
            {
                SetCursorPosition(offset.X, y);
                BackgroundColor = ConsoleColor.DarkGreen;
                Write("".PadRight(UserInterface.PlayerInterfaceWidth - 1, ' '));
            }
            if (player.Money == 0) //list broke players as broke
            {
                ForegroundColor = ConsoleColor.Gray;
                SetCursorPosition(offset.X, player.Bets.Count + offset.Y + 1);
                Write("no money left".PadRight(UserInterface.PlayerInterfaceWidth - 1, ' '));
            }
        }
        private void DisplayTable(Point offset) //messy
        {
            int x, y;
            BackgroundColor = ConsoleColor.DarkGreen;
            ForegroundColor = ConsoleColor.White;
            SetCursorPosition(offset.X, offset.Y);
            for (int i = 0; i < _tableLayout.Length; i++) //draw grid
            {
                SetCursorPosition(offset.X, offset.Y + i);
                Write(_tableLayout[i]);
            }
            for(int i = 1; i < 37; i++)
            {
                x = _betTypes[i].InterfacePosition.X+offset.X;
                y = _betTypes[i].InterfacePosition.Y+offset.Y;
                Console.BackgroundColor = _numberColours[i]; //number BG colour from dictionary lookup
                SetCursorPosition(x, y);
                Write(_betTypes[i].DisplayString); //2-digit number
            }
            ResetColor();
            BackgroundColor = ConsoleColor.Blue;
            x = _betTypes[_selectedBetTypeIndex].InterfacePosition.X + offset.X;
            y = _betTypes[_selectedBetTypeIndex].InterfacePosition.Y + offset.Y;
            SetCursorPosition(x, y);
            Write(_betTypes[_selectedBetTypeIndex].DisplayString); //redisplay selected bet with blue BG
            SetCursorPosition(0, offset.Y + 12);
            ResetColor();
        }
        private void DisplayWheel(Point offset, string message, bool spinningAnimation)
        {
            int number;
            int count = 28;
            int radius = 5;
            double angle = 0;
            double angleStep = 2 * Math.PI / count;
            int x;
            int y;
            BackgroundColor = ConsoleColor.DarkGreen;
            ForegroundColor = ConsoleColor.White;
            for (int ypos = 0; ypos < 11; ypos++) //draw background rectagnle
            {
                for (int xpos = 0; xpos < 23; xpos++)
                {
                    SetCursorPosition(xpos + offset.X, ypos + offset.Y);
                    Write(" ");
                }
            }
            offset.X += 11;
            offset.Y += 5;
            for (int i = 0; i < count; i++) //draw numbers in circle with MATHS
            {
                x = (int)Math.Round(offset.X + radius * 2 * Math.Cos(angle));
                y = (int)Math.Round(offset.Y + radius * Math.Sin(angle));
                SetCursorPosition(x, y);
                Write("*");
                angle += angleStep;
            }
            SetCursorPosition(offset.X - message.Length / 2, offset.Y);
            if (int.TryParse(message, out number)) //if message is number...
            {
                Console.BackgroundColor = _numberColours[number]; //draw in correct colour
            }
            Write(message);
            BackgroundColor = ConsoleColor.DarkGreen;
            if (spinningAnimation)
            {
                x = (int)Math.Round(offset.X + radius * 2 * Math.Cos(0));
                y = (int)Math.Round(offset.Y + radius * Math.Sin(0));
                for (int i = 0; i < _wheelSpinTime; i++) //spin!
                {
                    System.Threading.Thread.Sleep(20);
                    SetCursorPosition(x, y);
                    ForegroundColor = ConsoleColor.White;
                    Write("*");
                    x = (int)Math.Round(offset.X + radius * 2 * Math.Cos(angle));
                    y = (int)Math.Round(offset.Y + radius * Math.Sin(angle));
                    SetCursorPosition(x, y);
                    ForegroundColor = ConsoleColor.Black;
                    Write("☻");
                    angle += angleStep;
                }
            }
        }
    }
    public class Player
    {
        private string _name;
        private int _money;
        private List<PlayerBet> _bets;
        public Player(string name, int money)
        {
            _name = name;
            _money = money;
            _bets = new List<PlayerBet>();
        }
        public string Name { get { return _name; } }
        public int Money { get { return _money; } }
        public List<PlayerBet> Bets { get { return _bets; } }
        public void ClearBets()
        {
            _bets.Clear();
        }
        public void AwardMoney(int money)
        {
            _money += money;
        }
        public void PlaceBet(BetType betType, Point offset)
        {
            string input;
            int betAmount = 0;
            do
            {
                input = "";
                SetCursorPosition(offset.X, offset.Y + _bets.Count + 1);
                ForegroundColor = ConsoleColor.Gray;
                BackgroundColor = ConsoleColor.DarkGreen;
                Write("Amount? £".PadRight(UserInterface.PlayerInterfaceWidth - 1, ' '));
                input = GetNumericInput(new Point(offset.X + 9, offset.Y + _bets.Count + 1), 5);               
            }
            while (!int.TryParse(input, out betAmount) || betAmount <= 0 || betAmount > _money);
            if (betAmount > 0)
            {
                _money -= betAmount;
                _bets.Add(new PlayerBet(betType, betAmount));
            }
        }
        private string GetNumericInput(Point position, int maxLength) //messy
        {
            ConsoleKeyInfo key;
            string input = "";
            do
            {
                key = ReadKey(true); //get input one key at a time to limit input length & chars
                if (char.IsDigit(key.KeyChar) && input.Length < maxLength) //only numbers count
                {
                    input += key.KeyChar;
                }
                else if (key.Key == ConsoleKey.Backspace && input.Length > 0) //allow deletion of digits
                {
                    input = input.Remove(input.Length - 1);
                }
                ForegroundColor = ConsoleColor.Gray;
                BackgroundColor = ConsoleColor.DarkGreen;
                SetCursorPosition(position.X, position.Y);
                Write(input.PadRight(maxLength, ' ')); //re-output after each key press
            }
            while (key.Key != ConsoleKey.Enter);
            return input;
        }
    }
    public class PlayerBet
    {
        private BetType _betType;
        private int _amount;
        private BetStatus _betStatus;
        public PlayerBet(BetType bet_type, int amount)
        {
            _betType = bet_type;
            _amount = amount;
            _betStatus = BetStatus.Pending;
        }
        public BetStatus BetStatus { get => _betStatus; }
        public void Process(int winningNumber)
        {
            if (_betType.IsWinner(winningNumber))
            {
                _betStatus = BetStatus.Win;
            }
            else
            {
                _betStatus = BetStatus.Loss;
            }
        }
        public string Description()
        {
            return "£" + _amount + " on " + _betType.Name;
        }
        public int Winnings()
        {
            return _betType.Odds * _amount;
        }
    }
    public class BetType
    {
        private string _name;
        private int[] _winningNumbers;
        private Rectangle _interfacePosition;
        private string _displayString;
        private int _odds;
        public BetType(string name, int[] winning_numbers, int odds, Rectangle interfacePosition, string displayString)
        {
            _name = name;
            _winningNumbers = winning_numbers;
            _odds = odds;
            _interfacePosition = interfacePosition;
            _displayString = displayString;
        }
        public string Name { get => _name; }
        public Rectangle InterfacePosition { get => _interfacePosition; }
        public string DisplayString { get => _displayString; }
        public int Odds { get => _odds; }
        public bool IsWinner(int _winningNumber)
        {
            return _winningNumbers.Contains(_winningNumber);
        }
    }
    public enum BetStatus
    {
        Pending,
        Win,
        Loss
    }
}


