using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Entities
{
    public class AppUser
    {

        #region Backing Fields
        private int _id;
        private string _username;
        private byte[] _passwordHash;
        private byte[] _passwordSalt;
        #endregion

        #region Properties
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string UserName
        {
            get { return _username; }
            set { _username = value; }
        }

        public byte[] PasswordHash
        {
            get { return _passwordHash; }
            set { _passwordHash = value; }
        }

         public byte[] PasswordSalt
        {
            get { return _passwordSalt; }
            set { _passwordSalt = value; }
        }
        #endregion

    }
}