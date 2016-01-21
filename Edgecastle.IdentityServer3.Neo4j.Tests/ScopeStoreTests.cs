using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Edgecastle.IdentityServer3.Neo4j.Interfaces;
using Edgecastle.IdentityServer3.Neo4j.Models;
using System.Threading.Tasks;

namespace Edgecastle.IdentityServer3.Neo4j.Tests
{
    [TestClass]
    public class ScopeStoreTests
    {
        [TestMethod]
        public async Task Test_ValidScope_GetsCreatedSuccessfully()
        {
            // Arrange            
            Scope newScope = new Scope
            {
                Name = String.Format("TestScope_{0}", Guid.NewGuid())
            };

            // Act
            ScopeAdminResult result = await CreateScope(newScope);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Scope);
            Assert.AreEqual(newScope.Name, result.Scope.Name);
        }

        private static async Task<ScopeAdminResult> CreateScope(Scope newScope)
        {
            IScopeAdminService service = new Neo4jScopeStore();

            // Act
            ScopeAdminResult result = await service.CreateScope(newScope);
            return result;
        }
    }
}
