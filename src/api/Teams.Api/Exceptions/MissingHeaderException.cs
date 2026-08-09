namespace Teams.Api.Exceptions;

public class MissingHeaderException(string headerName) : Exception($"'{headerName}' header value is required.");