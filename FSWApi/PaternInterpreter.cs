using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSWApi
{
    internal class PaternInterpreter
    {
        internal enum TokenKind
        {
            OpenCurlyBrace,
            CloseCurlyBrace,

            Dot,
            Slash,
            Backslash,

            StringLiteral,
        }

        internal List<KeyValuePair<TokenKind, object>> PaternLexer(string paternString)
        {
            List<KeyValuePair<TokenKind, object>> _PaternTokens = new List<KeyValuePair<TokenKind, object>>();

            for (int i = 0; i < paternString.Length; i++)
            {
                char c = paternString[i];
                switch (c)
                {
                    case '{':
                        _PaternTokens.Add(new KeyValuePair<TokenKind, object>(TokenKind.OpenCurlyBrace, c));
                        break;
                    case '}':
                        _PaternTokens.Add(new KeyValuePair<TokenKind, object>(TokenKind.CloseCurlyBrace, c));
                        break;
                    case '.':
                        _PaternTokens.Add(new KeyValuePair<TokenKind, object>(TokenKind.Dot, c));
                        break;
                    case '/':
                        _PaternTokens.Add(new KeyValuePair<TokenKind, object>(TokenKind.Slash, c));
                        break;
                    case '\\':
                        _PaternTokens.Add(new KeyValuePair<TokenKind, object>(TokenKind.Backslash, c));
                        break;
                    default:
                        StringBuilder sb = new StringBuilder();
                        while (i < paternString.Length && c != '{' && c != '}' && c != '.')
                        {
                            sb.Append(c);
                            i++;
                            if (i < paternString.Length)
                                c = paternString[i];
                        }
                        i--; // Adjust for the extra increment in the while loop
                        _PaternTokens.Add(new KeyValuePair<TokenKind, object>(TokenKind.StringLiteral, sb.ToString()));
                        break;
                }
            }

            return _PaternTokens;
        }

        internal string PaternParser(List<KeyValuePair<TokenKind, object>> paternTokens, FileMetaInfo fileMetaInfo)
        {
            bool PaternVariable = false;
            bool PaternFunction = false;
            object PaternValue = "";
            StringBuilder result = new StringBuilder();

            foreach (var token in paternTokens)
            {
                switch (token.Key)
                {
                    case TokenKind.OpenCurlyBrace:
                        PaternVariable = true;
                        PaternFunction = false;
                        break;
                    case TokenKind.CloseCurlyBrace:
                        if (PaternVariable && !PaternFunction)
                        {
                            result.Append(PaternValue);
                        }
                        PaternVariable = false;
                        PaternFunction = false;
                        break;
                    case TokenKind.Dot:
                        if (!PaternVariable)
                            result.Append(".");
                        else
                            PaternFunction = true;
                        break;
                    case TokenKind.Slash:
                        if (!PaternVariable)
                            result.Append(@"\");
                        break;
                    case TokenKind.Backslash:
                        if (!PaternVariable)
                            result.Append(@"\");
                        break;
                    case TokenKind.StringLiteral:
                        if (PaternVariable)
                        {
                            if (!PaternFunction)
                            {
                                var propertyInfo = fileMetaInfo.GetType().GetProperty(token.Value.ToString());
                                if (propertyInfo != null)
                                {
                                    PaternValue = propertyInfo?.GetValue(fileMetaInfo);
                                }
                            }
                            else
                            {
                                if (PaternValue is DateTime)
                                {
                                    switch (token.Value.ToString().ToUpper())
                                    {
                                        case "YEAR":
                                            result.Append(((DateTime)PaternValue).Year.ToString("D4"));
                                            break;
                                        case "MONTH":
                                            result.Append(((DateTime)PaternValue).Month.ToString("D2"));
                                            break;
                                        case "DAY":
                                            result.Append(((DateTime)PaternValue).Day.ToString("D2"));
                                            break;
                                        case "HOUR":
                                            result.Append(((DateTime)PaternValue).Hour.ToString("D2"));
                                            break;
                                        case "MINUTE":
                                            result.Append(((DateTime)PaternValue).Minute.ToString("D2"));
                                            break;
                                        case "SECOND":
                                            result.Append(((DateTime)PaternValue).Second.ToString("D2"));
                                            break;
                                        default:
                                            result.Append(PaternValue.ToString());
                                            break;
                                    }
                                }
                                else if (PaternValue is string)
                                {
                                    if (token.Value.ToString().ToUpper() == "UPPER")
                                    {
                                        result.Append(PaternValue.ToString().ToUpper());
                                    }
                                    else if (token.Value.ToString().ToUpper() == "LOWER")
                                    {
                                        result.Append(PaternValue.ToString().ToLower());
                                    }
                                    else if (token.Value.ToString().ToUpper() == "NOEXT")
                                    {
                                        string str = PaternValue.ToString();
                                        int index = str.LastIndexOf('.');
                                        if (index > 0)
                                            result.Append(str.Substring(0, index));
                                        else
                                            result.Append(str);
                                    }
                                    else
                                    {
                                        result.Append(PaternValue.ToString());
                                    }
                                }
                                else
                                {
                                    result.Append(PaternValue.ToString());
                                }
                            }
                        }
                        else
                            result.Append(token.Value.ToString());
                        break;
                }
            }
            return result.ToString();
        }
    }
}
