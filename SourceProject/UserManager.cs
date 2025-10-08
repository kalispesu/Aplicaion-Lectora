using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LectorPdf
{
    public class UserRecord
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public int? Age { get; set; }
    }

    public class UserManager
    {
        private string dataFolder;
        private string usersFile;
        private List<UserRecord> users;

        public UserManager()
        {
            dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LectorPDF");
            if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);
            usersFile = Path.Combine(dataFolder, "users.json");
            Load();
        }

        private void Load()
        {
            if (!File.Exists(usersFile)) { users = new List<UserRecord>(); return; }
            users = JsonConvert.DeserializeObject<List<UserRecord>>(File.ReadAllText(usersFile)) ?? new List<UserRecord>();
        }

        private void Save() => File.WriteAllText(usersFile, JsonConvert.SerializeObject(users, Formatting.Indented));

        public bool UserExists(string email) => users.Exists(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

        public void CreateUser(string email, string password, int? age=null)
        {
            var salt = GenerateSalt();
            var hash = HashPassword(password, salt);
            users.Add(new UserRecord { Email = email, Salt = Convert.ToBase64String(salt), PasswordHash = Convert.ToBase64String(hash), Age = age });
            Save();
        }

        public bool VerifyCredentials(string email, string password)
        {
            var user = users.Find(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
            if (user == null) return false;
            var salt = Convert.FromBase64String(user.Salt);
            var hash = HashPassword(password, salt);
            return Convert.ToBase64String(hash) == user.PasswordHash;
        }

        private static byte[] GenerateSalt()
        {
            var salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(salt);
            return salt;
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            // PBKDF2 with SHA256, 10000 iterations
            using (var derive = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                return derive.GetBytes(32);
            }
        }
    }
}
