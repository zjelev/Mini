# BigBlueButton
Automating rake commands for creating users in BigBlueButton using Greenlight

If you have txt files with user names and their emails you can easily convert them  to commands for creating these users in BigBluButton server and vice versa.

If found file ..\Input.txt the program takes the names and emails from it and creates 2 files in the parent directory: 
..\SendEmails.txt : for coping in To: field in your mail client.
..\CreateUsers.txt : for coping in the console of BBB server 
If started with arguments - the first arg is the user password, the second - filter for the e-mail domain. Default "123456" and "".

If there is a file ..\CreatedUsers.txt, it takes the names and emails and sorts them in one line in the file ..\UsersSortedByFirstName.txt, again ready for emailing.
