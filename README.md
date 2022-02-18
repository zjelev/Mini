# BigBlueButton

From txt files with user names and emails generate commands for creating these users in BigBluButton server and informing them about that.

If these is a file ..\Input.txt the program takes the names and emails from it and creates 2 files in the parent directory:

..\CreateUsers.txt : for coping in the console of BBB server.

..\SendEmails.html : for sending emails in chunks of 6 (due to spam restictions).

If started with arguments - the first is the user password, the second - filter for the e-mail domain. Default "123456" and "".

If there is a file ..\CreatedUsers.txt, it takes the names and emails and sorts them in the file ..\UsersSortedByFirstName.txt.
