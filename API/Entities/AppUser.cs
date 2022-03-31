using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Extensions;

namespace API.Entities
{
    public class AppUser
    {

        #region Backing Fields
        private int _id;
        private string _username;
        private byte[] _passwordHash;
        private byte[] _passwordSalt;
        private DateTime _dateOfBirth;
        private DateTime _created = DateTime.Now;
        private DateTime _lastActive;
        private string _knownAs;
        private string _gender;
        private string _introduction;
        private string _lookingFor;
        private string _interests;
        private string _city;
        private string _country;
        private ICollection<Photo> _photos;
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
        public DateTime DateOfBirth
        {
            get { return _dateOfBirth; }
            set { _dateOfBirth = value; }
        }
        public int Age { get { return _dateOfBirth.CalculateAge(); } }
        
        public DateTime Created
        {
            get { return _created; }
            set { _created = value; }
        }
        public DateTime LastActive
        {
            get { return _lastActive; }
            set { _lastActive = value; }
        }
        public string KnownAs
        {
            get { return _knownAs; }
            set { _knownAs = value; }
        }
        public string Gender
        {
            get { return _gender; }
            set { _gender = value; }
        }
        public string Introduction
        {
            get { return _introduction; }
            set { _introduction = value; }
        }
        public string LookingFor
        {
            get { return _lookingFor; }
            set { _lookingFor = value; }
        }
        public string Interests
        {
            get { return _interests; }
            set { _interests = value; }
        }
        public string City
        {
            get { return _city; }
            set { _city = value; }
        }
        public string Country
        {
            get { return _country; }
            set { _country = value; }
        }
        public ICollection<Photo> Photos
        {
            get { return _photos; }
            set { _photos = value; }
        }
        #endregion

        public int GetAge()
        {
            return _dateOfBirth.CalculateAge();
        }

    }
}