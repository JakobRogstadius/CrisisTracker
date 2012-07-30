using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace EnglishStemmer
{

	public class EnglishWord
	{
		internal static EnglishWord CreateWithR1R2(string text)
		{
			EnglishWord result = CreateForTest(text);
			result._r1 = CalculateR(result.Stem, 0);
			result._r2 = CalculateR(result.Stem, result._r1.Start);
			return result;
		}

		internal static EnglishWord CreateForTest(string text)
		{
			EnglishWord word = new EnglishWord();
			word.Create(text);
			return word;
		}

		/// <summary>
		/// Default constructor for Unit Testing
		/// </summary>
		internal EnglishWord()
		{ }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="input">Text that we use to build the stem</param>
		public EnglishWord(string input)
		{
            try
            {
                Create(input);
                GenerateStem();
            }
            catch (Exception)
            {
                Stem = input;
            }
		}

		/// <summary>
		/// Refactored construction. Gets the object into the correct initial state
		/// </summary>
		/// <param name="input"></param>
		private void Create(string input)
		{
			Stem = input.ToLower();
			_original = input;
		}

		static readonly char[] Vowels = new char[6] { 'a', 'e', 'i', 'o', 'u', 'y' };
		static readonly char[] NotShortSyllableNonVowels = new char[9] { 'a', 'e', 'i', 'o', 'u', 'y', 'w', 'x', 'Y' };
		static readonly string[] DoubleChars = new string[9] { "bb", "dd", "ff", "gg", "mm", "nn", "pp", "rr", "tt" };
		const string ValidLIEnding = "cdeghkmnrt";

		private String _stem = String.Empty;
		private String _original = String.Empty;
		private WordRegion _r1 = null;
		private WordRegion _r2 = null;

		private bool _cachedUpToDate = false;

		public string Stem
		{
			get { return _stem; }
			set
			{
				if (_stem != value)
				{
					_stem = value;
					_cachedUpToDate = false;
				}
			}
		}

		internal string GetR1()
		{
			CheckCache();
			return _r1.Text;
		}

		internal string GetR2()
		{
			CheckCache();
			return _r2.Text;
		}

		private void CheckCache()
		{
			if (_cachedUpToDate == false)
			{
				_r1.GenerateRegion(_stem);
				_r2.GenerateRegion(_stem);
				_cachedUpToDate = true;
			}
		}

		public String Original
		{
			get { return _original; }
		}

		internal void GenerateStem()
		{
			if (IsException1())
				return;
			//If the word has two letters or less, leave it as it is. 
			if (Stem.Length < 3)
				return;

			//Remove initial ', if present. +
			StandardiseApostrophesAndStripLeading();

			//Set initial y, or y after a vowel, to Y
			MarkYs();

			//establish the regions R1 and R2. (See note on vowel marking.)
			if (Stem.StartsWith("gener")
				|| Stem.StartsWith("arsen"))
			{
				_r1 = CalculateR(Stem, 2);
			}
			else if (Stem.StartsWith("commun"))
			{
				_r1 = CalculateR(Stem, 3);
			}
			else
			{
				_r1 = CalculateR(Stem, 0);
			}
			_r2 = CalculateR(Stem, _r1.Start);

			//Step0
			StripTrailingApostrophe();
			//Step1a
			StripSuffixStep1a();

			if (IsException2())
				return;

			//Step1b
			StripSuffixStep1b();
			//Step 1c: * 
			ReplaceSuffixStep1c();
			//Step2
			ReplaceEndingStep2();
			//Step3
			ReplaceEndingStep3();
			//Step4
			StripSuffixStep4();
			//Step5
			StripSuffixStep5();
			//Finally, turn any remaining Y letters in the word back into lower case. 
			Finally();
		}



		/// <summary>
		///define exception2 as (
		///[substring] atlimit among(
		///    'inning' 'outing' 'canning' 'herring' 'earring'
		///    'proceed' 'exceed' 'succeed'
		///
		///    // ... extensions possible here ...
		///)
		/// </summary>
		/// <returns></returns>
		private bool IsException2()
		{
			switch (Stem)
			{
				case "inning":
					return true;
				case "outing":
					return true;
				case "canning":
					return true;
				case "herring":
					return true;
				case "earring":
					return true;
				case "proceed":
					return true;
				case "exceed":
					return true;
				case "succeed":
					return true;
			}
			return false;
		}

		/// <summary>
		///	define exception1 as (
		///    [substring] atlimit among(
		///
		///        /* special changes: */
		///
		///        'skis'      (<-'ski')
		///        'skies'     (<-'sky')
		///        'dying'     (<-'die')
		///        'lying'     (<-'lie')
		///        'tying'     (<-'tie')
		///
		///        /* special -LY cases */
		///
		///        'idly'      (<-'idl')
		///        'gently'    (<-'gentl')
		///        'ugly'      (<-'ugli')
		///        'early'     (<-'earli')
		///        'only'      (<-'onli')
		///        'singly'    (<-'singl')
		///
		///        // ... extensions possible here ...
		///
		///        /* invariant forms: */
		///
		///        'sky'
		///        'news'
		///        'howe'
		///
		///        'atlas' 'cosmos' 'bias' 'andes' // not plural forms
		///
		///        // ... extensions possible here ...
		///    )
		///)
		/// </summary>
		/// <returns></returns>
		private bool IsException1()
		{
			switch (Stem)
			{
				case "skis":
					Stem = "ski";
					return true;
				case "skies":
					Stem = "sky";
					return true;
				case "dying":
					Stem = "die";
					return true;
				case "lying":
					Stem = "lie";
					return true;
				case "tying":
					Stem = "tie";
					return true;

				case "idly":
					Stem = "idl";
					return true;
				case "gently":
					Stem = "gentl";
					return true;
				case "ugly":
					Stem = "ugli";
					return true;
				case "early":
					Stem = "earli";
					return true;
				case "only":
					Stem = "onli";
					return true;
				case "singly":
					Stem = "singl";
					return true;

				case "sky":
					return true;
				case "news":
					return true;
				case "howe":
					return true;

				case "atlas":
					return true;
				case "cosmos":
					return true;
				case "bias":
					return true;
				case "andes":
					return true;
			}
			return false;
		}

		/// <summary>
		///Finally, turn any remaining Y letters in the word back into lower case. 
		/// </summary>
		internal void Finally()
		{
			Stem = Stem.Replace("Y", "y");
		}

		/// <summary>
		/// Step 5: * Search for the the following suffixes, and, if found, perform the action indicated. 
		///e delete if in R2, or in R1 and not preceded by a short syllable 
		/// l delete if in R2 and preceded by l 
		/// </summary>
		internal void StripSuffixStep5()
		{
			if (EndsWithAndInR2("e")
				|| (EndsWithAndInR1("e") && IsShortSyllable(Stem.Length - 3) == false))
			{
				Stem = Stem.Remove(Stem.Length - 1);
				return;
			}
			if (EndsWithAndInR2("l")
				&& Stem.EndsWith("ll"))
			{
				Stem = Stem.Remove(Stem.Length - 1);
				return;
			}
		}

		/// <summary>
		/// Step 4: Search for the longest among the following suffixes, and, if found and in R2, perform the action indicated. 
		/// al   ance   ence   er   ic   able   ible   ant   ement   ment   ent   ism   ate   iti   ous   ive   ize delete 
		/// ion delete if preceded by s or t 
		/// </summary>
		internal void StripSuffixStep4()
		{
			if (EndsWithAndInR2("ement"))
			{
				Stem = Stem.Remove(Stem.Length - 5);
				return;
			}
			if (EndsWithAndInR2("ance")
				|| EndsWithAndInR2("ence")
				|| EndsWithAndInR2("able")
				|| EndsWithAndInR2("ible")
				|| EndsWithAndInR2("ment"))
			{
				Stem = Stem.Remove(Stem.Length - 4);
				return;
			}

			if (EndsWithAndInR2("ion")
				&& (Stem.EndsWith("tion") || Stem.EndsWith("sion")))
			{
				Stem = Stem.Remove(Stem.Length - 3);
				return;
			}

			if (Stem.EndsWith("ment"))
				return; //breaking change, but makes the voc.txt parse correctly

			if (EndsWithAndInR2("ant")
				|| EndsWithAndInR2("ent")
				|| EndsWithAndInR2("ism")
				|| EndsWithAndInR2("ate")
				|| EndsWithAndInR2("iti")
				|| EndsWithAndInR2("ous")
				|| EndsWithAndInR2("ize")
				|| EndsWithAndInR2("ive"))
			{
				Stem = Stem.Remove(Stem.Length - 3);
				return;
			}
			if (EndsWithAndInR2("al")
				|| EndsWithAndInR2("er")
				|| EndsWithAndInR2("ic")
				)
			{
				Stem = Stem.Remove(Stem.Length - 2);
				return;
			}
		}

		/// <summary>
		/// Step 3: 
		///Search for the longest among the following suffixes, and, if found and in R1, perform the action indicated. 
		///
		///tional+:   replace by tion 
		///ational+:   replace by ate 
		///alize:   replace by al 
		///icate   iciti   ical:   replace by ic 
		///ful   ness:   delete 
		///ative*:   delete if in R2 
		/// </summary>
		internal void ReplaceEndingStep3()
		{
			//ational+:   replace by ate 
			if (EndsWithAndInR1("ational"))
			{
				Stem = Stem.Substring(0, Stem.Length - 5) + "e";
				return;
			};
			//tional+:   replace by tion
			if (EndsWithAndInR1("tional"))
			{
				Stem = Stem.Substring(0, Stem.Length - 2);
				return;
			};
			//alize:   replace by al
			if (EndsWithAndInR1("alize"))
			{
				Stem = Stem.Substring(0, Stem.Length - 3);
				return;
			};
			//ative*:   delete if in R2 
			if (EndsWithAndInR2("ative"))
			{
				Stem = Stem.Substring(0, Stem.Length - 5);
				return;
			};
			//icate  :   replace by ic 
			//iciti :   replace by ic 
			if (EndsWithAndInR1("icate") || EndsWithAndInR1("iciti"))
			{
				Stem = Stem.Substring(0, Stem.Length - 3);
				return;
			};
			//ical:   replace by ic 
			if (EndsWithAndInR1("ical"))
			{
				Stem = Stem.Substring(0, Stem.Length - 2);
				return;
			};

			//   ness:   delete 
			if (EndsWithAndInR1("ness"))
			{
				Stem = Stem.Substring(0, Stem.Length - 4);
				return;
			};
			//ful   :   delete 
			if (EndsWithAndInR1("ful"))
			{
				Stem = Stem.Substring(0, Stem.Length - 3);
				return;
			};
		}

		/// <summary>
		/// Step 2: 
		///Search for the longest among the following suffixes, and, if found and in R1, perform the action indicated.
		///
		///tional:   replace by tion 
		///enci:   replace by ence 
		///anci:   replace by ance 
		///abli:   replace by able 
		///entli:   replace by ent 
		///izer   ization:   replace by ize 
		///ational   ation   ator:   replace by ate 
		///alism   aliti   alli:   replace by al 
		///fulness:   replace by ful 
		///ousli   ousness:   replace by ous 
		///iveness   iviti:   replace by ive 
		///biliti   bli+:   replace by ble 
		///ogi+:   replace by og if preceded by l 
		///fulli+:   replace by ful 
		///lessli+:   replace by less 
		///li+:   delete if preceded by a valid li-ending
		/// </summary>
		internal void ReplaceEndingStep2()
		{
			//Step 2: 
			//Search for the longest among the following suffixes, and, if found and in R1, perform the action indicated. 

			if (EndsWithAndInR1("ational"))
			{
				//7 ational   ation   ator:   replace by ate 
				Stem = Stem.Substring(0, Stem.Length - 7) + "ate";
				return;
			};
			if (EndsWithAndInR1("fulness"))
			{
				//7 fulness:   replace by ful 
				Stem = Stem.Substring(0, Stem.Length - 7) + "ful";
				return;
			};
			if (EndsWithAndInR1("iveness"))
			{
				//7 iveness   iviti:   replace by ive 
				Stem = Stem.Substring(0, Stem.Length - 7) + "ive";
				return;
			};
			if (EndsWithAndInR1("ization"))
			{
				//7 izer   ization:   replace by ize 
				Stem = Stem.Substring(0, Stem.Length - 7) + "ize";
				return;
			};
			if (EndsWithAndInR1("ousness"))
			{
				//7 ousli   ousness:   replace by ous
				Stem = Stem.Substring(0, Stem.Length - 7) + "ous";
				return;
			};
			if (EndsWithAndInR1("biliti"))
			{
				//6 biliti   bli+:   replace by ble
				Stem = Stem.Substring(0, Stem.Length - 6) + "ble";
				return;
			};
			if (EndsWithAndInR1("lessli"))
			{
				//6 lessli+:   replace by less 
				Stem = Stem.Substring(0, Stem.Length - 6) + "less";
				return;
			};
			if (EndsWithAndInR1("tional"))
			{
				//6 tional:   replace by tion 
				Stem = Stem.Substring(0, Stem.Length - 6) + "tion";
				return;
			};
			if (EndsWithAndInR1("alism"))
			{
				//5 alism   aliti   alli:   replace by al 
				Stem = Stem.Substring(0, Stem.Length - 5) + "al";
				return;
			};
			if (EndsWithAndInR1("aliti"))
			{
				//5 alism   aliti   alli:   replace by al 
				Stem = Stem.Substring(0, Stem.Length - 5) + "al";
				return;
			};
			if (EndsWithAndInR1("ation"))
			{
				//5 ational   ation   ator:   replace by ate
				Stem = Stem.Substring(0, Stem.Length - 5) + "ate";
				return;
			};
			if (EndsWithAndInR1("entli"))
			{
				//5 entli:   replace by ent 
				Stem = Stem.Substring(0, Stem.Length - 5) + "ent";
				return;
			};
			if (EndsWithAndInR1("fulli"))
			{
				//5 fulli+:   replace by ful 
				Stem = Stem.Substring(0, Stem.Length - 5) + "ful";
				return;
			};
			if (EndsWithAndInR1("iviti"))
			{
				//5 iveness   iviti:   replace by ive 
				Stem = Stem.Substring(0, Stem.Length - 5) + "ive";
				return;
			};
			if (EndsWithAndInR1("ousli"))
			{
				//5 ousli   ousness:   replace by ous 
				Stem = Stem.Substring(0, Stem.Length - 5) + "ous";
				return;
			};
			if (EndsWithAndInR1("abli"))
			{
				//4 abli:   replace by able 
				Stem = Stem.Substring(0, Stem.Length - 4) + "able";
				return;
			};
			if (EndsWithAndInR1("alli"))
			{
				//4 alism   aliti   alli:   replace by al 
				Stem = Stem.Substring(0, Stem.Length - 4) + "al";
				return;
			};
			if (EndsWithAndInR1("anci"))
			{
				//4 anci:   replace by ance 
				Stem = Stem.Substring(0, Stem.Length - 4) + "ance";
				return;
			};
			if (EndsWithAndInR1("ator"))
			{
				//5 ational   ation   ator:   replace by ate
				Stem = Stem.Substring(0, Stem.Length - 4) + "ate";
				return;
			};
			if (EndsWithAndInR1("enci"))
			{
				//4 enci:   replace by ence 
				Stem = Stem.Substring(0, Stem.Length - 4) + "ence";
				return;
			};
			if (EndsWithAndInR1("izer"))
			{
				//4 izer   ization:   replace by ize 
				Stem = Stem.Substring(0, Stem.Length - 4) + "ize";
				return;
			};
			if (EndsWithAndInR1("bli"))
			{
				//3 biliti   bli+:   replace by ble
				Stem = Stem.Substring(0, Stem.Length - 3) + "ble";
				return;
			};
			if (EndsWithAndInR1("ogi"))
			{
				//3 ogi+:   replace by og if preceded by l 
				Stem = Stem.Substring(0, Stem.Length - 3) + "og";
				return;
			};
			if (EndsWithAndInR1("li")
				&& IsValidLIEnding())
			{
				//2 li+:   delete if preceded by a valid li-ending
				Stem = Stem.Substring(0, Stem.Length - 2);
				return;
			};
		}

		private bool IsValidLIEnding()
		{
			if (Stem.Length > 2)
			{
				string preLi = Stem.Substring(Stem.Length - 3, 1);
				return ValidLIEnding.Contains(preLi);
			}
			return false;
		}

		private bool EndsWithAndInR1(string suffix)
		{
			return (Stem.EndsWith(suffix)
				&& GetR1().Contains(suffix));
		}

		private bool EndsWithAndInR2(string suffix)
		{
			return (Stem.EndsWith(suffix)
				&& GetR2().Contains(suffix));
		}


		/// <summary>
		/// replace suffix y or Y by i if preceded by a non-vowel which is not the first letter of the word (so cry -> cri, by -> by, say -> say)
		/// </summary>
		internal void ReplaceSuffixStep1c()
		{
			//replace suffix y or Y by i if preceded by a non-vowel which is not the first letter of the word (so cry -> cri, by -> by, say -> say)
			if (Stem.EndsWith("y", StringComparison.OrdinalIgnoreCase)
				&& (Stem.Length > 2)
				&& (Stem.IndexOfAny(Vowels, Stem.Length - 2) != Stem.Length - 2))
			{
				Stem = Stem.Substring(0, Stem.Length - 1) + "i";
			}
		}

		/// <summary>
		/// Converts all quote variants `’ " to standard '. Removes an open quote in First char
		/// </summary>
		internal void StandardiseApostrophesAndStripLeading()
		{
			//Make Apostrophes consistent
			Stem = Stem.Replace('’', '\'').Replace('`', '\'').Replace('"', '\'');
			//Remove initial ', if present. 
			if (Stem[0] == '\'')
				Stem = Stem.Remove(0, 1);
		}

		/// <summary>
		/// R1 is the region after the first non-vowel following a vowel, or is the null region at the end of the word if there is no such non-vowel. 
		/// </summary>
		/// <returns></returns>
		internal static WordRegion CalculateR(string word, int offset)
		{
			if (offset >= word.Length)
				return new WordRegion(word.Length, word.Length);

			int firstVowel = word.IndexOfAny(Vowels, offset);
			int firstNonVowel = IndexOfNone(word, Vowels, firstVowel);
			int nextVowel = firstNonVowel + 1;
			//int nextNonVowel = IndexOfNone(word, nextVowel, vowels);

			WordRegion result = new WordRegion();
			if (nextVowel > 0
				&& nextVowel < word.Length)
				result.Start = nextVowel;
			else
				result.Start = word.Length;
			result.End = word.Length;
			return result;
		}

		internal static int IndexOfNone(string word, char[] ignoredChars, int first)
		{
			if (first < 0)
				return -1;

			int firstNone = first;
			do
			{
				firstNone++;
			}
			while (firstNone < word.Length
					&& word.Substring(firstNone, 1).IndexOfAny(ignoredChars) > -1);
			return firstNone;
		}

		/// <summary>
		/// Search for the longest among the following suffixes, and perform the action indicated. 
		/// eed   eedly+
		///		replace by ee if in R1 
		///
		///ed   edly+   ing   ingly+ 
		///		delete if the preceding word part contains a vowel, and then 
		///		if the word ends at, bl or iz add e (so luxuriat -> luxuriate), or 
		///		if the word ends with a double remove the last letter (so hopp -> hop), or 
		///		if the word is short, add e (so hop -> hope) 
		/// </summary>
		internal void StripSuffixStep1b()
		{
			// eed   eedly+ - replace by ee if in R1 
			if (Stem.EndsWith("eed")
				|| Stem.EndsWith("eedly"))
			{
				if (EndsWithAndInR1("eed")
					|| EndsWithAndInR1("eedly"))
				{
					if (_r1.Contains(Stem.Length))
					{
						Stem = Stem.Substring(0, Stem.LastIndexOf("eed")) + "ee";
					}
				}
				return;
			}

			// ed   edly+   ing   ingly+ - delete if the preceding word part contains a vowel, and then 
			if ((Stem.EndsWith("ed") && Stem.IndexOfAny(Vowels, 0, Stem.Length - 2) != -1)
				|| (Stem.EndsWith("edly") && Stem.IndexOfAny(Vowels, 0, Stem.Length - 4) != -1)
				|| (Stem.EndsWith("ing") && Stem.IndexOfAny(Vowels, 0, Stem.Length - 3) != -1)
				|| (Stem.EndsWith("ingly") && Stem.IndexOfAny(Vowels, 0, Stem.Length - 5) != -1))
			{
				StripEnding(new string[4] { "ed", "edly", "ing", "ingly" });
				// if the word ends at, bl or iz add e (so luxuriat -> luxuriate), or 
				if (Stem.EndsWith("at")
					|| Stem.EndsWith("bl")
					|| Stem.EndsWith("iz"))
				{
					Stem += "e";
					return;
				}
				// if the word ends with a double remove the last letter (so hopp -> hop), or 				
				string end2chars = Stem.Substring(Stem.Length - 2, 2);
				List<string> doubleEndings = new List<string>(DoubleChars);
				if (doubleEndings.Contains(end2chars))
				{
					Stem = Stem.Remove(Stem.Length - 1);
					return;
				}
				// if the word is short, add e (so hop -> hope) 
				if (IsShortWord())
				{
					Stem += "e";
					return;
				}

			}
		}

		/// <summary>
		/// A word is called short if it ends in a short syllable, and if R1 is null. 
		/// </summary>
		/// <returns></returns>
		internal bool IsShortWord()
		{
			//about to use R1
			CheckCache();

			//A word is called short if it ends in a short syllable, and if R1 is null. 
			int lastVowelIndex = Stem.LastIndexOfAny(Vowels);
			return (lastVowelIndex > -1
				&& IsShortSyllable(lastVowelIndex)
				&& String.IsNullOrEmpty(GetR1()));
		}

		/// <summary>
		/// Define a short syllable in a word as either 
		/// (a) a vowel followed by a non-vowel other than w, x or Y and preceded by a non-vowel, or * 
		/// (b) a vowel at the beginning of the word followed by a non-vowel. 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		internal bool IsShortSyllable(int index)
		{
			//Define a short syllable in a word as either 
			int vowelIndex = Stem.IndexOfAny(Vowels, index);
			if (vowelIndex < 0)  //Don't have a vowel
				return false;

			//(a) a vowel followed by a non-vowel other than w, x or Y and preceded by a non-vowel, 
			if (vowelIndex > 0)
			{
				int expectedShortEnd = vowelIndex + 2;//Check word has room for a single non-vowel 
				//and so we add two because length is one more than index of last char
				int nextVowelIndex = Stem.IndexOfAny(Vowels, vowelIndex + 1);
				if (nextVowelIndex == expectedShortEnd
					|| Stem.Length == expectedShortEnd)
				{
					int nonVowelIndex = Stem.IndexOfAny(NotShortSyllableNonVowels, vowelIndex + 1);
					int earlyVowelIndex = Stem.IndexOfAny(Vowels, vowelIndex - 1);
					return (nonVowelIndex != vowelIndex + 1  //Check not-a-vowel (or w,x,Y) found after vowel
								&& earlyVowelIndex == vowelIndex); // Check no vowels found in char before vowel
				}
				else
					return false;
			}
			else
			//or * (b) a vowel at the beginning of the word followed by a non-vowel. 
			{
				return (Stem.IndexOfAny(Vowels) == 0
					&& Stem.Length > 1
					&& Stem.IndexOfAny(Vowels, 1) != 1);
			}
		}

		private bool StripEnding(string[] endings)
		{
			foreach (string ending in endings)
			{
				if (Stem.EndsWith(ending))
				{
					Stem = Stem.Remove(Stem.Length - ending.Length);
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// Search for the longest among the following suffixes, and perform the action indicated.
		///	sses 
		///		replace by ss 
		///		
		///	ied+   ies* 
		///		replace by i if preceded by more than one letter, otherwise by ie (so ties -> tie, cries -> cri) 
		///
		///	s 
		///		delete if the preceding word part contains a vowel not immediately before the s (so gas and this retain the s, gaps and kiwis lose it)
		///
		///	us+   ss 
		///		do nothing 
		/// </summary>
		internal void StripSuffixStep1a()
		{
			//sses - replace by ss 
			if (Stem.EndsWith("sses"))
			{
				Stem = Stem.Substring(0, Stem.Length - 2); //4 to remove sses -2 to re-introduce ss
				return;
			}

			//ied+   ies* - replace by i if preceded by more than one letter, otherwise by ie (so ties -> tie, cries -> cri) 
			if (Stem.EndsWith("ies")
				|| Stem.EndsWith("ied"))
			{
				if (Stem.Length > 4)
					Stem = Stem.Substring(0, Stem.Length - 2);
				else
					Stem = Stem.Substring(0, Stem.Length - 1);
				return;
			}

			//us+   ss - do nothing 
			if (Stem.EndsWith("us")
				|| Stem.EndsWith("ss"))
			{
				return;
			}

			//s  - delete if the preceding word part contains a vowel not immediately 
			//		before the s (so gas and this retain the s, gaps and kiwis lose it)
			if (Stem.EndsWith("s")
				&& Stem.Length > 2
				&& Stem.Substring(0, Stem.Length - 2).IndexOfAny(Vowels) > -1)
			{
				Stem = Stem.Substring(0, Stem.Length - 1);
				return;
			}
		}

		/// <summary>
		/// Handle the three forms of closing apostrophe
		/// </summary>
		internal void StripTrailingApostrophe()
		{
			if (Stem.EndsWith("'s'"))
			{
				Stem = Stem.Substring(0, Stem.Length - 3);
				return;
			}

			if (Stem.EndsWith("'s"))
			{
				Stem = Stem.Substring(0, Stem.Length - 2);
				return;
			}

			if (Stem.EndsWith("'"))
			{
				Stem = Stem.Substring(0, Stem.Length - 1);
				return;
			}
		}

		internal void MarkYs()
		{
			List<char> vowelsSearch = new List<char>(Vowels);
			bool previousWasVowel = true;
			StringBuilder result = new StringBuilder();
			foreach (char c in Stem)
			{
				if (c == 'y'
					&& previousWasVowel)
					result.Append('Y');
				else
					result.Append(c);

				previousWasVowel = vowelsSearch.Contains(c);
			}
			Stem = result.ToString();
		}

		public int Length
		{
			get { return Stem.Length; }
		}

	}
}
