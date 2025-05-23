﻿using Implem.DefinitionAccessor;
using Quartz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Implem.Pleasanter.Libraries.BackgroundServices
{
    public class DeleteTemporaryFilesTimer : ExecutionTimerBase
    {
        public class Param : IExecutionTimerBaseParam
        {
            public static readonly JobKey jobKey = new JobKey("DeleteTemporaryFilesTimer", "ExecutionTimerBase");
            public Type JobType => typeof(DeleteTemporaryFilesTimer);
            public IEnumerable<string> TimeList => Parameters.BackgroundService.DeleteTemporaryFilesTime;
            public bool Enabled => Parameters.BackgroundService.DeleteTemporaryFiles;
            public JobKey JobKey => jobKey;
            public string JobName => "DeleteTemporaryFilesService";
            public Task<bool> SetCustomTimer(IScheduler scheduler) => Task.FromResult(false);
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(() =>
            {
                var context = CreateContext();
                var log = CreateSysLogModel(
                    context: context,
                    message: "Delete Temporary Files.");
                Initializer.DeleteTemporaryFiles();
                log.Finish(context: context);
            }, context.CancellationToken);
        }

        internal static IExecutionTimerBaseParam GetParam()
        {
            return new Param();
        }
    }
}
