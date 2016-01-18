using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Edgecastle.IdentityServer3.Neo4j.Tests
{
    [TestClass]
    public class PasswordSecurityTests
    {
        [TestMethod]
        public void CanCreateStableSaltedHash()
        {
            //  Arrange
            string password = "secret";

            // Act
            string hash = PasswordSecurity.Hash(password);

            // Assert
            Assert.IsNotNull(hash);
            Assert.AreEqual("$2a$10$L5t4hlYSpmDtUs36Of4Lc.LLiH67lk.qv1.GTrihYu3u7QnP4SJTS", hash);
        }
    }
}
