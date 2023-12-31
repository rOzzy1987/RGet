﻿namespace RSoft.RGet;

public class RGetContext
{
    public bool Quiet { get; set; }
    public bool SuccessOnly { get; set; }
    public int? Timeout { get; internal set; }
    public FileInfo? LogFile { get; set; }
    public string UserAgent { get; set; } = "";
    public string? BaseUri { get; set; }
    public int? Wait { get; set; }

    public HttpMethod? Method { get; set; }
    public byte[]? Body { get; set; }
    public string? BodyStr { get; set; }
    public string? MediaType { get; set; }

}
