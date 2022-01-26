namespace BigBlueButtonUsers
{
    class User
    {
        private string firstName;
        private string lastName;
        private string email;

        public User(string firstName, string lastName, string email)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Email = email;
        }
        public string FirstName { get => firstName; private set => firstName = value; }

        public string LastName { get => lastName; private set => lastName = value; }

        public string Email { get => email; private set => email = value; }

        public override string ToString() 
        {
            return FirstName + ' ' + LastName + ' ' + Email;
        }
    }
}