using System;
using System.Collections.Generic;

namespace Fanex.Bettype3rdProduct.Business.Models;

public class ConfigSettingModel
{
    public bool IsManual { get; set; }

    public bool IsCheckEnoughData { get; set; }

    public string SentryClientKey { get; set; }

    public string CategorySentryLog { get; set; }

    public string Path { get; set; }

    public string Password { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime DateEnd { get; set; }

    public string NapReport { get; set; }

    public List<SourceMapping> SourceMappings { get; set; }

    public int MailId { get; set; }
}