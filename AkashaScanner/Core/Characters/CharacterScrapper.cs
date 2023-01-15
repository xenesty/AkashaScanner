using AkashaScanner.Core.DataCollections;
using AkashaScanner.Core.Navigation.Character;
using AkashaScanner.Core.ProcessControl;
using AkashaScanner.Core.ResultHandler;
using AkashaScanner.Core.Scappers;
using AkashaScanner.Core.ScrapPlans;
using AkashaScanner.Core.Screenshot;
using AkashaScanner.Core.Suspender;
using AkashaScanner.Core.TextRecognition;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace AkashaScanner.Core.Characters
{
    public class CharacterScrapper : BaseScrapper<Character, ICharacterConfig>
    {
        private static readonly string LockImageFile = Path.Combine(Utils.ExecutableDirectory, "Resources", "lock.png");
        private const double ConstellationLockScore = 0.8;
        private const int CharacterNameOverridesScore = 70;
        private const int ElementScore = 70;
        private const int FriendshipScore = 70;
        private const int TalentBonusScore = 85;

        private static readonly Dictionary<string, Element> Elements = new()
        {
            { "Anemo", Element.Anemo },
            { "Pyro", Element.Pyro },
            { "Cryo", Element.Cryo },
            { "Electro", Element.Electro },
            { "Hydro", Element.Hydro },
            { "Geo", Element.Geo },
            { "Dendro", Element.Dendro },
        };
        private static readonly List<string> ElementNames = Elements.Keys.ToList();

        private static readonly Dictionary<string, (int, int)> AllLevels = new();
        private static readonly List<string> AllLevelsText = new();

        static CharacterScrapper()
        {

            var ascensions = new List<int>() { 1, 20, 40, 50, 60, 70, 80, 90 };
            for (int i = 1; i < ascensions.Count; ++i)
            {
                var asc = i - 1;
                var minLevel = ascensions[asc];
                var maxLevel = ascensions[i];
                for (int level = minLevel; level <= maxLevel; ++level)
                {
                    var text = $"{level} / {maxLevel}";
                    AllLevels.Add(text, (level, asc));
                    AllLevelsText.Add(text);
                }
            }
        }

        private readonly ICharacterNavigation Navigation;
        private readonly ICharacterCollection CharacterCollection;

        private Rectangle AttributesRect;
        private Rectangle ConstellationRect;
        private Rectangle ElementRect;
        private Rectangle TalentRect;

        private Rectangle NameRect;
        private Rectangle LevelRect;
        private Rectangle FriendshipRect;
        private Rectangle TalentLevelRect;
        private Rectangle TalentBonusRect;

        private List<Rectangle> ConstellationIconRects = new();

        public CharacterScrapper(
            ILogger<CharacterScrapper> logger,
            ISuspender suspender,
            GameWindow win,
            IProcessControl control,
            ITextRecognitionService ocr,
            IScreenshotProvider screenshots,
            IResultHandler<Character> resultHandler,
            ICharacterNavigation navigation,
            IScrapPlanManager<ICharacterConfig, Character> scrapPlan,
            ICharacterCollection characterCollection)
        {
            Logger = logger;
            Suspender = suspender;
            Win = win;
            Control = control;
            ResultHandler = resultHandler;
            ScrapPlan = scrapPlan;
            Ocr = ocr;
            Screenshots = screenshots;
            Navigation = navigation;
            CharacterCollection = characterCollection;
        }

        protected override void Init()
        {
            base.Init();
            Navigation.Init();

            AttributesRect = Win.GetRectangle(-310, 80, -70, 380);
            ConstellationRect = Win.GetRectangle(-290, Win.RefHeight / 2 - 195, -190, Win.RefHeight / 2 + 210);
            ElementRect = Win.GetRectangle(90, 10, 160, 40);
            TalentRect = Win.GetRectangle(25, 114, 214, 215);

            NameRect = Win.ScaleRectangle(0, 0, 240, 35);
            LevelRect = Win.ScaleRectangle(60, 54, 90, 30);
            FriendshipRect = Win.ScaleRectangle(25, 274, 194, 25);
            TalentLevelRect = Win.ScaleRectangle(143, 0, 45, 26);
            TalentBonusRect = Win.ScaleRectangle(0, 72, 125, 28);

            ConstellationIconRects = new()
            {
                Win.ScaleRectangle(10, 0, 33, 40),
                Win.ScaleRectangle(46, 76, 33, 40),
                Win.ScaleRectangle(66, 147, 33, 40),
                Win.ScaleRectangle(66, 222, 33, 40),
                Win.ScaleRectangle(46, 296, 33, 40),
                Win.ScaleRectangle(0, 364, 33, 40),
            };
        }

        protected override void Execute(ICharacterConfig config)
        {
            StartMonitoringProcess();
            ScrapPlan.Activate();
            Navigation.SelectAttributes();
            Suspender.Sleep(600);
            using var lockImg = GetLockImage();
            var queue = new TaskQueue();
            var names = new HashSet<string>();
            int order = 0;

            while (!ShouldStop())
            {
                var character = new Character();
                // Capture Attributes
                var attributesImg = Screenshots.Capture(AttributesRect);
                CharacterEntry? entry = null;
                var loadEntryTask = queue.Add(() => entry = LoadEntry(attributesImg, character, config.CharacterNameOverrides));
                var loadLevelTask = queue.Add(() => LoadLevel(attributesImg, character));

                // Select Constellation
                Navigation.SelectConstellation();
                Suspender.Sleep(800);
                loadEntryTask();

                if (entry == null)
                {
                    Logger.LogWarning("Cannot parse character");
                    Navigation.SelectAttributes();
                    Suspender.Sleep(500);
                    Navigation.GoNext();
                    Suspender.Sleep(500);
                    if (ShouldStop()) break;
                    continue;
                }

                if (!names.Add(entry.Name)) break;

                if (ShouldStop()) break;

                // Capture Constellation
                var constellationImg = Screenshots.Capture(ConstellationRect);
                queue.Add(() => character.Constellation = GetConstellation(constellationImg, lockImg));

                loadLevelTask();

                if (character.Ascension > 1)
                {
                    // Select Talents
                    Navigation.SelectTalents();
                    Suspender.Sleep(600);
                    if (ShouldStop()) break;

                    // Capture Talents
                    var attackIdx = entry.Talents.FindIndex(t => t.Type == TalentType.Attack);
                    Navigation.SelectTalent(attackIdx);
                    Suspender.Sleep(200);
                    var attackImg = Screenshots.Capture(TalentRect);
                    queue.Add(() => character.AttackLevel = GetTalentLevel(attackImg, "Normal Attack"));

                    var skillIdx = entry.Talents.FindIndex(t => t.Type == TalentType.Skill);
                    Navigation.SelectTalent(skillIdx);
                    Suspender.Sleep(200);
                    var skillImg = Screenshots.Capture(TalentRect);
                    queue.Add(() => character.SkillLevel = GetTalentLevel(skillImg, "Skill"));

                    var burstIdx = entry.Talents.FindIndex(t => t.Type == TalentType.Burst);
                    Navigation.SelectTalent(burstIdx);
                    Suspender.Sleep(200);
                    var burstImg = Screenshots.Capture(TalentRect);
                    queue.Add(() => character.BurstLevel = GetTalentLevel(burstImg, "Burst"));

                    Navigation.DeselectTalent();
                    Suspender.Sleep(200);
                }
                else
                {
                    character.AttackLevel = 1;
                    character.SkillLevel = 1;
                    character.BurstLevel = 1;
                }

                // Go next and select Attributes
                Navigation.SelectAttributes();
                Suspender.Sleep(500);
                Navigation.GoNext();

                if (ShouldStop()) break;

                queue.WaitAll();

                Logger.LogInformation("character {character}", character);

                var k = ++order;
                var result = ScrapPlan.Add(character, k);
                if (result.ShouldKeep())
                    ResultHandler.Add(character, k);

                if (result.ShouldStop()) break;
                if (ShouldStop()) break;
            }
            StopMonitoringProcess();
            if (!Interrupted)
            {
                ResultHandler.Save();
            }
        }

        private Mat GetLockImage()
        {
            using var lockImg = Cv2.ImRead(LockImageFile, ImreadModes.Grayscale);
            var scale = Win.ScaleMultiplier;
            var dest = new Mat();
            Cv2.Resize(lockImg, dest, new OpenCvSharp.Size(width: scale * lockImg.Cols, height: scale * lockImg.Rows));
            return dest;
        }

        private CharacterEntry? LoadEntry(Bitmap image, Character character, Dictionary<string, string> CharacterNameOverrides)
        {
            var friendship = Ocr.FindText(image, region: FriendshipRect, inverted: true);
            var hasFriendship = ProcessFriendship(character, friendship);
            if (hasFriendship)
            {
                // Is not the traveler
                var name = Ocr.FindText(image, region: NameRect, inverted: true);
                return ProcessName(character, name, CharacterNameOverrides);
            }
            else
            {
                // Is the traveler
                var elementImg = Screenshots.Capture(ElementRect);
                var element = Ocr.FindText(elementImg, inverted: true);
                return ProcessTravelerElement(character, element);
            }
        }

        private bool ProcessFriendship(Character character, string text)
        {
            var noDigitText = Regex.Replace(text, @"\d", string.Empty).Trim()!;
            if (noDigitText.FuzzySearch("Friendship ") > FriendshipScore)
            {
                var digitText = Regex.Replace(text[^2..], @"\D", string.Empty);
                if (int.TryParse(digitText, out int value))
                {
                    character.Friendship = value;
                }
                else
                {
                    Logger.LogWarning("Fail to parse friendship: {text}", text);
                }
                return true;
            }
            Logger.LogInformation("Cannot find friendship, assuming traveler");
            return false;
        }

        private void LoadLevel(Bitmap image, Character character)
        {
            var text = Ocr.FindText(image, region: LevelRect, inverted: true);
            (var score, var output) = text.FuzzySearch(AllLevelsText);
            (var level, var ascension) = AllLevels[output!];
            Logger.LogDebug("Identify {text} as (Level: {level}, Ascension: {ascension}) with confidence {score}/100", text, level, ascension, score);
            character.Level = level;
            character.Ascension = ascension;
        }

        private CharacterEntry? ProcessName(Character character, string text, Dictionary<string, string> CharacterNameOverrides)
        {
            text = Regex.Replace(text, @"[^a-zA-Z ]", string.Empty).Trim();
            foreach (var (actualName, givenName) in CharacterNameOverrides)
            {
                var score = givenName.PartialSearch(text);
                if (score > CharacterNameOverridesScore)
                {
                    Logger.LogInformation("Identify {text} as {actualName}", text, actualName);
                    text = actualName;
                    break;
                }
            }
            var entry = CharacterCollection.PartialSearchByName(text);
            if (entry != null)
            {
                character.Name = entry.Name;
                character.Rarity = entry.Rarity;
                character.WeaponType = entry.WeaponType;
                character.Element = entry.Element;
            }
            else
            {
                Logger.LogWarning("Fail to find a character matching {text}", text);
            }
            return entry;
        }

        private CharacterEntry? ProcessTravelerElement(Character character, string text)
        {
            var asciiText = Regex.Replace(text, @"[^a-zA-Z]", string.Empty).Trim();
            (int score, var elementName) = ElementNames.Search(name => name.PartialSearch(asciiText));
            Logger.LogDebug("Identify {text} as {elementName} with confidence {score}/100", text, elementName, score);
            if (elementName != null && score > ElementScore)
            {
                var entry = CharacterCollection.GetTravelerByElement(Elements[elementName]);
                if (entry != null)
                {
                    character.Name = entry.Name;
                    character.Rarity = entry.Rarity;
                    character.WeaponType = entry.WeaponType;
                    character.Element = entry.Element;
                    return entry;
                }
            }
            Logger.LogWarning("Fail to identify Traveler's element: {text}", text);
            return null;
        }

        private int GetConstellation(Bitmap image, Mat lockImg)
        {
            if (MatchConstellationIcon(image, lockImg, 3))
            {
                if (MatchConstellationIcon(image, lockImg, 5))
                {
                    if (MatchConstellationIcon(image, lockImg, 6))
                    {
                        return 6;
                    }
                    else
                    {
                        return 5;
                    }
                }
                else if (MatchConstellationIcon(image, lockImg, 4))
                {
                    return 4;
                }
                else
                {
                    return 3;
                }
            }
            else if (MatchConstellationIcon(image, lockImg, 1))
            {
                if (MatchConstellationIcon(image, lockImg, 2))
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return 0;
            }
        }

        private bool MatchConstellationIcon(Bitmap image, Mat lockImg, int constellation)
        {
            var rect = ConstellationIconRects[constellation - 1];
            using var img = image.Clone(rect, PixelFormat.Format8bppIndexed);
            using var mat = img.ToMat();
            using var ret = new Mat();
            Cv2.Threshold(mat, ret, 128, 255, ThresholdTypes.Binary);
            using var result = new Mat();
            Cv2.MatchTemplate(ret, lockImg, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out double _, out double max);
            return max < ConstellationLockScore;
        }

        private int GetTalentLevel(Bitmap image, string field)
        {
            using var ocr = Ocr.GetInstance();
            var levelText = ocr.FindText(image, region: TalentLevelRect, inverted: true);
            var digitLevelText = Regex.Replace(levelText, @"\D", string.Empty);
            if (int.TryParse(digitLevelText, out int currentLevel))
            {
                Logger.LogDebug("Identify {text} as {level} when parsing talent level of {field}", levelText, currentLevel, field);
                int bonusLevel = 0;
                var bonusText = ocr.FindText(image, region: TalentBonusRect, inverted: true);
                var noDigitBonusText = Regex.Replace(bonusText, @"\d", string.Empty);
                if (noDigitBonusText.FuzzySearch("Talent Lv. +") > TalentBonusScore)
                {
                    var digitBonusText = Regex.Replace(bonusText, @"\D", string.Empty);
                    _ = int.TryParse(digitBonusText, out bonusLevel);
                    Logger.LogDebug("Found bonus level {bonusLevel} when parsing talent level of {field}", bonusLevel, field);
                }
                else
                {
                    Logger.LogDebug("Bonus level not found when parsing talent level of {field}", field);
                }
                if (bonusLevel > currentLevel)
                {
                    Logger.LogWarning("Bonus level is greater than parsed level, assuming talent level of {field} is {actualLevel} instead of {level}", field, bonusLevel + 1, currentLevel);
                    return 1;
                }
                return currentLevel - bonusLevel;
            }
            // Whenever it fails to recognize the level it is most likely to be 1
            Logger.LogWarning("Fail to parse talent level of {field}, assuming it is 1: {text}", field, levelText);
            return 1;
        }
    }
}
