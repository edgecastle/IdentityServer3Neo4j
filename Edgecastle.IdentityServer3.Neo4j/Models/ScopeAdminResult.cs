using Edgecastle.IdentityServer3.Neo4j.Interfaces;

namespace Edgecastle.IdentityServer3.Neo4j.Models
{
    /// <summary>
    /// Result of an <see cref="IScopeAdminService"/> operation.
    /// </summary>
    public class ScopeAdminResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeAdminResult"/> class in an error state.
        /// </summary>
        /// <param name="error"></param>
        public ScopeAdminResult(string error)
        {
            this.Success = false;
            this.ErrorMessage = error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeAdminResult"/> class in a success state.
        /// </summary>
        /// <param name="scope"></param>
        public ScopeAdminResult(Scope scope)
        {
            this.Success = true;
            this.Scope = scope;
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
        /// The scope (if successful)
        /// </summary>
        public Scope Scope { get; private set; }
    }
}