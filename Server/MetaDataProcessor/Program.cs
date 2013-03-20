using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using Procurios.Public;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using CrisisTracker.Common;
using MySql.Data;
using System.Threading;

namespace MetaDataProcessor
{
    class Geotag
    {
        public string Type;
        public double Latitude;
        public double Longitude;
        public string Name;
        public string ParentCountry;
        public int Count;

        public static double Distance(Geotag g1, Geotag g2)
        {
            return Math.Sqrt(Math.Pow(g1.Latitude - g2.Latitude, 2) + Math.Pow(g1.Longitude - g2.Longitude, 2));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Geotag))
                return false;
            Geotag other = (Geotag)obj;
            return other.Longitude == Longitude && other.Latitude == Latitude;
        }
    }
    class Story
    {
        public string Text;
        public List<Geotag> TwitterGeotags;
    }
    class StoryTagCollection
    {
        public List<Geotag> Geotags;
        public HashSet<string> Keywords;
        public HashSet<string> Entities;

        public StoryTagCollection()
        {
            Geotags = new List<Geotag>();
            Keywords = new HashSet<string>();
            Entities = new HashSet<string>();
        }

        public bool HasTags
        {
            get { return Geotags.Count > 0 || Keywords.Count > 0 || Entities.Count > 0; }
        }

        public override string ToString()
        {
            if (!HasTags)
                return "-";
            return (Geotags.Count > 0 ? "Geo: {" + string.Join(",", Geotags.Select(n => n.Name).ToArray()) + "} " : "")
                + (Keywords.Count > 0 ? "Words: {" + string.Join(",", Keywords.ToArray()) + "} " : "")
                + (Entities.Count > 0 ? "Names: {" + string.Join(",", Entities.ToArray()) + "}" : "");
        }
    }

    class Program
    {
        static string Name = "MetaDataProcessor";
        static string[] _OCInterestingEventTypes = new string[] { 
            "Person", 
            "Location",
            "Position", 
            "AnalystRecommendation", 
            "ArmedAttack", 
            "ArmsPurchaseSale", 
            "Arrest", 
            "Convivtion", 
            "DiplomaticRelations", 
            "EnvironmentalIssue", 
            "Extinction", 
            "ManMadeDisaster", 
            "MilitaryAction", 
            "NaturalDisaster", 
            "PersonAttributes", 
            "PersonCareer", 
            "PersonCommunication", 
            "PersonLocation", 
            "PersonParty", 
            "PersonRelation", 
            "PersonTravel", 
            "PoliticalEndorsment", 
            "PoliticalRelationship"};
        static string[] _OCKeywordFields = new string[] { 
            "attacktype", 
            "weaponused", 
            "armsdesc", 
            "diplomaticaction", 
            "environmentalissue", 
            "disastertype", 
            "manmadedisaster", 
            "naturaldisaster", 
            "personrelationtype", 
            "politicalrelationshiptype"};
        static string[] _OCEntityFields = new string[] { 
            "name", 
            "commonname", 
            "person_source", 
            "company_source", 
            "partyinvolved1", 
            "partyinvolved2", 
            "attacker", 
            "organizationarmspurchaser", 
            "person", 
            "diplomaticentity1", 
            "diplomaticentity2", 
            "species", 
            "person_relative", 
            "military", 
            "position", 
            "country", 
            "facility", 
            "organizationorcompany", 
            "persondescription", 
            "persongroup", 
            "party", 
            "groupendorsee", 
            "groupendorser", 
            "groupendorsed", 
            "politicalentity1", 
            "politicalentity2"};

        static void Main(string[] args)
        {
            #region debug
            //string text = "LONDON (Reuters): GAZIANTEP, Turkey — Syria pulled both Turkey and Israel closer to military entanglements in its civil war on Monday, bombing a rebel-held Syrian village a few yards from the Turkish border in a deadly aerial assault and provoking Israeli tank commanders in the disputed Golan Heights into blasting a mobile Syrian artillery unit across their own armistice line.";
            //string text = "Tel Aviv (AP): Fighter planes from Israel have bombed the Golan Heights for the second day since protests began earlier this week. Today the planes targeted Hamas controlled buildings in the southern parts of the region.";
            //string[] texts = new string[] {
            //    "LONDON (Reuters): GAZIANTEP, Turkey — Syria pulled both Turkey and Israel closer to military entanglements in its civil war on Monday, bombing a rebel-held Syrian village a few yards from the Turkish border in a deadly aerial assault and provoking Israeli tank commanders in the disputed Golan Heights into blasting a mobile Syrian artillery unit across their own armistice line.",
            //    " So much for the dog walk. #trentlock #flood http://t.co/nLC7UYCr, RT @NYorksPolice: RT @envagencyyne: We update our website every 15 mins - stay updated on the latest flood information http://t.co/dNyqc ..., Good day at work #flood http://t.co/zlvIqz7N, #loftus #flood http://t.co/I2fUST6w, Flooding news, flood warnings, weather news, travel news... It's all on our website: http://t.co/TWHe0ymA, We update our website every 15 mins - stay updated on the latest flood information http://t.co/fzLVzYLZ #floodaware, @alisongow and you can also check out the latest on the #flood situation on our website too! http://t.co/oSmOFrXu, RT @EnvAgency: Keep up to date with the latest flood warnings on our website #floodaware http://t.co/HxwVkwCf, Oundle is an island this morning #flood, Our website has broken down – we're working on it! We apologise, VERY bad timing! For #flood info, pls email britt.warg@palletbarrier.com",
            //    " Gunmen Free Inmates At Nigeria Police Station: Gunmen free inmates at Nigeria police station http://t.co/GPVVNo8o, Attack frees Nigeria police inmates, leaves 2 dead: LAGOS, Nigeria     (AP) -- Gunmen attacked a major police st... http://t.co/cvhBu5s6, Gunmen Attack Abuja Boko-Haram Holding Facility http://t.co/BqRFaypR, Gunmen invade anti-robbery squad in Abuja, free suspects http://t.co/w0u0K8pB via @sharethis, Gunmen attack Nigeria police HQ: Unknown gunmen attack a Nigerian police base in the capital, Abuja, where Boko... http://t.co/e5yLONA8, Attack by gunmen on police HQ in #Abuja, Nigeria, leaves 2 officers dead and 5 suspected robbers still on the loose http://t.co/JThuSNVF, Police: Gunmen attack Nigeria police station holding radical sect suspects, freeing prisoners - @AP, Boko Haram Attacks SARS Police HQ, Abuja http://t.co/uiYSwARj, Police in Nigeria say gunmen have attacked a security headquarters in a heavily guarded part of the capital, Abuja, freeing some prisoners., Gunmen attack police stati",
            //    " Syria cluster bomb dropped on playground east of Damascus kills 10 children, activists say http://t.co/NVY0iBud, Syrian cluster bomb attack 'kills several children' http://t.co/kVtGxJvI, Opposition says cluster bombs from Syrian warplanes killed 10 children on a playground: http://t.co/RmsrvTLx, Syrian children 'killed by cluster bombs': Activists say 10 children killed after government forces drop cluster... http://t.co/Zpl6RpNh, RT @Independent: Syria cluster bomb attack 'kills 10 children' in village http://t.co/CVDceSdz, Kids killed by 'cluster bomb'? http://t.co/yYgS2kuH, Syrian attack 'kills 10 children' - Activists in Syria say a government MiG fighter jet has dropped a cluster bomb o... http://t.co/OpJXFzDD, 10 Syrian children 'killed by government bombs' http://t.co/U0hRWOlg, Syria: Evidence Shows Cluster Bombs Killed Children http://t.co/6KVrajVn, BBC News - Syria cluster bomb attack 'kills 10 children' http://t.co/TvQTATmI",
            //    " Flood defences looking good here in stamford bridge, Drought spells tough times for U.S. corn exporters | Harvest Public Media http://t.co/3WCEcRIz, UK floods wreak havoc on houses thought to be protected by state of the art £1.7million flood defences: Da... http://t.co/IWFg69Ey #news, The flood defences of Leamington seem to be holding but it's sending the lot a mile downstream to my neighbourhood. http://t.co/WcvThqlt, Shit getting real out flood defences are out., At #upton with @itvnews @rupertevelyn talking about flood defences",
            //    "How The Flood Of Digital Photos Adds Significance To The Ones We Print by John Paul Titlow : http://t.co/fkgIcBTW, Lol ! RT @IamDJSOSO: A Bomb threat is when a grenade threatens to beat you up., As your face is filled with horror *Bursts out in laughter!*...",
            //    "+Egyptian security forces and protesters clashed near Tahrir Square in Cairo on Sunday. http://t.co/6kIGdeWO, 25/11/12 General view of tents occupy Tahrir square as protesters and activists continue with their sit-in, in... http://t.co/iBIelIae, Egyptian ...",
            //    "Human error to blame for Springfield explosion http://t.co/WaI8dKlP #wfsb, Springfield strip club explosion blamed on 'human error': The massive explosion that rocked Springfield’s... http://t.co/W4Q9UK5S, Human error blamed in gas explosion at strip cl...",
            //    "kamseupay badai lauy becek gempa topan tsunami.........., Traffic Tsunami: Solid, tested methods to drive thousands of visitors to your website. Forget about SEO! http://t.co/UbHCCKgP, Koprol cetar membahana badai halilintar magma tsunami longsor banjir R...",
            //    "Jika's funeral now in #Tahrir - 1st protester to die since #Morsi took power.He had 40 pellets in his head,reportedly, shot by police #Egypt, RT @Hammonda1: This is surreal: #Egypt Interior minister says protesters shot each w birdshot, not police - Egypt...",
            //    "RT @500ThingsMyCatT: PLEASE RT Joy Amid Tragedy Woman Finds Her Cat 2 Weeks After Killer Tornado! http://t.co/amU852p3   http://t.co/yRT ..., A tornado flew around my room before you came, excuse the mess it made., A tornado blew around my room before you...",
            //    "he's jealous for me love's like a hurricane i am a tree bending beneath the weight of his wind and mercy, @BLavell He is jealous for me. Loves like a hurricane, I am a tree. Bending beneath the weight of His wind and mercy!, He is jealous for me.. Loves l...",
            //    "#leo 12,000 inmates gave up their food for the flood victims. . . This made me cry. Salute! http://t.co/ljCHsX0Y, #武士朝代 12,000 inmates gave up their food for the flood victims. . . This made me cry. Salute! http://t.co/HkEc1iEK, #takipedenitakiped...",
            //    "RT @ScotlandEuropa: The protesters are here... http://t.co/OuZaQ6GL, Protesters demand justice for factory fire victims http://t.co/g0BEe4mW, RT @YourAnonNews: Over 6000 Tibetan students on the streets. Riot Police severly injure children. Schools locked ..."
            //};
            #endregion

            Console.WriteLine(Name);

            DateTime t;
            while (true)
            {
                t = DateTime.Now;

                try
                {
                    ProcessBatch();
                }
                catch (Exception e)
                {
                    Output.Print(Name, e);
                }

                if ((DateTime.Now - t).TotalSeconds < 5)
                    Thread.Sleep(30000);
            }
        }

        private static void ProcessBatch()
        {
            Console.Write("Fetching stories...");
            Dictionary<long, Story> stories = GetStories();
            Console.WriteLine(" done");
            Dictionary<long, StoryTagCollection> storyTags = new Dictionary<long, StoryTagCollection>();

            foreach (var story in stories)
            {
                string text = story.Value.Text;
                text = Regex.Replace(text, "[A-Z]+(,\\s+[A-Z][a-z]+)?(\\s+\\((AP|ap|Reuters|reuters)\\))?:?\\s+(—?\\s+)?", "");
                text = Regex.Replace(text, "RT @[a-zA-Z0-9_]+: ", "");
                text = Regex.Replace(text, " @[a-zA-Z0-9_]+ ", "");
                text = text.Replace("#", "");
                story.Value.Text = text;

                storyTags.Add(story.Key, OCGetTags(story.Value));

                Console.WriteLine(storyTags[story.Key]);
            }

            StoreGeotags(storyTags);
        }

        static Dictionary<long, Story> GetStories()
        {
            Dictionary<long, Story> stories = new Dictionary<long, Story>();

            //            stories.Add(1, "A small fire at a seniors' complex in Halifax forced more than 30 residents from their rooms Monday morning.");
            //            stories.Add(2, "A roadside bomb killed three pilgrims walking to a major Shi'ite religious event south of Baghdad on Monday.");
            //            stories.Add(3, "Issac's brother Ramzi Issac, who was arrested along with the London suspect, was also charged with possessing false documents");
            //            stories.Add(4, "More than 250 people died when torrential rains and flooding swept China last month, according to the China Daily.");
            //            stories.Add(5, "Ariel Sharon and Mahmoud Abbas are scheduled to meet with U.S. President Bush on Wednesday at Aqaba, Jordan, to discuss the U.S.-backed road map to Middle East peace.");
            //            stories.Add(10, "LONDON (Reuters): GAZIANTEP, Turkey — Syria pulled both Turkey and Israel closer to military entanglements in its civil war on Monday, bombing a rebel-held Syrian village a few yards from the Turkish border in a deadly aerial assault and provoking Israeli tank commanders in the disputed Golan Heights into blasting a mobile Syrian artillery unit across their own armistice line.");
            //            stories.Add(8,
            //                @"Insurgent attacks kill 21, wound dozens in Iraq: Insurgents launched attacks against security forces and civilia... http://t.co/oui6eEqd 
            //                Multiple car bomb explosions in Iraq kill at least 21: Insurgents launched attacks against security forces and c... http://t.co/zfgmgToV
            //                Insurgent attacks kill 15, wound dozens in Iraq: Insurgents launched attacks against security forces and civilia... http://t.co/9bgBz5NI
            //                Deaths in Iraq bomb explosions: At least 21 people killed, 70 injured in attacks on security forces and civilian... http://t.co/euBS90Jm
            //                DTN Iraq: Insurgent attacks kill 21, wound dozens in Iraq: Insurgents launched attacks against security forces a... http://t.co/vtTjaz2v");
            //            return stories;

            Helpers.RunSelect(Name, @"select 
	            StoryID, 
	            group_concat(Geo separator '\n') Geo,
	            group_concat(Text separator '\n') Text from (
            select T.*,
	            @n:=if(StoryID=@sid,@n+1,1) as itemnumber,
	            @sid:=StoryID
            from
	            (select @sid:=-1, @n:=0) initvars,
	            (
	            select 
		            s.StoryID,
		            count(*) as Cnt,
		            replace(replace(replace(Text,'\r',''),'\n',' '),'""','') Text,
		            group_concat(concat(Latitude, ',', Longitude) separator '\n') Geo
	            from 
		            (select StoryID from Story 
			            where not ProcessedForMetadata and TagScore=0 and TweetCount>10 limit 50) s
		            join TweetCluster tc on tc.StoryID=s.StoryID
		            join Tweet t on t.TweetClusterID=tc.TweetClusterID
            		where RetweetOf is null
	            group by s.StoryID, lcase(IF(left(Text, 30) REGEXP '^RT @[a-zA-Z0-9_]+: ', SUBSTR(Text, LOCATE(':', Text) + 2, 30), left(Text, 30)))
	            order by 1,2 desc
	            ) T
            ) T 
            where itemnumber <= 10
            group by StoryID
            limit 100;",
                stories, (values, reader) =>
                {
                    string text = reader.GetString("Text");
                    string geo = reader["Geo"] == DBNull.Value ? null : reader.GetString("Geo");
                    List<Geotag> tags = new List<Geotag>();
                    if (geo != null)
                    {
                        foreach (string tag in geo.Split("\n".ToArray(), StringSplitOptions.RemoveEmptyEntries))
                        {
                            string[] latlon = tag.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                            if (latlon.Length == 2)
                                tags.Add(new Geotag() { Latitude = Convert.ToDouble(latlon[0]), Longitude = Convert.ToDouble(latlon[1]) });
                        }
                    }
                    stories.Add(reader.GetInt64("StoryID"),
                    new Story() { Text = text, TwitterGeotags = tags });
                }
            );

            return stories;
        }

        static void StoreGeotags(Dictionary<long, StoryTagCollection> stories)
        {
            if (stories.Count == 0)
                return;

            Console.Write("Saving geotags...");

            var hasTags = stories.Where(n => n.Value.HasTags);
            foreach (var item in hasTags)
            {
                //Insert geotags
                foreach (Geotag geotag in item.Value.Geotags)
                {
                    string lon = geotag.Longitude.ToString(CultureInfo.InvariantCulture);
                    string lat = geotag.Latitude.ToString(CultureInfo.InvariantCulture);
                    Helpers.RunSqlStatement(Name, "call AddRemoveStoryTag(1, 'location', 1, 1, " + item.Key + ", null, " + lon + "," + lat + ");");
                }
                //Insert keywords
                foreach (string tag in item.Value.Keywords)
                {
                    string tag2 = tag.Replace("'", "&#39;").ToLower();
                    Helpers.RunSqlStatement(Name, "insert ignore into InfoKeyword (Keyword) values ('" + tag2 + "');");
                    List<long> tagID = new List<long>();
                    Helpers.RunSelect(Name, "select InfoKeywordID from InfoKeyword where Keyword = '" + tag2 + "';", tagID, (values, reader) => values.Add(reader.GetInt64("InfoKeywordID")));
                    Helpers.RunSqlStatement(Name, "call AddRemoveStoryTag(1, 'keyword', 1, 1, " + item.Key + ", " + tagID[0] + ", null, null);");
                }
                //Insert entities
                foreach (string tag in item.Value.Entities)
                {
                    string tag2 = tag.Replace("'", "&#39;").ToLower();
                    Helpers.RunSqlStatement(Name, "insert ignore into InfoEntity (Entity) values ('" + tag2 + "');");
                    List<long> tagID = new List<long>();
                    Helpers.RunSelect(Name, "select InfoEntityID from InfoEntity where Entity = '" + tag2 + "';", tagID, (values, reader) => values.Add(reader.GetInt64("InfoEntityID")));
                    Helpers.RunSqlStatement(Name, "call AddRemoveStoryTag(1, 'entity', 1, 1, " + item.Key + ", " + tagID[0] + ", null, null);");
                }
            }

            //Update Story.ProcessedForMetadata
            Helpers.RunSqlStatement(Name, "update Story set ProcessedForMetadata=1 where StoryID in (" + string.Join(",", stories.Keys.Select(n => n.ToString()).ToArray()) + ");");

            Console.WriteLine(" done");
        }

        static StoryTagCollection OCGetTags(Story story)
        {
            StoryTagCollection tagCollection = new StoryTagCollection();

            try
            {
                Uri address = new Uri("http://api.opencalais.com/tag/rs/enrich");

                //Create the web request  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

                //Set type to POST  
                request.Method = "POST";
                request.ContentType = "text/raw";
                request.Accept = "Application/Json";
                request.Timeout = 15000;

                //Set headers
                request.Headers.Add("x-calais-licenseID", "jb47yzz6jsqkhkbsqc3urfs7");

                //Append text as byte array
                StringBuilder data = new StringBuilder();
                data.Append(story.Text);
                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                //Set the content length in the request headers  
                request.ContentLength = byteData.Length;

                //Write data  
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                //Get response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string resultString = reader.ReadToEnd();

                    //Extract all geotags that have lat-lon coordinates
                    Hashtable result = JSON.JsonDecode(resultString) as Hashtable;
                    List<Geotag> allGeotags = new List<Geotag>();
                    foreach (Hashtable item in result.Values)
                    {
                        if (item.ContainsKey("resolutions")) //This is a geotag
                        {
                            Hashtable resolutions = (Hashtable)((ArrayList)item["resolutions"])[0];

                            if (!resolutions.ContainsKey("latitude"))
                                continue;

                            string parentCountry = "";
                            if (resolutions.ContainsKey("containedbycountry"))
                                parentCountry = resolutions["containedbycountry"].ToString();
                            allGeotags.Add(new Geotag()
                                {
                                    Name = item["name"].ToString(),
                                    ParentCountry = parentCountry,
                                    Type = item["_type"].ToString(),
                                    Latitude = double.Parse(resolutions["latitude"].ToString(), CultureInfo.InvariantCulture),
                                    Longitude = double.Parse(resolutions["longitude"].ToString(), CultureInfo.InvariantCulture),
                                    Count = ((ArrayList)item["instances"]).Count
                                }
                            );
                        }
                        else if (item.ContainsKey("_type") && _OCInterestingEventTypes.Contains((string)item["_type"]))
                        {
                            foreach (DictionaryEntry field in item)
                            {
                                if (_OCKeywordFields.Contains(field.Key) && !((string)field.Value).Contains("http"))
                                    tagCollection.Keywords.Add((string)field.Value);
                                if (_OCEntityFields.Contains(field.Key) && !((string)field.Value).Contains("http"))
                                    tagCollection.Entities.Add((string)field.Value);
                            }
                        }

                        if (item.ContainsKey("_typeGroup") && item["_typeGroup"].ToString() == "topics")
                            tagCollection.Keywords.Add((string)item["categoryName"]);
                    }

                    //Pick geotags to use
                    if (allGeotags.Count > 0 || story.TwitterGeotags.Count > 0)
                    {
                        //Calculate a weighted average location based on OC tags and Twitter geotags, then take the nearest tag and use it as the story location

                        double lonSum = 0;
                        double latSum = 0;
                        double weightSum = 0;

                        foreach (Geotag g in allGeotags.Distinct())
                        {
                            double weight = 0;
                            if (g.Type == "Country")
                                weight = 2;
                            else if (g.Type == "ProvinceOrState")
                                weight = 3;
                            else //if (g.Type == "City")
                                weight = 4;
                            lonSum += weight * g.Longitude;
                            latSum += weight * g.Latitude;
                            weightSum += weight;
                        }

                        foreach (Geotag g in story.TwitterGeotags.Distinct())
                        {
                            double weight = 5;
                            lonSum += weight * g.Longitude;
                            latSum += weight * g.Latitude;
                            weightSum += weight;
                        }

                        Geotag meanLoc = new Geotag() { Latitude = latSum / weightSum, Longitude = lonSum / weightSum };
                        Geotag nearestTag = allGeotags.Union(story.TwitterGeotags).OrderBy(n => Geotag.Distance(n, meanLoc)).First();

                        Console.WriteLine("OC: " + allGeotags.Count + ", Twitter: " + story.TwitterGeotags.Count);

                        tagCollection.Geotags.Add(nearestTag);
                    }

                    //List<Geotag> geotags = new List<Geotag>();
                    //if (allGeotags.Count > 0)
                    //{
                    //    //1) Cities for which the parent country is also mentioned, exept all caps cities (e.g. "LONDON (AP)")
                    //    geotags.AddRange(allGeotags.Where(n => n.Type == "City" && n.ParentCountry == "" || allGeotags.Any(m => m.Name == n.ParentCountry)));
                    //    //2) else: Top city if no country was mentioned
                    //    if (geotags.Count == 0 && allGeotags.Any(n => n.Type == "City") && !allGeotags.Any(n => n.Type == "Country"))
                    //        geotags.Add(allGeotags.Where(n => n.Type == "City").OrderByDescending(n => n.Count).First());
                    //    //3) else: All countries
                    //    if (geotags.Count == 0 && allGeotags.Any(n => n.Type == "Country"))
                    //        geotags.AddRange(allGeotags.Where(n => n.Type == "Country"));
                    //}

                    //if (story.TwitterGeotags.Count > 0)
                    //{
                    //    double meanLon = story.TwitterGeotags.Average(n => n.Longitude);
                    //    double meanLat = story.TwitterGeotags.Average(n => n.Latitude);
                    //    geotags.Add(new Geotag() { Longitude = meanLon, Latitude = meanLat });
                    //}

                    //tagCollection.Geotags.AddRange(geotags);
                }
            }
            catch (Exception e)
            {
                if (e is WebException && (e.Message.Contains("Timeout") || e.Message.Contains("timed out")))
                    Console.WriteLine("Timeout");
                else if (e is WebException && e.Message.Contains("Internal Server Error"))
                    Console.WriteLine("Internal Server Error");
                else
                    CrisisTracker.Common.Output.Print(Name, e);
            }

            return tagCollection;
        }
    }
}
