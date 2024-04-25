// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests
{
    public class TokenizerTests
    {
        [Theory]
        [InlineData("", ".", false, new string[0])]
        [InlineData("", ".", true, new string[0])]
        [InlineData(".", ".", false, new string[0])]
        [InlineData(".", ".", true, new string[0])]
        [InlineData("..", ".", false, new string[0])]
        [InlineData("..", ".", true, new[] { "." })]
        [InlineData(".,", ".,", false, new string[0])]
        [InlineData(".,", ".,", true, new[] { "." })]
        [InlineData("a", ".", false, new[] { "a" })]
        [InlineData("a", ".", true, new[] { "a" })]
        [InlineData("a.", ".", false, new[] { "a" })]
        [InlineData("a.", ".", true, new[] { "a", "." })]
        [InlineData("a..", ".", false, new[] { "a" })]
        [InlineData("a..", ".", true, new[] { "a", "." })]
        [InlineData("a..b", ".", false, new[] { "a", "b" })]
        [InlineData("a..b", ".", true, new[] { "a", ".", ".", "b" })]
        [InlineData("a.,b", ".,", false, new[] { "a", "b" })]
        [InlineData("a.,b", ".,", true, new[] { "a", ".", ",", "b" })]
        [InlineData("a.b.", ".", false, new[] { "a", "b" })]
        [InlineData("a.b.", ".", true, new[] { "a", ".", "b", "." })]
        [InlineData("a.b..", ".", false, new[] { "a", "b" })]
        [InlineData("a.b..", ".", true, new[] { "a", ".", "b", "." })]
        [InlineData("a.b.,", ".,", false, new[] { "a", "b" })]
        [InlineData("a.b.,", ".,", true, new[] { "a", ".", "b", "." })]
        [InlineData("a.b.c", ".", false, new[] { "a", "b", "c" })]
        [InlineData("a.b.c", ".", true, new[] { "a", ".", "b", ".", "c" })]
        [InlineData(".b.c", ".", false, new[] { "b", "c" })]
        [InlineData(".b.c", ".", true, new[] { ".", "b", ".", "c" })]
        [InlineData("..c", ".", false, new[] { "c" })]
        [InlineData("..c", ".", true, new[] { ".", ".", "c" })]
        [InlineData(".,c", ".,", false, new[] { "c" })]
        [InlineData(".,c", ".,", true, new[] { ".", ",", "c" })]
        public void StringGetsTokenized(string source, string delimiters, bool returnDelimiters, string[] tokens)
        {
            var tokenizer = new Tokenizer(source, delimiters, returnDelimiters);

            Assert.Equal(tokens.Length, tokenizer.Count);

            for (var i = 0; i < tokens.Length; i++)
            {
                Assert.True(tokenizer.HasMoreTokens(), $"Next should be tokens[{i}]: {tokens[i]}");
                Assert.Equal(tokens[i], tokenizer.NextToken());
            }

            Assert.False(tokenizer.HasMoreTokens());
        }
    }
}
