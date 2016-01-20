using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Edgecastle.IdentityServer3.Neo4j.Tests
{
    [TestClass]
    public class PasswordSecurityTests
    {
        [TestMethod]
        public void Test_ValidPassword_IsCorrectlyHashedAndVerified()
        {
            //  Arrange
            string password = "secret";

            // Act
            string hash = PasswordSecurity.Hash(password);
            bool isVerified = PasswordSecurity.Verify(password, hash);

            // Assert
            Assert.IsNotNull(hash);
            Assert.IsTrue(isVerified);
        }

        [TestMethod]
        public void Test_InvalidPassword_DoesNotVerify()
        {
            //  Arrange
            string password = "secret";

            // Act
            string hash = PasswordSecurity.Hash(password);
            bool isVerified = PasswordSecurity.Verify("Not the password!!!!", hash);

            // Assert
            Assert.IsNotNull(hash);
            Assert.IsFalse(isVerified);
        }
    }
}
