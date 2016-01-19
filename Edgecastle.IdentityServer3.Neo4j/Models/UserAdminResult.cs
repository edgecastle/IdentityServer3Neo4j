using Edgecastle.IdentityServer3.Neo4j.Interfaces;

namespace Edgecastle.IdentityServer3.Neo4j.Models
{
    /// <summary>
    /// Result of an <see cref="IUserAdminService"/> operation
    /// </summary>
    public class UserAdminResult
    {
        /// <summary>
        /// Initializes a new instance of <see cref="UserAdminResult"/> in an error state
        /// </summary>
        /// <param name="error">The error</param>
        public UserAdminResult(string error)
        {
            this.Success = false;
            this.ErrorMessage = error;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UserAdminResult"/> in a success state.
        /// </summary>
        /// <param name="user"></param>
        public UserAdminResult(User user)
        {
            this.Success = true;
            this.User = user;
        }

        /// <summary>
        /// The error message
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Whether the operation was successful or not.
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// The user (if successful)
        /// </summary>
        public User User { get; private set; }
    }
}