# CrisisTracker

During large-scale complex crises such as the Haiti earthquake, the Indian Ocean tsunami and the Arab Spring, social media has emerged as a source of timely and detailed reports regarding important events. However, individual disaster responders, government officials or citizens who wish to access this vast knowledge base are met with a torrent of information that quickly results in information overload. Without a way to organize and navigate the reports, important details are easily overlooked and it is challenging to use the data to get an overview of the situation as a whole.

CrisisTracker has been developed based on the idea that volunteers around the world would be willing to assist hard-pressed decision makers with information management, if the tools were available.

## Automated Story Detection

CrisisTracker offers an alternative way to browse Twitter activity around large-scale events, in particular disasters. The system tracks an admin-defined set of keywords on Twitter and clusters tweets based on their word similarity. Information propagates on Twitter mostly through duplication, either directly through retweets or by multiple people independently talking about the same event, and information about a single event is often split on thousands of messages. By clustering messages, each piece of information (a "story") is decoupled from its sharing pattern.

## Crowdsourced Information Management

The web interface supports exploration of stories as they unfold in real-time; by time, location, topic and named entities. As current state-of-the-art text processing algorithms struggle with reliably extracting such meta-data from the text in tweets, CrisisTracker instead explores the use of crowdsourcing techniques. Any user of the system can directly contribute tags that make it easier for other users to retrieve information and explore stories by similarity. In addition, users of the system can influence how tweets are grouped into stories.

## Upcoming features

During the summer of 2013, a new data exploration interface will be rolled out. This new interface offers three major changes:
- Improved support for text- and time-based search and filtering.
- Integration of automated topic-classification based on supervised learning, using the <a href="http://irevolution.net/2013/02/11/update-twitter-dashboard/">AIDR</a> platform
- Clean and easy-to-extend source code, so that new visualizations can be added with minimal effort.

## Resources

[Introduction video](https://vimeo.com/45366518)

[Example deployment](http://ufn.virtues.fi/crisistracker)

[Installation instructions](https://github.com/JakobRogstadius/CrisisTracker/Documentation/installation_instructions.txt)

[Evaluation report](http://hci.uma.pt/~jakob/files/Rogstadius_2013_CrisisTracker_Crowdsourced_Social_Media_Curation_for_Disaster_Awareness.pdf)

## Contact
[Jakob Rogstadius](http://hci.uma.pt/~jakob/)