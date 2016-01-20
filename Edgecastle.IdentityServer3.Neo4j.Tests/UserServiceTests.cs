using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Edgecastle.IdentityServer3.Neo4j.Interfaces;
using System.Threading.Tasks;

namespace Edgecastle.IdentityServer3.Neo4j.Tests
{
    /// <summary>
    /// Summary description for UserServiceTests
    /// </summary>
    [TestClass]
    public class UserServiceTests
    {
        [TestMethod]
        public async Task Test_ValidNewUser_CreatesSuccessfully()
        {
            // Arrange
            IUserAdminService userService = new Neo4jUsersService();

            // Act
            var result = await CreateUser(userService, String.Format("TestUser_{0}", Guid.NewGuid()));

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.User);
            Assert.AreNotEqual(Guid.Empty, result.User.Id);
        }

        [TestMethod]
        public async Task Test_NewUserWithExistingUsername_FailsToCreate()
        {
            // Arrange
            IUserAdminService service = new Neo4jUsersService();
            string usernameToDuplicate = String.Format("TestUser_{0}", Guid.NewGuid());

            // Act
            // This one should work.
            Models.UserAdminResult shouldBeSuccessful = await CreateUser(service, usernameToDuplicate);

            // This one should fail due to username duplication.
            Models.UserAdminResult shouldFail = await CreateUser(service, usernameToDuplicate);

            // Assert
            Assert.IsTrue(shouldBeSuccessful.Success);
            Assert.IsFalse(shouldFail.Success);
            Assert.AreEqual("Username already exists", shouldFail.ErrorMessage);

        }

        private static async Task<Models.UserAdminResult> CreateUser(IUserAdminService service, string username)
        {
            return await service.CreateUser(
                            username: username,
                            password: "secret",
                            email: "blahblah@blahhhhhh.com"
                        );
        }
    }
}
