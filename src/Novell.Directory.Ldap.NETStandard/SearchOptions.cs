using JetBrains.Annotations;
using System;

namespace Novell.Directory.Ldap
{
    public class SearchOptions
    {
        public string SearchBase { get; }
        public int Scope { get; }
        public string Filter { get; }
        public string[] TargetAttributes { get; }
        public bool TypesOnly { get; }
        public LdapSearchConstraints SearchConstraints { get; }

        public SearchOptions(
            [NotNull] string searchBase,
            int scope,
            [NotNull] string filter,
            [NotNull] string[] targetAttributes)
            : this(searchBase, scope, filter, targetAttributes, false, null)
        {
        }

        public SearchOptions(
            [NotNull] string searchBase,
            int scope,
            [NotNull] string filter,
            [NotNull] string[] targetAttributes,
            bool typesOnly,
            LdapSearchConstraints searchConstraints)
        {
            SearchBase = searchBase ?? throw new ArgumentNullException(nameof(searchBase));
            Scope = scope;
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
            TargetAttributes = targetAttributes;
            TypesOnly = typesOnly;
            SearchConstraints = searchConstraints;
        }
    }
}
