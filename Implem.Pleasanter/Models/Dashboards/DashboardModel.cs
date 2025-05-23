﻿using Implem.DefinitionAccessor;
using Implem.Libraries.Classes;
using Implem.Libraries.DataSources.SqlServer;
using Implem.Libraries.Utilities;
using Implem.Pleasanter.Libraries.DataSources;
using Implem.Pleasanter.Libraries.DataTypes;
using Implem.Pleasanter.Libraries.Extensions;
using Implem.Pleasanter.Libraries.General;
using Implem.Pleasanter.Libraries.Html;
using Implem.Pleasanter.Libraries.HtmlParts;
using Implem.Pleasanter.Libraries.Models;
using Implem.Pleasanter.Libraries.Requests;
using Implem.Pleasanter.Libraries.Responses;
using Implem.Pleasanter.Libraries.Security;
using Implem.Pleasanter.Libraries.Server;
using Implem.Pleasanter.Libraries.ServerScripts;
using Implem.Pleasanter.Libraries.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using static Implem.Pleasanter.Libraries.ServerScripts.ServerScriptModel;
namespace Implem.Pleasanter.Models
{
    [Serializable]
    public class DashboardModel : BaseItemModel
    {
        public long DashboardId = 0;
        public bool Locked = false;

        public TitleBody TitleBody
        {
            get
            {
                return new TitleBody(DashboardId, Ver, VerType == Versions.VerTypes.History, Title.Value, Title.DisplayValue, Body);
            }
        }

        public long SavedDashboardId = 0;
        public bool SavedLocked = false;

        public bool Locked_Updated(Context context, bool copy = false, Column column = null)
        {
            if (copy && column?.CopyByDefault == true)
            {
                return column.GetDefaultInput(context: context).ToBool() != Locked;
            }
            return Locked != SavedLocked
                &&  (column == null
                    || column.DefaultInput.IsNullOrEmpty()
                    || column.GetDefaultInput(context: context).ToBool() != Locked);
        }

        public string PropertyValue(Context context, Column column)
        {
            switch (column?.ColumnName)
            {
                case "SiteId": return SiteId.ToString();
                case "UpdatedTime": return UpdatedTime.Value.ToString();
                case "DashboardId": return DashboardId.ToString();
                case "Ver": return Ver.ToString();
                case "Title": return Title.Value;
                case "Body": return Body;
                case "TitleBody": return TitleBody.ToString();
                case "Locked": return Locked.ToString();
                case "Comments": return Comments.ToJson();
                case "Creator": return Creator.Id.ToString();
                case "Updator": return Updator.Id.ToString();
                case "CreatedTime": return CreatedTime.Value.ToString();
                case "VerUp": return VerUp.ToString();
                case "Timestamp": return Timestamp;
                default: return GetValue(
                    context: context,
                    column: column);
            }
        }

        public string SavedPropertyValue(Context context, Column column)
        {
            switch (column?.ColumnName)
            {
                case "SiteId": return SavedSiteId.ToString();
                case "UpdatedTime": return SavedUpdatedTime.ToString();
                case "DashboardId": return SavedDashboardId.ToString();
                case "Ver": return SavedVer.ToString();
                case "Title": return SavedTitle;
                case "Body": return SavedBody;
                case "Locked": return SavedLocked.ToString();
                case "Comments": return SavedComments;
                case "Creator": return SavedCreator.ToString();
                case "Updator": return SavedUpdator.ToString();
                case "CreatedTime": return SavedCreatedTime.ToString();
                default: return GetSavedValue(
                    context: context,
                    column: column);
            }
        }

        public Dictionary<string, string> PropertyValues(Context context, List<Column> columns)
        {
            var hash = new Dictionary<string, string>();
            columns?
                .Where(column => column != null)
                .ForEach(column =>
                {
                    switch (column.ColumnName)
                    {
                        case "SiteId":
                            hash.Add("SiteId", SiteId.ToString());
                            break;
                        case "UpdatedTime":
                            hash.Add("UpdatedTime", UpdatedTime.Value.ToString());
                            break;
                        case "DashboardId":
                            hash.Add("DashboardId", DashboardId.ToString());
                            break;
                        case "Ver":
                            hash.Add("Ver", Ver.ToString());
                            break;
                        case "Title":
                            hash.Add("Title", Title.Value);
                            break;
                        case "Body":
                            hash.Add("Body", Body);
                            break;
                        case "TitleBody":
                            hash.Add("TitleBody", TitleBody.ToString());
                            break;
                        case "Locked":
                            hash.Add("Locked", Locked.ToString());
                            break;
                        case "Comments":
                            hash.Add("Comments", Comments.ToJson());
                            break;
                        case "Creator":
                            hash.Add("Creator", Creator.Id.ToString());
                            break;
                        case "Updator":
                            hash.Add("Updator", Updator.Id.ToString());
                            break;
                        case "CreatedTime":
                            hash.Add("CreatedTime", CreatedTime.Value.ToString());
                            break;
                        case "VerUp":
                            hash.Add("VerUp", VerUp.ToString());
                            break;
                        case "Timestamp":
                            hash.Add("Timestamp", Timestamp);
                            break;
                        default:
                            hash.Add(column.ColumnName, GetValue(
                                context: context,
                                column: column));
                            break;
                    }
                });
            return hash;
        }

        public bool PropertyUpdated(Context context, string name)
        {
            switch (name)
            {
                case "SiteId": return SiteId_Updated(context: context);
                case "Ver": return Ver_Updated(context: context);
                case "Title": return Title_Updated(context: context);
                case "Body": return Body_Updated(context: context);
                case "Locked": return Locked_Updated(context: context);
                case "Comments": return Comments_Updated(context: context);
                case "Creator": return Creator_Updated(context: context);
                case "Updator": return Updator_Updated(context: context);
                default: 
                    switch (Def.ExtendedColumnTypes.Get(name ?? string.Empty))
                    {
                        case "Class": return Class_Updated(name);
                        case "Num": return Num_Updated(name);
                        case "Date": return Date_Updated(name);
                        case "Description": return Description_Updated(name);
                        case "Check": return Check_Updated(name);
                        case "Attachments": return Attachments_Updated(name);
                    }
                    break;
            }
            return false;
        }

        public List<long> SwitchTargets;

        public DashboardModel()
        {
        }

        public DashboardModel(
            Context context,
            SiteSettings ss,
            Dictionary<string, string> formData = null,
            DashboardApiModel dashboardApiModel = null,
            MethodTypes methodType = MethodTypes.NotSet)
        {
            OnConstructing(context: context);
            SiteId = ss.SiteId;
            if (formData != null)
            {
                SetByForm(
                    context: context,
                    ss: ss,
                    formData: formData);
            }
            if (dashboardApiModel != null)
            {
                SetByApi(context: context, ss: ss, data: dashboardApiModel);
            }
            MethodType = methodType;
            OnConstructed(context: context);
        }

        public DashboardModel(
            Context context,
            SiteSettings ss,
            long dashboardId,
            bool setCopyDefault = false,
            Dictionary<string, string> formData = null,
            DashboardApiModel dashboardApiModel = null,
            SqlColumnCollection column = null,
            bool clearSessions = false,
            List<long> switchTargets = null,
            MethodTypes methodType = MethodTypes.NotSet)
        {
            OnConstructing(context: context);
            DashboardId = dashboardId;
            SiteId = ss.SiteId;
            if (context.QueryStrings.ContainsKey("ver"))
            {
                Get(
                    context: context,
                    tableType: Sqls.TableTypes.NormalAndHistory,
                    column: column,
                    where: Rds.DashboardsWhereDefault(
                        context: context,
                        dashboardModel: this)
                            .Dashboards_Ver(context.QueryStrings.Int("ver")), ss: ss);
            }
            else
            {
                Get(
                    context: context,
                    ss: ss,
                    column: column);
            }
            if (clearSessions) ClearSessions(context: context);
            if (formData != null)
            {
                SetByForm(
                    context: context,
                    ss: ss,
                    formData: formData);
            }
            if (dashboardApiModel != null)
            {
                SetByApi(context: context, ss: ss, data: dashboardApiModel);
            }
            if (SavedLocked)
            {
                ss.SetLockedRecord(
                    context: context,
                    time: UpdatedTime,
                    user: Updator);
            }
            SwitchTargets = switchTargets;
            MethodType = methodType;
            OnConstructed(context: context);
        }

        public DashboardModel(
            Context context,
            SiteSettings ss,
            DataRow dataRow,
            Dictionary<string, string> formData = null,
            string tableAlias = null)
        {
            OnConstructing(context: context);
            if (dataRow != null)
            {
                Set(
                    context: context,
                    ss: ss,
                    dataRow: dataRow,
                    tableAlias: tableAlias);
            }
            if (formData != null)
            {
                SetByForm(
                    context: context,
                    ss: ss,
                    formData: formData);
            }
            OnConstructed(context: context);
        }

        private void OnConstructing(Context context)
        {
        }

        private void OnConstructed(Context context)
        {
        }

        public void ClearSessions(Context context)
        {
        }

