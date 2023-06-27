using Fanex.Bettype3rdProduct.Business.Models;
using Fanex.MailDelivery.Core.Models;
using System;

namespace Fanex.Bettype3rdProduct.Business.Services;

public interface MainService
{
    JobMessage Execute();
}

public class MainServiceImp : MainService
{
    private readonly Log log;
    private readonly ExcelService excelService;
    private readonly MailService mailService;
    private readonly DataService dataService;

    public MainServiceImp(
        Log log,
        ExcelService excelService,
        MailService mailService,
        DataService dataService)
    {
        this.log = log;

        this.excelService = excelService;
        this.mailService = mailService;
        this.dataService = dataService;
    }

    public JobMessage Execute()
    {
        var result = new ExecuteResultModel();
        var jobMessage = new JobMessage();
        try
        {
            result = SendNotification();
        }
        catch (Exception e)
        {
            result.Success = false;
            result.Message = e.Message;
            log.Log(e);
        }
        finally
        {
            jobMessage.Mail = result.Mail;
            jobMessage.JobStatus = result.Success ? JobStatus.Success : JobStatus.DataIsNotReady;
            jobMessage.TaskLog = new TaskLog
            {
                LogTime = DateTime.Now,
                Message = result.Message
            };
        }

        return jobMessage;
    }

    private ExecuteResultModel SendNotification()
    {
        if (!dataService.IsDataAvailable())
        {
            return new ExecuteResultModel
            {
                Success = false,
                Message = "DataIsNotReady, TaskID = 6,8,9,10"
            };
        }

        var excelFile = excelService.Execute();
        mailService.SendMailByFunction(excelFile);
        var mail = mailService.CreateMail(excelFile);
        return new ExecuteResultModel
        {
            Mail = mail,
            Message = string.Empty,
            Success = true
        };
    }
}