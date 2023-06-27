using Fanex.MailDelivery.Client;
using Fanex.MailDelivery.Core.Models;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;

namespace Fanex.Bettype3rdProduct.Business.Services;

public interface MailService
{
    Mail CreateMail(string fileName);

    void SendMailByFunction(string fileName);
}

public class MailServiceImp : MailService
{
    public Mail CreateMail(string fileName)
    {
        var attachments = new List<MailAttachment>
        {
            new(fileName, FileExtension.MS_EXCEL)
        };

        return new Mail
        {
            Title = "Auto Generated Report Administrator",
            Subject = "Automail Report - Bettype With 3rd Products",
            Body = LoadBodyWithAttachment(fileName),
            Attachments = attachments
        };
    }

    public void SendMailByFunction(string fileName)
    {
        if (AppConfigService.Config.MailId == 0)
        {
            return;
        }

        var mail = new Mail
        {
            Title = "Auto Generated Report Administrator",
            Subject = "Automail Report - Bettype With 3rd Products",
            Body = LoadBodyWithoutAttachment(fileName),
            MailingListId = AppConfigService.Config.MailId
        };

        MailClientHandler.SendFunctionMail(mail);
    }

    private string LoadBodyWithAttachment(string fileName)
    {
        var model = new Tuple<string, string>(new FileInfo(fileName).Name, AppConfigService.Config.NapReport);
        var template = File.ReadAllText("Templates/Template.cshtml");
        const string tplKey = "Fanex.Bettype3rdProduct.Service";
        var result = Engine.Razor.IsTemplateCached(tplKey, null)
            ? Engine.Razor.Run(tplKey, null, model)
            : Engine.Razor.RunCompile(template, tplKey, model: model);

        return result;
    }

    private string LoadBodyWithoutAttachment(string fileName)
    {
        var model = new Tuple<string, string>(new FileInfo(fileName).Name, AppConfigService.Config.NapReport);
        var template = File.ReadAllText("Templates/TemplateWithoutAttachment.cshtml");
        const string tplKey = "Fanex.Bettype3rdProduct.Service-TemplateWithoutAttachment";
        var result = Engine.Razor.IsTemplateCached(tplKey, null)
            ? Engine.Razor.Run(tplKey, null, model)
            : Engine.Razor.RunCompile(template, tplKey, model: model);

        return result;
    }
}