        public DashboardModel Get(
            Context context,
            SiteSettings ss,
            Sqls.TableTypes tableType = Sqls.TableTypes.Normal,
            SqlColumnCollection column = null,
            SqlJoinCollection join = null,
            SqlWhereCollection where = null,
            SqlOrderByCollection orderBy = null,
            SqlParamCollection param = null,
            bool distinct = false,
            int top = 0)
        {
            where = where ?? Rds.DashboardsWhereDefault(
                context: context,
                dashboardModel: this);
            column = (column ?? Rds.DashboardsEditorColumns(ss));
            join = join ?? Rds.DashboardsJoinDefault();
            Set(context, ss, Repository.ExecuteTable(
                context: context,
                statements: Rds.SelectDashboards(
                    tableType: tableType,
                    column: column,
                    join: join,
                    where: where,
                    orderBy: orderBy,
                    param: param,
                    distinct: distinct,
                    top: top)));
            return this;
        }

        public DashboardApiModel GetByApi(Context context, SiteSettings ss)
        {
            var data = new DashboardApiModel()
            {
                ApiVersion = context.ApiVersion
            };
            ss.ReadableColumns(context: context, noJoined: true).ForEach(column =>
            {
                switch (column.ColumnName)
                {
                    case "SiteId": data.SiteId = SiteId; break;
                    case "UpdatedTime": data.UpdatedTime = UpdatedTime.Value.ToLocal(context: context); break;
                    case "DashboardId": data.DashboardId = DashboardId; break;
                    case "Ver": data.Ver = Ver; break;
                    case "Title": data.Title = Title.Value; break;
                    case "Body": data.Body = Body; break;
                    case "Locked": data.Locked = Locked; break;
                    case "Creator": data.Creator = Creator.Id; break;
                    case "Updator": data.Updator = Updator.Id; break;
                    case "CreatedTime": data.CreatedTime = CreatedTime.Value.ToLocal(context: context); break;
                    case "Comments": data.Comments = Comments.ToLocal(context: context).ToJson(); break;
                    default: 
                        data.Value(
                            context: context,
                            column: column,
                            value: GetValue(
                                context: context,
                                column: column,
                                toLocal: true));
                        break;
                }
            });
            data.ItemTitle = Title.DisplayValue;
            return data;
        }

        public string ToValue(Context context, SiteSettings ss, Column column, List<string> mine)
        {
            if (!ss.ReadColumnAccessControls.Allowed(
                context: context,
                ss: ss,
                column: column,
                mine: mine))
            {
                return string.Empty;
            }
            return PropertyValue(
                context: context,
                column: column);
        }

