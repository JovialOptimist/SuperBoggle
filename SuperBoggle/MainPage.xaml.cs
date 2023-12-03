using System.Timers;
using Plugin.Maui.Audio;

namespace SuperBoggle;

public partial class MainPage : ContentPage
{
	public string currentWord;

	public BetterButton[] allButtons;

	public List<Point> prevButtonLocation;

    public string[] AllEnglishWords;

    private string alreadyGuessed = "";

    private int score = 0;

    private Color selectedBackColor = Colors.Yellow;
    private Color waitingBackColor = Colors.Blue;

    private Color selectedForeColor = Colors.Black;
    private Color waitingForeColor = Colors.White;

    public MainPage()
	{
		InitializeComponent();

        using var stream = FileSystem.OpenAppPackageFileAsync("AllEnglishWords.txt");
        using var reader = new StreamReader(stream.Result);

        var contents = reader.ReadToEnd();
        AllEnglishWords = contents
            .ToString()
            .Split("\r\n")
            .Select(x => x.ToLower())
            .ToArray();

        InitBoard();

        trash.Clicked += Trash_Clicked;
        submit.Clicked += Submit_Clicked;
        reset.Clicked += Reset_Clicked;

        trash.Background = waitingBackColor;
        submit.Background = waitingBackColor;  
        reset.Background = waitingBackColor;

        
    }

    private void HowManyWordsAreThere()
    {
        // get a button
        
        // get its surrounding buttons

        // is there word that starts with this combination of letters?

        // if that word is the same length as our current test word, log it as a successful word we could play

        List<string> wordsOnBoard = new List<string>();
        for (int i = 0; i < allButtons.Length; i++)
        {
            string currentWord;
            BetterButton currentButton = allButtons[i];
            currentWord = currentButton.Text;

            string heyLook = RecursiveWordFinder(currentButton.location, currentWord);
            wordsOnBoard.Add(heyLook);
        }
        wordsOnBoard.Sort();
    }

    private string RecursiveWordFinder(Point location, string currentWord)
    {
        // get neighbors of button at location
        BetterButton[] neighbors = getNeighbors(location);
        for (int j = 0; j < neighbors.Length; j++)
        {
            string testing = currentWord + neighbors[j].Text;
            bool validStart = false;
            // for each neighbor, check if it makes the start of a word
            for (int q = 0; q < AllEnglishWords.Length; q++)
            {
                if (AllEnglishWords[q].ToUpper().StartsWith(testing.ToUpper()))
                {
                    validStart = true;
                    break;
                }
            }
            if (validStart)
            {
                return RecursiveWordFinder(neighbors[j].location, testing);
            }
        }
        return currentWord;
    }

