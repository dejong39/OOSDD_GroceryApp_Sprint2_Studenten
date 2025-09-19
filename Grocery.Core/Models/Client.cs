
using Grocery.Core.Helpers;

namespace Grocery.Core.Models
{
    public partial class Client : Model
    {
        public string _emailAddress { get; }
        private string _password { get; set; }
        public Client(int id, string name, string emailAddress, string password) : base(id, name)
        {
            _emailAddress=emailAddress;
            _password=password;
        }

        internal bool verifyPassword(string inputPassword)
        {
            return PasswordHelper.VerifyPassword(inputPassword, _password);
        }
    }
}
