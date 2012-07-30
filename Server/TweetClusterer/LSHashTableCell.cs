using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrisisTracker.TweetClusterer
{
    class LSHashTableCell
    {
        Queue<LSHashTweet> _content = new Queue<LSHashTweet>();

        public int Capacity { get; private set; }

        public LSHashTableCell(int capacity)
        {
            Capacity = capacity;
        }

        public void Add(LSHashTweet item)
        {
            _content.Enqueue(item);
            if (_content.Count > Capacity)
                _content.Dequeue();
        }

        public void UndoLastAdd()
        {
            if (_content.Count > 0)
            {
                Queue<LSHashTweet> newContent = new Queue<LSHashTweet>();
                for (int i = 0; i < _content.Count - 1; i++)
                    newContent.Enqueue(_content.Dequeue());
                _content = newContent;
            }
        }

        public List<LSHashTweet> GetTweets()
        {
            return new List<LSHashTweet>(_content);
        }

        public void RemoveTweetsByID(IEnumerable<long> tweetIDs)
        {
            List<LSHashTweet> tmp = new List<LSHashTweet>(_content);
            tmp.RemoveAll(n => tweetIDs.Contains(n.ID));
            _content = new Queue<LSHashTweet>(tmp);
        }

        public int Count { get { return _content.Count; } }
    }
}
