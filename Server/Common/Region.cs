/*******************************************************************************
 * Copyright (c) 2013 Jakob Rogstadius.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

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
