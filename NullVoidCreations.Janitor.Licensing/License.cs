﻿using System;
using System.IO;
using System.Text;
using System.Xml;

namespace NullVoidCreations.Janitor.Licensing
{
    public class License: IDisposable
    {
        const string ValidCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
        const string EncryptionKey = "QFByb3Blcl9QYXRvbGEhMjAxNQ==";
        const int KeySize = 20;

        string _serialKey, _activationKey, _fileName;

        #region constructor / destructor

        public License(DateTime issueDate, DateTime expirationDate, string email, string whoIsTheBoss)
        {
            if (string.IsNullOrEmpty(whoIsTheBoss))
                throw new ArgumentNullException("whoIsTheBoss", "Who is the boss not specified.");
            if (whoIsTheBoss.Equals("Rubal Walia", StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("For generating key, go and find out who is the boss first.");

            Generate();
            GenerateActivationKey(issueDate, expirationDate, email);
        }

        public License(string fileName)
        {
            Load(fileName);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~License()
        {
            Dispose(false);
        }

        #endregion

        #region properties

        public string SerialKey
        {
            get { return _serialKey; }
            private set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("Serial key not specified.");
                if (value.Length != KeySize)
                    throw new ArgumentException("Invalid serial key size.");

                Segment1 = value.Substring(0, 5);
                Segment2 = value.Substring(6, 5);
                Segment3 = value.Substring(12, 5);
                Segment4 = value.Substring(18, 5);
                _serialKey = value;
            }
        }

        public string Segment1
        {
            get;
            private set;
        }

        public string Segment2
        {
            get;
            private set;
        }

        public string Segment3
        {
            get;
            private set;
        }

        public string Segment4
        {
            get;
            private set;
        }

        public string ActivationKey
        {
            get { return _activationKey; }
            private set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("Activation key not specified.");

                var decryptedKey = StringCipher.Instance.Decrypt(value, EncryptionKey);
                try
                {
                    IssueDate = ExtractDate(decryptedKey, 0);
                    ExpirationDate = ExtractDate(decryptedKey, 8);
                    Email = decryptedKey.Substring(16, decryptedKey.Length - 16);
                    IsActivated = IsValid(this);

                    _activationKey = value;
                    IsActivated = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Invalid activation key.");
                }
            }
        }

        public bool IsActivated
        {
            get;
            private set;
        }

        public string Email
        {
            get;
            private set;
        }

        public DateTime IssueDate
        {
            get;
            private set;
        }

        public DateTime ExpirationDate
        {
            get;
            private set;
        }

        #endregion

        public void Activate(string activationKey)
        {
            ActivationKey = activationKey;
        }

        #region private methods

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!string.IsNullOrEmpty(_fileName))
                    Save(_fileName);
            }

            IsActivated = false;
            _serialKey = _activationKey = _fileName = null;
            Segment1 = Segment2 = Segment3 = Segment4 = Email = null;
            ExpirationDate = IssueDate = DateTime.MinValue;
        }

        void Load(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("License file not found.", fileName);

            var document = new XmlDocument();
            document.Load(fileName);

            SerialKey = document.SelectSingleNode("/License/SerialKey").InnerText;
            ActivationKey = document.SelectSingleNode("/License/ActivationKey").InnerText;

            _fileName = fileName;
        }

        void Save(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            var document = new XmlDocument();
            var root = document.CreateElement("License");

            var serialKeyNode = document.CreateElement("SerialKey");
            serialKeyNode.InnerText = SerialKey;
            root.AppendChild(serialKeyNode);

            var activationKeyNode = document.CreateElement("ActivationKey");
            activationKeyNode.InnerText = ActivationKey;
            root.AppendChild(activationKeyNode);

            document.AppendChild(root);

            document.Save(fileName);
            _fileName = fileName;
        }

        void Generate()
        {
            var randomGenerator = new Random();
            var licenseBuilder = new StringBuilder(24);

            for (int index = 1; index <= KeySize; index++)
            {
                var serialChar = randomGenerator.Next(ValidCharacters.Length);
                if (index % 5 == 0 && index < KeySize)
                    licenseBuilder.AppendFormat("{0}-", serialChar);
                else
                    licenseBuilder.Append(serialChar);
            }

            SerialKey = licenseBuilder.ToString();
        }

        void GenerateActivationKey(DateTime isssueDate, DateTime expirationDate, string email)
        {
            IssueDate = isssueDate;
            ExpirationDate = expirationDate;
            Email = email;

            var key = string.Format("{0:0000}{1:00}{2:00}{3:0000}{4:00}{5:00}{6}",
                IssueDate.Year,
                IssueDate.Month,
                IssueDate.Day,
                ExpirationDate.Year,
                ExpirationDate.Month,
                ExpirationDate.Day,
                Email);
            _activationKey = StringCipher.Instance.Encrypt(key, EncryptionKey);
        }

        DateTime ExtractDate(string text, int startIndex)
        {
            var date = new DateTime(
                int.Parse(text.Substring(startIndex, 4)),
                int.Parse(text.Substring(startIndex + 4, 2)),
                int.Parse(text.Substring(startIndex + 6, 2)));

            return date;
        }

        bool IsValid(License license)
        {
            var currentDate = DateTime.Now;
            if (currentDate.Date < license.IssueDate.Date)
                return false;
            if (currentDate.Date > license.ExpirationDate.Date)
                return false;

            return true;
        }

        #endregion
    }
}