        public string ToDisplay(Context context, SiteSettings ss, Column column, List<string> mine)
        {
            if (!ss.ReadColumnAccessControls.Allowed(
                context: context,
                ss: ss,
                column: column,
                mine: mine))
            {
                return string.Empty;
            }
            switch (column.Name)
            {
                case "DashboardId":
                    return DashboardId.ToDisplay(
                        context: context,
                        ss: ss,
                        column: column);
                case "Title":
                    return Title.ToDisplay(
                        context: context,
                        ss: ss,
                        column: column);
                case "Body":
                    return Body.ToDisplay(
                        context: context,
                        ss: ss,
                        column: column);
                case "Locked":
                    return Locked.ToDisplay(
                        context: context,
                        ss: ss,
                        column: column);
                case "Timestamp":
                    return Timestamp.ToDisplay(
                        context: context,
                        ss: ss,
                        column: column);
                case "TitleBody":
                    return TitleBody.ToDisplay(
                        context: context,
                        ss: ss,
                        column: column);
                case "Ver":
                    return Ver.ToDisplay(
                        context: context,
                        ss: ss,
                        column: column);
                case "Comments":
                    return Comments.ToDisplay(
                        context: context,
                        ss: ss,
                        column: column);
                case "Creator":
                    return Creator.ToDisplay(
                        context: context,
                        ss: ss,
                        column: column);
                case "Updator":
                    return Updator.ToDisplay(
                        context: context,
                        ss: ss,
                        column: column);
                case "CreatedTime":
                    return CreatedTime.ToDisplay(
                        context: context,
                        ss: ss,
                        column: column);
                case "UpdatedTime":
                    return UpdatedTime.ToDisplay(
                        context: context,
                        ss: ss,
                        column: column);
                default:
                    switch (Def.ExtendedColumnTypes.Get(column?.Name ?? string.Empty))
                    {
                        case "Class":
                            return GetClass(columnName: column.Name).ToDisplay(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Num":
                            return GetNum(columnName: column.Name).ToDisplay(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Date":
                            return GetDate(columnName: column.Name).ToDisplay(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Description":
                            return GetDescription(columnName: column.Name).ToDisplay(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Check":
                            return GetCheck(columnName: column.Name).ToDisplay(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Attachments":
                            return GetAttachments(columnName: column.Name).ToDisplay(
                                context: context,
                                ss: ss,
                                column: column);
                        default:
                            return string.Empty;
                    }
            }
        }

        public object ToApiDisplayValue(Context context, SiteSettings ss, Column column, List<string> mine)
        {
            if (!ss.ReadColumnAccessControls.Allowed(
                context: context,
                ss: ss,
                column: column,
                mine: mine))
            {
                return string.Empty;
            }
            switch (column.Name)
            {
                case "SiteId":
                    return SiteId.ToApiDisplayValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "UpdatedTime":
                    return UpdatedTime.ToApiDisplayValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "DashboardId":
                    return DashboardId.ToApiDisplayValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Ver":
                    return Ver.ToApiDisplayValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Title":
                    return Title.ToApiDisplayValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Body":
                    return Body.ToApiDisplayValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "TitleBody":
                    return TitleBody.ToApiDisplayValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Locked":
                    return Locked.ToApiDisplayValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Comments":
                    return Comments.ToApiDisplayValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Creator":
                    return Creator.ToApiDisplayValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Updator":
                    return Updator.ToApiDisplayValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "CreatedTime":
                    return CreatedTime.ToApiDisplayValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "VerUp":
                    return VerUp.ToApiDisplayValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Timestamp":
                    return Timestamp.ToApiDisplayValue(
                        context: context,
                        ss: ss,
                        column: column);
                default:
                    switch (Def.ExtendedColumnTypes.Get(column?.Name ?? string.Empty))
                    {
                        case "Class":
                            return GetClass(columnName: column.Name).ToApiDisplayValue(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Num":
                            return GetNum(columnName: column.Name).ToApiDisplayValue(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Date":
                            return GetDate(columnName: column.Name).ToApiDisplayValue(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Description":
                            return GetDescription(columnName: column.Name).ToApiDisplayValue(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Check":
                            return GetCheck(columnName: column.Name).ToApiDisplayValue(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Attachments":
                            return GetAttachments(columnName: column.Name).ToApiDisplayValue(
                                context: context,
                                ss: ss,
                                column: column);
                        default:
                            return string.Empty;
                    }
            }
        }

        public object ToApiValue(Context context, SiteSettings ss, Column column, List<string> mine)
        {
            if (!ss.ReadColumnAccessControls.Allowed(
                context: context,
                ss: ss,
                column: column,
                mine: mine))
            {
                return string.Empty;
            }
            switch (column.Name)
            {
                case "SiteId":
                    return SiteId.ToApiValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "UpdatedTime":
                    return UpdatedTime.ToApiValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "DashboardId":
                    return DashboardId.ToApiValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Ver":
                    return Ver.ToApiValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Title":
                    return Title.ToApiValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Body":
                    return Body.ToApiValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "TitleBody":
                    return TitleBody.ToApiValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Locked":
                    return Locked.ToApiValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Comments":
                    return Comments.ToApiValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Creator":
                    return Creator.ToApiValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Updator":
                    return Updator.ToApiValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "CreatedTime":
                    return CreatedTime.ToApiValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "VerUp":
                    return VerUp.ToApiValue(
                        context: context,
                        ss: ss,
                        column: column);
                case "Timestamp":
                    return Timestamp.ToApiValue(
                        context: context,
                        ss: ss,
                        column: column);
                default:
                    switch (Def.ExtendedColumnTypes.Get(column?.Name ?? string.Empty))
                    {
                        case "Class":
                            return GetClass(columnName: column.Name).ToApiValue(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Num":
                            return GetNum(columnName: column.Name).ToApiValue(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Date":
                            return GetDate(columnName: column.Name).ToApiValue(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Description":
                            return GetDescription(columnName: column.Name).ToApiValue(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Check":
                            return GetCheck(columnName: column.Name).ToApiValue(
                                context: context,
                                ss: ss,
                                column: column);
                        case "Attachments":
                            return GetAttachments(columnName: column.Name).ToApiValue(
                                context: context,
                                ss: ss,
                                column: column);
                        default:
                            return string.Empty;
                    }
            }
        }

        public string FullText(
            Context context,
            SiteSettings ss,
            bool backgroundTask = false,
            bool onCreating = false)
        {
            if (!Parameters.Search.CreateIndexes && !backgroundTask) return null;
            if (AccessStatus == Databases.AccessStatuses.NotFound) return null;
            var fullText = new System.Text.StringBuilder();
            if (ss.FullTextIncludeBreadcrumb == true)
            {
                SiteInfo.TenantCaches
                    .Get(context.TenantId)?
                    .SiteMenu.Breadcrumb(
                        context: context,
                        siteId: SiteId)
                    .FullText(
                        context: context,
                        fullText: fullText);
            }
            if (ss.FullTextIncludeSiteId == true)
            {
                fullText.Append($" {ss.SiteId}");
            }
            if (ss.FullTextIncludeSiteTitle == true)
            {
                fullText.Append($" {ss.Title}");
            }
            ss.GetEditorColumnNames(
                context: context,
                columnOnly: true)
                    .Select(columnName => ss.GetColumn(
                        context: context,
                        columnName: columnName))
                    .ForEach(column =>
                    {
                        switch (column.ColumnName)
                        {
                            case "DashboardId":
                                DashboardId.FullText(
                                    context: context,
                                    column: column,
                                    fullText: fullText);
                                break;
                            case "Title":
                                Title.FullText(
                                    context: context,
                                    column: column,
                                    fullText: fullText);
                                break;
                            case "Body":
                                Body.FullText(
                                    context: context,
                                    column: column,
                                    fullText: fullText);
                                break;
                            case "Comments":
                                Comments.FullText(
                                    context: context,
                                    column: column,
                                    fullText: fullText);
                                break;
                            default:
                                BaseFullText(
                                    context: context,
                                    column: column,
                                    fullText: fullText);
                                break;
                        }
                    });
            Creator.FullText(
                context,
                column: ss.GetColumn(
                    context: context,
                    columnName: "Creator"),
                fullText);
            Updator.FullText(
                context,
                column: ss.GetColumn(
                    context: context,
                    columnName: "Updator"),
                fullText);
            CreatedTime.FullText(
                context,
                column: ss.GetColumn(
                    context: context,
                    columnName: "CreatedTime"),
                fullText);
            UpdatedTime.FullText(
                context,
                column: ss.GetColumn(
                    context: context,
                    columnName: "UpdatedTime"),
                fullText);
            if (!onCreating)
            {
                FullTextExtensions.OutgoingMailsFullText(
                    context: context,
                    ss: ss,
                    fullText: fullText,
                    referenceType: "Dashboards",
                    referenceId: DashboardId);
            }
            return fullText
                .ToString()
                .Replace("　", " ")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Split(' ')
                .Select(o => o.Trim())
                .Where(o => o != string.Empty)
                .Distinct()
                .Join(" ");
        }

        public ErrorData Create(
            Context context,
            SiteSettings ss,
            Sqls.TableTypes tableType = Sqls.TableTypes.Normal,
            SqlParamCollection param = null,
            List<Process> processes = null,
            long copyFrom = 0,
            bool extendedSqls = true,
            bool synchronizeSummary = true,
            bool forceSynchronizeSourceSummary = false,
            bool notice = false,
            string noticeType = "Created",
            bool otherInitValue = false,
            bool get = true)
        {
            var statements = new List<SqlStatement>();
            if (extendedSqls)
            {
                statements.OnCreatingExtendedSqls(
                    context: context,
                    siteId: SiteId);
            }
            statements.AddRange(CreateStatements(
                context: context,
                ss: ss,
                tableType: tableType,
                param: param,
                otherInitValue: otherInitValue));
            var response = Repository.ExecuteScalar_response(
                context: context,
                transactional: true,
                selectIdentity: true,
                statements: statements.ToArray());
            DashboardId = (response.Id ?? DashboardId).ToLong();
            if (context.ContractSettings.Notice != false && notice)
            {
                SetTitle(
                    context: context,
                    ss: ss);
                Notice(
                    context: context,
                    ss: ss,
                    notifications: GetNotifications(
                        context: context,
                        ss: ss,
                        notice: notice),
                    type: noticeType);
                processes?
                    .Where(process => process.MatchConditions)
                    .ForEach(process =>
                        process?.Notifications?.ForEach(notification =>
                            notification.Send(
                                context: context,
                                ss: ss,
                                title: ReplacedDisplayValues(
                                    context: context,
                                    ss: ss,
                                    value: notification.Subject),
                                body: ReplacedDisplayValues(
                                    context: context,
                                    ss: ss,
                                    value: notification.Body),
                                valuesTo: ss.IncludedColumns(notification.Address)
                                    .ToDictionary(
                                        column => column,
                                        column => PropertyValue(
                                            context: context,
                                            column: column)),
                                valuesCc: ss.IncludedColumns(notification.CcAddress)
                                    .ToDictionary(
                                        column => column,
                                        column => PropertyValue(
                                            context: context,
                                            column: column)),
                                valuesBcc: ss.IncludedColumns(notification.BccAddress)
                                    .ToDictionary(
                                        column => column,
                                        column => PropertyValue(
                                            context: context,
                                            column: column)))));
            }
            if (get) Get(context: context, ss: ss);
            var fullText = FullText(context, ss: ss, onCreating: true);
            statements = new List<SqlStatement>();
            statements.Add(Rds.UpdateItems(
                param: Rds.ItemsParam()
                    .Title(Title.DisplayValue)
                    .FullText(fullText, _using: fullText != null)
                    .SearchIndexCreatedTime(DateTime.Now, _using: fullText != null),
                where: Rds.ItemsWhere().ReferenceId(DashboardId)));
            statements.Add(BinaryUtilities.UpdateReferenceId(
                context: context,
                ss: ss,
                referenceId: DashboardId,
                values: fullText));
            if (extendedSqls)
            {
                statements.OnCreatedExtendedSqls(
                    context: context,
                    siteId: SiteId,
                    id: DashboardId);
            }
            Repository.ExecuteNonQuery(
                context: context,
                transactional: true,
                statements: statements.ToArray());
            if (get && Rds.ExtendedSqls(
                context: context,
                siteId: SiteId,
                id: DashboardId)
                    ?.Any(o => o.OnCreated) == true)
            {
                Get(
                    context: context,
                    ss: ss);
            }
            return new ErrorData(type: Error.Types.None);
        }

        public List<SqlStatement> CreateStatements(
            Context context,
            SiteSettings ss,
            string dataTableName = null,
            Sqls.TableTypes tableType = Sqls.TableTypes.Normal,
            SqlParamCollection param = null,
            bool otherInitValue = false)
        {
            var statements = new List<SqlStatement>();
            statements.AddRange(new List<SqlStatement>
            {
                Rds.InsertItems(
                    dataTableName: dataTableName,
                    selectIdentity: true,
                    param: Rds.ItemsParam()
                        .ReferenceType("Dashboards")
                        .SiteId(SiteId)
                        .Title(Title.DisplayValue)),
                Rds.InsertDashboards(
                    dataTableName: dataTableName,
                    tableType: tableType,
                    param: param ?? Rds.DashboardsParamDefault(
                        context: context,
                        ss: ss,
                        dashboardModel: this,
                        setDefault: true,
                        otherInitValue: otherInitValue)),
            });
            return statements;
        }

        public ErrorData Update(
            Context context,
            SiteSettings ss,
            List<Process> processes = null,
            bool extendedSqls = true,
            bool synchronizeSummary = true,
            bool forceSynchronizeSourceSummary = false,
            bool notice = false,
            string previousTitle = null,
            SqlParamCollection param = null,
            List<SqlStatement> additionalStatements = null,
            bool otherInitValue = false,
            bool setBySession = true,
            bool get = true,
            bool checkConflict = true)
        {
            var notifications = GetNotifications(
                context: context,
                ss: ss,
                notice: notice,
                before: true);
            if (setBySession)
            {
                SetBySession(context: context);
            }
            var statements = new List<SqlStatement>();
            if (extendedSqls)
            {
                statements.OnUpdatingExtendedSqls(
                    context: context,
                    siteId: SiteId,
                    id: DashboardId,
                    timestamp: Timestamp.ToDateTime());
            }
            var verUp = Versions.VerUp(
                context: context,
                ss: ss,
                verUp: VerUp);
            statements.AddRange(UpdateStatements(
                context: context,
                ss: ss,
                param: param,
                otherInitValue: otherInitValue,
                additionalStatements: additionalStatements,
                checkConflict: checkConflict,
                verUp: verUp));
            var response = Repository.ExecuteScalar_response(
                context: context,
                transactional: true,
                statements: statements.ToArray());
            if (response.Event == "Conflicted")
            {
                return new ErrorData(
                    type: Error.Types.UpdateConflicts,
                    id: DashboardId);
            }
            if (context.ContractSettings.Notice != false && notice)
            {
                Notice(
                    context: context,
                    ss: ss,
                    notifications: NotificationUtilities.MeetConditions(
                        ss: ss,
                        before: notifications,
                        after: GetNotifications(
                            context: context,
                            ss: ss,
                            notice: notice)),
                    type: "Updated");
                processes?
                    .Where(process => process.MatchConditions)
                    .ForEach(process =>
                        process?.Notifications?.ForEach(notification =>
                            notification.Send(
                                context: context,
                                ss: ss,
                                title: ReplacedDisplayValues(
                                    context: context,
                                    ss: ss,
                                    value: notification.Subject),
                                body: ReplacedDisplayValues(
                                    context: context,
                                    ss: ss,
                                    value: notification.Body),
                                valuesTo: ss.IncludedColumns(notification.Address)
                                    .ToDictionary(
                                        column => column,
                                        column => PropertyValue(
                                            context: context,
                                            column: column)),
                                valuesCc: ss.IncludedColumns(notification.CcAddress)
                                    .ToDictionary(
                                        column => column,
                                        column => PropertyValue(
                                            context: context,
                                            column: column)),
                                valuesBcc: ss.IncludedColumns(notification.BccAddress)
                                    .ToDictionary(
                                        column => column,
                                        column => PropertyValue(
                                            context: context,
                                            column: column)))));
            }
            if (get)
            {
                Get(context: context, ss: ss);
            }
            UpdateRelatedRecords(
                context: context,
                ss: ss,
                extendedSqls: extendedSqls,
                previousTitle: previousTitle,
                get: get,
                addUpdatedTimeParam: true,
                addUpdatorParam: true,
                updateItems: true);
            return new ErrorData(type: Error.Types.None);
        }

        public List<SqlStatement> UpdateStatements(
            Context context,
            SiteSettings ss,
            string dataTableName = null,
            SqlParamCollection param = null,
            bool otherInitValue = false,
            List<SqlStatement> additionalStatements = null,
            bool checkConflict = true,
            bool verUp = false)
        {
            var timestamp = Timestamp.ToDateTime();
            var statements = new List<SqlStatement>();
            var where = Rds.DashboardsWhereDefault(
                context: context,
                dashboardModel: this)
                    .UpdatedTime(timestamp, _using: timestamp.InRange() && checkConflict);
            if (verUp)
            {
                statements.Add(Rds.DashboardsCopyToStatement(
                    where: where,
                    tableType: Sqls.TableTypes.History,
                    ColumnNames()));
                Ver++;
            }
            statements.AddRange(UpdateStatements(
                context: context,
                ss: ss,
                dataTableName: dataTableName,
                where: where,
                param: param,
                otherInitValue: otherInitValue));
            if (ss.PermissionForUpdating?.Any() == true)
            {
                statements.AddRange(PermissionUtilities.UpdateStatements(
                    context: context,
                    ss: ss,
                    referenceId: DashboardId,
                    columns: ss.Columns
                        .Where(o => o.Type != Column.Types.Normal)
                        .ToDictionary(
                            o => $"{o.ColumnName},{o.Type}",
                            o => (o.MultipleSelections == true
                                ? PropertyValue(
                                    context: context,
                                    column: o)?.Deserialize<List<int>>()
                                : PropertyValue(
                                    context: context,
                                    column: o)?.ToInt().ToSingleList()) ?? new List<int>()),
                    permissions: ss.PermissionForUpdating));
            }
            else if (RecordPermissions != null)
            {
                statements.UpdatePermissions(
                    context: context,
                    ss: ss,
                    referenceId: DashboardId,
                    permissions: RecordPermissions);
            }
            if (additionalStatements?.Any() == true)
            {
                statements.AddRange(additionalStatements);
            }
            return statements;
        }

        private List<SqlStatement> UpdateStatements(
            Context context,
            SiteSettings ss,
            string dataTableName = null,
            SqlWhereCollection where = null,
            SqlParamCollection param = null,
            bool otherInitValue = false)
        {
            return new List<SqlStatement>
            {
                Rds.UpdateDashboards(
                    dataTableName: dataTableName,
                    where: where,
                    param: param ?? Rds.DashboardsParamDefault(
                        context: context,
                        ss: ss,
                        dashboardModel: this,
                        otherInitValue: otherInitValue)),
                new SqlStatement()
                {
                    DataTableName = dataTableName,
                    IfConflicted = true,
                    Id = DashboardId
                }
            };
        }

        public void UpdateRelatedRecords(
            Context context,
            SiteSettings ss,
            bool extendedSqls = false,
            bool get = false,
            string previousTitle = null,
            bool addUpdatedTimeParam = true,
            bool addUpdatorParam = true,
            bool updateItems = true)
        {
            Repository.ExecuteNonQuery(
                context: context,
                transactional: true,
                statements: UpdateRelatedRecordsStatements(
                    context: context,
                    ss: ss,
                    extendedSqls: extendedSqls,
                    addUpdatedTimeParam: addUpdatedTimeParam,
                    addUpdatorParam: addUpdatorParam,
                    updateItems: updateItems)
                        .ToArray());
            var titleUpdated = Title_Updated(context: context);
            if (get && Rds.ExtendedSqls(
                context: context,
                siteId: SiteId,
                id: DashboardId)
                    ?.Any(o => o.OnUpdated) == true)
            {
                Get(
                    context: context,
                    ss: ss);
            }
            if (previousTitle != null
                && previousTitle != Title.DisplayValue
                && ss.Sources?.Any() == true)
            {
                ItemUtilities.UpdateSourceTitles(
                    context: context,
                    ss: ss,
                    siteIdList: new List<long>() { ss.SiteId },
                    idList: DashboardId.ToSingleList());
            }
        }

        public List<SqlStatement> UpdateRelatedRecordsStatements(
            Context context,
            SiteSettings ss,
            bool extendedSqls = false,
            bool addUpdatedTimeParam = true,
            bool addUpdatorParam = true,
            bool updateItems = true)
        {
            var fullText = FullText(context, ss: ss);
            var statements = new List<SqlStatement>();
            statements.Add(Rds.UpdateItems(
                where: Rds.ItemsWhere().ReferenceId(DashboardId),
                param: Rds.ItemsParam()
                    .SiteId(SiteId)
                    .Title(Title.DisplayValue)
                    .FullText(fullText, _using: fullText != null)
                    .SearchIndexCreatedTime(DateTime.Now, _using: fullText != null),
                addUpdatedTimeParam: addUpdatedTimeParam,
                addUpdatorParam: addUpdatorParam,
                _using: updateItems));
            if (extendedSqls)
            {
                statements.OnUpdatedExtendedSqls(
                    context: context,
                    siteId: SiteId,
                    id: DashboardId);
            }
            return statements;
        }

        public ErrorData UpdateOrCreate(
            Context context,
            SiteSettings ss,
            string dataTableName = null,
            SqlWhereCollection where = null,
            SqlParamCollection param = null)
        {
            SetBySession(context: context);
            var statements = new List<SqlStatement>
            {
                Rds.InsertItems(
                    dataTableName: dataTableName,
                    selectIdentity: true,
                    param: Rds.ItemsParam()
                        .ReferenceType("Dashboards")
                        .SiteId(SiteId)
                        .Title(Title.DisplayValue)),
                Rds.UpdateOrInsertDashboards(
                    where: where ?? Rds.DashboardsWhereDefault(
                        context: context,
                        dashboardModel: this),
                    param: param ?? Rds.DashboardsParamDefault(
                        context: context,
                        ss: ss,
                        dashboardModel: this,
                        setDefault: true))
            };
            var response = Repository.ExecuteScalar_response(
                context: context,
                transactional: true,
                selectIdentity: true,
                statements: statements.ToArray());
            DashboardId = (response.Id ?? DashboardId).ToLong();
            Get(context: context, ss: ss);
            return new ErrorData(type: Error.Types.None);
        }

        public ErrorData Delete(Context context, SiteSettings ss, bool notice = false)
        {
            var notifications = context.ContractSettings.Notice != false && notice
                ? GetNotifications(
                    context: context,
                    ss: ss,
                    notice: notice)
                : null;
            var statements = new List<SqlStatement>();
            var where = Rds.DashboardsWhere().SiteId(SiteId).DashboardId(DashboardId);
            statements.OnDeletingExtendedSqls(
                context: context,
                siteId: SiteId,
                id: DashboardId);
            statements.AddRange(new List<SqlStatement>
            {
                Rds.DeleteItems(
                    factory: context,
                    where: Rds.ItemsWhere().ReferenceId(DashboardId)),
                Rds.DeleteBinaries(
                    factory: context,
                    where: Rds.BinariesWhere()
                        .TenantId(context.TenantId)
                        .ReferenceId(DashboardId)
                        .BinaryType(
                            value: "Images",
                            _operator: "<>",
                            _using: ss.DeleteImageWhenDeleting == false)),
                Rds.DeleteDashboards(
                    factory: context,
                    where: where)
            });
            statements.OnDeletedExtendedSqls(
                context: context,
                siteId: SiteId,
                id: DashboardId);
            Repository.ExecuteNonQuery(
                context: context,
                transactional: true,
                statements: statements.ToArray());
            if (context.ContractSettings.Notice != false && notice)
            {
                Notice(
                    context: context,
                    ss: ss,
                    notifications: notifications,
                    type: "Deleted");
            }
            return new ErrorData(type: Error.Types.None);
        }

        public ErrorData Restore(Context context, SiteSettings ss,long dashboardId)
        {
            DashboardId = dashboardId;
            Repository.ExecuteNonQuery(
                context: context,
                connectionString: Parameters.Rds.OwnerConnectionString,
                transactional: true,
                statements: new SqlStatement[]
                {
                    Rds.RestoreItems(
                        factory: context,
                        where: Rds.ItemsWhere().ReferenceId(DashboardId)),
                    Rds.RestoreDashboards(
                        factory: context,
                        where: Rds.DashboardsWhere().DashboardId(DashboardId))
                });
            return new ErrorData(type: Error.Types.None);
        }

        public ErrorData PhysicalDelete(
            Context context, SiteSettings ss,Sqls.TableTypes tableType = Sqls.TableTypes.Normal)
        {
            Repository.ExecuteNonQuery(
                context: context,
                transactional: true,
                statements: Rds.PhysicalDeleteDashboards(
                    tableType: tableType,
                    where: Rds.DashboardsWhere().SiteId(SiteId).DashboardId(DashboardId)));
            return new ErrorData(type: Error.Types.None);
        }

        public void SetByForm(
            Context context,
            SiteSettings ss,
            Dictionary<string, string> formData)
        {
            SetByFormData(
                context: context,
                ss: ss,
                formData: formData);
            if (context.QueryStrings.ContainsKey("ver"))
            {
                Ver = context.QueryStrings.Int("ver");
            }
            SetByFormula(context: context, ss: ss);
            SetChoiceHash(context: context, ss: ss);
            if (context.Action == "deletecomment")
            {
                DeleteCommentId = formData.Get("ControlId")?
                    .Split(',')
                    ._2nd()
                    .ToInt() ?? 0;
                Comments.RemoveAll(o => o.CommentId == DeleteCommentId);
            }
        }

        private void SetByFormData(Context context, SiteSettings ss, Dictionary<string, string> formData)
        {
            formData.ForEach(data =>
            {
                var key = data.Key;
                var value = data.Value ?? string.Empty;
                switch (key)
                {
                    case "Dashboards_Title": Title = new Title(DashboardId, value); break;
                    case "Dashboards_Body": Body = value.ToString(); break;
                    case "Dashboards_Locked": Locked = value.ToBool(); break;
                    case "Dashboards_Timestamp": Timestamp = value.ToString(); break;
                    case "Comments": Comments.Prepend(
                        context: context,
                        ss: ss,
                        body: value); break;
                    case "VerUp": VerUp = value.ToBool(); break;
                    default:
                        if (key.RegexExists("Comment[0-9]+"))
                        {
                            Comments.Update(
                                context: context,
                                ss: ss,
                                commentId: key.Substring("Comment".Length).ToInt(),
                                body: value);
                        }
                        else
                        {
                            var column = ss.GetColumn(
                                context: context,
                                columnName: key.Split_2nd('_'));
                            switch (Def.ExtendedColumnTypes.Get(column?.ColumnName ?? string.Empty))
                            {
                                case "Class":
                                    SetClass(
                                        columnName: column.ColumnName,
                                        value: value);
                                    break;
                                case "Num":
                                    SetNum(
                                        columnName: column.ColumnName,
                                        value: new Num(
                                            context: context,
                                            column: column,
                                            value: value));
                                    break;
                                case "Date":
                                    SetDate(
                                        columnName: column.ColumnName,
                                        value: value.ToDateTime().ToUniversal(context: context));
                                    break;
                                case "Description":
                                    SetDescription(
                                        columnName: column.ColumnName,
                                        value: value);
                                    break;
                                case "Check":
                                    SetCheck(
                                        columnName: column.ColumnName,
                                        value: value.ToBool());
                                    break;
                                case "Attachments":
                                    SetAttachments(
                                        columnName: column.ColumnName,
                                        value: value.Deserialize<Attachments>());
                                    break;
                            }
                        }
                        break;
                }
            });
        }

        public DashboardModel CopyAndInit(
            Context context,
            SiteSettings ss)
        {
            return new DashboardModel(
                context: context,
                ss: ss,
                methodType: MethodTypes.New);
        }

        public void SetByModel(DashboardModel dashboardModel)
        {
            SiteId = dashboardModel.SiteId;
            UpdatedTime = dashboardModel.UpdatedTime;
            Title = dashboardModel.Title;
            Body = dashboardModel.Body;
            Locked = dashboardModel.Locked;
            Comments = dashboardModel.Comments;
            Creator = dashboardModel.Creator;
            Updator = dashboardModel.Updator;
            CreatedTime = dashboardModel.CreatedTime;
            VerUp = dashboardModel.VerUp;
            Comments = dashboardModel.Comments;
            ClassHash = dashboardModel.ClassHash;
            NumHash = dashboardModel.NumHash;
            DateHash = dashboardModel.DateHash;
            DescriptionHash = dashboardModel.DescriptionHash;
            CheckHash = dashboardModel.CheckHash;
            AttachmentsHash = dashboardModel.AttachmentsHash;
        }

        public void SetByApi(Context context, SiteSettings ss, DashboardApiModel data)
        {
            if (data.Title != null) Title = new Title(data.DashboardId.ToLong(), data.Title);
            if (data.Body != null) Body = data.Body.ToString().ToString();
            if (data.Locked != null) Locked = data.Locked.ToBool().ToBool();
            if (data.Comments != null) Comments.Prepend(context: context, ss: ss, body: data.Comments);
            if (data.VerUp != null) VerUp = data.VerUp.ToBool();
            data.ClassHash?.ForEach(o => SetClass(
                columnName: o.Key,
                value: o.Value));
            data.NumHash?.ForEach(o => SetNum(
                columnName: o.Key,
                value: new Num(
                    context: context,
                    column: ss.GetColumn(
                        context: context,
                        columnName: o.Key),
                    value: o.Value.ToString())));
            data.DateHash?.ForEach(o => SetDate(
                columnName: o.Key,
                value: o.Value.ToDateTime().ToUniversal(context: context)));
            data.DescriptionHash?.ForEach(o => SetDescription(
                columnName: o.Key,
                value: o.Value));
            data.CheckHash?.ForEach(o => SetCheck(
                columnName: o.Key,
                value: o.Value));
            data.AttachmentsHash?.ForEach(o =>
            {
                string columnName = o.Key;
                Attachments newAttachments = o.Value;
                Attachments oldAttachments;
                if (columnName == "Attachments#Uploading")
                {
                    var kvp = AttachmentsHash
                        .FirstOrDefault(x => x.Value
                            .Any(att => att.Guid == newAttachments.FirstOrDefault()?.Guid?.Split_1st()));
                    columnName = kvp.Key;
                    oldAttachments = kvp.Value;
                    var column = ss.GetColumn(
                        context: context,
                        columnName: columnName);
                    if (column.OverwriteSameFileName == true)
                    {
                        var oldAtt = oldAttachments
                            .FirstOrDefault(att => att.Guid == newAttachments.FirstOrDefault()?.Guid?.Split_1st());
                        if (oldAtt != null)
                        {
                            oldAtt.Deleted = true;
                            oldAtt.Overwritten = true;
                        }
                    }
                    newAttachments.ForEach(att => att.Guid = att.Guid.Split_2nd());
                }
                else
                {
                    oldAttachments = AttachmentsHash.Get(columnName);
                }
                if (oldAttachments != null)
                {
                    var column = ss.GetColumn(
                        context: context,
                        columnName: columnName);
                    var newGuidSet = new HashSet<string>(newAttachments.Select(x => x.Guid).Distinct());
                    var newNameSet = new HashSet<string>(newAttachments.Select(x => x.Name).Distinct());
                    newAttachments.ForEach(newAttachment =>
                    {
                        newAttachment.AttachmentAction(
                            context: context,
                            column: column,
                            oldAttachments: oldAttachments);
                    });
                    if (column.OverwriteSameFileName == true)
                    {
                        newAttachments.AddRange(oldAttachments.
                            Where((oldvalue) =>
                                !newGuidSet.Contains(oldvalue.Guid) &&
                                !newNameSet.Contains(oldvalue.Name)));
                    }
                    else
                    {
                        newAttachments.AddRange(oldAttachments.
                            Where((oldvalue) => !newGuidSet.Contains(oldvalue.Guid)));
                    }
                }
                SetAttachments(columnName: columnName, value: newAttachments);
            });
            SetByFormula(context: context, ss: ss);
            SetChoiceHash(context: context, ss: ss);
        }

        public void UpdateFormulaColumns(
            Context context, SiteSettings ss, IEnumerable<int> selected = null)
        {
            SetByFormula(context: context, ss: ss);
            var param = Rds.DashboardsParam();
            ss.Formulas?
                .Where(o => selected == null || selected.Contains(o.Id))
                .ForEach(formulaSet =>
                {
                    if (string.IsNullOrEmpty(formulaSet.CalculationMethod)
                        || formulaSet.CalculationMethod == FormulaSet.CalculationMethods.Default.ToString())
                    {
                        switch (formulaSet.Target)
                        {
                            default:
                                if (Def.ExtendedColumnTypes.ContainsKey(formulaSet.Target ?? string.Empty))
                                {
                                    param.Add(
                                        columnBracket: $"\"{formulaSet.Target}\"",
                                        name: formulaSet.Target,
                                        value: GetNum(formulaSet.Target).Value);
                                }
                                break;
                        }
                    }
                    else if (formulaSet.CalculationMethod == FormulaSet.CalculationMethods.Extended.ToString())
                    {
                        switch (formulaSet.Target)
                        {
                            case "Title": param.Title(Title.Value); break;
                            case "Body": param.Body(Body); break;
                            case "Locked": param.Locked(Locked); break;
                                case "Comments": param.Comments(Comments.ToString()); break;
                            default:
                                if (Def.ExtendedColumnTypes.ContainsKey(formulaSet.Target ?? string.Empty))
                                {
                                    switch (Def.ExtendedColumnTypes.Get(formulaSet.Target))
                                    {
                                        case "Class":
                                            param.Add(
                                                columnBracket: $"\"{formulaSet.Target}\"",
                                                name: formulaSet.Target,
                                                value: GetClass(formulaSet.Target));
                                            break;
                                        case "Num":
                                            param.Add(
                                                columnBracket: $"\"{formulaSet.Target}\"",
                                                name: formulaSet.Target,
                                                value: GetNum(formulaSet.Target).Value);
                                            break;
                                        case "Date":
                                            param.Add(
                                                columnBracket: $"\"{formulaSet.Target}\"",
                                                name: formulaSet.Target,
                                                value: GetDate(formulaSet.Target));
                                            break;
                                        case "Description":
                                            param.Add(
                                                columnBracket: $"\"{formulaSet.Target}\"",
                                                name: formulaSet.Target,
                                                value: GetDescription(formulaSet.Target));
                                            break;
                                        case "Check":
                                            param.Add(
                                                columnBracket: $"\"{formulaSet.Target}\"",
                                                name: formulaSet.Target,
                                                value: GetCheck(formulaSet.Target));
                                            break;
                                    }
                                    break;
                                }
                                break;
                        }
                    }
                });
            var paramFilter = param.Where(p => p.Value != null).ToList();
            if (paramFilter.Count > 0)
            {
                Repository.ExecuteNonQuery(
                    context: context,
                    statements: Rds.UpdateDashboards(
                        param: param,
                        where: Rds.DashboardsWhereDefault(
                            context: context,
                            dashboardModel: this),
                        addUpdatedTimeParam: false,
                        addUpdatorParam: false));
            }
        }

        public void SetByFormula(Context context, SiteSettings ss)
        {
            SetByBeforeFormulaServerScript(
                context: context,
                ss: ss);
            ss.Formulas?.ForEach(formulaSet =>
            {
                var columnName = formulaSet.Target;
                var view = ss.Views?.Get(formulaSet.Condition);
                var isOutOfCondition = view != null && !Matched(context: context, ss: ss, view: view);
                if (string.IsNullOrEmpty(formulaSet.CalculationMethod)
                    || formulaSet.CalculationMethod == FormulaSet.CalculationMethods.Default.ToString())
                {
                    var formula = formulaSet.Formula;
                    if (isOutOfCondition)
                    {
                        if (formulaSet.OutOfCondition != null)
                        {
                            formula = formulaSet.OutOfCondition;
                        }
                        else
                        {
                            return;
                        }
                    }
                    var data = new Dictionary<string, decimal>
                    {
                    };
                    data.AddRange(NumHash.ToDictionary(
                        o => o.Key,
                        o => o.Value?.Value?.ToDecimal() ?? 0));
                    var value = formula?.GetResult(
                        data: data,
                        column: ss.GetColumn(
                            context: context,
                            columnName: columnName)) ?? 0;
                    switch (columnName)
                    {
                        default:
                            SetNum(
                                columnName: columnName,
                                value: new Num(value));
                            break;
                    }
                    if (ss.OutputFormulaLogs == true)
                    {
                        context.LogBuilder?.AppendLine($"formulaSet: {formulaSet.GetRecordingData().ToJson()}");
                        context.LogBuilder?.AppendLine($"formulaSource: {data.ToJson()}");
                        context.LogBuilder?.AppendLine($"formulaResult: {{\"{columnName}\":{value}}}");
                    }
                }
                else if (formulaSet.CalculationMethod == FormulaSet.CalculationMethods.Extended.ToString())
                {
                    var formula = formulaSet.Formula;
                    if (isOutOfCondition && formulaSet.FormulaScriptOutOfCondition == null)
                    {
                        return;
                    }
                    var value = ExecFormulaExtended(
                        context: context,
                        ss: ss,
                        columnName: columnName,
                        formulaSet: formulaSet,
                        isOutOfCondition: isOutOfCondition,
                        outputFormulaLogs: ss.OutputFormulaLogs);
                    var formData =  new Dictionary<string, string>
                    {
                        { $"Dashboards_{columnName}", value }
                    };
                    SetByFormData(
                        context: context,
                        ss: ss,
                        formData: formData);
                }
            });
            SetByAfterFormulaServerScript(
                context: context,
                ss: ss);
        }

        private string ExecFormulaExtended(
            Context context,
            SiteSettings ss,
            string columnName,
            FormulaSet formulaSet,
            bool isOutOfCondition,
            bool? outputFormulaLogs)
        {
            var script = isOutOfCondition == false
                ? formulaSet.FormulaScript
                : formulaSet.FormulaScriptOutOfCondition;
            if (script == null) script = string.Empty;
            SetExtendedColumnDefaultValue(
                ss: ss,
                formulaScript: script,
                calculationMethod: formulaSet.CalculationMethod);
            formulaSet = FormulaBuilder.UpdateColumnDisplayText(
                ss: ss,
                formulaSet: formulaSet);
            script = FormulaBuilder.ParseFormulaScript(
                ss: ss,
                formulaScript: formulaSet.FormulaScript,
                calculationMethod: formulaSet.CalculationMethod);
            var value = FormulaServerScriptUtilities.Execute(
                context: context,
                ss: ss,
                itemModel: this,
                formulaScript: script);
            switch (value)
            {
                case "#N/A":
                case "#VALUE!":
                case "#REF!":
                case "#DIV/0!":
                case "#NUM!":
                case "#NAME?":
                case "#NULL!":
                case "Invalid Parameter":
                    if (formulaSet.IsDisplayError == true)
                    {
                        throw new Exception($"Formula error {value}");
                    }
                    new SysLogModel(
                        context: context,
                        method: nameof(SetByFormula),
                        message: $"Formula error {value}",
                        sysLogType: SysLogModel.SysLogTypes.Exception);
                    break;
            }
            if (outputFormulaLogs == true)
            {
                context.LogBuilder?.AppendLine($"formulaSet: {formulaSet.GetRecordingData().ToJson()}");
                context.LogBuilder?.AppendLine($"formulaSource: {this.ToJson()}");
                context.LogBuilder?.AppendLine($"formulaResult: {{\"{columnName}\":{value}}}");
            }
            return value.ToString();
        }

        public void SetTitle(Context context, SiteSettings ss)
        {
            if (Title?.ItemTitle != true)
            {
                Title = new Title(
                    context: context,
                    ss: ss,
                    id: DashboardId,
                    ver: Ver,
                    isHistory: VerType == Versions.VerTypes.History,
                    data: PropertyValues(
                        context: context,
                        columns: ss.GetTitleColumns(context: context)));
            }
        }

        private bool Matched(Context context, SiteSettings ss, View view)
        {
            if (view.ColumnFilterHash != null)
            {
                foreach (var filter in view.ColumnFilterHash)
                {
                    var match = true;
                    var column = ss.GetColumn(context: context, columnName: filter.Key);
                    switch (filter.Key)
                    {
                        case "UpdatedTime":
                            match = UpdatedTime?.Value.Matched(
                                context: context,
                                column: column,
                                condition: filter.Value) == true;
                            break;
                        case "DashboardId":
                            match = DashboardId.Matched(
                                column: column,
                                condition: filter.Value);
                            break;
                        case "Ver":
                            match = Ver.Matched(
                                context: context,
                                column: column,
                                condition: filter.Value);
                            break;
                        case "Title":
                            match = Title.Value.Matched(
                                context: context,
                                column: column,
                                condition: filter.Value);
                            break;
                        case "Body":
                            match = Body.Matched(
                                context: context,
                                column: column,
                                condition: filter.Value);
                            break;
                        case "Locked":
                            match = Locked.Matched(
                                column: column,
                                condition: filter.Value);
                            break;
                        case "Creator":
                            match = Creator.Id.Matched(
                                context: context,
                                column: column,
                                condition: filter.Value);
                            break;
                        case "Updator":
                            match = Updator.Id.Matched(
                                context: context,
                                column: column,
                                condition: filter.Value);
                            break;
                        case "CreatedTime":
                            match = CreatedTime?.Value.Matched(
                                context: context,
                                column: column,
                                condition: filter.Value) == true;
                            break;
                        default:
                            switch (Def.ExtendedColumnTypes.Get(filter.Key ?? string.Empty))
                            {
                                case "Class":
                                    match = GetClass(column: column).Matched(
                                        context: context,
                                        column: column,
                                        condition: filter.Value);
                                    break;
                                case "Num":
                                    match = GetNum(column: column).Matched(
                                        column: column,
                                        condition: filter.Value);
                                    break;
                                case "Date":
                                    match = GetDate(column: column).Matched(
                                        context: context,
                                        column: column,
                                        condition: filter.Value);
                                    break;
                                case "Description":
                                    match = GetDescription(column: column).Matched(
                                        context: context,
                                        column: column,
                                        condition: filter.Value);
                                    break;
                                case "Check":
                                    match = GetCheck(column: column).Matched(
                                        column: column,
                                        condition: filter.Value);
                                    break;
                            }
                            break;
                    }
                    if (!match) return false;
                }
            }
            return true;
        }

        public string ReplacedDisplayValues(
            Context context,
            SiteSettings ss,
            string value)
        {
            ss.IncludedColumns(value: value).ForEach(column =>
                value = value.Replace(
                    $"[{column.ColumnName}]",
                    ToDisplay(
                        context: context,
                        ss: ss,
                        column: column,
                        mine: Mine(context: context))));
            value = ReplacedContextValues(context, value);
            return value;
        }

        private string ReplacedContextValues(Context context, string value)
        {
            var url = Locations.ItemEditAbsoluteUri(
                context: context,
                id: DashboardId);
            var mailAddress = MailAddressUtilities.Get(
                context: context,
                userId: context.UserId);
            value = value
                .Replace("{Url}", url)
                .Replace("{LoginId}", context.User.LoginId)
                .Replace("{UserName}", context.User.Name)
                .Replace("{MailAddress}", mailAddress);
            return value;
        }

        public List<Notification> GetNotifications(
            Context context,
            SiteSettings ss,
            bool notice,
            bool before = false,
            Sqls.TableTypes tableTypes = Sqls.TableTypes.Normal)
        {
            if (context.ContractSettings.Notice == false || !notice)
            {
                return null;
            }
            var notifications = NotificationUtilities.Get(
                context: context,
                ss: ss);
            if (notifications?.Any() == true)
            {
                var dataSet = Repository.ExecuteDataSet(
                    context: context,
                    statements: notifications.Select(notification =>
                    {
                        var where = ss.Views?.Get(before
                            ? notification.BeforeCondition
                            : notification.AfterCondition)
                                ?.Where(
                                    context: context,
                                    ss: ss,
                                    where: Rds.DashboardsWhere().DashboardId(DashboardId))
                                        ?? Rds.DashboardsWhere().DashboardId(DashboardId);
                        return Rds.SelectDashboards(
                            dataTableName: notification.Index.ToString(),
                            tableType: tableTypes,
                            column: Rds.DashboardsColumn().DashboardId(),
                            join: ss.Join(
                                context: context,
                                join: where),
                            where: where);
                    }).ToArray());
                return notifications
                    .Where(notification =>
                        dataSet.Tables[notification.Index.ToString()].Rows.Count == 1)
                    .ToList();
            }
            else
            {
                return null;
            }
        }

        public void Notice(
            Context context,
            SiteSettings ss,
            List<Notification> notifications,
            string type)
        {
            notifications?.ForEach(notification =>
            {
                if (notification.HasRelatedUsers())
                {
                    var users = new List<int>();
                    Repository.ExecuteTable(
                        context: context,
                        statements: Rds.SelectDashboards(
                            tableType: Sqls.TableTypes.All,
                            distinct: true,
                            column: Rds.DashboardsColumn()
                                .Creator()
                                .Updator(),
                            where: Rds.DashboardsWhere().DashboardId(DashboardId)))
                                .AsEnumerable()
                                .ForEach(dataRow =>
                                {
                                    users.Add(dataRow.Int("Creator"));
                                    users.Add(dataRow.Int("Updator"));
                                });
                    notification.ReplaceRelatedUsers(
                        context: context,
                        users: users);
                }
                var valuesTo = ss.IncludedColumns(notification.Address)
                    .ToDictionary(
                        column => column,
                        column => PropertyValue(
                            context: context,
                            column: column));
                var valuesCc = ss.IncludedColumns(notification.CcAddress)
                    .ToDictionary(
                        column => column,
                        column => PropertyValue(
                            context: context,
                            column: column));
                var valuesBcc = ss.IncludedColumns(notification.BccAddress)
                    .ToDictionary(
                        column => column,
                        column => PropertyValue(
                            context: context,
                            column: column));
                switch (type)
                {
                    case "Created":
                    case "Copied":
                        if ((type == "Created" && notification.AfterCreate != false)
                            || (type == "Copied" && notification.AfterCopy != false))
                        {
                            notification.Send(
                                context: context,
                                ss: ss,
                                title: notification.Subject.IsNullOrEmpty()
                                    ? Displays.Created(
                                        context: context,
                                        data: Title.DisplayValue).ToString()
                                    : ReplacedDisplayValues(
                                        context: context,
                                        ss: ss,
                                        value: notification.Subject.Replace(
                                            "[NotificationTrigger]",
                                            Displays.CreatedWord(context: context))),
                                body: NoticeBody(
                                    context: context,
                                    ss: ss,
                                    notification: notification),
                                valuesTo: valuesTo,
                                valuesCc: valuesCc,
                                valuesBcc: valuesBcc);
                        }
                        break;
                    case "Updated":
                        if (notification.AfterUpdate != false
                            && notification.MonitorChangesColumns.Any(columnName => PropertyUpdated(
                                context: context,
                                name: columnName)))
                        {
                            var body = NoticeBody(
                                context: context,
                                ss: ss,
                                notification: notification,
                                update: true);
                            notification.Send(
                                context: context,
                                ss: ss,
                                title: notification.Subject.IsNullOrEmpty()
                                    ? Displays.Updated(
                                        context: context,
                                        data: Title.DisplayValue).ToString()
                                    : ReplacedDisplayValues(
                                        context: context,
                                        ss: ss,
                                        value: notification.Subject.Replace(
                                            "[NotificationTrigger]",
                                            Displays.UpdatedWord(context: context))),
                                body: body,
                                valuesTo: valuesTo,
                                valuesCc: valuesCc,
                                valuesBcc: valuesBcc);
                        }
                        break;
                    case "Deleted":
                        if (notification.AfterDelete != false)
                        {
                            notification.Send(
                                context: context,
                                ss: ss,
                                title: notification.Subject.IsNullOrEmpty()
                                    ? Displays.Deleted(
                                        context: context,
                                        data: Title.DisplayValue).ToString()
                                    : ReplacedDisplayValues(
                                        context: context,
                                        ss: ss,
                                        value: notification.Subject.Replace(
                                            "[NotificationTrigger]",
                                            Displays.DeletedWord(context: context))),
                                body: NoticeBody(
                                    context: context,
                                    ss: ss,
                                    notification: notification),
                                valuesTo: valuesTo,
                                valuesCc: valuesCc,
                                valuesBcc: valuesBcc);
                        }
                        break;
                }
            });
        }

        private string NoticeBody(
            Context context,
            SiteSettings ss,
            Notification notification,
            bool update = false)
        {
            var body = new System.Text.StringBuilder();
            notification.GetFormat(
                context: context,
                ss: ss)
                    .Split('\n')
                    .Select(line => new
                    {
                        Line = line.Trim(),
                        Format = line.Trim().Deserialize<NotificationColumnFormat>()
                    })
                    .ForEach(data =>
                    {
                        var column = ss.IncludedColumns(data.Format?.Name)?.FirstOrDefault();
                        if (column == null)
                        {
                            body.Append(ReplacedContextValues(
                                context: context,
                                value: data.Line));
                            body.Append("\n");
                        }
                        else
                        {
                            switch (column.Name)
                            {
                                case "Title":
                                    body.Append(Title.ToNotice(
                                        context: context,
                                        saved: SavedTitle,
                                        column: column,
                                        notificationColumnFormat: data.Format,
                                        updated: Title_Updated(context: context),
                                        update: update));
                                    break;
                                case "Body":
                                    body.Append(Body.ToNotice(
                                        context: context,
                                        saved: SavedBody,
                                        column: column,
                                        notificationColumnFormat: data.Format,
                                        updated: Body_Updated(context: context),
                                        update: update));
                                    break;
                                case "Locked":
                                    body.Append(Locked.ToNotice(
                                        context: context,
                                        saved: SavedLocked,
                                        column: column,
                                        notificationColumnFormat: data.Format,
                                        updated: Locked_Updated(context: context),
                                        update: update));
                                    break;
                                case "Comments":
                                    body.Append(Comments.ToNotice(
                                        context: context,
                                        saved: SavedComments,
                                        column: column,
                                        notificationColumnFormat: data.Format,
                                        updated: Comments_Updated(context: context),
                                        update: update));
                                    break;
                                case "Creator":
                                    body.Append(Creator.ToNotice(
                                        context: context,
                                        saved: SavedCreator,
                                        column: column,
                                        notificationColumnFormat: data.Format,
                                        updated: Creator_Updated(context: context),
                                        update: update));
                                    break;
                                case "Updator":
                                    body.Append(Updator.ToNotice(
                                        context: context,
                                        saved: SavedUpdator,
                                        column: column,
                                        notificationColumnFormat: data.Format,
                                        updated: Updator_Updated(context: context),
                                        update: update));
                                    break;
                                default:
                                    switch (Def.ExtendedColumnTypes.Get(column?.Name ?? string.Empty))
                                    {
                                        case "Class":
                                            body.Append(GetClass(columnName: column.Name).ToNotice(
                                                context: context,
                                                saved: GetSavedClass(columnName: column.Name),
                                                column: column,
                                                notificationColumnFormat: data.Format,
                                                updated: Class_Updated(columnName: column.Name),
                                                update: update));
                                            break;
                                        case "Num":
                                            body.Append(GetNum(columnName: column.Name).ToNotice(
                                                context: context,
                                                saved: GetSavedNum(columnName: column.Name),
                                                column: column,
                                                notificationColumnFormat: data.Format,
                                                updated: Num_Updated(columnName: column.Name),
                                                update: update));
                                            break;
                                        case "Date":
                                            body.Append(GetDate(columnName: column.Name).ToNotice(
                                                context: context,
                                                saved: GetSavedDate(columnName: column.Name),
                                                column: column,
                                                notificationColumnFormat: data.Format,
                                                updated: Date_Updated(columnName: column.Name),
                                                update: update));
                                            break;
                                        case "Description":
                                            body.Append(GetDescription(columnName: column.Name).ToNotice(
                                                context: context,
                                                saved: GetSavedDescription(columnName: column.Name),
                                                column: column,
                                                notificationColumnFormat: data.Format,
                                                updated: Description_Updated(columnName: column.Name),
                                                update: update));
                                            break;
                                        case "Check":
                                            body.Append(GetCheck(columnName: column.Name).ToNotice(
                                                context: context,
                                                saved: GetSavedCheck(columnName: column.Name),
                                                column: column,
                                                notificationColumnFormat: data.Format,
                                                updated: Check_Updated(columnName: column.Name),
                                                update: update));
                                            break;
                                        case "Attachments":
                                            body.Append(GetAttachments(columnName: column.Name).ToNotice(
                                                context: context,
                                                saved: GetSavedAttachments(columnName: column.Name),
                                                column: column,
                                                notificationColumnFormat: data.Format,
                                                updated: Attachments_Updated(columnName: column.Name),
                                                update: update));
                                            break;
                                    }
                                    break;
                            }
                        }
                    });
            return body.ToString();
        }

        private void SetBySession(Context context)
        {
        }

        private void Set(Context context, SiteSettings ss, DataTable dataTable)
        {
            switch (dataTable.Rows.Count)
            {
                case 1: Set(context, ss, dataTable.Rows[0]); break;
                case 0: AccessStatus = Databases.AccessStatuses.NotFound; break;
                default: AccessStatus = Databases.AccessStatuses.Overlap; break;
            }
            SetChoiceHash(context: context, ss: ss);
        }

        public void SetChoiceHash(Context context, SiteSettings ss)
        {
            if (!ss.SetAllChoices)
            {
                ss.GetUseSearchLinks(context: context).ForEach(link =>
                {
                    var column = ss.GetColumn(
                        context: context,
                        columnName: link.ColumnName);
                    var value = PropertyValue(
                        context: context,
                        column: column);
                    if (!value.IsNullOrEmpty() 
                        && column?.ChoiceHash?.Any(o => o.Value.Value == value) != true)
                    {
                        ss.SetChoiceHash(
                            context: context,
                            columnName: column.ColumnName,
                            selectedValues: value.ToSingleList());
                    }
                });
            }
            SetTitle(context: context, ss: ss);
        }

        private void Set(Context context, SiteSettings ss, DataRow dataRow, string tableAlias = null)
        {
            AccessStatus = Databases.AccessStatuses.Selected;
            foreach (DataColumn dataColumn in dataRow.Table.Columns)
            {
                var column = new ColumnNameInfo(dataColumn.ColumnName);
                if (column.TableAlias == tableAlias)
                {
                    switch (column.Name)
                    {
                        case "SiteId":
                            if (dataRow[column.ColumnName] != DBNull.Value)
                            {
                                SiteId = dataRow[column.ColumnName].ToLong();
                                SavedSiteId = SiteId;
                            }
                            break;
                        case "UpdatedTime":
                            if (dataRow[column.ColumnName] != DBNull.Value)
                            {
                                UpdatedTime = new Time(context, dataRow, column.ColumnName); Timestamp = dataRow.Field<DateTime>(column.ColumnName).ToString("yyyy/M/d H:m:s.fff");
                                SavedUpdatedTime = UpdatedTime.Value;
                            }
                            break;
                        case "DashboardId":
                            if (dataRow[column.ColumnName] != DBNull.Value)
                            {
                                DashboardId = dataRow[column.ColumnName].ToLong();
                                SavedDashboardId = DashboardId;
                            }
                            break;
                        case "Ver":
                            Ver = dataRow[column.ColumnName].ToInt();
                            SavedVer = Ver;
                            break;
                        case "Title":
                            Title = new Title(context: context, ss: ss, dataRow: dataRow, column: column);
                            SavedTitle = Title.Value;
                            break;
                        case "Body":
                            Body = dataRow[column.ColumnName].ToString();
                            SavedBody = Body;
                            break;
                        case "Locked":
                            Locked = dataRow[column.ColumnName].ToBool();
                            SavedLocked = Locked;
                            break;
                        case "Comments":
                            Comments = dataRow[column.ColumnName].ToString().Deserialize<Comments>() ?? new Comments();
                            SavedComments = Comments.ToJson();
                            break;
                        case "Creator":
                            Creator = SiteInfo.User(context: context, userId: dataRow.Int(column.ColumnName));
                            SavedCreator = Creator.Id;
                            break;
                        case "Updator":
                            Updator = SiteInfo.User(context: context, userId: dataRow.Int(column.ColumnName));
                            SavedUpdator = Updator.Id;
                            break;
                        case "CreatedTime":
                            CreatedTime = new Time(context, dataRow, column.ColumnName);
                            SavedCreatedTime = CreatedTime.Value;
                            break;
                        case "IsHistory":
                            VerType = dataRow.Bool(column.ColumnName)
                                ? Versions.VerTypes.History
                                : Versions.VerTypes.Latest; break;
                        default:
                            switch (Def.ExtendedColumnTypes.Get(column?.Name ?? string.Empty))
                            {
                                case "Class":
                                    SetClass(
                                        columnName: column.Name,
                                        value: dataRow[column.ColumnName].ToString());
                                    SetSavedClass(
                                        columnName: column.Name,
                                        value: GetClass(columnName: column.Name));
                                    break;
                                case "Num":
                                    SetNum(
                                        columnName: column.Name,
                                        value: new Num(
                                            dataRow: dataRow,
                                            name: column.ColumnName));
                                    SetSavedNum(
                                        columnName: column.Name,
                                        value: GetNum(columnName: column.Name).Value);
                                    break;
                                case "Date":
                                    SetDate(
                                        columnName: column.Name,
                                        value: dataRow[column.ColumnName].ToDateTime());
                                    SetSavedDate(
                                        columnName: column.Name,
                                        value: GetDate(columnName: column.Name));
                                    break;
                                case "Description":
                                    SetDescription(
                                        columnName: column.Name,
                                        value: dataRow[column.ColumnName].ToString());
                                    SetSavedDescription(
                                        columnName: column.Name,
                                        value: GetDescription(columnName: column.Name));
                                    break;
                                case "Check":
                                    SetCheck(
                                        columnName: column.Name,
                                        value: dataRow[column.ColumnName].ToBool());
                                    SetSavedCheck(
                                        columnName: column.Name,
                                        value: GetCheck(columnName: column.Name));
                                    break;
                                case "Attachments":
                                    SetAttachments(
                                        columnName: column.Name,
                                        value: dataRow[column.ColumnName].ToString()
                                            .Deserialize<Attachments>() ?? new Attachments());
                                    SetSavedAttachments(
                                        columnName: column.Name,
                                        value: GetAttachments(columnName: column.Name).ToJson());
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        public bool Updated(Context context)
        {
            return Updated()
                || SiteId_Updated(context: context)
                || Ver_Updated(context: context)
                || Title_Updated(context: context)
                || Body_Updated(context: context)
                || Locked_Updated(context: context)
                || Comments_Updated(context: context)
                || Creator_Updated(context: context)
                || Updator_Updated(context: context);
        }

        private bool UpdatedWithColumn(Context context, SiteSettings ss)
        {
            return ClassHash.Any(o => Class_Updated(
                    columnName: o.Key,
                    column: ss.GetColumn(context: context, o.Key)))
                || NumHash.Any(o => Num_Updated(
                    columnName: o.Key,
                    column: ss.GetColumn(context: context, o.Key)))
                || DateHash.Any(o => Date_Updated(
                    columnName: o.Key,
                    column: ss.GetColumn(context: context, o.Key)))
                || DescriptionHash.Any(o => Description_Updated(
                    columnName: o.Key,
                    column: ss.GetColumn(context: context, o.Key)))
                || CheckHash.Any(o => Check_Updated(
                    columnName: o.Key,
                    column: ss.GetColumn(context: context, o.Key)))
                || AttachmentsHash.Any(o => Attachments_Updated(
                    columnName: o.Key,
                    column: ss.GetColumn(context: context, o.Key)));
        }

        public bool Updated(Context context, SiteSettings ss)
        {
            return UpdatedWithColumn(context: context, ss: ss)
                || SiteId_Updated(context: context)
                || Ver_Updated(context: context)
                || Title_Updated(context: context)
                || Body_Updated(context: context)
                || Locked_Updated(context: context)
                || Comments_Updated(context: context)
                || Creator_Updated(context: context)
                || Updator_Updated(context: context);
        }

        public override List<string> Mine(Context context)
        {
            if (MineCache == null)
            {
                var mine = new List<string>();
                var userId = context.UserId;
                if (SavedCreator == userId) mine.Add("Creator");
                if (SavedUpdator == userId) mine.Add("Updator");
                MineCache = mine;
            }
            return MineCache;
        }

        public string IdSuffix()
        {
            return $"_{SiteId}_{(DashboardId == 0 ? -1 : DashboardId)}";
        }
    }
}
