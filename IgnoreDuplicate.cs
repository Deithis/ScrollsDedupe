using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dedupe
{
    public class IgnoreDuplicate
    {
        //Initialize dictionary for 1000 lines to begin with
        private Dictionary<ChatLine, ChatLine> _previousLinesMap = new Dictionary<ChatLine, ChatLine>(1000);
        //The oldest will be always be first in the list so we can remove old chat lines.
        private LinkedList<ChatLine> _previousLinesList = new LinkedList<ChatLine>();

        public static double TIMEOUT_MINUTES = 10.0;

        class ChatLine
        {
            public ChatLine(string from, string text)
            {
                From = from;
                Text = text;
                Time = DateTime.Now;
            }

            public string From { get; set; }
            public string Text { get; set; }
            public DateTime Time { get; set; }

            public bool hasExpired(DateTime now)
            {
                return now > Time.AddMinutes(TIMEOUT_MINUTES);
            }

            public override int GetHashCode()
            {
                return From.GetHashCode() ^ Text.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                ChatLine cl = obj as ChatLine;

                if (cl == null)
                {
                    return false;
                }

                return (cl.From == From && cl.Text == Text);
            }
        }

        //Cleans up any messages that have passed TIMEOUT_MINUTES
        public void cleanupOldMessages()
        {
            DateTime now = DateTime.Now;

            ChatLine first = _previousLinesList.ElementAtOrDefault<ChatLine>(0);
            while (first != null && first.hasExpired(now))
            {
                _previousLinesList.RemoveFirst();
                _previousLinesMap.Remove(first);
                first = _previousLinesList.ElementAtOrDefault<ChatLine>(0);
            }
        }

        public bool hooksReceive(RoomChatMessageMessage rcmm)
        {
            cleanupOldMessages();

            //Don't dedupe "Scrolls"
            //Should we also not dedupe ourself?
            if (rcmm.from == "Scrolls")
            {
                return false;
            }

            ChatLine newChatLine = new ChatLine(rcmm.from, rcmm.text);
            bool previouslyExisted = _previousLinesMap.ContainsKey(newChatLine);

            if (!previouslyExisted)
            {
                _previousLinesMap.Add(newChatLine, newChatLine);
                _previousLinesList.AddLast(newChatLine);
            }

            //Returns TRUE if the line was a duplicate. This will ignore the line in the chat.
            return previouslyExisted;
        }
    }
}
