using System;
using System.Collections.Generic;
using System.Text;

namespace EnglishStemmer
{
	internal class WordRegion
	{
		public WordRegion()
		{ }

		public WordRegion(int start, int end)
		{
			Start = start;
			End = end;
		}

		public int Start;
		public int End;
		private string _text;

		public string Text
		{
			get { return _text; }
		}


		internal bool Contains(int index)
		{
			return (index >= Start && index <= End);
		}

		internal void GenerateRegion(string text)
		{
			if (text.Length > Start)
				_text = text.Substring(Start, Math.Min(End, text.Length) - Start);
			else
				_text = String.Empty;
		}

	}
}
