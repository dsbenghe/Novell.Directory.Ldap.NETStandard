namespace Novell.Directory.Ldap.NETStandard.Asn1
{
    public enum TagClass : int
    {
        /// <summary>
        ///     Universal tag class.
        ///     UNIVERSAL = 0
        /// </summary>
        UNIVERSAL = 0,

        /// <summary>
        ///     Application-wide tag class.
        ///     APPLICATION = 1
        /// </summary>
        APPLICATION = 1,

        /// <summary>
        ///     Context-specific tag class.
        ///     CONTEXT = 2
        /// </summary>
        CONTEXT = 2,

        /// <summary>
        ///     Private-use tag class.
        ///     PRIVATE = 3
        /// </summary>
        PRIVATE = 3
    }
}
