﻿using Implem.DefinitionAccessor;
using Implem.IRds;
using Implem.Libraries.DataSources.SqlServer;
using Implem.Libraries.Utilities;
using Implem.Pleasanter.Libraries.DataSources;
using Implem.Pleasanter.Libraries.Extensions;
using Implem.Pleasanter.Libraries.General;
using Implem.Pleasanter.Libraries.Html;
using Implem.Pleasanter.Libraries.HtmlParts;
using Implem.Pleasanter.Libraries.Models;
using Implem.Pleasanter.Libraries.Requests;
using Implem.Pleasanter.Libraries.Responses;
using Implem.Pleasanter.Libraries.Security;
using Implem.Pleasanter.Libraries.Settings;
using Implem.Pleasanter.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Implem.Pleasanter.Libraries.Search
{
    public static class Indexes
    {
        public static void Create(Context context, SiteSettings ss, long id, bool force = false)
        {
            if (force)
            {
                var itemModel = new ItemModel(
                    context: context,
                    referenceId: id);
                switch (itemModel.ReferenceType)
                {
                    case "Sites":
                        var siteModel = new SiteModel(context: context, siteId: id);
                        CreateFullText(
                            context: context,
                            id: id,
                            fullText: siteModel.FullText(
                                context: context,
                                ss: ss,
                                backgroundTask: true));
                        break;
                    case "Dashboards":
                        var dashboardModel = new DashboardModel(
                            context: context,
                            ss: ss,
                            dashboardId: id);
                        CreateFullText(
                            context: context,
                            id: id,
                            fullText: dashboardModel.FullText(
                                context: context,
                                ss: ss,
                                backgroundTask: true));
                        break;
                    case "Issues":
                        var issueModel = new IssueModel(
                            context: context,
                            ss: ss,
                            issueId: id);
                        ss.Links
                            ?.Where(o => o.SiteId > 0)
                            .Select(o => ss.GetColumn(
                                context: context,
                                columnName: o.ColumnName))
                            .Where(column => column?.UseSearch == true)
                            .ForEach(column =>
                                ss.SetChoiceHash(
                                    context: context,
                                    columnName: column.ColumnName,
                                    selectedValues: new List<string>
                                    {
                                        issueModel.PropertyValue(
                                            context: context,
                                            column: column)
                                    }));
                        CreateFullText(
                            context: context,
                            id: id,
                            fullText: issueModel.FullText(
                                context: context,
                                ss: ss,
                                backgroundTask: true));
                        break;
                    case "Results":
                        var resultModel = new ResultModel(
                            context: context,
                            ss: ss,
                            resultId: id);
                        ss.Links
                            ?.Where(o => o.SiteId > 0)
                            .Select(o => ss.GetColumn(
                                context: context,
                                columnName: o.ColumnName))
                            .Where(column => column?.UseSearch == true)
                            .ForEach(column =>
                                ss.SetChoiceHash(
                                    context: context,
                                    columnName: column.ColumnName,
                                    selectedValues: new List<string>
                                    {
                                        resultModel.PropertyValue(
                                            context: context,
                                            column: column)
                                    }));
                        CreateFullText(
                            context: context,
                            id: id,
                            fullText: resultModel.FullText(
                                context: context,
                                ss: ss,
                                backgroundTask: true));
                        break;
                    case "Wikis":
                        var wikiModel = new WikiModel(
                            context: context,
                            ss: ss,
                            wikiId: id);
                        CreateFullText(
                            context: context,
                            id: id,
                            fullText: wikiModel.FullText(
                                context: context,
                                ss: ss,
                                backgroundTask: true));
                        break;
                }
            }
        }

        private static void CreateFullText(Context context, long id, string fullText)
        {
            if (fullText != null)
            {
                Repository.ExecuteNonQuery(
                    context: context,
                    statements: Rds.UpdateItems(
                        where: Rds.ItemsWhere().ReferenceId(id),
                        param: Rds.ItemsParam()
                            .FullText(fullText)
                            .SearchIndexCreatedTime(DateTime.Now),
                        addUpdatorParam: false,
                        addUpdatedTimeParam: false));
            }
        }

        private static DataSet ResultContents(
            Context context, EnumerableRowCollection<DataRow> dataRows)
        {
            var statements = new List<SqlStatement>();
            if (dataRows.Any(o => o.String("ReferenceType") == "Sites"))
            {
                statements.Add(Rds.SelectSites(
                    dataTableName: "Sites",
                    column: Rds.SitesColumn()
                        .ParentId(_as: "SiteId")
                        .SiteId(_as: "Id")
                        .Body()
                        .Items_Title(),
                    join: new SqlJoinCollection(
                        new SqlJoin(
                            tableBracket: "\"Items\"",
                            joinType: SqlJoin.JoinTypes.Inner,
                            joinExpression: "\"Items\".\"ReferenceId\"=\"Sites\".\"SiteId\"")),
                    where: Rds.SitesWhere()
                        .TenantId(context.TenantId)
                        .SiteId_In(dataRows
                            .Where(o => o.String("ReferenceType") == "Sites")
                            .Select(o => o.Long("ReferenceId")))));
            }
            if (dataRows.Any(o => o.String("ReferenceType") == "Issues"))
            {
                statements.Add(Rds.SelectIssues(
                    dataTableName: "Issues",
                    column: Rds.IssuesColumn()
                        .SiteId()
                        .IssueId(_as: "Id")
                        .Body()
                        .Items_Title(),
                    join: new SqlJoinCollection(
                        new SqlJoin(
                            tableBracket: "\"Items\"",
                            joinType: SqlJoin.JoinTypes.Inner,
                            joinExpression: "\"Items\".\"ReferenceId\"=\"Issues\".\"IssueId\"")),
                    where: Rds.IssuesWhere()
                        .IssueId_In(dataRows
                            .Where(o => o.String("ReferenceType") == "Issues")
                            .Select(o => o.Long("ReferenceId")))));
            }
            if (dataRows.Any(o => o.String("ReferenceType") == "Results"))
            {
                statements.Add(Rds.SelectResults(
                    dataTableName: "Results",
                    column: Rds.ResultsColumn()
                        .SiteId()
                        .ResultId(_as: "Id")
                        .Body()
                        .Items_Title(),
                    join: new SqlJoinCollection(
                        new SqlJoin(
                            tableBracket: "\"Items\"",
                            joinType: SqlJoin.JoinTypes.Inner,
                            joinExpression: "\"Items\".\"ReferenceId\"=\"Results\".\"ResultId\"")),
                    where: Rds.ResultsWhere()
                        .ResultId_In(dataRows
                            .Where(o => o.String("ReferenceType") == "Results")
                            .Select(o => o.Long("ReferenceId")))));
            }
            if (dataRows.Any(o => o.String("ReferenceType") == "Wikis"))
            {
                statements.Add(Rds.SelectWikis(
                    dataTableName: "Wikis",
                    column: Rds.WikisColumn()
                        .SiteId()
                        .WikiId(_as: "Id")
                        .Body()
                        .Items_Title(),
                    join: new SqlJoinCollection(
                        new SqlJoin(
                            tableBracket: "\"Items\"",
                            joinType: SqlJoin.JoinTypes.Inner,
                            joinExpression: "\"Items\".\"ReferenceId\"=\"Wikis\".\"WikiId\"")),
                    where: Rds.WikisWhere()
                        .WikiId_In(dataRows
                            .Where(o => o.String("ReferenceType") == "Wikis")
                            .Select(o => o.Long("ReferenceId")))));
            }
            return Repository.ExecuteDataSet(
                context: context,
                statements: statements.ToArray());
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        public static string Search(Context context)
        {
            if (Parameters.Search.DisableCrossSearch)
            {
                return HtmlTemplates.Error(
                    context: context,
                    errorData: new ErrorData(type: Error.Types.InvalidRequest));
            }
            var dataSet = Get(
                context: context,
                searchText: context.QueryStrings.Data("text"),
                dataTableName: "SearchResults",
                offset: context.QueryStrings.Int("offset"),
                pageSize: Parameters.Search.PageSize);
            return MainContainer(
                context: context,
                text: context.QueryStrings.Data("text"),
                offset: 0,
                results: dataSet?.Tables["SearchResults"].AsEnumerable(),
                count: Rds.Count(dataSet)).ToString();
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        public static string SearchJson(Context context)
        {
            var offset = context.QueryStrings.Int("offset");
            var searchText = context.QueryStrings.Data("text");
            var dataSet = Get(
                context: context,
                searchText: searchText,
                dataTableName: "SearchResults",
                offset: offset,
                pageSize: Parameters.Search.PageSize);
            var dataRows = dataSet?.Tables["SearchResults"].AsEnumerable();
            var res = new ResponseCollection(context: context);
            return offset == 0
                ? res
                    .ReplaceAll(
                        "#MainContainer",
                        MainContainer(
                            context: context,
                            text: searchText,
                            offset: 0,
                            results: dataRows,
                            count: Rds.Count(dataSet)))
                    .Focus("#Search")
                    .ToJson()
                : res
                    .Append(
                        "#SearchResults",
                        new HtmlBuilder().Results(
                            context: context,
                            text: searchText,
                            offset: offset,
                            dataRows: dataRows))
                    .Val(
                        "#SearchOffset",
                        (dataRows != null &&
                        dataRows.Any() &&
                        dataRows.Count() == Parameters.Search.PageSize
                            ? offset + Parameters.Search.PageSize
                            : -1).ToString())
                    .ToJson();
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static HtmlBuilder MainContainer(
            Context context,
            string text,
            int offset,
            EnumerableRowCollection<DataRow> results,
            int count)
        {
            var hb = new HtmlBuilder();
            var searchIndexes = text.SearchIndexes();
            return hb.Template(
                context: context,
                ss: new SiteSettings(),
                view: null,
                referenceType: "SearchIndexes",
                title: string.Empty,
                useNavigationMenu: true,
                useTitle: false,
                useSearch: false,
                useBreadcrumb: false,
                action: () => hb
                    .Div(id: "SearchResults", action: () => hb
                        .Command(
                            context: context,
                            text: text)
                        .Count(
                            context: context,
                            count: count)
                        .Results(
                            context: context,
                            text: text,
                            offset: offset,
                            dataRows: results))
                    .Hidden(
                        controlId: "SearchOffset",
                        value: Parameters.Search.PageSize.ToString()));
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static HtmlBuilder Command(this HtmlBuilder hb, Context context, string text)
        {
            return hb.Div(css: "command-center", action: () => hb
                .TextBox(
                    controlId: "Search",
                    controlCss: " w600 focus",
                    text: text,
                    placeholder: Displays.Search(context: context)));
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static HtmlBuilder Count(this HtmlBuilder hb, Context context, int count)
        {
            return hb.Div(css: "count", action: () => hb
                .Span(css: "label", action: () => hb
                    .Text(text: Displays.Quantity(context: context)))
                .Span(css: "data", action: () => hb
                    .Text(text: count.ToString())));
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static HtmlBuilder Results(
            this HtmlBuilder hb,
            Context context,
            string text,
            int offset,
            EnumerableRowCollection<DataRow> dataRows)
        {
            if (dataRows?.Any() == true)
            {
                var dataSet = ResultContents(context: context, dataRows: dataRows);
                dataRows.ForEach(result =>
                {
                    var referenceType = result.String("ReferenceType");
                    var referenceId = result.Long("ReferenceId");
                    var dataRow = dataSet.Tables[referenceType]
                        .AsEnumerable()
                        .FirstOrDefault(o => o["Id"].ToLong() == referenceId);
                    if (dataRow != null)
                    {
                        var href = string.Empty;
                        var highlightSplitedTitles = dataRow.String("Title").Split(text);
                        var highlightSplitedBodys = dataRow.String("Body").Split(text);
                        switch (referenceType)
                        {
                            case "Sites":
                                href = Locations.ItemIndex(
                                    context: context,
                                    id: referenceId);
                                break;
                            default:
                                href = Locations.ItemEdit(
                                    context: context,
                                    id: referenceId);
                                break;
                        }
                        href += "?back=1";
                        hb.Section(
                            attributes: new HtmlAttributes()
                                .Class("result")
                                .Add("data-href", href),
                            action: () => hb
                                .Breadcrumb(
                                    context: context,
                                    ss: new SiteSettings()
                                    {
                                        SiteId = dataRow.Long("SiteId")
                                    })
                                .H(number: 3, action: () => hb
                                    .A(
                                         href: href,
                                         action: () =>
                                             highlightSplitedTitles.ForEach(highlightSplitedTitle =>
                                             {
                                                 if(highlightSplitedTitle != highlightSplitedTitles[0])
                                                 {
                                                     hb.Span(
                                                         css: "highlight",
                                                         action: () => hb
                                                             .Text(text));
                                                 }
                                                 hb.Text(highlightSplitedTitle);
                                             })))
                                .P(action: () => 
                                    highlightSplitedBodys.ForEach(highlightSplitedBody =>
                                    {
                                        if (highlightSplitedBody != highlightSplitedBodys[0])
                                        {
                                            hb.Span(
                                                css: "highlight",
                                                action: () => hb
                                                    .Text(text));
                                        }
                                        hb.Text(highlightSplitedBody);
                                    })));
                    }
                });
            }
            return hb;
        }

        public static SqlWhereCollection SearchTextWhere(
            this SqlWhereCollection where,
            Context context,
            SiteSettings ss,
            string searchText)
        {
            if (ss != null && ss.TableType != Sqls.TableTypes.Normal)
            {
                return where.SqlWhereLike(
                    tableName: "Items",
                    name: "SearchText",
                    searchText: searchText,
                    clauseCollection: Rds.Items_FullText_WhereLike(
                        factory: context,
                        forward: false).ToSingleList());
            }
            switch (ss?.SearchType)
            {
                case SiteSettings.SearchTypes.FullText:
                    var words = searchText.IsNullOrEmpty()
                        ? Words(searchText.SearchIndexes().Join(" "))
                        : context.SqlCommandText.CreateSearchTextWords(
                            words: Words(searchText.SearchIndexes().Join(" ")),
                            searchText: searchText.SearchIndexes().Join(" "));
                    if (words?.Any() != true) return where;
                    return where.FullTextWhere(
                        context: context,
                        words: words);
                case SiteSettings.SearchTypes.MatchInFrontOfTitle:
                    return where.SqlWhereLike(
                        tableName: "Items",
                        name: "SearchText",
                        searchText: searchText,
                        clauseCollection: Rds.Items_Title_WhereLike(
                            factory: context,
                            forward: true).ToSingleList());
                case SiteSettings.SearchTypes.BroadMatchOfTitle:
                    return where.SqlWhereLike(
                        tableName: "Items",
                        name: "SearchText",
                        searchText: searchText,
                        clauseCollection: Rds.Items_Title_WhereLike(
                            factory: context,
                            forward: false).ToSingleList());
                case SiteSettings.SearchTypes.PartialMatch:
                default:
                    return where.SqlWhereLike(
                        tableName: "Items",
                        name: "SearchText",
                        searchText: searchText,
                        clauseCollection: Rds.Items_FullText_WhereLike(
                            factory: context,
                            forward: false).ToSingleList());
            }
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        public static SqlWhereCollection FullTextWhere(
            this SqlWhereCollection where,
            Context context,
            SiteSettings ss,
            string searchText,
            bool itemJoin,
            bool negative)
        {
            var name = Strings.NewGuid();
            if (ss != null && ss.TableType != Sqls.TableTypes.Normal)
            {
                return where.ItemWhereLike(
                    context: context,
                    ss: ss,
                    columnName: "FullText",
                    searchText: searchText,
                    name: name,
                    forward: false,
                    itemJoin: itemJoin,
                    negative: negative);
            }
            switch (ss?.SearchType)
            {
                case SiteSettings.SearchTypes.FullText:
                    var words = context.SqlCommandText.CreateSearchTextWords(
                        words: Words(searchText.SearchIndexes().Join(" ")),
                        searchText: searchText.SearchIndexes().Join(" "));
                    if (words?.Any() != true) return where;
                    if (itemJoin)
                    {
                        where.FullTextWhere(
                            context: context,
                            words: words,
                            itemsTableName: ss.ReferenceType + "_Items",
                            negative: negative);
                    }
                    else
                    {
                        where.FullTextWhere(
                            context: context,
                            words: words,
                            idColumnBracket: ss.IdColumnBracket(),
                            tableType: ss.TableType,
                            negative: negative);
                    }
                    return where;
                case SiteSettings.SearchTypes.MatchInFrontOfTitle:
                    return where.ItemWhereLike(
                        context: context,
                        ss: ss,
                        columnName: "Title",
                        searchText: searchText,
                        name: name,
                        forward: true,
                        itemJoin: itemJoin,
                        negative: negative);
                case SiteSettings.SearchTypes.BroadMatchOfTitle:
                    return where.ItemWhereLike(
                        context: context,
                        ss: ss,
                        columnName: "Title",
                        searchText: searchText,
                        name: name,
                        forward: false,
                        itemJoin: itemJoin,
                        negative: negative);
                case SiteSettings.SearchTypes.PartialMatch:
                default:
                    return where.ItemWhereLike(
                        context: context,
                        ss: ss,
                        columnName: "FullText",
                        searchText: searchText,
                        name: name,
                        forward: false,
                        itemJoin: itemJoin,
                        negative: negative);
            }
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static SqlWhereCollection ItemWhereLike(
            this SqlWhereCollection where,
            Context context,
            SiteSettings ss,
            string columnName,
            string searchText,
            string name,
            bool forward,
            bool itemJoin,
            bool negative = false)
        {
            var tableName = ItemTableName(
                ss: ss,
                itemJoin: itemJoin);
            return where.Add(or: Rds.ItemsWhere().SqlWhereLike(
                tableName: "Items",
                name: name,
                searchText: context.Sqls.EscapeValue(searchText),
                clauseCollection: itemJoin
                    ? ItemWhereLike(
                        context: context,
                        tableName: tableName,
                        columnName: columnName,
                        name: name,
                        forward: forward,
                        negative: negative)
                            .ToSingleList()
                    : SelectItemWhereLike(
                        context: context,
                        ss: ss,
                        tableName: tableName,
                        columnName: columnName,
                        name: name,
                        forward: forward,
                        negative: negative)
                            .ToSingleList()));
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static string ItemTableName(SiteSettings ss, bool itemJoin)
        {
            if (itemJoin)
            {
                return ss.ReferenceType + "_Items";
            }
            else
            {
                switch (ss.TableType)
                {
                    case Sqls.TableTypes.Deleted:
                        return "Items_deleted";
                    default:
                        return "Items";
                }
            }
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static string ItemWhereLike(
            Context context,
            string tableName,
            string columnName,
            string name,
            bool forward,
            bool negative = false)
        {
            switch (columnName)
            {
                case "Title":
                    return Rds.Items_Title_WhereLike(
                        factory: context,
                        tableName: tableName,
                        name: name,
                        forward: forward,
                        escape: true,
                        negative: negative);
                case "FullText":
                    return Rds.Items_FullText_WhereLike(
                        factory: context,
                        tableName: tableName,
                        name: name,
                        forward: forward,
                        escape: true,
                        negative: negative);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static string SelectItemWhereLike(
            Context context,
            SiteSettings ss,
            string tableName,
            string columnName,
            string name,
            bool forward = false,
            bool negative = false)
        {
            var like = ItemWhereLike(
                context: context,
                tableName: tableName,
                columnName: columnName,
                name: name,
                forward: forward,
                negative: negative);
            return $"exists(select * from \"{tableName}\" where \"{tableName}\".\"ReferenceId\"={ss.IdColumnBracket()} and {like})";
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        public static DataSet Get(
            Context context,
            string searchText,
            string dataTableName,
            int offset,
            int pageSize)
        {
            return Get(
                context: context,
                searchText: searchText,
                dataTableName: dataTableName,
                offset: offset,
                pageSize: pageSize,
                countRecord: offset == 0);
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        public static DataSet Get(
            Context context,
            string searchText,
            IEnumerable<long> siteIdList = null,
            string dataTableName = null,
            int offset = 0,
            int pageSize = 0,
            bool countRecord = false)
        {
            var words = context.SqlCommandText.CreateSearchTextWords(
                words: Words(searchText.SearchIndexes().Join(" ")),
                searchText: searchText.SearchIndexes().Join(" "));
            if (words?.Any() != true) return null;
            var statements = new List<SqlStatement>
            {
                SelectByFullText(
                    context: context,
                    words: words,
                    column: Rds.ItemsColumn()
                        .ReferenceId()
                        .ReferenceType()
                        .Title(),
                    orderBy: Rds.ItemsOrderBy()
                        .UpdatedTime(SqlOrderBy.Types.desc),
                    siteIdList: siteIdList,
                    dataTableName: dataTableName,
                    offset: offset,
                    pageSize: pageSize)
            };
            if (countRecord)
            {
                statements.Add(SelectByFullText(
                    context: context,
                    words: words,
                    countRecord: true));
            }
            return Repository.ExecuteDataSet(
                context: context,
                statements: statements.ToArray());
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static Dictionary<string, string> Words(string searchText)
        {
            return searchText?
                .Replace("　", " ")
                .Replace("\"", " ")
                .Replace("'", "’")
                .Trim()
                .Split(' ')
                .Where(o => o != string.Empty)
                .Distinct()
                .Select(o => FullTextClause(o))
                .ToDictionary(o => Strings.NewGuid(), o => o);
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static SqlSelect SelectByFullText(
            Context context,
            Dictionary<string, string> words,
            SqlColumnCollection column = null,
            SqlOrderByCollection orderBy = null,
            IEnumerable<long> siteIdList = null,
            string dataTableName = null,
            int offset = 0,
            int pageSize = 0,
            bool countRecord = false)
        {
            return !countRecord
                ? Rds.SelectItems(
                    dataTableName: dataTableName,
                    column: column,
                    join: new SqlJoinCollection(
                        new SqlJoin(
                            tableBracket: "\"Sites\"",
                            joinType: SqlJoin.JoinTypes.Inner,
                            joinExpression: "\"Items\".\"SiteId\"=\"Sites\".\"SiteId\"")),
                    where: Rds.ItemsWhere().FullTextWhere(
                        context: context,
                        words: words,
                        siteIdList: siteIdList),
                    param: FullTextParam(words),
                    orderBy: orderBy,
                    offset: offset,
                    pageSize: pageSize)
                : Rds.SelectCount(
                    tableName: "Items",
                    join: new SqlJoinCollection(
                        new SqlJoin(
                            tableBracket: "\"Sites\"",
                            joinType: SqlJoin.JoinTypes.Inner,
                            joinExpression: "\"Items\".\"SiteId\"=\"Sites\".\"SiteId\"")),
                    where: Rds.ItemsWhere().FullTextWhere(
                        context: context,
                        words: words,
                        siteIdList: siteIdList),
                    param: FullTextParam(words));
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static SqlWhereCollection FullTextWhere(
            this SqlWhereCollection where,
            Context context,
            Dictionary<string, string> words,
            IEnumerable<long> siteIdList)
        {
            return Rds.ItemsWhere()
                .Add(
                    raw: "\"Items\".\"SiteId\" in ({0})".Params(siteIdList?.Join()),
                    _using: siteIdList?.Any() == true)
                .FullTextWhere(
                    context: context,
                    words: words)
                .Add(
                    raw: Def.Sql.CanRead,
                    _using: !context.HasPrivilege && !context.Publish)
                .Add(raw: $"{context.Sqls.IsNull}(\"Sites\".\"DisableCrossSearch\",{context.Sqls.FalseString})={context.Sqls.FalseString}")
                .Add(
                    raw: "\"Items\".\"ReferenceType\"<>'Sites'",
                    _using: Parameters.Search.DisableCrossSearchSites);
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        public static SqlWhereCollection FullTextWhere(
            this SqlWhereCollection where,
            Context context,
            Dictionary<string, string> words,
            string itemsTableName = "Items",
            bool negative = false)
        {
            words.ForEach(data => where.Add(
                name: data.Key,
                value: data.Value,
                raw: FullTextWhere(
                    factory: context,
                    name: data.Key,
                    itemsTableName: itemsTableName,
                    negative: negative)));
            return where;
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static string FullTextWhere(
            ISqlObjectFactory factory,
            string name,
            string itemsTableName = "Items",
            bool negative = false)
        {
            var item = factory.SqlCommandText.CreateFullTextWhereItem(itemsTableName, name, negative);
            var binary = factory.SqlCommandText.CreateFullTextWhereBinary(itemsTableName, name, negative);
            return Parameters.Search.SearchDocuments
                ? $"({item} or {binary})"
                : item;
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static SqlWhereCollection FullTextWhere(
            this SqlWhereCollection where,
            Context context,
            Dictionary<string, string> words,
            string idColumnBracket,
            Sqls.TableTypes tableType,
            bool negative)
        {
            words.ForEach(data => where.Add(
                name: data.Key,
                value: data.Value,
                raw: FullTextWhere(
                    factory: context,
                    name: data.Key,
                    idColumnBracket: idColumnBracket,
                    tableType: tableType,
                    negative: negative)));
            return where;
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static string FullTextWhere(
            ISqlObjectFactory factory,
            string name,
            string idColumnBracket,
            Sqls.TableTypes tableType,
            bool negative)
        {
            var suffix = string.Empty;
            switch (tableType)
            {
                case Sqls.TableTypes.Deleted:
                    suffix = "_deleted";
                    break;
            }
            string item;
            string binary;
            var not = negative
                ? "not "
                : string.Empty;
            item = $"{not}exists(select * from \"Items{suffix}\" where \"Items{suffix}\".\"ReferenceId\"={idColumnBracket} and {factory.SqlCommandText.CreateFullTextWhereItem("Items" + suffix, name, negative)})";
            binary = $"{not}exists(select * from \"Binaries{suffix}\" where \"Binaries{suffix}\".\"ReferenceId\"={idColumnBracket} and {factory.SqlCommandText.CreateFullTextWhereBinary("Binaries" + suffix, name, negative)})";
            return Parameters.Search.SearchDocuments
                ? $"({item} or {binary})"
                : item;
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static Rds.ItemsParamCollection FullTextParam(Dictionary<string, string> words)
        {
            var param = Rds.ItemsParam();
            words
                .Select(o => new { Name = o.Key, o.Value })
                .ForEach(data =>
                    param.Add(
                        name: data.Name,
                        value: data.Value));
            return param;
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static string FullTextClause(string word)
        {
            var data = new List<string> { word };
            var katakana = CSharp.Japanese.Kanaxs.KanaEx.ToKatakana(word);
            var hiragana = CSharp.Japanese.Kanaxs.KanaEx.ToHiragana(word);
            if (word != katakana) data.Add(katakana);
            if (word != hiragana) data.Add(hiragana);
            return "(" + data
                .SelectMany(part => new List<string>
                {
                    part,
                    ForwardMatchSearch(part: part)
                })
                .Where(o => o != null)
                .Distinct()
                .Select(o => "\"" + o + "\"")
                .Join(" or ") + ")";
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private static string ForwardMatchSearch(string part)
        {
            var separators = "!#$%&()*+,-./:<=>?@[\\]^_`{|}~";
            foreach (var separator in separators)
            {
                if (part.Split(separator).Any(o => o.RegexExists("^[0-9]$")))
                {
                    return null;
                }
            }
            return part + "*";
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        public static string RebuildSearchIndexes(Context context, SiteModel siteModel)
        {
            siteModel.SiteSettings = SiteSettingsUtilities.Get(
                context: context,
                siteModel: siteModel,
                referenceId: siteModel.SiteId);
            var ss = siteModel.SiteSettings.SiteSettingsOnUpdate(context: context);
            var invalid = SiteValidators.OnUpdating(
                context: context,
                ss: ss,
                siteModel: siteModel);
            switch (invalid.Type)
            {
                case Error.Types.None: break;
                default: return invalid.MessageJson(context: context);
            }
            RebuildSearchIndexes(
                context: context,
                siteId: siteModel.SiteId);
            return new ResponseCollection(context: context)
                .Message(Messages.RebuildingCompleted(context: context))
                .ToJson();
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        public static void RebuildSearchIndexes(Context context, long siteId = -1)
        {
            var hash = new Dictionary<long, SiteModel>();
            context.PermissionHash = Rds.ExecuteTable(
                context: context,
                statements: Rds.SelectSites(
                    column: Rds.SitesColumn().SiteId()))
                        .AsEnumerable()
                        .ToDictionary(
                            o => o.Long("SiteId"),
                            o => Permissions.Types.Read);
            Repository.ExecuteTable(
                context: context,
                statements: Rds.SelectItems(
                    column: Rds.ItemsColumn()
                        .ReferenceId()
                        .SiteId(),
                    where: siteId > 0
                        ? Rds.ItemsWhere().SiteId(siteId)
                        : Rds.ItemsWhere().Add(raw: new List<string>()
                        {
                            "\"Items\".\"SearchIndexCreatedTime\" is null",
                            "\"Items\".\"SearchIndexCreatedTime\"<>\"Items\".\"UpdatedTime\""
                        }.Join(" or ")),
                    top: siteId > 0
                        ? 0
                        : Parameters.BackgroundTask.CreateSearchIndexLot))
                            .AsEnumerable()
                            .Select(o => new
                            {
                                ReferenceId = o["ReferenceId"].ToLong(),
                                SiteId = o["SiteId"].ToLong()
                            })
                            .ForEach(data =>
                            {
                                var siteModel = hash.Get(data.SiteId) ??
                                    new SiteModel().Get(
                                        context: context,
                                        where: Rds.SitesWhere().SiteId(data.SiteId));
                                if (!hash.ContainsKey(data.SiteId))
                                {
                                    siteModel.SiteSettings = SiteSettingsUtilities.Get(
                                        context: context,
                                        siteModel: siteModel,
                                        referenceId: siteModel.SiteId);
                                    hash.Add(data.SiteId, siteModel);
                                }
                                Create(
                                    context: context,
                                    ss: siteModel.SiteSettings,
                                    id: data.ReferenceId,
                                    force: true);
                                Repository.ExecuteNonQuery(
                                    context: context,
                                    statements: Rds.UpdateItems(
                                        where: Rds.ItemsWhere()
                                            .ReferenceId(data.ReferenceId),
                                        param: Rds.ItemsParam()
                                            .SearchIndexCreatedTime(raw: "\"UpdatedTime\""),
                                        addUpdatorParam: false,
                                        addUpdatedTimeParam: false));
                            });
        }
    }
}
