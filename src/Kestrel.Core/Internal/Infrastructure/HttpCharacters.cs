// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure
{
    internal static class HttpCharacters
    {
        public static readonly int TableSize = 128;
        public static readonly ReadOnlyMemory<bool> Authority;
        public static readonly ReadOnlyMemory<bool> Token;
        public static readonly ReadOnlyMemory<bool> Host;
        public static readonly ReadOnlyMemory<bool> FieldValue;

        static HttpCharacters()
        {
            // ALPHA and DIGIT https://tools.ietf.org/html/rfc5234#appendix-B.1
            var alphaNumeric = new bool[TableSize];
            for (var c = '0'; c <= '9'; c++)
            {
                alphaNumeric[c] = true;
            }
            for (var c = 'A'; c <= 'Z'; c++)
            {
                alphaNumeric[c] = true;
            }
            for (var c = 'a'; c <= 'z'; c++)
            {
                alphaNumeric[c] = true;
            }

            // Authority https://tools.ietf.org/html/rfc3986#section-3.2
            // Examples:
            // microsoft.com
            // hostname:8080
            // [::]:8080
            // [fe80::]
            // 127.0.0.1
            // user@host.com
            // user:password@host.com
            var authority = new bool[TableSize];
            Array.Copy(alphaNumeric, authority, TableSize);
            authority[':'] = true;
            authority['.'] = true;
            authority['['] = true;
            authority[']'] = true;
            authority['@'] = true;
            Authority = authority;

            // Matches Http.Sys
            // Matches RFC 3986 except "*" / "+" / "," / ";" / "=" and "%" HEXDIG HEXDIG which are not allowed by Http.Sys
            var host = new bool[TableSize];
            Array.Copy(alphaNumeric, host, TableSize);
            host['!'] = true;
            host['$'] = true;
            host['&'] = true;
            host['\''] = true;
            host['('] = true;
            host[')'] = true;
            host['-'] = true;
            host['.'] = true;
            host['_'] = true;
            host['~'] = true;
            Host = host;

            // tchar https://tools.ietf.org/html/rfc7230#appendix-B
            var token = new bool[TableSize];
            Array.Copy(alphaNumeric, token, TableSize);
            token['!'] = true;
            token['#'] = true;
            token['$'] = true;
            token['%'] = true;
            token['&'] = true;
            token['\''] = true;
            token['*'] = true;
            token['+'] = true;
            token['-'] = true;
            token['.'] = true;
            token['^'] = true;
            token['_'] = true;
            token['`'] = true;
            token['|'] = true;
            token['~'] = true;
            Token = token;

            // field-value https://tools.ietf.org/html/rfc7230#section-3.2
            var fieldValue = new bool[TableSize];
            for (var c = 0x20; c <= 0x7e; c++) // VCHAR and SP
            {
                fieldValue[c] = true;
            }
            FieldValue = fieldValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsInvalidAuthorityChar(Span<byte> s)
        {
            var authority = Authority.Span;

            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c >= (uint)authority.Length || !authority[c])
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfInvalidHostChar(string s)
        {
            var host = Host.Span;

            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c >= (uint)host.Length || !host[c])
                {
                    return i;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfInvalidTokenChar(string s)
        {
            var token = Token.Span;

            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c >= (uint)token.Length || !token[c])
                {
                    return i;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfInvalidFieldValueChar(string s)
        {
            var fieldValue = FieldValue.Span;

            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c >= (uint)fieldValue.Length || !fieldValue[c])
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