    private BetterButton[] getNeighbors(Point me)
    {
        int numberOfNeighbors = 0;
        
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (me.X + i >= 0 && me.X + i <= 5 &&
                    me.Y + j >= 0 && me.Y + j <= 5)
                {
                    numberOfNeighbors++;
                }
            }
        }

        BetterButton[] ret = new BetterButton[numberOfNeighbors];
        int index = 0;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (me.X + i >= 0 && me.X + i <= 5 &&
                    me.Y + j >= 0 && me.Y + j <= 5)
                {
                    ret[index] = allButtons[index];
                    index++;
                }
            }
        }

        return ret;
    }

    private async void InitBoard()
    {
        score = 0;
        scoreLabel.Text = score + "";
        
        // Define the letters for each die based on the provided patterns.
        string[] patterns = {
            "HISTRV", "TENCCS", "SAAFRA", "DNHHOW", "AENGNM", "QuHeAu",
            "SIYPRY", "NEDNNA", "ICTEIT", "AEEAEE", "GUCFNY", "BZXBJK",
            "NESSUS", "HOHDRL", "TISLEI", "TONWUO", "SETPIL", "◼O◼IE◼",
            "TONDHD", "PISCET", "QuZWJX", "MEEGUA", "MEEEEA", "EOIAUN",
            "SPHTOR", "AOBEID", "WVRROG", "LINMAE", "YIRSAF", "TETTOM",
            "NLDCND", "SELHRI", "ThErLn", "TOUTOK", "OEEAAO", "LORHDN"
        };

        // Fisher-Yates shuffling method
        Random rng = new Random();
        int n = patterns.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            string temp = patterns[k];
            patterns[k] = patterns[n];
            patterns[n] = temp;
        }

        allButtons = new BetterButton[36];

        int patternIndex = 0;

        string[,] board = new string[6, 6];

        prevButtonLocation = new List<Point>();
        prevButtonLocation.Add(new Point(50, 50));
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                BetterButton foo = new BetterButton();
                foo.location = new Point(i, j);
                foo.Background = waitingBackColor;
                foo.TextColor = waitingForeColor;
                foo.FontSize = 18;
                foo.Clicked += (object sender, EventArgs e) =>
                {
                    if (prevButtonLocation.Last() == new Point(50, 50) || prevButtonLocation.Last().Distance(foo.location) <= Math.Sqrt(2) || string.IsNullOrEmpty(currentWord))
                    {
                        if (foo.Background.Equals(new SolidColorBrush(selectedBackColor)))
                        {
                            if (foo.location == prevButtonLocation.Last())
                            {
                                foo.Background = waitingBackColor;
                                foo.TextColor = waitingForeColor;
                                currentWord = currentWord.Remove(currentWord.LastIndexOf(foo.Text[0]), foo.Text.Length);
                                prevButtonLocation.Remove(prevButtonLocation.Last());
                            }
                        }
                        else
                        {
                            foo.Background = selectedBackColor;
                            foo.TextColor = selectedForeColor;
                            currentWord += foo.Text;
                            prevButtonLocation.Add(foo.location);
                        }
                        Word.Text = currentWord.ToUpper();
                    }
                };
                board[i, j] = new SingleDie(patterns[patternIndex]).faceLetter;
                foo.Text += board[i, j];
                patternIndex++;
                allButtons[i * 6 + j] = foo;
                grid.Add(foo, j, i);
            }
        }

        // wordSuccessAudioPlayer = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("winmusic.mp3"));
    }

    private async void Reset_Clicked(object sender, EventArgs e)
    {
        string result = (await DisplayActionSheet("Reset the Board and Points?", "", "", "Yes", "No"));

        if (result.Equals("Yes"))
        {
            ResetGuesses();
            InitBoard();
        }


    }

    private void Submit_Clicked(object sender, EventArgs e)
    {
        
        // Check if the input string is in the list of English words
        bool isRealWord = AllEnglishWords.Contains(currentWord.ToLower());
        bool hasNotBeenGuessedAlready = !alreadyGuessed.Contains(currentWord.ToLower());
        System.Timers.Timer foo = new System.Timers.Timer();

        if (isRealWord && hasNotBeenGuessedAlready)
        {
            int pointsForWord = 0;
            switch (currentWord.Length)
            {
                case 0:
                    pointsForWord = 0;
                    break;
                case 1:
                    pointsForWord = 0;
                    break;
                case 2:
                    pointsForWord = 10;
                    break;
                case 3:
                    pointsForWord = 20;
                    break;
                case 4:
                    pointsForWord = 50;
                    break;
                case 5:
                    pointsForWord = 100;
                    break;
                case 6:
                    pointsForWord = 150;
                    break;
                case 7:
                    pointsForWord = 200;
                    break;
                case 8:
                    pointsForWord = 300;
                    break;
                case >= 9:
                    pointsForWord = 1000;
                    break;
                default:
                    pointsForWord = 1;
                    break;
            }
            scoreIncrementerLabel.Opacity = 1;
            scoreIncrementerLabel.Text = "+ " + pointsForWord;
            score += pointsForWord;
            alreadyGuessed += currentWord.ToLower();
            scoreLabel.Text = score + "";
            foo.Interval = (pointsForWord >= 100) ? 30 : 10;
            // Celebrate();
        }
        ResetGuesses();

        

        foo.Elapsed += Foo_Elapsed;
        foo.Start();
    }

    private IAudioPlayer wordSuccessAudioPlayer;
    private void Celebrate()
    {
        wordSuccessAudioPlayer.Speed += 0.5;
        wordSuccessAudioPlayer.Play();
    }

    private void Foo_Elapsed(object sender, ElapsedEventArgs e)
    {
        scoreIncrementerLabel.Opacity -= 0.01;
        if (scoreIncrementerLabel.Opacity <= 0)
        {
            ((System.Timers.Timer)sender).Stop();
        }
    }

    public class BetterButton : Button
	{
		public Point location;
	}

    private void Trash_Clicked(object sender, EventArgs e)
    {
        // HowManyWordsAreThere();
        ResetGuesses();
    }

    private void ResetGuesses()
    {
        currentWord = "";
        Word.Text = currentWord;

        foreach (Button foo in allButtons)
        {
            foo.Background = waitingBackColor;
            foo.TextColor = waitingForeColor;
        }

        prevButtonLocation = new List<Point>();
        prevButtonLocation.Add(new Point(50, 50));
    }

    /*
	 * Dice
	 * HISTRV
	 * TENCCS
	 * SAAFRA
	 * DNHHOW
	 * AENGNM
	 * SIYPRY
	 * NEDNNA
	 * ICTEIT
	 * AEEAEE
	 * GUCFNY
	 * BZXBJK
	 * NESSUS
	 * HOHDRL
	 * TISLEI
	 * TONWUO
	 * SETPIL
	 * #O#IE#
	 * TONDHD
	 * PISCET
	 * QuZWJXK
	 * MEEGUA
	 * MEEEEA
	 * EOIAUN
	 * SPHTOR
	 * AOBEID
	 * WVRROG
	 * LINMAE
	 * YIRSAF
	 * TETTOM
	 * NLDCND
	 * SELHRI
	 * ThErLnQuHeAu
	 * TOUTOO
	 * OEEAAO
	 * LORHDN
	 * 
	 * 
	 * 
	 * 
	 */

    public class SingleDie
    {
        public string[] letters;
        public string faceLetter
        {
            get
            {
                Random rand = new Random();
                int random = rand.Next(0, 6);

                if (random + 1 < 6 && letters[random + 1].ToString().ToLower().Equals(letters[random + 1]) && !letters[random + 1].Equals("◼"))
                {
                    return letters[random] + letters[random + 1];
                } else if (random - 1 >= 0 && letters[random].ToString().ToLower().Equals(letters[random]) && !letters[random].Equals("◼"))
                {
                    return letters[random - 1] + letters[random];
                }
                {
                    return letters[random];
                }

            }
        }

        public SingleDie(string inputLetters)
        {
            letters = new string[inputLetters.Length];
            for (int i = 0; i < inputLetters.Length; i++)
            {
                if (inputLetters[i].ToString().ToLower().Equals(inputLetters[i]))
                {
                    letters[i - 1] += inputLetters[i];
                } else
                {
                    letters[i] = inputLetters[i] + "";
                }
            }
        }
    }

}

