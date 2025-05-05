

using FrontEnd.Commons.Tokens;

namespace KukuLang.Parser.Models.Exceptions;

public class UnknownTokenException(Token token) : Exception($"Unknown token {token}");